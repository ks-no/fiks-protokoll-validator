using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Threading.Tasks;
using KS.Fiks.IO.Client;
using Serilog;

namespace KS.FiksProtokollValidator.WebAPI.FiksIO;

public class FiksProtokollConnectionManager
{
    private static readonly ILogger Logger = Log.ForContext(MethodBase.GetCurrentMethod()?.DeclaringType);
    private readonly AppSettings _appSettings;
    public Dictionary<string, FiksProtokollConnectionService> FiksProtokollConnectionServices { get; }


    public FiksProtokollConnectionManager(AppSettings appAppSettings)
    {
        _appSettings = appAppSettings;
        FiksProtokollConnectionServices = new Dictionary<string, FiksProtokollConnectionService>();
        foreach (var protokollKontoConfig in _appSettings.FiksIOConfig.ProtocolAccounts)
        {
            var service = new FiksProtokollConnectionService(MapToSettings(_appSettings, protokollKontoConfig));
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
            Logger.Error("FiksIOClientConsumerService: FiksIOClient is null. Returning not healthy.");
            return false;
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
                fiksProtokollConnectionService.Value.Reconnect();
            }
        }
    }

    private FiksProtokollConsumerServiceSettings MapToSettings(AppSettings appSettings,
        ProtokollKonto protokollKonto)
    {
        return new FiksProtokollConsumerServiceSettings
        {
            AccountId = protokollKonto.AccountId,
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
            PrivateKey = protokollKonto.PrivateKey,
        };
    }

    public void Dispose()
    {
        
    }
}