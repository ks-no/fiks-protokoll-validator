using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using KS.Fiks.Arkiv.Models.V1.Innsyn.Sok;
using KS.FiksProtokollValidator.WebAPI.TjenerValidator.Validation.Resources;

namespace KS.FiksProtokollValidator.WebAPI.TjenerValidator.Validation.FiksArkiv
{
    public class SokeresultatMinimumValidator : AbstractSokeResultatValidator
    {
        public static void Validate(TextReader sokResponseTextReader, Sok sok, List<string> validationErrors)
        {
            SokeresultatMinimum sokResponse;
            try
            {
                sokResponse = (SokeresultatMinimum)new XmlSerializer(typeof(SokeresultatMinimum)).Deserialize(
                    sokResponseTextReader);
            }
            catch (Exception)
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

            switch (sok.Sokdefinisjon)
            {
                case SaksmappeSokdefinisjon sokdefinisjon :
                    ValidateSaksmappeSok(sok, sokdefinisjon, sokResponse, validationErrors);
                    break;
            }
        }
        
        private static void ValidateSaksmappeSok(Sok sok, SaksmappeSokdefinisjon sokdefinisjon, SokeresultatMinimum sokResponse, List<string> validationErrors)
        {
            foreach (var parameter in sokdefinisjon.Parametere)
            {
                switch (parameter.Operator)
                {
                    case OperatorType.Equal:
                        if (parameter.Felt == SaksmappeSokefelt.MappeTittel)
                        {
                            ValidateMappeTittelEqual(sok, sokResponse, validationErrors, parameter);
                        }
                        break;
                    case OperatorType.Between:
                        if (isDateSokeFelt(parameter.Felt))
                        {
                            var listOfDates = GetDateResults(sokResponse, parameter.Felt, validationErrors);

                            foreach (var dateTimeValue in listOfDates)
                            {
                                ValidateBetweenDates(dateTimeValue, parameter.SokVerdier.Datevalues[0],
                                    parameter.SokVerdier.Datevalues[1], validationErrors);
                            }
                        }
                        break;
                }
            }
        }

        private static void ValidateMappeTittelEqual(Sok sok, SokeresultatMinimum sokResponse, List<string> validationErrors,
            Parameter parameter)
        {
            var notFoundExpectedRespons = false;
            foreach (var resultatMinimum in sokResponse.ResultatListe)
            {
                var searchText = parameter.SokVerdier.Stringvalues.First(); 
                var searchTextStripped = searchText.Replace("*", string.Empty).ToLower();
                if (sok.Sokdefinisjon is MappeSokdefinisjon)
                {
                    if (resultatMinimum.Mappe == null && !notFoundExpectedRespons)
                    {
                        validationErrors.Add(
                            string.Format(ValidationErrorMessages.FoundUnexpectedResultTypeAccordingToRespons, sok.Sokdefinisjon.GetType()));
                        notFoundExpectedRespons = true; //Only show this validation message once. Else it will overflow the list.
                    } 
                    if (resultatMinimum.Mappe != null && !resultatMinimum.Mappe.Tittel.ToLower().Contains(searchTextStripped))
                    {
                        validationErrors.Add(string.Format(ValidationErrorMessages.ResultDontMatchSearchText,
                            ((MappeParameter) parameter).Felt, resultatMinimum.Mappe.Tittel, searchText));
                    }
                }
                else if(sok.Sokdefinisjon is SaksmappeSokdefinisjon)
                {
                    if (resultatMinimum.Saksmappe == null && !notFoundExpectedRespons)
                    {
                        validationErrors.Add(string.Format(ValidationErrorMessages.FoundUnexpectedResultTypeAccordingToRespons, sok.Sokdefinisjon.GetType())); 
                        notFoundExpectedRespons = true; //Only show this validation message once. Else it will overflow the list.
                    }
                    if (resultatMinimum.Saksmappe != null && !resultatMinimum.Saksmappe.Tittel.ToLower().Contains(searchTextStripped))
                    {
                        validationErrors.Add(string.Format(ValidationErrorMessages.ResultDontMatchSearchText,
                            ((SaksmappeParameter) parameter).Felt, resultatMinimum.Mappe.Tittel, searchText));
                    }
                }
            }
        }

        private static List<DateTime> GetDateResults(SokeresultatMinimum sokResponse, SaksmappeSokefelt parameterFelt,
            List<string> validationErrors)
        {
            switch (parameterFelt)
            {
                case SaksmappeSokefelt.SakSaksdato:
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