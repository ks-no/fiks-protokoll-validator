using System.Collections.Generic;
using System.Xml.Schema;
using Serilog;

namespace KS.FiksProtokollValidator.WebAPI.TjenerValidator.Validation
{
    public class ValidationHandler
    {
        public List<string> warnings;
        public List<string> errors;

        public ValidationHandler()
        {
            warnings = new List<string>();
            errors = new List<string>();
        }

        public void HandleValidationError(object sender, ValidationEventArgs e)
        {
            switch (e.Severity)
            {
                case XmlSeverityType.Warning:
                    warnings.Add(e.Message);
                    break;
                case XmlSeverityType.Error:
                    errors.Add(e.Message);
                    break;
                default:
                    Log.Warning("SeverityType {Severity} blir behandlet som warning", e.Severity);
                    warnings.Add(e.Message);
                    break;
            }
        }
        
        public bool HasErrors()
        {
            return errors.Count > 0;
        } 
    }
}