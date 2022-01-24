using System;
using System.Collections.Generic;

namespace KS.FiksProtokollValidator.WebAPI.Validation
{
    public abstract class AbstractValidator
    {
        protected static void ValidateBetweenDates(DateTime value, DateTime dateFrom, DateTime dateTo,
            ICollection<string> validationErrors)
        {
            if (value < dateFrom || value > dateTo)
            {
                validationErrors.Add($"Dato {value} treffer ikke søket mellom 'fra dato' {dateFrom} og 'til dato' {dateTo}");   
            }
        }
    }
}