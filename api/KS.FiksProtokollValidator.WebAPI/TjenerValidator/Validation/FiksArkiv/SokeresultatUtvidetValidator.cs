using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using KS.Fiks.Arkiv.Models.V1.Innsyn.Sok;
using KS.FiksProtokollValidator.WebAPI.Validation.Resources;

namespace KS.FiksProtokollValidator.WebAPI.TjenerValidator.Validation.FiksArkiv
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
            catch (Exception)
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

            
        }
        
        private static void ValidateSaksmappeSok(Sok sok, SaksmappeSokdefinisjon sokdefinisjon, Sokeresultat sokResponse, List<string> validationErrors)
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

        private static void ValidateMappeTittelEqual(Sok sok, Sokeresultat sokResponse, List<string> validationErrors, Parameter parameter)
        {
            var notFoundExpectedRespons = false;
            foreach (var resultat in sokResponse.ResultatListe)
            {
                var searchText = parameter.SokVerdier.Stringvalues.First();
                var searchTextStripped = searchText.Replace("*", string.Empty).ToLower();
                if (sok.Sokdefinisjon is MappeSokdefinisjon)
                {
                    if (resultat.Mappe == null && !notFoundExpectedRespons)
                    {
                        validationErrors.Add(string.Format(ValidationErrorMessages.FoundUnexpectedResultTypeAccordingToRespons, sok.Sokdefinisjon.GetType()));
                        notFoundExpectedRespons = true; //Only show this validation message once. Else it will overflow the list.
                    }
                    if (resultat.Mappe != null && !resultat.Mappe.Tittel.ToLower().Contains(searchTextStripped))
                    {
                        validationErrors.Add(string.Format(ValidationErrorMessages.ResultDontMatchSearchText,
                            ((MappeParameter) parameter).Felt,
                            resultat.Mappe.Tittel, searchText));
                    }
                } else if (sok.Sokdefinisjon is SaksmappeSokdefinisjon)
                {
                    if (resultat.Saksmappe == null && !notFoundExpectedRespons)
                    {
                        validationErrors.Add(string.Format(ValidationErrorMessages.FoundUnexpectedResultTypeAccordingToRespons, sok.Sokdefinisjon.GetType())); 
                        notFoundExpectedRespons = true; //Only show this validation message once. Else it will overflow the list.
                    }
                    if (resultat.Saksmappe != null && !resultat.Saksmappe.Tittel.ToLower().Contains(searchTextStripped))
                    {
                        validationErrors.Add(string.Format(ValidationErrorMessages.ResultDontMatchSearchText,
                            ((SaksmappeParameter) parameter).Felt, resultat.Mappe.Tittel, searchText));
                    }
                }
            }
        }

        private static List<DateTime> GetDateResults(Sokeresultat sokResponse, SaksmappeSokefelt parameterFelt,
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