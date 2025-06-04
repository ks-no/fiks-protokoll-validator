using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using KS.Fiks.Plan.Models.V2.innsyn.ArealplanHentTyper;
using KS.Fiks.Saksfaser.V1.SaksfaseHentTyper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace KS.FiksProtokollValidator.WebAPI.TjenerValidator.Validation
{
    public class JsonValidator : IDisposable
    {
        private const string SaksfaserAssemblyBaseFilename = "KS.Fiks.Saksfaser.Models.V1.Schema.V1"; 
        private const string FiksPlanAssemblyBaseFilename = "KS.Fiks.Plan.Models.V2.Schema.V2"; 
        private string assemblyName;
        private string assemblyBaseFilename;

        public static JsonValidator Init()
        {
            Directory.CreateDirectory("./../../../Schemas/");
            return new JsonValidator();
        }
        
        public JsonValidator WithFiksSaksfaser()
        {
            var foo = new HentSaksfase(); // Må gjøres for at assembly blir lastet inn?
            assemblyName = "KS.Fiks.Saksfaser.Models.V1";
            assemblyBaseFilename = SaksfaserAssemblyBaseFilename;
            writeSchemaToDisk("no.ks.fiks.saksfaser.v1.felles.dokument");
            writeSchemaToDisk("no.ks.fiks.saksfaser.v1.felles.dokumentbeskrivelse");
            writeSchemaToDisk("no.ks.fiks.saksfaser.v1.felles.eksternnoekkel");
            writeSchemaToDisk("no.ks.fiks.saksfaser.v1.felles.fase");
            writeSchemaToDisk("no.ks.fiks.saksfaser.v1.felles.gradering");
            writeSchemaToDisk("no.ks.fiks.saksfaser.v1.felles.journalnummer");
            writeSchemaToDisk("no.ks.fiks.saksfaser.v1.felles.journalpost");
            writeSchemaToDisk("no.ks.fiks.saksfaser.v1.felles.korrespondansepart");
            writeSchemaToDisk("no.ks.fiks.saksfaser.v1.felles.milepel");
            writeSchemaToDisk("no.ks.fiks.saksfaser.v1.felles.referansedokumentfil");
            writeSchemaToDisk("no.ks.fiks.saksfaser.v1.felles.referansejournalpost");
            writeSchemaToDisk("no.ks.fiks.saksfaser.v1.felles.saksjournalpostnummer");
            writeSchemaToDisk("no.ks.fiks.saksfaser.v1.felles.saksnummer");
            writeSchemaToDisk("no.ks.fiks.saksfaser.v1.felles.skjerming");
            //TODO last inn og skriv til disk resten av felles schemas
            return this;
        }
        
        public JsonValidator WithFiksPlan()
        {
            var foo = new HentArealplan(); // Må gjøres for at assembly blir lastet inn?
            assemblyName = "KS.Fiks.Plan.Models.V2";
            assemblyBaseFilename = FiksPlanAssemblyBaseFilename;
            writeSchemaToDisk("no.ks.fiks.plan.v2.felles.arealplan");
            writeSchemaToDisk("no.ks.fiks.plan.v2.felles.dispensasjon");
            writeSchemaToDisk("no.ks.fiks.plan.v2.felles.dokument");
            writeSchemaToDisk("no.ks.fiks.plan.v2.felles.flate");
            writeSchemaToDisk("no.ks.fiks.plan.v2.felles.midlertidigforbud");
            writeSchemaToDisk("no.ks.fiks.plan.v2.felles.nasjonalarealplanid");
            writeSchemaToDisk("no.ks.fiks.plan.v2.felles.planbehandling");
            writeSchemaToDisk("no.ks.fiks.plan.v2.felles.posisjon");
            writeSchemaToDisk("no.ks.fiks.plan.v2.felles.saksnummer");
            return this;
        }

        private void writeSchemaToDisk(string messageType)
        {
            var content = GetSchemaFromAssembly(messageType);
            File.WriteAllText($"./../../../Schemas/{messageType}.schema.json", content);
        }

        private string GetSchemaFromAssembly(string messagename)
        {
            var arkivModelsAssembly = Assembly
                .GetExecutingAssembly()
                .GetReferencedAssemblies()
                .Select(a => Assembly.Load(a.FullName)).SingleOrDefault(assembly => assembly.GetName().Name == assemblyName); 

            var fileName = $"{assemblyBaseFilename}.{messagename}.schema.json";
            using (var schemaStream =
                arkivModelsAssembly.GetManifestResourceStream(fileName))
            {
                var streamReader = new StreamReader(schemaStream);
                return streamReader.ReadToEnd();
            }
        }

        private StreamReader GetSchemaFromAssemblyAsStreamReader(string messagename)
        {
            var arkivModelsAssembly = Assembly
                .GetExecutingAssembly()
                .GetReferencedAssemblies()
                .Select(a => Assembly.Load(a.FullName)).SingleOrDefault(assembly => assembly.GetName().Name == assemblyName);

            var fileName = $"{assemblyBaseFilename}.{messagename}.schema.json";
            var schemaStream = arkivModelsAssembly.GetManifestResourceStream(fileName);
            return new StreamReader(schemaStream);
        }

        public void Validate(string payload, List<string> validationErrors, string messageType)
        {
            var streamReader = GetSchemaFromAssemblyAsStreamReader(messageType);
            ValidateJsonWithSchemaString(payload, validationErrors, streamReader, messageType);
        }

        public void ValidateJsonWithSchemaString(string payload, List<string> validationErrors, StreamReader schemaStreamReader, string messageType)
        {
            writeSchemaToDisk(messageType);

            using var reader = new JsonTextReader(schemaStreamReader);
            var resolver = new JSchemaUrlResolver();
            var baseUri = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;

            var schema = JSchema.Load(reader, new JSchemaReaderSettings()
                {
                    Resolver = resolver,
                    BaseUri = new Uri($"{baseUri}/Schemas/{messageType}.schema.json") // Hoved schema
                }
            );
            schema.ExtensionData.Remove("definitions");
            AddAdditionalPropertiesFalseToSchemaProperties(schema.Properties);
            schema.AllowAdditionalProperties = false;

            //TODO:Skille mellom errors og warnings hvis det er 
            var jObject = JObject.Parse(payload);
            jObject.Validate(schema, (o, a) => { validationErrors.Add(a.Message); });
        }

        private void AddAdditionalPropertiesFalseToSchemaProperties(IDictionary<string, JSchema> properties)
        {
            foreach (var item in properties)
            {
                item.Value.AllowAdditionalProperties = false;
                foreach (var itemItem in item.Value.Items)
                {
                    AddAdditionalPropertiesFalseToSchemaProperties(itemItem.Properties);

                }
                AddAdditionalPropertiesFalseToSchemaProperties(item.Value.Properties);
            }
        }

        public void Dispose()
        {
            Directory.Delete("./../../../Schemas/", true);
        }
    }
}