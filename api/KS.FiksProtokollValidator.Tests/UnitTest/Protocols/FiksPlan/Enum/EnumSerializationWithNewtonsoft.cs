#nullable enable
using KS.Fiks.Plan.Models.V2.felles.NasjonalarealplanidTyper;
using KS.Fiks.Plan.Models.V2.oppdatering.ArealplanOpprettKvitteringTyper;
using KS.FiksProtokollValidator.Tests.UnitTest.Protocols.FiksPlan.Enum.TestObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Xunit;
using Xunit.Abstractions;


namespace KS.FiksProtokollValidator.Tests.UnitTest.Protocols.FiksPlan.Enum
{
    public class EnumSerializationWithNewtonsoft
    {
        private readonly ITestOutputHelper _testOutputHelper;
        public EnumSerializationWithNewtonsoft(ITestOutputHelper testOutputHelper)
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
            _testOutputHelper.WriteLine($"JSON fra Kvittering uten StringEnumConverter: {jsonString1}");

            var jsonString2 =
                JsonConvert.SerializeObject(kvittering, new StringEnumConverter());
            _testOutputHelper.WriteLine($"JSON fra kvittering MED StringEnumConverter: {jsonString2}");
            
            var testObjekt = new TestObjekt()
            {
                testEnum = TestEnum.KNR
            };

            var jsonString3 = JsonConvert.SerializeObject(testObjekt);
            _testOutputHelper.WriteLine($"JSON fra testobjekt, med Enum som ikke har annotations, uten StringEnumConverter: {jsonString3}");
            
            var jsonString4 = JsonConvert.SerializeObject(testObjekt, new StringEnumConverter());
            _testOutputHelper.WriteLine($"JSON fra testobjekt, med Enum som ikke har annotations, MED StringEnumConverter: {jsonString4}");
            
            var testObjekt2 = new TestObjekt2()
            {
                testEnum = TestEnumMedEnumMember.KNR
            };
            
            var jsonString5 = JsonConvert.SerializeObject(testObjekt2);
            _testOutputHelper.WriteLine($"JSON fra testobjekt, med Enum som HAR annotations, uten StringEnumConverter: {jsonString5}");
            
            var jsonString6 = JsonConvert.SerializeObject(testObjekt2, new StringEnumConverter());
            _testOutputHelper.WriteLine($"JSON fra testobjekt, med Enum som HAR annotations, MED StringEnumConverter: {jsonString6}");
        }
    }
}