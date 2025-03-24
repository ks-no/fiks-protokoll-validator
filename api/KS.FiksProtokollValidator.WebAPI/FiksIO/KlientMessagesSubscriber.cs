using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using KS.Fiks.Arkiv.Models.V1.Meldingstyper;
using KS.Fiks.ASiC_E;
using KS.Fiks.ASiC_E.Xsd;
using KS.Fiks.IO.Client.Models;
using KS.Fiks.IO.Crypto.Models;
using KS.FiksProtokollValidator.WebAPI.FiksIO.Connection;
using KS.FiksProtokollValidator.WebAPI.KlientValidator.Engines.FiksArkiv;
using KS.FiksProtokollValidator.WebAPI.KlientValidator.Managers.FiksArkiv;
using KS.FiksProtokollValidator.WebAPI.KlientValidator.Models;
using KS.FiksProtokollValidator.WebAPI.KlientValidator.Utilities.Serialization;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Serilog;
using FiksPayload = KS.FiksProtokollValidator.WebAPI.TjenerValidator.Models.FiksPayload;

namespace KS.FiksProtokollValidator.WebAPI.FiksIO
{
    public class KlientMessagesSubscriber : BackgroundService
    {
        private readonly FiksIOConnectionManager _fiksIOConnectionManager;
        private static readonly ILogger Logger = Log.ForContext(MethodBase.GetCurrentMethod()?.DeclaringType);
        private readonly RegistreringHentManager _registreringHentManager;
        private readonly MappeHentManager _mappeHentManager;
        private readonly SokManager _sokManager;
        private readonly ArkivmeldingManager _arkivmeldingManager;
        private readonly ArkivmeldingOppdaterManager _arkivmeldingOppdaterManager;

        public KlientMessagesSubscriber(FiksIOConnectionManager manager)
        {
            _fiksIOConnectionManager = manager;
            _registreringHentManager = new RegistreringHentManager();
            _mappeHentManager = new MappeHentManager();
            _sokManager = new SokManager();
            _arkivmeldingManager = new ArkivmeldingManager();
            _arkivmeldingOppdaterManager = new ArkivmeldingOppdaterManager();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Logger.Information($"ExectueAsync start. Oppretter subscriptions for {_fiksIOConnectionManager.KlientConnectionServices.Count} protokoller");

            foreach (var fiksIoClientConsumerService in _fiksIOConnectionManager.KlientConnectionServices)
            {
                await fiksIoClientConsumerService.Value.Initialization;
                //TODO her bør det være en OnMottatt for hver protokoll
              await fiksIoClientConsumerService.Value.FiksIOClient.NewSubscriptionAsync(OnMottattFiksArkivMelding);
                Logger.Information($"Startet subscription for {fiksIoClientConsumerService.Key} med kontoid {fiksIoClientConsumerService.Value.FiksIOClient.KontoId}");
            }

            stoppingToken.ThrowIfCancellationRequested();
            await Task.CompletedTask;
        }

        private bool AsiceIsVerified(asicManifest asicManifest)
        {
            return asicManifest != null && asicManifest.certificate != null && asicManifest.certificate.Length == 1 && asicManifest.file != null && asicManifest.rootfile == null;
        }

        private async Task OnMottattFiksArkivMelding(MottattMeldingArgs mottatt)
        {
            Logger.Information("Melding med meldingType {MeldingType} mottatt med meldingId {MeldingId},", mottatt.Melding.MeldingType, mottatt.Melding.MeldingId);

            if (FiksArkivMeldingtype.IsArkiveringType(mottatt.Melding.MeldingType))
            {
                await HandleArkiveringMelding(mottatt);
            }
            else if (FiksArkivMeldingtype.IsInnsynType(mottatt.Melding.MeldingType))
            {
                await HandleInnsynMelding(mottatt);
            }
            else
            { // Ukjent meldingstype
                Logger.Information("Ukjent meldingType {MeldingType} og meldingId {MeldingId} mottatt. Sender ugyldigforespørsel",
                    GetType().Name, mottatt.Melding.MeldingType, mottatt.Melding.MeldingId);
                await mottatt.SvarSender.AckAsync();
                var payloads = new List<IPayload>();
                payloads.Add(
                    new StringPayload(
                        JsonConvert.SerializeObject(FeilmeldingEngine.CreateUgyldigforespoerselMelding($"Ukjent meldingstype {mottatt.Melding.MeldingType} mottatt")),
                        "feilmelding.xml"));
                await mottatt.SvarSender.Svar(FiksArkivMeldingtype.Ugyldigforespørsel, payloads );
            }
        }

