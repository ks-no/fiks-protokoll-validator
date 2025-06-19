using System.Runtime.Serialization;

namespace KS.FiksProtokollValidator.Tests.UnitTest.Protocols.FiksPlan.Enum.TestObjects;

[DataContract]
public enum TestEnumMedEnumMember
{
        [EnumMember(Value = "tjohei")]KNR,
        [EnumMember(Value = "tjobing")]FYNR,
        [EnumMember(Value = "blabla")]LNKD,
}