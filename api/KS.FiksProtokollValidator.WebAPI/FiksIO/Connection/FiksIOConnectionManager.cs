using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using KS.FiksProtokollValidator.WebAPI.FiksIO.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog;
using ILogger = Serilog.ILogger;

namespace KS.FiksProtokollValidator.WebAPI.FiksIO.Connection;

public class FiksIOConnectionManager
{
    private static readonly ILogger Logger = Log.ForContext(MethodBase.GetCurrentMethod()?.DeclaringType);
    private static ILoggerFactory _loggerFactory;
    private readonly AppSettings _appSettings;
    public Dictionary<string, FiksIOConnectionService> TjenerConnectionServices { get; }
    public Dictionary<string, FiksIOConnectionService> KlientConnectionServices { get; }


    public FiksIOConnectionManager(AppSettings appAppSettings, ILoggerFactory loggerFactory)
    {
        _appSettings = appAppSettings;
        _loggerFactory = loggerFactory;
        TjenerConnectionServices = new Dictionary<string, FiksIOConnectionService>();
        KlientConnectionServices = new Dictionary<string, FiksIOConnectionService>();

        var protocolAccounts = JsonConvert.DeserializeObject<ProtocolAccountConfigurations>(_appSettings.TjenerValidatorFiksIOConfig.ProtocolAccountConfigs);
        foreach (var protokollKontoConfig in protocolAccounts.ProtocolAccounts)
        {
             var service = new FiksIOConnectionService(MapToSettings(_appSettings.TjenerValidatorFiksIOConfig, protokollKontoConfig), _loggerFactory);
             TjenerConnectionServices.Add(protokollKontoConfig.Protocol, service);
        }
        
        protocolAccounts = JsonConvert.DeserializeObject<ProtocolAccountConfigurations>(_appSettings.KlientValidatorFiksIOConfig.ProtocolAccountConfigs);
        foreach (var protokollKontoConfig in protocolAccounts.ProtocolAccounts)
        {
            var service = new FiksIOConnectionService(MapToSettings(_appSettings.KlientValidatorFiksIOConfig, protokollKontoConfig), _loggerFactory);
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
                Logger.Error("FiksIOClient is null. Returning not healthy.");
                return false;
            }
        }
        return true;
    }

    public async Task Reconnect()
    {
        foreach (var fiksProtokollConnectionService in TjenerConnectionServices.Where(fiksProtokollConnectionService => !fiksProtokollConnectionService.Value.IsHealthy()))
        {
            Log.Information($"Reconnect unhealthy {fiksProtokollConnectionService.Key}");
            await fiksProtokollConnectionService.Value.Reconnect();
        }
    }

    private FiksProtokollKontoConfig MapToSettings(ValidatorFiksIOConfig validatorFiksIoConfig,
        ProtocolAccount protocolAccount)
    {
        return new FiksProtokollKontoConfig
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