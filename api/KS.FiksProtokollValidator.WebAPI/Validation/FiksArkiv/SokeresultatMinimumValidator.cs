using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using KS.Fiks.IO.Arkiv.Client.Models.Innsyn.Sok;
using KS.FiksProtokollValidator.WebAPI.Validation.Resources;

namespace KS.FiksProtokollValidator.WebAPI.Validation.FiksArkiv
{
    public class SokeresultatMinimumValidator : AbstractSokeResultatValidator
    {
        public static void Validate(TextReader sokResponseTextReader, Sok sok, List<string> validationErrors)
        {
            SokeresultatMinimum sokResponse = null;
            try
            {
                sokResponse = (SokeresultatMinimum)new XmlSerializer(typeof(SokeresultatMinimum)).Deserialize(
                    sokResponseTextReader);
            }
            catch (Exception e)
            {
                validationErrors.Add(string.Format(ValidationErrorMessages.CouldNotParseSokeresultat, "minimum"));
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

        private static void ValidateMappePeriodTittelEqual(Sok sok, SokeresultatMinimum sokResponse, List<string> validationErrors,
            Parameter parameter)
        {
            var notFoundExpectedRespons = false;
            foreach (var resultatMinimum in sokResponse.ResultatListe)
            {
                var searchText = parameter.Parameterverdier.Stringvalues.First(); //TODO skal vi støtte * 
                var searchTextStripped = searchText.Replace("*", string.Empty).ToLower();
                if (sok.Respons == Respons.Mappe)
                {
                    if (resultatMinimum.Mappe == null && !notFoundExpectedRespons)
                    {
                        validationErrors.Add(string.Format(ValidationErrorMessages.FoundUnexpectedResultTypeAccordingToRespons, sok.Respons.ToString()));
                        notFoundExpectedRespons = true; //Only show this validation message once. Else it will overflow the list.
                    } 
                    if (resultatMinimum.Mappe != null && !resultatMinimum.Mappe.Tittel.Contains(searchTextStripped))
                    {
                        validationErrors.Add(string.Format(ValidationErrorMessages.ResultDontMatchSearchText,
                            parameter.Felt, resultatMinimum.Mappe.Tittel, searchText));
                    }
                }
                else if(sok.Respons == Respons.Saksmappe)
                {
                    if (resultatMinimum.Saksmappe == null && !notFoundExpectedRespons)
                    {
                        validationErrors.Add(string.Format(ValidationErrorMessages.FoundUnexpectedResultTypeAccordingToRespons, sok.Respons.ToString())); 
                        notFoundExpectedRespons = true; //Only show this validation message once. Else it will overflow the list.
                    }
                    if (resultatMinimum.Saksmappe != null && !resultatMinimum.Saksmappe.Tittel.ToLower().Contains(searchTextStripped))
                    {
                        validationErrors.Add(string.Format(ValidationErrorMessages.ResultDontMatchSearchText,
                            parameter.Felt, resultatMinimum.Mappe.Tittel, searchText));
                    }
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