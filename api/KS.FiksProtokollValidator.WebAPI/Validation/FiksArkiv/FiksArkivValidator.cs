using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using KS.Fiks.IO.Arkiv.Client.Models.Innsyn.Sok;
using KS.FiksProtokollValidator.WebAPI.Models;

namespace KS.FiksProtokollValidator.WebAPI.Validation.FiksArkiv
{
    public class FiksArkivValidator : AbstractXmlValidator
    {
        public static void ValidateXmlPayloadWithSokRequest(string xmlPayloadContent, FiksRequest fiksRequest,
            List<string> validationErrors)
       {
           var sokXml = "";
           if (fiksRequest.CustomPayloadFile != null)
           {
               sokXml = GetCustomRequestXml(fiksRequest);
           }
           else
           {
               sokXml = GetStandardRequestXml(fiksRequest);
           }

           using (var sokTextReader = (TextReader)new StringReader(sokXml))
           {
               //Parse the sok request
               var sok = (Sok)new XmlSerializer(typeof(Sok)).Deserialize(sokTextReader);

               SokeresultatMinimum sokResponse = null;
               //Parse the sok response
               using (var sokResponseTextReader = (TextReader)new StringReader(xmlPayloadContent))
               {
                   switch (sok.ResponsType)
                   {
                       case ResponsType.Minimum:
                           sokResponse =
                               (SokeresultatMinimum)new XmlSerializer(typeof(SokeresultatMinimum)).Deserialize(
                                   sokResponseTextReader);

                           break;
                   }
               }
               
               // Too many responses. Doesnt match Take request. 
               if (sokResponse.ResultatListe.Count > sok.Take)
               {
                   validationErrors.Add($"Too large ResultatListe. The 'take' parameter was {sok.Take} and the response had {sokResponse.ResultatListe.Count} items in ResultatListe");
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
                                       validationErrors.Add($"The result for '{parameter.Felt}' with value '{resultatMinimum.Mappe.Tittel}' doesnt match the search text '{searchText}'?" );
                                   }
                               }
                           }
                           break;
                       case OperatorType.Between:
                           if (isDateSokeFelt(parameter.Felt))
                           {
                               var listOfDates = GetDateResults(sokResponse, parameter.Felt);

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
       }
        private static bool isDateSokeFelt(SokFelt parameterFelt)
        {
            switch (parameterFelt)
            {
                case SokFelt.JournalpostPeriodDokumentetsdato:
                case SokFelt.JournalpostPeriodForfallsdato:
                case SokFelt.JournalpostPeriodJournaldato:
                case SokFelt.DokumentbeskrivelsePeriodOpprettetDato:
                case SokFelt.MappePeriodOpprettetDato:
                case SokFelt.MappePeriodAvsluttetDato:
                case SokFelt.DokumentbeskrivelsePeriodSkjermingPeriodSkjermingOpphoererDato:
                case SokFelt.RegistreringPeriodOpprettetDato:
                case SokFelt.SakPeriodSaksdato:
                case SokFelt.RegistreringPeriodSkjermingPeriodSkjermingOpphoererDato:
                case SokFelt.MappePeriodSkjermingPeriodSkjermingOpphoererDato:    
                case SokFelt.JournalpostPeriodSaksaar:
                case SokFelt.SakPeriodSaksaar:
                case SokFelt.JournalpostPeriodJournalaar:
                    return true;
                default:
                    return false;
            }
        }
    }
}