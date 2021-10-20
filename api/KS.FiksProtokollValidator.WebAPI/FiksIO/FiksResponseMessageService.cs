using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KS.Fiks.ASiC_E;
using KS.Fiks.IO.Client;
using KS.Fiks.IO.Client.Models;
using KS.FiksProtokollValidator.WebAPI.Data;
using KS.FiksProtokollValidator.WebAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace KS.FiksProtokollValidator.WebAPI.FiksIO
{
    public class FiksResponseMessageService : BackgroundService
    {
        private IFiksIOClient _client;
        private static readonly ILogger Logger = Log.ForContext(MethodBase.GetCurrentMethod()?.DeclaringType);
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly AppSettings _appSettings;

        public FiksResponseMessageService(IServiceScopeFactory scopeFactory, AppSettings appSettings)
        {
            _scopeFactory = scopeFactory;
            _appSettings = appSettings;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();
            _client = new FiksIOClient(FiksIOConfigurationBuilder.CreateFiksIOConfiguration(_appSettings));
            var onReceived = new EventHandler<MottattMeldingArgs>(OnMottattMelding);
            _client.NewSubscription(onReceived);   
            
            await Task.CompletedTask;
        }

        private async void OnMottattMelding(object sender, MottattMeldingArgs mottattMeldingArgs)
        {
            Logger.Information("Henter melding med MeldingId: {MeldingId}", mottattMeldingArgs.Melding.MeldingId);
            var payloads = new Dictionary<string, string>();

            if (mottattMeldingArgs.Melding.HasPayload)
            {
                try
                {
                    // Verify that message has payload
                    IAsicReader reader = new AsiceReader();
                    await using var inputStream = mottattMeldingArgs.Melding.DecryptedStream.Result;
                    using var asice = reader.Read(inputStream);
                    foreach (var asiceReadEntry in asice.Entries)
                    {
                        await using var entryStream = asiceReadEntry.OpenStream();
                        var reader1 = new StreamReader(entryStream, Encoding.UTF8);
                        var content = await reader1.ReadToEndAsync();
                        payloads.Add(asiceReadEntry.FileName, content);
                    } 
                }
                catch (Exception e)
                {
                    Logger.Error("Klarte ikke hente payload og melding blir dermed ikke parset. MeldingId: {MeldingId}, Error: {Message}", mottattMeldingArgs.Melding?.MeldingId, e.Message);
                    mottattMeldingArgs.SvarSender?.Ack();
                    return;
                }
            }

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<FiksIOMessageDBContext>();
                var testSession = context.TestSessions.Include(t => t.FiksRequests).FirstOrDefaultAsync(t =>
                    t.FiksRequests.Any(r => r.MessageGuid.Equals(mottattMeldingArgs.Melding.SvarPaMelding))).Result;

                // Ikke optimalt? Det er gjort slik fordi man ikke vet om databasen har rukket å skrive før man får svar.
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
                        Payload = payloads.Count > 1 ? string.Join(',', payloads.Keys) :
                            payloads.Count == 1 ? payloads.Keys.ElementAt(0) : null,
                        PayloadContent = payloads.Count == 1 ? payloads.Values.ElementAt(0) : null,
                    };

                    if (fiksRequest == null)
                    {
                        mottattMeldingArgs.SvarSender?.Ack();
                        Logger.Error("Klarte ikke å matche svar-melding fra FIKS med en eksisterende forespørsel. Testsession med id {TestSessionId} funnet. Svarmelding forkastes. SvarPaMelding id: {Id}", testSession.Id, mottattMeldingArgs.Melding.SvarPaMelding);
                        return;
                    }

                    fiksRequest.FiksResponses ??= new List<FiksResponse>();

                    fiksRequest.FiksResponses.Add(responseMessage);

                    context.Entry(testSession).State = EntityState.Modified;
                    await context.SaveChangesAsync();
                }
                else
                {
                    Logger.Error("Klarte ikke å matche svar-melding fra FIKS med en eksisterende testsesjon. Testsession ikke funnet. Svarmelding forkastes. SvarPaMelding id: {Id}", mottattMeldingArgs.Melding.SvarPaMelding);
                }
            }
            finally
            {
                mottattMeldingArgs.SvarSender?.Ack();
            }
        }
    }
}
