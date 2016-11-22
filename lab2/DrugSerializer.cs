using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace lab2 {
	interface DrugSerializer {
		DrugCollection<Drug> Deserialize(string json);
		DrugCollection<Drug> DeserializeFromFile(string path);
		string Serialize(DrugCollection<Drug> list);
		void SerializeToFile(DrugCollection<Drug> list, string path);
	};

	class DrugJSONSerializer: DrugSerializer {

		private JsonSerializer serializer = new JsonSerializer();

		public DrugJSONSerializer() {
			serializer.Formatting =Newtonsoft.Json.Formatting.Indented;
			serializer.TypeNameHandling = TypeNameHandling.All;
		}

		private JsonSerializerSettings settings = new JsonSerializerSettings {
			TypeNameHandling = TypeNameHandling.All
		};

		public DrugCollection<Drug> Deserialize(string json) {
			return JsonConvert.DeserializeObject<DrugCollection<Drug>>
				(json, settings);
		}

		public Drug DeserializeDrug(string json) {
			return JsonConvert.DeserializeObject<Drug>(json, settings);
		}

		public DrugCollection<Drug> DeserializeFromFile(string path) {

			using (StreamReader streamReader = new StreamReader(path))
            using (JsonTextReader jsonTextReader = new JsonTextReader(streamReader))
            {
                return serializer.Deserialize<DrugCollection<Drug>>(jsonTextReader);
			}
		}

		public string Serialize(DrugCollection<Drug> list) {
			return JsonConvert.SerializeObject(list, Newtonsoft.Json.Formatting.Indented, settings);
		}
		public void SerializeToFile(DrugCollection<Drug> list, string path) {
			using (StreamWriter streamWriter = new StreamWriter(path))
            using (JsonTextWriter writer = new JsonTextWriter(streamWriter))
            {
                serializer.Serialize(writer, list);
			}
		}
	}

	class DrugXMLSerializer: DrugSerializer {

		private bool validationError = false;
		public string xsdPath="../../../data/drugs.xsd";
	
		private XmlSerializer serializer = new XmlSerializer(
			typeof(DrugCollection<Drug>),
			null,
			new[] { typeof(DrugWrapper), typeof(CompoundedDrug), typeof(UnifiedDescriptor),
			        typeof(TrademarkDescriptor), typeof(ChemicalDescriptor)},
			null,
			"drugSchema"
		);
		public DrugCollection<Drug> Deserialize(string json) {
			using (var reader = new StringReader(json))
            {
                return (DrugCollection<Drug>) serializer.Deserialize(reader);
			}
		}

		public DrugCollection<Drug> DeserializeFromFile(string path) {
			validationError = false;
			VaildateXmlFile(path);
			if(validationError) {
				Console.WriteLine("Error!\n\n\n\n");
				return null;
			}

			using (var reader = new StreamReader(path))
            {
                return (DrugCollection<Drug>) serializer.Deserialize(reader);
			}
		}

		public string Serialize(DrugCollection<Drug> list) {
			using (var serialized = new StringWriter())
			{
				serializer.Serialize(serialized, list);
				return serialized.ToString();
			}
		}

		public void SerializeToFile(DrugCollection<Drug> list, string path) {
			using (var serialized = new StreamWriter(path))
			{
				serializer.Serialize(serialized, list);
			}
		}
		private void VaildateXmlFile(string path) {
			
			XmlSchemaSet sc = new XmlSchemaSet();

			// Add the schema to the collection.
			sc.Add("drugSchema", xsdPath);

			// Set the validation settings.
			XmlReaderSettings settings = new XmlReaderSettings();
			settings.ValidationType = ValidationType.Schema;
			settings.Schemas = sc;
			settings.ValidationEventHandler += new ValidationEventHandler (XmlValidationHandler);
 
			// Create the XmlReader object.
			XmlReader reader = XmlReader.Create(path, settings);

			// Parse the file. 
			while (reader.Read());
		}

		void XmlValidationHandler(object sender, ValidationEventArgs e)
		{
			validationError = true;
			switch (e.Severity){
				case XmlSeverityType.Error:
					Console.WriteLine("Error: {0}", e.Message);
					break;
				case XmlSeverityType.Warning:
					Console.WriteLine("Warning {0}", e.Message);
					break;
			}
		}
	}

	class DrugBinSerializer: DrugSerializer {
	
		private BinaryFormatter serializer = new BinaryFormatter();
		public DrugCollection<Drug> Deserialize(string json) {
			throw new NotImplementedException();
		}

		public DrugCollection<Drug> DeserializeFromFile(string path) {
			using (var reader = new FileStream(path, FileMode.Open))
            {
                return (DrugCollection<Drug>) serializer.Deserialize(reader);
			}
		}

		public string Serialize(DrugCollection<Drug> list) {
			throw new NotImplementedException();
		}

		public void SerializeToFile(DrugCollection<Drug> list, string path) {
			using (var writer = new FileStream(path, FileMode.Create))
            {
                serializer.Serialize(writer, list);
			}
		}
	}

	public class DrugWrapper: Drug {

		public UnifiedDescriptor unified;
		public TrademarkDescriptor trademark;
		public ChemicalDescriptor chemical;
		public CompoundedDrug compounded;

		public override object Clone() {
			throw new NotImplementedException();
		}
		public override bool Equals(IDrug other) {
			return (other is DrugWrapper) && (other as DrugWrapper).unwrap()
				.Equals(unwrap());
		}
		public override bool isNarcotic() {
			throw new NotImplementedException();
		}
		public override bool requiresFridge() {
			throw new NotImplementedException();
		}
		private DrugWrapper() { }

		public DrugWrapper(Drug drug) {
			if(drug is UnifiedDescriptor) unified = drug as UnifiedDescriptor;
			else if(drug is TrademarkDescriptor) trademark = drug as TrademarkDescriptor;
			else if(drug is ChemicalDescriptor) chemical = drug as ChemicalDescriptor;
			else if(drug is CompoundedDrug) compounded = drug as CompoundedDrug;
		}
		public Drug unwrap() {
			if(unified != null) return unified;
			if(trademark != null) return trademark;
			if(chemical != null) return chemical;
			if(compounded != null) return compounded;
			return null;
		}
	}
}