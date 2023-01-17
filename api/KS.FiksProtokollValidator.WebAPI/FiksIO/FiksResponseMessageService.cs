using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using KS.Fiks.Arkiv.Models.V1.Meldingstyper;
using KS.Fiks.ASiC_E;
using KS.Fiks.ASiC_E.Crypto;
using KS.Fiks.ASiC_E.Xsd;
using KS.Fiks.IO.Client.Models;
using KS.FiksProtokollValidator.WebAPI.Data;
using KS.FiksProtokollValidator.WebAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace KS.FiksProtokollValidator.WebAPI.FiksIO
{
    /* This is the consumer that receives messages from Fiks-Protokoller/Fiks-IO
     * That means we are interested in keeping the connection to Fiks-IO open.
     * Here we are using the health-check (IsOpen) on the Fiks-IO client and have implemented a self-healing BackgroundService
     * We are also exposing the health as a healthz endpoint. That is why the FiksIOClient is kept in a singleton outside this BackgroundService.
     * This way we can use the health status for the healthz endpoint in the running application.
     */
    public class FiksResponseMessageService : BackgroundService
    {
        
        private readonly IFiksIOClientConsumerService _fiksIoClientConsumerService;
        private static readonly ILogger Logger = Log.ForContext(MethodBase.GetCurrentMethod()?.DeclaringType);
        private readonly IServiceScopeFactory _scopeFactory;
        private const int HealthCheckInterval = 5 * 60 * 1000;
        
        private Timer _ensureFiksIOConnectionIsOpenTimer { get; set; }

        public FiksResponseMessageService(IServiceScopeFactory scopeFactory, IFiksIOClientConsumerService fiksIoFiksIoClientConsumerService)
        {
            _scopeFactory = scopeFactory;
            _fiksIoClientConsumerService = fiksIoFiksIoClientConsumerService;
            // Self-healing check
            _ensureFiksIOConnectionIsOpenTimer = new Timer(Callback, null, HealthCheckInterval, HealthCheckInterval);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Logger.Information("Starter subscription - ExectueAsync");
            
            await _fiksIoClientConsumerService.Initialization;

            stoppingToken.ThrowIfCancellationRequested();
            _fiksIoClientConsumerService.FiksIOConsumerClient.NewSubscription(OnMottattMelding);   
            await Task.CompletedTask;
        }

        private bool AsiceIsVerified(asicManifest asicManifest)
        {
            return asicManifest != null && asicManifest.certificate != null && asicManifest.certificate.Length == 1 && asicManifest.file != null && asicManifest.rootfile == null;
        }

        private async void OnMottattMelding(object sender, MottattMeldingArgs mottattMeldingArgs)
        {
            Logger.Information("FiksResponseMessageService: Henter melding med MeldingId: {MeldingId}", mottattMeldingArgs.Melding.MeldingId);
            var payloads = new List<FiksPayload>();

            var isAsiceVerified = false;
            var payloadErrors = "";

            if (mottattMeldingArgs.Melding.HasPayload)
            {
                try
                {
                    using var verifyStream2 = mottattMeldingArgs.Melding.DecryptedStream;
                    IAsicReader asiceReader = new AsiceReader();
                    using var asiceReadModel = asiceReader.Read(await verifyStream2);
                  
                    // Verify asice and read payload
                    foreach (var asiceVerifyReadEntry in asiceReadModel.Entries)
                    {
                        await using (var entryStream = asiceVerifyReadEntry.OpenStream())
                        {
                            byte[] fileAsBytes;
                            await using (MemoryStream ms = new MemoryStream())
                            {
                                await entryStream.CopyToAsync(ms);
                                fileAsBytes = ms.ToArray();
                            }
                            payloads.Add(new FiksPayload() { Filename = asiceVerifyReadEntry.FileName, Payload = fileAsBytes });
                        }
                    }
                    try
                    {
                        var verificationResult = asiceReadModel.DigestVerifier.Verification();
                        if (verificationResult != null)
                        {
                            if (!verificationResult.AllValid)
                            {
                                var invalidFileList = verificationResult.InvalidElements.Aggregate((aggregate, element) =>
                                    aggregate + "," + element);
                                payloadErrors = $"Asice validering: klarte ikke validere digest for følgende filer {invalidFileList}";
                            }

                            isAsiceVerified = AsiceIsVerified(asiceReadModel.VerifiedManifest());
                        }
                    }
                    catch (Exception e)
                    {
                        payloadErrors = $"Asice validering: klarte ikke validere digest for payload";
                    }
                }
                catch (Exception e)
                {
                    Logger.Error("FiksResponseMessageService: Klarte ikke hente payload og melding blir dermed ikke parset. MeldingId: {MeldingId}, Error: {Message}", mottattMeldingArgs.Melding?.MeldingId, e.Message);
                    payloadErrors += $"Klarte ikke hente payload og melding blir dermed ikke parset. MeldingId: {mottattMeldingArgs.Melding?.MeldingId}, Error: {e.Message}";
                    mottattMeldingArgs.SvarSender?.Ack();
                }
            }

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<FiksIOMessageDBContext>();
                var testSession = context.TestSessions.Include(t => t.FiksRequests).FirstOrDefaultAsync(t =>
                    t.FiksRequests.Any(r => r.MessageGuid.Equals(mottattMeldingArgs.Melding.SvarPaMelding))).Result;

                // Not optimal? Det er gjort slik fordi man ikke vet om databasen har rukket å skrive før man får svar.
                var timesTried = 1;
                while (testSession == null && timesTried <= 5)
                {
                    Thread.Sleep(1000);
                    testSession = context.TestSessions.Include(t => t.FiksRequests).FirstOrDefault(t =>
                        t.FiksRequests.Any(r => r.MessageGuid.Equals(mottattMeldingArgs.Melding.SvarPaMelding)));
                    timesTried++;
                }

                if (testSession != null)
                {
                    var fiksRequest =
                        testSession.FiksRequests.Find(r => r.MessageGuid.Equals(mottattMeldingArgs.Melding.SvarPaMelding));

                    var responseMessage = new FiksResponse
                    {
                        ReceivedAt = DateTime.Now,
                        Type = mottattMeldingArgs.Melding.MeldingType,
                        FiksPayloads = payloads,
                        IsAsiceVerified = isAsiceVerified,
                        PayloadErrors = payloadErrors
                    };

                    if (fiksRequest == null)
                    {
                        mottattMeldingArgs.SvarSender?.Ack();
                        Logger.Error("FiksResponseMessageService: Klarte ikke å matche svar-melding fra FIKS med en eksisterende forespørsel. Testsession med id {TestSessionId} funnet. Svarmelding forkastes. SvarPaMelding id: {Id}", testSession.Id, mottattMeldingArgs.Melding.SvarPaMelding);
                        return;
                    }

                    fiksRequest.FiksResponses ??= new List<FiksResponse>();
                    fiksRequest.FiksResponses.Add(responseMessage);

                    context.Entry(testSession).State = EntityState.Modified;
                    await context.SaveChangesAsync();
                }
                else
                {
                    Logger.Error("FiksResponseMessageService: Klarte ikke å matche svar-melding fra FIKS med en eksisterende testsesjon. Testsession ikke funnet. Svarmelding forkastes. SvarPaMelding id: {Id}", mottattMeldingArgs.Melding.SvarPaMelding);
                }
            }
            finally
            {
                mottattMeldingArgs.SvarSender?.Ack();
            }
        }
        
        private async void Callback(object o)
        {
            await EnsureFiksIOConnectionIsOpen().ConfigureAwait(false);
        }
        
        
        private async Task EnsureFiksIOConnectionIsOpen()
        {
            // await FiksIOClient initialization
            await _fiksIoClientConsumerService.Initialization;
            
            if (!_fiksIoClientConsumerService.IsHealthy())
            {
                Logger.Error("FiksResponseMessageService: self-check detects FiksIOClient connection is down! Restarting background service");
                await _fiksIoClientConsumerService.Reconnect();
                await StopAsync(default);
                await StartAsync(default);
            }
            else
            {
                Logger.Information("FiksResponseMessageService: self-check detects FiksIOClient is ok");
            }
        }
    }
}
