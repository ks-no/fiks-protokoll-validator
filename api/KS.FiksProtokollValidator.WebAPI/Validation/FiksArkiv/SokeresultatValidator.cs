using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
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
               var sok = (Sok) new XmlSerializer(typeof(Sok)).Deserialize(sokTextReader);

               
               //Parse the sok response
               using (var sokResponseTextReader = (TextReader)new StringReader(xmlPayloadContent))
               {
                   switch (sok.ResponsType)
                   {
                       case ResponsType.Utvidet:
                           SokeresultatUtvidetValidator.Validate((Sokeresultat) new XmlSerializer(typeof(SokeresultatMinimum)).Deserialize(
                               sokResponseTextReader), sok, validationErrors);
                           break;
                       case ResponsType.Noekler:
                           SokeresultatNoeklerValidator.Validate((SokeresultatNoekler) new XmlSerializer(typeof(SokeresultatMinimum)).Deserialize(
                               sokResponseTextReader), sok, validationErrors);
                           break;
                       case ResponsType.Minimum:
                       default:
                           SokeresultatMinimumValidator.Validate((SokeresultatMinimum) new XmlSerializer(typeof(SokeresultatMinimum)).Deserialize(
                               sokResponseTextReader), sok, validationErrors);
                           break;
                   }
               }
               
               
               
           }
       }
    }
}