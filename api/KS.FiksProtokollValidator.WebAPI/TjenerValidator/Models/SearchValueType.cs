namespace KS.FiksProtokollValidator.WebAPI.TjenerValidator.Models
{
    public enum SearchValueType
    {
        ValueEqual,
        Attribute,
        ValueNotEqual,
        ValueBetween,
        ValueLessThan,
        ValueLessThanOrEqual,
        ValueGreaterThan,
        ValueGreaterThanOrEqual,
        Regex,
        YearNow,
        DateNow,
    }
}