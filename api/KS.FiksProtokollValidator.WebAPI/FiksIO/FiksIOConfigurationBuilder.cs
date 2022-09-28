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
        public static FiksIOConfiguration CreateFiksIOConfiguration(AppSettings appSettings)
        {
            var ignoreSSLError = Environment.GetEnvironmentVariable("AMQP_IGNORE_SSL_ERROR");
            
            var accountConfiguration = new KontoConfiguration(
                kontoId: appSettings.FiksIOConfig.FiksIoAccountId,
                privatNokkel: File.ReadAllText(appSettings.FiksIOConfig.FiksIoPrivateKey)
            );

            // Id and password for integration associated to the Fiks IO account.
            var integrationConfiguration = new IntegrasjonConfiguration(
                integrasjonId: appSettings.FiksIOConfig.FiksIoIntegrationId,
                integrasjonPassord: appSettings.FiksIOConfig.FiksIoIntegrationPassword,
                scope: appSettings.FiksIOConfig.FiksIoIntegrationScope);

            // ID-porten machine to machine configuration
            var maskinportenClientConfiguration = new MaskinportenClientConfiguration(
                audience: appSettings.FiksIOConfig.MaskinPortenAudienceUrl,
                tokenEndpoint: appSettings.FiksIOConfig.MaskinPortenTokenUrl,
                issuer: appSettings.FiksIOConfig.MaskinPortenIssuer,
                numberOfSecondsLeftBeforeExpire: 10, // The token will be refreshed 10 seconds before it expires
                certificate: GetCertificate(appSettings.FiksIOConfig.MaskinPortenCompanyCertificateThumbprint, appSettings.FiksIOConfig.MaskinPortenCompanyCertificatePath, appSettings.FiksIOConfig.MaskinPortenCompanyCertificatePassword));

            // Optional: Use custom api host (i.e. for connecting to test api)
            var apiConfiguration = new ApiConfiguration(
                scheme: appSettings.FiksIOConfig.ApiScheme,
                host: appSettings.FiksIOConfig.ApiHost,
                port: appSettings.FiksIOConfig.ApiPort);


            var sslOption1 = (!string.IsNullOrEmpty(ignoreSSLError) && ignoreSSLError == "true")
                ? new SslOption()
                {
                    Enabled = true,
                    ServerName = appSettings.FiksIOConfig.AmqpHost,
                    CertificateValidationCallback =
                        (RemoteCertificateValidationCallback) ((sender, certificate, chain, errors) => true)
                }
                : null;
            
            // Optional: Use custom amqp host (i.e. for connection to test queue)
            AmqpConfiguration amqpConfiguration = new AmqpConfiguration(
                host: appSettings.FiksIOConfig.AmqpHost,
                port: appSettings.FiksIOConfig.AmqpPort, 
                sslOption1,
                "Fiks Protokollvalidator",
                keepAlive: true);

            // Combine all configurations
            return new FiksIOConfiguration(
                kontoConfiguration: accountConfiguration,
                integrasjonConfiguration: integrationConfiguration,
                maskinportenConfiguration: maskinportenClientConfiguration,
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