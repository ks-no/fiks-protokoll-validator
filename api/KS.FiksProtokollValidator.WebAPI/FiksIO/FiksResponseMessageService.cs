using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using Microsoft.Extensions.Logging;

namespace KS.FiksProtokollValidator.WebAPI.FiksIO
{
    public class FiksResponseMessageService : IHostedService
    {
        private IFiksIOClient _client;
        private ILogger<FiksResponseMessageService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public FiksResponseMessageService(ILogger<FiksResponseMessageService> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _client = new FiksIOClient(FiksIOConfigurationProvider.GetFromConfigurationFile());
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _client.NewSubscription(OnMottattMelding);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private async void OnMottattMelding(object sender, MottattMeldingArgs fileArgs)
        {
            var payloads = new Dictionary<string, string>();

            if (fileArgs.Melding.HasPayload)
            {
                try
                {
                    // Verify that message has payload
                    IAsicReader reader = new AsiceReader();
                    await using var inputStream = fileArgs.Melding.DecryptedStream.Result;
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
                    _logger.Log(LogLevel.Error ,"Klarte ikke hente payload og melding blir dermed ikke parset");
                    fileArgs.SvarSender?.Ack();
                    return;
                }
            }

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<FiksIOMessageDBContext>();
                var testSession = context.TestSessions.Include(t => t.FiksRequests).FirstOrDefaultAsync(t =>
                    t.FiksRequests.Any(r => r.MessageGuid.Equals(fileArgs.Melding.SvarPaMelding))).Result;

                // Er dette optimalt? Er det gjort slik fordi man ikke vet om databasen har rukket å skrive før man får svar?
                var timesTried = 1; 
                while (testSession == null && timesTried <= 5)
                {
                    Thread.Sleep(1000);
                    testSession = context.TestSessions.Include(t => t.FiksRequests).FirstOrDefault(t =>
                        t.FiksRequests.Any(r => r.MessageGuid.Equals(fileArgs.Melding.SvarPaMelding))); //Hvorfor er det ikke .Result her?
                    timesTried++;
                }


                if (testSession != null)
                {
                    var fiksRequest =
                        testSession.FiksRequests.Find(r => r.MessageGuid.Equals(fileArgs.Melding.SvarPaMelding));

                    var responseMessage = new FiksResponse
                    {
                        ReceivedAt = DateTime.Now,
                        Type = fileArgs.Melding.MeldingType,
                        Payload = payloads.Count > 1 ? string.Join(',', payloads.Keys) :
                            payloads.Count == 1 ? payloads.Keys.ElementAt(0) : null,
                        PayloadContent = payloads.Count == 1 ? payloads.Values.ElementAt(0) : null,
                    };

                    if (fiksRequest == null)
                    {
                        fileArgs.SvarSender?.Ack();
                        _logger.Log(LogLevel.Error,
                            "Klarte ikke å matche svar-melding fra FIKS med en eksisterende forespørsel. Svarmelding forkastes.");
                        return;
                    }

                    fiksRequest.FiksResponses ??= new List<FiksResponse>();

                    fiksRequest.FiksResponses.Add(responseMessage);

                    context.Entry(testSession).State = EntityState.Modified;
                    await context.SaveChangesAsync();
                }
                else
                {
                    _logger.Log(LogLevel.Error,
                        "Klarte ikke å matche svar-melding fra FIKS med en eksisterende testsesjon. Svarmelding forkastes.");
                }

                /*await using (var context = new FiksIOMessageDBContext(new DbContextOptions<FiksIOMessageDBContext>()))
                {
                    var testSession = context.TestSessions.Include(t => t.FiksRequests).FirstOrDefaultAsync(t =>
                        t.FiksRequests.Any(r => r.MessageGuid.Equals(fileArgs.Melding.SvarPaMelding))).Result;

                    var timesTried = 1;
                    while (testSession == null && timesTried <= 5)
                    {
                        Thread.Sleep(1000);
                        testSession = context.TestSessions.Include(t => t.FiksRequests).FirstOrDefault(t =>
                            t.FiksRequests.Any(r => r.MessageGuid.Equals(fileArgs.Melding.SvarPaMelding)));
                        timesTried++;
                    }


                    if (testSession != null)
                    {
                        var fiksRequest =
                            testSession.FiksRequests.Find(r => r.MessageGuid.Equals(fileArgs.Melding.SvarPaMelding));

                        var responseMessage = new FiksResponse
                        {
                            ReceivedAt = DateTime.Now,
                            Type = fileArgs.Melding.MeldingType,
                            Payload = payloads.Count > 1 ? string.Join(',', payloads.Keys) :
                                payloads.Count == 1 ? payloads.Keys.ElementAt(0) : null,
                            PayloadContent = payloads.Count == 1 ? payloads.Values.ElementAt(0) : null,
                        };

                        if (fiksRequest == null)
                        {
                            fileArgs.SvarSender?.Ack();
                            _logger.Log(LogLevel.Error,
                                "Klarte ikke å matche svar-melding fra FIKS med en eksisterende forespørsel. Svarmelding forkastes.");
                            return;
                        }

                        fiksRequest.FiksResponses ??= new List<FiksResponse>();

                        fiksRequest.FiksResponses.Add(responseMessage);

                        context.Entry(testSession).State = EntityState.Modified;
                        await context.SaveChangesAsync();
                    }
                    else
                    {
                        _logger.Log(LogLevel.Error,
                            "Klarte ikke å matche svar-melding fra FIKS med en eksisterende testsesjon. Svarmelding forkastes.");
                    }
                }*/
            }
            finally
            {
                fileArgs.SvarSender?.Ack();
            }
        }
    }
}