         private async Task HandleInnsynMelding(MottattMeldingArgs mottatt)
        {
            var payloads = new List<IPayload>();
            Melding melding;
            try
            {
                melding = mottatt.Melding.MeldingType switch
                {
                    FiksArkivMeldingtype.Sok => _sokManager.HandleMelding(mottatt),
                    FiksArkivMeldingtype.RegistreringHent => _registreringHentManager.HandleMelding(mottatt),
                    FiksArkivMeldingtype.MappeHent => _mappeHentManager.HandleMelding(mottatt),
                    _ => throw new ArgumentException("Case not handled")
                };
            }
            catch (Exception e)
            {
                melding = new Melding
                {
                    ResultatMelding =
                        FeilmeldingEngine.CreateUgyldigforespoerselMelding(
                            $"Klarte ikke håndtere innkommende melding. Feilmelding: {e.Message}"),
                    FileName = "feilmelding.xml",
                    MeldingsType = FiksArkivMeldingtype.Ugyldigforespørsel,
                };
            }
 
            payloads.Add(new StringPayload(ArkivmeldingSerializeHelper.Serialize(melding.ResultatMelding),
                    melding.FileName));

           await mottatt.SvarSender.AckAsync(); // Ack message to remove it from the queue

            var sendtMelding = await mottatt.SvarSender.Svar(melding.MeldingsType, payloads);
            Logger.Information("Svarmelding meldingId {MeldingId}, meldingType {MeldingType} sendt", sendtMelding.MeldingId,
                sendtMelding.MeldingType);
        }

        private async Task HandleArkiveringMelding(MottattMeldingArgs mottatt)
        {
            var meldinger = new List<Melding>();

            await mottatt.SvarSender.AckAsync(); // Ack message to remove it from the queue

            try
            {
                meldinger = mottatt.Melding.MeldingType switch
                {
                    FiksArkivMeldingtype.ArkivmeldingOpprett => _arkivmeldingManager.HandleMelding(mottatt),
                    FiksArkivMeldingtype.ArkivmeldingOppdater => _arkivmeldingOppdaterManager.HandleMelding(mottatt),
                    _ => throw new ArgumentException("Case not handled")
                };
            }
            catch (Exception e)
            {
                meldinger.Add(new Melding
                {
                    ResultatMelding = FeilmeldingEngine.CreateUgyldigforespoerselMelding($"{GetType().Name} - Klarte ikke håndtere innkommende melding. Feilmelding: {e.Message}"),
                    FileName = "feilmelding.xml",
                    MeldingsType = FiksArkivMeldingtype.Ugyldigforespørsel,
                });
            }
            

            foreach (var melding in meldinger)
            {
                var payloads = new List<IPayload>();
                if (melding.ResultatMelding != null)
                {
              
                    payloads.Add(new StringPayload(ArkivmeldingSerializeHelper.Serialize(melding.ResultatMelding), melding.FileName));
              
                }

                var sendtMelding = await mottatt.SvarSender.Svar(melding.MeldingsType, payloads);
                Logger.Information("Svarmelding meldingId {MeldingId}, meldingType {MeldingType} sendt",
                    sendtMelding.MeldingId,
                    sendtMelding.MeldingType);
                
            }
        }

        private async void OnMottattMelding(MottattMeldingArgs mottattMeldingArgs)
        {
            Logger.Information("Henter melding med MeldingId: {MeldingId}", mottattMeldingArgs.Melding.MeldingId);
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
                    Logger.Error("Klarte ikke hente payload og melding blir dermed ikke parset. MeldingId: {MeldingId}, Error: {Message}", mottattMeldingArgs.Melding?.MeldingId, e.Message);
                    payloadErrors += $"Klarte ikke hente payload og melding blir dermed ikke parset. MeldingId: {mottattMeldingArgs.Melding?.MeldingId}, Error: {e.Message}";
                }
            }

            Logger.Information(": Henter melding med MeldingId: {MeldingId}", mottattMeldingArgs.Melding.MeldingId);
            await mottattMeldingArgs.SvarSender?.AckAsync();
        }
    }
}
