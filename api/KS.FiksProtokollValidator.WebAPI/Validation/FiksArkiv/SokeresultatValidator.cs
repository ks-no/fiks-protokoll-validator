using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using KS.Fiks.Arkiv.Models.V1.Arkivstruktur;
using KS.Fiks.Arkiv.Models.V1.Innsyn.Sok;
using KS.FiksProtokollValidator.WebAPI.Models;

namespace KS.FiksProtokollValidator.WebAPI.Validation.FiksArkiv
{
    public class SokeresultatValidator : AbstractXmlValidator
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
               Sok sok;
               try
               {
                   sok = (Sok)new XmlSerializer(typeof(Sok)).Deserialize(sokTextReader);
               }
               catch (Exception e)
               {
                   validationErrors.Add(e.Message);
                   return;
               }

               //Parse the sok response
               using (var sokResponseTextReader = (TextReader)new StringReader(xmlPayloadContent))
               {
                   switch (sok.Sokdefinisjon.Responstype)
                   {
                       case Responstype.Utvidet:
                           SokeresultatUtvidetValidator.Validate(sokResponseTextReader, sok, validationErrors);
                           break;
                       case Responstype.Noekler:
                           SokeresultatNoeklerValidator.Validate(sokResponseTextReader, sok, validationErrors);
                           break;
                       case Responstype.Minimum:
                       default:
                           SokeresultatMinimumValidator.Validate(sokResponseTextReader, sok, validationErrors);
                           break;
                   }
               }
               
           }
       }
    }
}