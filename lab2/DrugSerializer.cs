using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
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
			serializer.Formatting = Formatting.Indented;
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
			return JsonConvert.SerializeObject(list, Formatting.Indented, settings);
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
	
		private XmlSerializer serializer = new XmlSerializer(
			typeof(DrugCollection<Drug>),
			new[] { typeof(CompoundedDrug), typeof(UnifiedDescriptor),
			        typeof(TrademarkDescriptor), typeof(ChemicalDescriptor)  }
		);
		public DrugCollection<Drug> Deserialize(string json) {
			using (var reader = new StringReader(json))
            {
                return (DrugCollection<Drug>) serializer.Deserialize(reader);
			}
		}

		public DrugCollection<Drug> DeserializeFromFile(string path) {
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
}