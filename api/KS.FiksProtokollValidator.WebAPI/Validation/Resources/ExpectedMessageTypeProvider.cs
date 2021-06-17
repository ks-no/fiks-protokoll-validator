using System.Collections.Generic;
using KS.FiksProtokollValidator.WebAPI.Models;

namespace KS.FiksProtokollValidator.WebAPI.Validation.Resources
{
    public static class ExpectedResponseMessageTypeProvider
    {
        private static Dictionary<string, List<string>> _expectedMessageTypes;

        private static Dictionary<string, List<string>> InitializeExpectedMessageTypes()
        {
            return new Dictionary<string, List<string>>
            {
                {
                    WebAPI.Resources.RequestMessageTypes.ArkivmeldingV1,
                    new List<string>
                    {
                        WebAPI.Resources.ResponseMessageTypes.MottattV1,
                        WebAPI.Resources.ResponseMessageTypes.KvitteringV1
                    }
                },
                {
                    WebAPI.Resources.RequestMessageTypes.OppdaterSaksmappeV1,
                    new List<string>
                    {
                        WebAPI.Resources.ResponseMessageTypes.MottattV1,
                        WebAPI.Resources.ResponseMessageTypes.KvitteringV1
                    }
                },
                {
                    WebAPI.Resources.RequestMessageTypes.SoekV1,
                    new List<string>
                    {
                        WebAPI.Resources.ResponseMessageTypes.InnsynSoekResultatV1
                    }
                },
                {
                    WebAPI.Resources.RequestMessageTypes.HentMoteplanV1,
                    new List<string>
                    {
                        WebAPI.Resources.ResponseMessageTypes.HentMoteplanResultatV1
                    }
                },
                {
                    WebAPI.Resources.RequestMessageTypes.HentUtvalgV1,
                    new List<string>
                    {
                        WebAPI.Resources.ResponseMessageTypes.HentUtvalgResultatV1
                    }
                },
                {
                    WebAPI.Resources.RequestMessageTypes.SendUtvalgV1,
                    new List<string>
                    {
                        WebAPI.Resources.ResponseMessageTypes.MottattPolitiskBehandlingV1
                    }
                },
                {
                    WebAPI.Resources.RequestMessageTypes.SendOrienteringssakV1,
                    new List<string>
                    {
                        WebAPI.Resources.ResponseMessageTypes.MottattPolitiskBehandlingV1
                    }
                },
                {
                    WebAPI.Resources.RequestMessageTypes.SendDelegertVedtakV1,
                    new List<string>
                    {
                        WebAPI.Resources.ResponseMessageTypes.MottattPolitiskBehandlingV1
                    }
                },
                {
                    WebAPI.Resources.RequestMessageTypes.SendVedtakFraUtvalgV1,
                    new List<string>
                    {
                        WebAPI.Resources.ResponseMessageTypes.MottattPolitiskBehandlingV1
                    }
                }
            };
        }

        public static List<string> GetExpectedResponseMessageTypesAsStrings(string requestMessageType,
            List<FiksExpectedResponseMessageType> testCaseExpectedResponseMessageTypes)
        {
            if (testCaseExpectedResponseMessageTypes.Count == 0)
            {
                _expectedMessageTypes ??= InitializeExpectedMessageTypes();

                return _expectedMessageTypes[requestMessageType];
            }

            var responseMessageTypesList = new List<string>();
            foreach (var responseMessageType in testCaseExpectedResponseMessageTypes)
            {
                responseMessageTypesList.Add(responseMessageType.ExpectedResponseMessageType);
            }

            return responseMessageTypesList;
        }
    }
}
