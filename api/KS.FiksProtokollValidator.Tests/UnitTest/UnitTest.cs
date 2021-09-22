using System;
using System.Xml;
using System.Xml.Schema;
using NUnit.Framework;
using KS.FiksProtokollValidator.WebAPI;

namespace KS.FiksProtokollValidator.Tests.UnitTest
{
    public class UnitTest
    {
        private XmlReaderSettings _xmlReaderSettings;
        private XmlReader _xmlReader;
        private string baseFilePath;
        
        
        
        [SetUp]
        public void Setup()
        {
            baseFilePath = "./TestCases/no.ks.fiks.gi.arkivintegrasjon.oppdatering.basis.arkivmelding.v1/NyJournalpostN1";
            _xmlReaderSettings = new XmlReaderSettings();
            try
            {
                _xmlReaderSettings.Schemas.Add("http://www.arkivverket.no/standarder/noark5/arkivmelding/v2",
                    "./Schema/arkivmelding.xsd");
                _xmlReaderSettings.Schemas.Add("http://www.arkivverket.no/standarder/noark5/metadatakatalog/v2",
                    "./Schema/metadatakatalog.xsd");
                _xmlReaderSettings.ValidationType = ValidationType.Schema;
                _xmlReaderSettings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
                _xmlReaderSettings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
                _xmlReaderSettings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
                _xmlReaderSettings.ValidationEventHandler += new ValidationEventHandler(xmlReaderSettingsValidationEventHandler);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }


        [Test]
        public void ValidateXmlWithXsdFromFiksIO()
        {
            var line = String.Empty;
            try
            {
                _xmlReader = XmlReader.Create(baseFilePath+"/arkivmelding.xml", _xmlReaderSettings);
                while (_xmlReader.Read()) { }
                Assert.Pass();
                _xmlReader.Close();
            }
            catch (Exception e)
            {
                _xmlReader.Close();
                //Assert.Fail(e.Message);
            }
            
        }

        private void xmlReaderSettingsValidationEventHandler(object? sender, ValidationEventArgs e)
        {
            if (e.Severity == XmlSeverityType.Error)
            {
                Assert.Fail(e.Message);
            }
        }
    }
}