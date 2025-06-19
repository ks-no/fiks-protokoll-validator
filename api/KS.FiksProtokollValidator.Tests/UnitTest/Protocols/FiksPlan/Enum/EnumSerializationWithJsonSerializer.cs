#nullable enable
using System.Text.Json;
using KS.Fiks.Plan.Models.V2.felles.NasjonalarealplanidTyper;
using KS.Fiks.Plan.Models.V2.oppdatering.ArealplanOpprettKvitteringTyper;
using KS.FiksProtokollValidator.Tests.UnitTest.Protocols.FiksPlan.Enum.TestObjects;
// using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;


namespace KS.FiksProtokollValidator.Tests.UnitTest.Protocols.FiksPlan.Enum
{
    public class EnumSerializationWithJsonSerializer
    {
        private readonly ITestOutputHelper _testOutputHelper;
        public EnumSerializationWithJsonSerializer(ITestOutputHelper testOutputHelper)
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
            
            var jsonString1 = JsonSerializer.Serialize(kvittering);
            _testOutputHelper.WriteLine($"JSON from Fiks Plan kvittering: {jsonString1}");

            var testObjekt = new TestObjekt()
            {
                testEnum = TestEnum.KNR
            };

            var jsonString2 = JsonSerializer.Serialize(testObjekt);
            _testOutputHelper.WriteLine($"JSON from testobjekt: {jsonString2}");
        }
    }
}