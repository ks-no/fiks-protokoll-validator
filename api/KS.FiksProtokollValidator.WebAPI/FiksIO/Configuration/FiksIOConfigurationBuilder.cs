using System;
using System.IO;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using KS.Fiks.IO.Client.Configuration;
using Ks.Fiks.Maskinporten.Client;
using RabbitMQ.Client;

namespace KS.FiksProtokollValidator.WebAPI.FiksIO.Configuration
{
    public static class FiksIOConfigurationBuilder
    {
        public static FiksIOConfiguration CreateFiksIOConfiguration(FiksProtokollKontoConfig config)
        {
            var ignoreSSLError = Environment.GetEnvironmentVariable("AMQP_IGNORE_SSL_ERROR");
            
            var accountConfiguration = new KontoConfiguration(
                kontoId: config.AccountId,
                privatNokkel: File.ReadAllText(config.PrivateKey)
            );

            // Id and password for integration associated to the Fiks IO account.
            var integrationConfiguration = new IntegrasjonConfiguration(
                integrasjonId: config.FiksIoIntegrationId,
                integrasjonPassord: config.FiksIoIntegrationPassword,
                scope: config.FiksIoIntegrationScope);

            // ID-porten machine to machine configuration
            var maskinportenClientConfiguration = new MaskinportenClientConfiguration(
                audience: config.MaskinPortenAudienceUrl,
                tokenEndpoint: config.MaskinPortenTokenUrl,
                issuer: config.MaskinPortenIssuer,
                numberOfSecondsLeftBeforeExpire: 10, // The token will be refreshed 10 seconds before it expires
                certificate: GetCertificate(config.MaskinPortenCompanyCertificateThumbprint, config.MaskinPortenCompanyCertificatePath, config.MaskinPortenCompanyCertificatePassword));

            // Optional: Use custom api host (i.e. for connecting to test api)
            var apiConfiguration = new ApiConfiguration(
                scheme: config.ApiScheme,
                host: config.ApiHost,
                port: config.ApiPort);


            var sslOption1 = (!string.IsNullOrEmpty(ignoreSSLError) && ignoreSSLError == "true")
                ? new SslOption()
                {
                    Enabled = true,
                    ServerName = config.AmqpHost,
                    CertificateValidationCallback =
                        (RemoteCertificateValidationCallback) ((sender, certificate, chain, errors) => true)
                }
                : null;
            
            // Optional: Use custom amqp host (i.e. for connection to test queue)
            var amqpConfiguration = new AmqpConfiguration(
                host: config.AmqpHost,
                port: config.AmqpPort, 
                sslOption1,
                "Fiks Protokollvalidator");

            var asiceSigningConfiguration = new AsiceSigningConfiguration(config.AsiceSigningPublicKey, config.AsiceSigningPrivateKey);

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