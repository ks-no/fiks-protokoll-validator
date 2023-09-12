using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using KS.Fiks.Arkiv.Models.V1.Innsyn.Sok;
using KS.FiksProtokollValidator.WebAPI.TjenerValidator.Validation.Resources;

namespace KS.FiksProtokollValidator.WebAPI.TjenerValidator.Validation.FiksArkiv
{
    public class SokeresultatNoeklerValidator : AbstractSokeResultatValidator
    {
        public static void Validate(TextReader sokResponseTextReader, Sok sok, List<string> validationErrors)
        {
            SokeresultatNoekler sokResponse = null;
            try
            {
                sokResponse = (SokeresultatNoekler)new XmlSerializer(typeof(SokeresultatNoekler)).Deserialize(
                    sokResponseTextReader);
            }
            catch (Exception)
            {
                validationErrors.Add(string.Format(ValidationErrorMessages.CouldNotParseSokeresultat, "noekler"));
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
                    ValidateSaksmappeSok(sokdefinisjon, sokResponse, validationErrors);
                    break;
            }
        }

        private static void ValidateSaksmappeSok(SaksmappeSokdefinisjon sokdefinisjon, SokeresultatNoekler sokResponse, List<string> validationErrors)
        {
            foreach (var parameter in sokdefinisjon.Parametere)
            {
                switch (parameter.Operator)
                {
                    case OperatorType.Equal:
                        if (parameter.Felt == SaksmappeSokefelt.MappeTittel)
                        {
                            //TODO Skal vi teste dette? 
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

        private static List<DateTime> GetDateResults(SokeresultatNoekler sokResponse, SaksmappeSokefelt parameterFelt,
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
                    //TODO Dette er ikke lenger mulig å gjøre siden Saksdato ikke lenger er med i resultatet
                    return new List<DateTime>(); //sokResponse.ResultatListe.Select(r => r.Saksmappe.Saksdato).ToList();
                default:
                    return new List<DateTime>();
            }
        }
    }
}