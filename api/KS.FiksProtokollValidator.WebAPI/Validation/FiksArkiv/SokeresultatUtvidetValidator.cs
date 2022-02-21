using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using KS.Fiks.IO.Arkiv.Client.Models.Innsyn.Sok;
using KS.FiksProtokollValidator.WebAPI.Validation.Resources;

namespace KS.FiksProtokollValidator.WebAPI.Validation.FiksArkiv
{
    public class SokeresultatUtvidetValidator : AbstractSokeResultatValidator
    {
        public static void Validate(TextReader sokResponseTextReader, Sok sok, List<string> validationErrors)
        {
            Sokeresultat sokResponse = null;
            try
            {
                 sokResponse = (Sokeresultat)new XmlSerializer(typeof(Sokeresultat)).Deserialize(
                    sokResponseTextReader);
            }
            catch (Exception e)
            {
                validationErrors.Add(string.Format(ValidationErrorMessages.CouldNotParseSokeresultat, "utvidet"));
                return;
            }

            if (sokResponse == null)
            {
                validationErrors.Add(ValidationErrorMessages.SokeresultatIsNull);
                return;
            }
            
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
                            ValidateMappePeriodTittelEqual(sok, sokResponse, validationErrors, parameter);
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

        private static void ValidateMappePeriodTittelEqual(Sok sok, Sokeresultat sokResponse, List<string> validationErrors, Parameter parameter)
        {
            var notFoundExpectedRespons = false;
            foreach (var resultat in sokResponse.ResultatListe)
            {
                var searchText = parameter.Parameterverdier.Stringvalues.First();
                var searchTextStripped = searchText.Replace("*", string.Empty).ToLower();
                if (sok.Respons == Respons.Mappe)
                {
                    if (resultat.Mappe == null && !notFoundExpectedRespons)
                    {
                        validationErrors.Add(string.Format(ValidationErrorMessages.FoundUnexpectedResultTypeAccordingToRespons, sok.Respons.ToString()));
                        notFoundExpectedRespons = true; //Only show this validation message once. Else it will overflow the list.
                    }
                    if (resultat.Mappe != null && !resultat.Mappe.Tittel.ToLower().Contains(searchTextStripped))
                    {
                        validationErrors.Add(string.Format(ValidationErrorMessages.ResultDontMatchSearchText,
                            parameter.Felt,
                            resultat.Mappe.Tittel, searchText));
                    }
                } else if (sok.Respons == Respons.Saksmappe)
                {
                    if (resultat.Saksmappe == null && !notFoundExpectedRespons)
                    {
                        validationErrors.Add(string.Format(ValidationErrorMessages.FoundUnexpectedResultTypeAccordingToRespons, sok.Respons.ToString())); 
                        notFoundExpectedRespons = true; //Only show this validation message once. Else it will overflow the list.
                    }
                    if (resultat.Saksmappe != null && !resultat.Saksmappe.Tittel.ToLower().Contains(searchTextStripped))
                    {
                        validationErrors.Add(string.Format(ValidationErrorMessages.ResultDontMatchSearchText,
                            parameter.Felt, resultat.Mappe.Tittel, searchText));
                    }
                }
            }
        }

        private static List<DateTime> GetDateResults(Sokeresultat sokResponse, SokFelt parameterFelt,
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