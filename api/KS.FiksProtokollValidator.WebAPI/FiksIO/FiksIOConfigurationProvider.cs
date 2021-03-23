using System;
using System.IO;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using KS.Fiks.IO.Client.Configuration;
using Ks.Fiks.Maskinporten.Client;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;

namespace KS.FiksProtokollValidator.WebAPI.FiksIO
{
    public static class FiksIOConfigurationProvider
    {
        private static readonly string FiksIOConfigFile =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FiksIO", "fiks-io-config.json");

        public static FiksIOConfiguration GetFromConfigurationFile()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            
            var config = JObject(FiksIOConfigFile);

            var accountConfiguration = new KontoConfiguration(
                kontoId: (Guid) config["FiksIoAccountId"],
                privatNokkel: File.ReadAllText((string) config["FiksIoPrivateKey"])
            );

            // Id and password for integration associated to the Fiks IO account.
            var integrationConfiguration = new IntegrasjonConfiguration(
                integrasjonId: (Guid) config["FiksIoIntegrationId"],
                integrasjonPassord: (string) config["FiksIoIntegrationPassword"],
                scope: (string) config["FiksIoIntegrationScope"]);

            // ID-porten machine to machine configuration
            var maskinportenClientConfiguration = new MaskinportenClientConfiguration(
                audience: (string) config["MaskinPortenAudienceUrl"],
                tokenEndpoint: (string) config["MaskinPortenTokenUrl"],
                issuer: (string) config["MaskinPortenIssuer"],
                numberOfSecondsLeftBeforeExpire: 10, // The token will be refreshed 10 seconds before it expires
                certificate: GetCertificate((string) config["MaskinPortenCompanyCertificateThumbprint"], (string) config["MaskinPortenCompanyCertificatePath"]));

            // Optional: Use custom api host (i.e. for connecting to test api)
            var apiConfiguration = new ApiConfiguration(
                scheme: (string) config["ApiScheme"],
                host: (string) config["ApiHost"],
                port: (int) config["ApiPort"]);


            var sslOption1 = (!string.IsNullOrEmpty(environment) && environment == "Development")
                ? new SslOption()
                {
                    Enabled = true,
                    ServerName = (string) config["AmqpHost"],
                    CertificateValidationCallback =
                        (RemoteCertificateValidationCallback) ((sender, certificate, chain, errors) => true)
                }
                : null;
            
            // Optional: Use custom amqp host (i.e. for connection to test queue)
            AmqpConfiguration amqpConfiguration = new AmqpConfiguration(
                host: (string) config["AmqpHost"],
                port: (int) config["AmqpPort"], sslOption1);

            // Combine all configurations
            return new FiksIOConfiguration(
                kontoConfiguration: accountConfiguration,
                integrasjonConfiguration: integrationConfiguration,
                maskinportenConfiguration: maskinportenClientConfiguration,
                apiConfiguration: apiConfiguration,
                amqpConfiguration: amqpConfiguration);
        }

        private static JObject JObject(string fiksIOConfigFile)
        {
            var configJson = File.ReadAllText(fiksIOConfigFile);

            var config = Newtonsoft.Json.Linq.JObject.Parse(configJson);
            return config;
        }

        private static X509Certificate2 GetCertificate(string thumbprint, string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                return new X509Certificate2(File.ReadAllBytes(path), "KS_PASSWORD");
            }
           
            var store = new X509Store(StoreLocation.CurrentUser);

            store.Open(OpenFlags.ReadOnly);

            var certificates = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);

            store.Close();

            return certificates.Count > 0 ? certificates[0] : null;
        }

        public static Guid
            GetFiksIoRecipientAccountId() // TODO: Get FiksIoRecipientAccountId from request, not from config.
        {
            return (Guid) JObject(FiksIOConfigFile)["FiksIoRecipientAccountId"];
        }
    }
}