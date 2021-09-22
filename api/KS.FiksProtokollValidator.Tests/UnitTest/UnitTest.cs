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
            _xmlReaderSettings = new XmlReaderSettings();
            baseFilePath = "./TestCases/no.ks.fiks.gi.arkivintegrasjon.oppdatering.basis.arkivmelding.v1/NyJournalpostN1";
        }


        [Test]
        public void ValidateXmlWithXsdFromFiksIO()
        {
            try
            {
                _xmlReaderSettings.Schemas.Add("http://www.arkivverket.no/standarder/noark5/arkivmelding/v2",
                    "./Schema/arkivmelding.xsd");
                _xmlReaderSettings.Schemas.Add("http://www.arkivverket.no/standarder/noark5/metadatakatalog/v2",
                    "./Schema/metadatakatalog.xsd");
                _xmlReaderSettings.ValidationType = ValidationType.Schema;
                _xmlReaderSettings.ValidationEventHandler += new ValidationEventHandler(xmlReaderSettingsValidationEventHandler);
                _xmlReader = XmlReader.Create(baseFilePath+"/arkivmelding.xml", _xmlReaderSettings);
                while (_xmlReader.Read()) { }
                Assert.Pass();
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
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