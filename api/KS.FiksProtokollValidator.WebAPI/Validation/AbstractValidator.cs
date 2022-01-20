using System;
using System.Collections.Generic;
using System.Linq;
using KS.Fiks.IO.Arkiv.Client.Models.Innsyn.Sok;

namespace KS.FiksProtokollValidator.WebAPI.Validation
{
    public abstract class AbstractValidator
    {
        protected static void ValidateBetweenDates(DateTime value, DateTime dateFrom, DateTime dateTo,
            ICollection<string> validationErrors)
        {
            if (value < dateFrom || value > dateTo)
            {
                validationErrors.Add($"Date {value} out of range from {dateFrom} to {dateTo}");   
            }
        }
    }
}