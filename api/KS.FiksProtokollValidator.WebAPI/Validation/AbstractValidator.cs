using System;
using System.Collections.Generic;
using System.Linq;
using KS.Fiks.IO.Arkiv.Client.Models.Innsyn.Sok;

namespace KS.FiksProtokollValidator.WebAPI.Validation
{
    public abstract class AbstractValidator
    {
        protected static List<DateTime> GetDateResults(SokeresultatMinimum sokResponse, SokFelt parameterFelt)
        {
            switch (parameterFelt)
            {
                case SokFelt.SakPeriodSaksdato:
                    return sokResponse.ResultatListe.Select(r => r.Saksmappe.Saksdato).ToList();
                
                default:
                    return new List<DateTime>();
            }
        }

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