using System.Runtime.Serialization;

namespace KS.FiksProtokollValidator.Tests.UnitTest.Protocols.FiksPlan.Enum.TestObjects;

public enum TestEnumMedEnumMember
{
        [EnumMember(Value = "kommunenummer")]KOMMUNENUMMER,
        [EnumMember(Value = "fylkesnummer")]FYLKESNUMMER,
        [EnumMember(Value = "landkode")]LANDKODE,
}