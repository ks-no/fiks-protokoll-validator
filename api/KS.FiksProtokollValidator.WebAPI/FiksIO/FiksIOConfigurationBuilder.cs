using System;
using System.IO;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using KS.Fiks.IO.Client.Configuration;
using Ks.Fiks.Maskinporten.Client;
using RabbitMQ.Client;

namespace KS.FiksProtokollValidator.WebAPI.FiksIO
{
    public static class FiksIOConfigurationBuilder
    {
        public static FiksIOConfiguration CreateFiksIOConfiguration(FiksProtokollConsumerServiceSettings settings)
        {
            var ignoreSSLError = Environment.GetEnvironmentVariable("AMQP_IGNORE_SSL_ERROR");
            
            var accountConfiguration = new KontoConfiguration(
                kontoId: settings.AccountId,
                privatNokkel: File.ReadAllText(settings.PrivateKey)
            );

            // Id and password for integration associated to the Fiks IO account.
            var integrationConfiguration = new IntegrasjonConfiguration(
                integrasjonId: settings.FiksIoIntegrationId,
                integrasjonPassord: settings.FiksIoIntegrationPassword,
                scope: settings.FiksIoIntegrationScope);

            // ID-porten machine to machine configuration
            var maskinportenClientConfiguration = new MaskinportenClientConfiguration(
                audience: settings.MaskinPortenAudienceUrl,
                tokenEndpoint: settings.MaskinPortenTokenUrl,
                issuer: settings.MaskinPortenIssuer,
                numberOfSecondsLeftBeforeExpire: 10, // The token will be refreshed 10 seconds before it expires
                certificate: GetCertificate(settings.MaskinPortenCompanyCertificateThumbprint, settings.MaskinPortenCompanyCertificatePath, settings.MaskinPortenCompanyCertificatePassword));

            // Optional: Use custom api host (i.e. for connecting to test api)
            var apiConfiguration = new ApiConfiguration(
                scheme: settings.ApiScheme,
                host: settings.ApiHost,
                port: settings.ApiPort);


            var sslOption1 = (!string.IsNullOrEmpty(ignoreSSLError) && ignoreSSLError == "true")
                ? new SslOption()
                {
                    Enabled = true,
                    ServerName = settings.AmqpHost,
                    CertificateValidationCallback =
                        (RemoteCertificateValidationCallback) ((sender, certificate, chain, errors) => true)
                }
                : null;
            
            // Optional: Use custom amqp host (i.e. for connection to test queue)
            var amqpConfiguration = new AmqpConfiguration(
                host: settings.AmqpHost,
                port: settings.AmqpPort, 
                sslOption1,
                "Fiks Protokollvalidator");

            var asiceSigningConfiguration = new AsiceSigningConfiguration(settings.AsiceSigningPublicKey, settings.AsiceSigningPrivateKey);

            // Combine all configurations
            return new FiksIOConfiguration(
                kontoConfiguration: accountConfiguration,
                integrasjonConfiguration: integrationConfiguration,
                maskinportenConfiguration: maskinportenClientConfiguration,
                asiceSigningConfiguration: asiceSigningConfiguration,
                apiConfiguration: apiConfiguration,
                amqpConfiguration: amqpConfiguration);
        }
        
        private static X509Certificate2 GetCertificate(string thumbprint, string path, string password)
        {
            if (!string.IsNullOrEmpty(path))
            {
                return new X509Certificate2(File.ReadAllBytes(path), password);
            }
           
            var store = new X509Store(StoreLocation.CurrentUser);

            store.Open(OpenFlags.ReadOnly);

            var certificates = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);

            store.Close();

            return certificates.Count > 0 ? certificates[0] : null;
        }
    }
}