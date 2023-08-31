using System;

namespace KS.FiksProtokollValidator.WebAPI
{
    public class AppSettings
    {
        public FiksIOConfig FiksIOConfig { get; set; }
    }

    public class FiksIOConfig
    {
        public string ApiHost { get; set; } 
        public int ApiPort { get; set; }
        public string ApiScheme { get; set; }
        public string AmqpHost { get; set; }
        public int AmqpPort { get; set; }
        public string ProtocolAccountConfigs { get; set; }
        public Guid FiksIoIntegrationId { get; set; }
        public string FiksIoIntegrationPassword { get; set; }
        public string FiksIoIntegrationScope { get; set; }
        public string MaskinPortenAudienceUrl { get; set; }
        public string MaskinPortenCompanyCertificateThumbprint { get; set; }
        public string MaskinPortenCompanyCertificatePath { get; set; }
        public string MaskinPortenCompanyCertificatePassword { get; set; }
        public string MaskinPortenIssuer { get; set; }
        public string MaskinPortenTokenUrl { get; set; }
        public string AsiceSigningPrivateKey { get; set; }
        public string AsiceSigningPublicKey { get; set; }
    }

    public class ProtocolAccountConfigurations
    {
        public ProtocolAccount[] ProtocolAccounts { get; set; }
    }

    public class ProtocolAccount
    {
        public string Protocol { get; set; }
        public Guid AccountId { get; set; }
        public string PrivateKey { get; set; }
    }
}