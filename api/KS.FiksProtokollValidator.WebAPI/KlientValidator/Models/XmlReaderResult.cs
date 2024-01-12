using System.Collections.Generic;

namespace KS.FiksProtokollValidator.WebAPI.KlientValidator.Models;

public class XmlReaderResult
{
    public bool XmlValidationErrorOccured { get; set; }
    public List<List<string>> validationMessages { get; set; }
    public string Xml { get; set; }
}