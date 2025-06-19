#nullable enable
using KS.Fiks.Plan.Models.V2.felles.NasjonalarealplanidTyper;
using KS.Fiks.Plan.Models.V2.oppdatering.ArealplanOpprettKvitteringTyper;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Xunit;
using Xunit.Abstractions;

namespace KS.FiksProtokollValidator.Tests.UnitTest.Protocols.FiksPlan.Enum
{
    public class EnumTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        public EnumTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;

        }

        [Fact]
        public void Enum_Serialization()
        {
            var kvittering = new OpprettArealplanKvittering
            {
                NasjonalArealplanId = new NasjonalArealplanId()
                {
                    AdministrativEnhet = new AdministrativEnhet()
                    {
                        Type = AdministrativEnhetType.Kommunenummer
                    },
                    Planidentifikasjon = "id"
                }
            };
            var jsonString1 = JsonConvert.SerializeObject(kvittering);
            _testOutputHelper.WriteLine($"JSON1: {jsonString1}");

            var jsonString2 =
                JsonConvert.SerializeObject(kvittering, new StringEnumConverter());
            _testOutputHelper.WriteLine($"JSON2: {jsonString2}");

        }
    }
}