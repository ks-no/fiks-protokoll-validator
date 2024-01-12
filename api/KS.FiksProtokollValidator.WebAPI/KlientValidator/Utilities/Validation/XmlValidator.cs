using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;

namespace KS.FiksProtokollValidator.WebAPI.KlientValidator.Utilities.Validation
{
    public class XmlValidator
    {
        private readonly List<List<string>>? _xmlValidationMessages = new List<List<string>>() { new List<string>(), new List<string>() };
        private const int XmlValidationErrorLimit = 25;

        public List<List<string>>? ValidateXml(Stream entryStream, XmlSchemaSet xmlSchemaSet)
        {
            var xmlReaderSettings = new XmlReaderSettings();
            xmlReaderSettings.ValidationType = ValidationType.Schema;
            xmlReaderSettings.Schemas.Add(xmlSchemaSet);
            xmlReaderSettings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
            xmlReaderSettings.ValidationEventHandler += ValidationCallBack;

            using (XmlReader validationReader = XmlReader.Create(entryStream, xmlReaderSettings))
            {
                try
                {
                    while (validationReader.Read())
                        if (_xmlValidationMessages != null && _xmlValidationMessages[0].Count >= XmlValidationErrorLimit)
                            break;
                }
                catch (XmlException ex)
                {
                    _xmlValidationMessages?[0].Add(ex.Message + " XSD validering");
                }
            }
            return _xmlValidationMessages;
        }
        private void ValidationCallBack(object sender, ValidationEventArgs args)
        {
            if (args.Severity == XmlSeverityType.Warning)
            {
                _xmlValidationMessages?[1].Add("XSD Validation Warning: linje " + args.Exception.LineNumber +
                                               ", posisjon " + args.Exception.LinePosition + " " + args.Message);
            }
            else if (args.Severity == XmlSeverityType.Error)
            {
                _xmlValidationMessages?[0].Add("XSD Validation Error: linje " + args.Exception.LineNumber +
                                               ", posisjon " + args.Exception.LinePosition + " " + args.Message);
            }
            else
            {
                _xmlValidationMessages?[0].Add("XSD Validation who knows?: linje " + args.Exception.LineNumber +
                                               ", posisjon " + args.Exception.LinePosition + " " + args.Message);
            }
        }
    }
}
