using System;
using System.Collections.Generic;
using System.Linq;
using KS.Fiks.IO.Arkiv.Client.Models.Innsyn.Sok;

namespace KS.FiksProtokollValidator.WebAPI.Validation.FiksArkiv
{
    public class SokeresultatMinimumValidator : AbstractSokeResultatValidator
    {
        public static void Validate(SokeresultatMinimum sokResponse, Sok sok, List<string> validationErrors)
        {
            // Too many responses. Doesnt match Take request. 
            if (sokResponse.ResultatListe.Count > sok.Take)
            {
                validationErrors.Add(
                    $"Too large ResultatListe. The 'take' parameter was {sok.Take} and the response had {sokResponse.ResultatListe.Count} items in ResultatListe");
            }

            foreach (var parameter in sok.Parameter)
            {
                switch (parameter.Operator)
                {
                    case OperatorType.Equal:
                        if (parameter.Felt == SokFelt.MappePeriodTittel)
                        {
                            foreach (var resultatMinimum in sokResponse.ResultatListe)
                            {
                                var searchText = parameter.Parameterverdier.Stringvalues.First();
                                var searchTextStripped = searchText.Replace("*", string.Empty);

                                if (!resultatMinimum.Mappe.Tittel.Contains(searchTextStripped))
                                {
                                    validationErrors.Add(
                                        $"The result for '{parameter.Felt}' with value '{resultatMinimum.Mappe.Tittel}' doesnt match the search text '{searchText}'?");
                                }
                            }
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

        private static List<DateTime> GetDateResults(SokeresultatMinimum sokResponse, SokFelt parameterFelt,
            List<string> validationErrors)
        {
            switch (parameterFelt)
            {
                case SokFelt.SakPeriodSaksdato:
                    if (sokResponse.ResultatListe.All(r => r.Saksmappe == null))
                    {
                        validationErrors.Add("Could not find any 'Saksmappe' in 'ResultatListe'. Is the result correct?");
                        return new List<DateTime>();
                    }
                    return sokResponse.ResultatListe.Select(r => r.Saksmappe.Saksdato).ToList();
                default:
                    return new List<DateTime>();
            }
        }
    }
}