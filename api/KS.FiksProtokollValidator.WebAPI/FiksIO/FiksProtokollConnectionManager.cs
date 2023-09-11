using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Serilog;

namespace KS.FiksProtokollValidator.WebAPI.FiksIO;

public class FiksProtokollConnectionManager
{
    private static readonly ILogger Logger = Log.ForContext(MethodBase.GetCurrentMethod()?.DeclaringType);
    private readonly AppSettings _appSettings;
    public Dictionary<string, FiksProtokollConnectionService> TjenerConnectionServices { get; }
    public Dictionary<string, FiksProtokollConnectionService> KlientConnectionServices { get; }


    public FiksProtokollConnectionManager(AppSettings appAppSettings)
    {
        _appSettings = appAppSettings;
        TjenerConnectionServices = new Dictionary<string, FiksProtokollConnectionService>();
        KlientConnectionServices = new Dictionary<string, FiksProtokollConnectionService>();

        var protocolAccounts = JsonConvert.DeserializeObject<ProtocolAccountConfigurations>(_appSettings.TjenerValidatorFiksIOConfig.ProtocolAccountConfigs);
        foreach (var protokollKontoConfig in protocolAccounts.ProtocolAccounts)
        {
             var service = new FiksProtokollConnectionService(MapToSettings(_appSettings.TjenerValidatorFiksIOConfig, protokollKontoConfig));
             TjenerConnectionServices.Add(protokollKontoConfig.Protocol, service);
        }
        
        protocolAccounts = JsonConvert.DeserializeObject<ProtocolAccountConfigurations>(_appSettings.KlientValidatorFiksIOConfig.ProtocolAccountConfigs);
        foreach (var protokollKontoConfig in protocolAccounts.ProtocolAccounts)
        {
            var service = new FiksProtokollConnectionService(MapToSettings(_appSettings.KlientValidatorFiksIOConfig, protokollKontoConfig));
            KlientConnectionServices.Add(protokollKontoConfig.Protocol, service);
        }
    }

    public bool IsHealthy()
    {
        foreach (var fiksIoClientConsumerService in TjenerConnectionServices)
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
        foreach (var fiksProtokollConnectionService in TjenerConnectionServices.Where(fiksProtokollConnectionService => !fiksProtokollConnectionService.Value.IsHealthy()))
        {
            Log.Information($"FiksProtokollConnectionManager reconnect unhealthy {fiksProtokollConnectionService.Key}");
            await fiksProtokollConnectionService.Value.Reconnect();
        }
    }

    private FiksProtokollConsumerServiceSettings MapToSettings(ValidatorFiksIOConfig validatorFiksIoConfig,
        ProtocolAccount protocolAccount)
    {
        return new FiksProtokollConsumerServiceSettings
        {
            AccountId = protocolAccount.AccountId,
            AmqpHost = validatorFiksIoConfig.AmqpHost,
            AmqpPort = validatorFiksIoConfig.AmqpPort,
            ApiHost = validatorFiksIoConfig.ApiHost,
            ApiPort = validatorFiksIoConfig.ApiPort,
            ApiScheme = validatorFiksIoConfig.ApiScheme,
            AsiceSigningPrivateKey = validatorFiksIoConfig.AsiceSigningPrivateKey,
            AsiceSigningPublicKey = validatorFiksIoConfig.AsiceSigningPublicKey,
            FiksIoIntegrationId = validatorFiksIoConfig.FiksIoIntegrationId,
            FiksIoIntegrationPassword = validatorFiksIoConfig.FiksIoIntegrationPassword,
            FiksIoIntegrationScope = validatorFiksIoConfig.FiksIoIntegrationScope,
            MaskinPortenCompanyCertificatePassword = validatorFiksIoConfig.MaskinPortenCompanyCertificatePassword,
            MaskinPortenCompanyCertificatePath = validatorFiksIoConfig.MaskinPortenCompanyCertificatePath,
            MaskinPortenCompanyCertificateThumbprint =
                validatorFiksIoConfig.MaskinPortenCompanyCertificateThumbprint,
            MaskinPortenIssuer = validatorFiksIoConfig.MaskinPortenIssuer,
            MaskinPortenAudienceUrl = validatorFiksIoConfig.MaskinPortenAudienceUrl,
            MaskinPortenTokenUrl = validatorFiksIoConfig.MaskinPortenTokenUrl,
            PrivateKey = protocolAccount.PrivateKey,
        };
    }

    public void Dispose()
    {
        
    }
}