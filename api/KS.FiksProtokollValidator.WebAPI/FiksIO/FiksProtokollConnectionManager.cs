using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog;
using ILogger = Serilog.ILogger;

namespace KS.FiksProtokollValidator.WebAPI.FiksIO;

public class FiksProtokollConnectionManager
{
    private static readonly ILogger Logger = Log.ForContext(MethodBase.GetCurrentMethod()?.DeclaringType);
    public Dictionary<string, FiksProtokollConnectionService> FiksProtokollConnectionServices { get; }


    public FiksProtokollConnectionManager(AppSettings appAppSettings, ILoggerFactory loggerFactory)
    {
        FiksProtokollConnectionServices = new Dictionary<string, FiksProtokollConnectionService>();

        var protocolAccounts = JsonConvert.DeserializeObject<ProtocolAccountConfigurations>(appAppSettings.FiksIOConfig.ProtocolAccountConfigs);
        foreach (var protokollKontoConfig in protocolAccounts.ProtocolAccounts)
        {
             var service = new FiksProtokollConnectionService(MapToSettings(appAppSettings, protokollKontoConfig), loggerFactory);
             FiksProtokollConnectionServices.Add(protokollKontoConfig.Protocol, service);
        }
    }

    public bool IsHealthy()
    {
        foreach (var fiksIoClientConsumerService in FiksProtokollConnectionServices)
        {
            if (fiksIoClientConsumerService.Value != null)
            {
                var isHealthy = fiksIoClientConsumerService.Value.IsHealthy();
                if (!isHealthy)
                {
                    return false;
                }
            }
            else
            {
                Logger.Error("FiksIOClientConsumerService: FiksIOClient is null. Returning not healthy.");
                return false;
            }
        }
        return true;
    }

    public async Task Reconnect()
    {
        foreach (var fiksProtokollConnectionService in FiksProtokollConnectionServices)
        {
            if (!fiksProtokollConnectionService.Value.IsHealthy())
            {
                Log.Information($"FiksProtokollConnectionManager reconnect unhealthy {fiksProtokollConnectionService.Key}");
                await fiksProtokollConnectionService.Value.Reconnect();
            }
        }
    }

    private FiksProtokollConsumerServiceSettings MapToSettings(AppSettings appSettings,
        ProtocolAccount protocolAccount)
    {
        return new FiksProtokollConsumerServiceSettings
        {
            AccountId = protocolAccount.AccountId,
            AmqpHost = appSettings.FiksIOConfig.AmqpHost,
            AmqpPort = appSettings.FiksIOConfig.AmqpPort,
            ApiHost = appSettings.FiksIOConfig.ApiHost,
            ApiPort = appSettings.FiksIOConfig.ApiPort,
            ApiScheme = appSettings.FiksIOConfig.ApiScheme,
            AsiceSigningPrivateKey = appSettings.FiksIOConfig.AsiceSigningPrivateKey,
            AsiceSigningPublicKey = appSettings.FiksIOConfig.AsiceSigningPublicKey,
            FiksIoIntegrationId = appSettings.FiksIOConfig.FiksIoIntegrationId,
            FiksIoIntegrationPassword = appSettings.FiksIOConfig.FiksIoIntegrationPassword,
            FiksIoIntegrationScope = appSettings.FiksIOConfig.FiksIoIntegrationScope,
            MaskinPortenCompanyCertificatePassword = appSettings.FiksIOConfig.MaskinPortenCompanyCertificatePassword,
            MaskinPortenCompanyCertificatePath = appSettings.FiksIOConfig.MaskinPortenCompanyCertificatePath,
            MaskinPortenCompanyCertificateThumbprint =
                appSettings.FiksIOConfig.MaskinPortenCompanyCertificateThumbprint,
            MaskinPortenIssuer = appSettings.FiksIOConfig.MaskinPortenIssuer,
            MaskinPortenAudienceUrl = appSettings.FiksIOConfig.MaskinPortenAudienceUrl,
            MaskinPortenTokenUrl = appSettings.FiksIOConfig.MaskinPortenTokenUrl,
            PrivateKey = protocolAccount.PrivateKey,
        };
    }

    public void Dispose()
    {
        
    }
}