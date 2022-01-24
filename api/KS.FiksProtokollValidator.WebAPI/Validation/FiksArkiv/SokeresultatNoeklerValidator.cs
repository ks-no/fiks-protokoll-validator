using System;
using System.Collections.Generic;
using System.Linq;
using KS.Fiks.IO.Arkiv.Client.Models.Innsyn.Sok;
using KS.FiksProtokollValidator.WebAPI.Validation.Resources;

namespace KS.FiksProtokollValidator.WebAPI.Validation.FiksArkiv
{
    public class SokeresultatNoeklerValidator : AbstractSokeResultatValidator
    {
        public static void Validate(SokeresultatNoekler sokResponse, Sok sok, List<string> validationErrors)
        {
            // Too many responses. Doesnt match Take request. 
            if (sokResponse.ResultatListe.Count > sok.Take)
            {
                validationErrors.Add(string.Format(
                    ValidationErrorMessages.TooLongResultListAccordingToTakeParameter, sok.Take,
                    sokResponse.ResultatListe.Count));
            }

            foreach (var parameter in sok.Parameter)
            {
                switch (parameter.Operator)
                {
                    case OperatorType.Equal:
                        if (parameter.Felt == SokFelt.MappePeriodTittel)
                        {
                            //TODO should we test this at all? 
                        }

                        break;
                    case OperatorType.Between:
                        if (isDateSokeFelt(parameter.Felt))
                        {
                            var listOfDates = GetDateResults(sokResponse, parameter.Felt, validationErrors);

                            foreach (var dateTimeValue in listOfDates)
                            {
                                ValidateBetweenDates(dateTimeValue, parameter.Parameterverdier.Datevalues[0],
                                    parameter.Parameterverdier.Datevalues[1], validationErrors);
                            }
                        }

                        break;
                }
            }
        }

        private static List<DateTime> GetDateResults(SokeresultatNoekler sokResponse, SokFelt parameterFelt,
            List<string> validationErrors)
        {
            switch (parameterFelt)
            {
                case SokFelt.SakPeriodSaksdato:
                    if (sokResponse.ResultatListe.All(r => r.Saksmappe == null))
                    {
                        validationErrors.Add(ValidationErrorMessages.CouldNotFindSaksmappe);
                        return new List<DateTime>();
                    }
                    return sokResponse.ResultatListe.Select(r => r.Saksmappe.Saksdato).ToList();
                default:
                    return new List<DateTime>();
            }
        }
    }
}