using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace lab2 {
	[TestFixture]
	public class CoreTests {
		private const String dataPath = @"C:\Users\дом\Documents\Visual Studio 2015\Projects\lab1\lab2\data\";
		[Test]
		public void CheckDepartments() {
			var dep = new Department("Bridges&Tunnels", budg: true);
			int reqCount = 0;

			foreach(var inn in PharmData.INNList) {
				var d = new UnifiedDescriptor(inn);
				if(dep.getRequiredNumber(d) >=0) reqCount++;
			}
			// department requires 6 different medications by default
			Assert.That(reqCount > 0 && reqCount < 6);
		}
		[Test]
		public void CheckDrugCollection() {
			DrugCollection<IDrug> dC = new DrugCollection<IDrug>(new List<IDrug> {
				new UnifiedDescriptor("lidocaine"),
				new TrademarkDescriptor("procaine-A","PharmSharp","procaine"),
				new ChemicalDescriptor("CP870","Certolizumab pegol")
			});
			Assert.That(
				dC.ToString(),
				Is.EqualTo("lidocaine, PharmSharp: procaine-A(procaine), CP870(Certolizumab pegol)")
			);

			var castedCollection = dC.Downcast<AbstractManufacturedDrug>();
			Assert.That(castedCollection.Count == 3);

			var compounded = new CompoundedDrug(castedCollection);
			dC.Add(compounded);
			Assert.That(dC.Downcast<AbstractManufacturedDrug>().Count == 3);

			Assert.That(dC.Contains(new ChemicalDescriptor("LK200","lidocaine")));

			// using Action to select narcotics
			DrugCollection<IDrug> narcotics = new DrugCollection<IDrug>();
			dC.ForEach(drug => {
				if(drug.isNarcotic())
					narcotics.Add(drug);
			});
			Assert.That
				(new List<IDrug>{ new UnifiedDescriptor("procaine"), compounded },
			Is.EquivalentTo(narcotics));

			dC.Clear();
			Assert.That(dC.Count == 0);

			dC.Add(TrademarkDescriptor.GenerateIndexedList(2, "A", "AProd", "atenolol"));
			var casted = dC.Downcast<Drug>();

			Drug[] arr = new Drug[4];
			dC.CopyTo(arr, 1);
			Assert.That(arr, Is.EquivalentTo(new List<Drug> {null, casted[0], casted[0], null }));

			Assert.That(dC.Remove(casted[0]));
			Assert.That(dC.Remove(casted[0]));
			Assert.That(!dC.Remove(casted[0]));
			Assert.That(dC.Count == 0);
		}
		[Test]
		public void CheckDrugDescriptors() {
			var list = TrademarkDescriptor.GenerateIndexedList(5, "A", "AProd", "atenolol");
			Assert.That(
				list.Select(d=>(d.Clone() as TrademarkDescriptor).trademark),
				Is.EquivalentTo(new List<String> {"A0","A1","A2","A3","A4" })
			);

			list.Add(new UnifiedDescriptor("procaine"));
			Assert.That(!list[0].Equals(list.Last()));

			object eq = new UnifiedDescriptor("atenolol");
			Assert.That(list[0].Equals(eq));

			var comCode = "lidocaine.procaine.Certolizumab pegol";
			var CP = new CompoundedDrug(comCode, true);
			Assert.That((CP.Clone() as CompoundedDrug).isSertified());
			Assert.That(CP.Equals(CP.Clone()));
			Assert.That(CP.ToString(), Is.EqualTo(comCode));
		}
		[Test]
		public void CheckDrugSerialization() {
			DrugCollection <Drug> dC = new DrugCollection<Drug>(new List<Drug> {
				new UnifiedDescriptor("lidocaine"),
				new TrademarkDescriptor("procaine-A","PharmSharp","procaine"),
				new ChemicalDescriptor("CP870","Certolizumab pegol")
			});

			var wrapped = dC.WrapCollection();
			Assert.That(wrapped.Downcast<DrugWrapper>().Count == 3);

			var xmlSerial = new DrugXMLSerializer();
			var xml = xmlSerial.Serialize(wrapped);
			var copy = xmlSerial.Deserialize(xml);
			Assert.That(copy, Is.EquivalentTo(wrapped));
			Assert.That(copy.UnwrapCollection(), Is.EquivalentTo(dC));

			xmlSerial.SerializeToFile(wrapped, dataPath+"drugs.xml");
			xmlSerial.xsdPath = dataPath+"drugs.xsd";
			copy = xmlSerial.DeserializeFromFile(dataPath+"drugs.xml");
			Assert.That(copy, Is.EquivalentTo(wrapped));
			Assert.That(copy.UnwrapCollection(), Is.EquivalentTo(dC));

			//corrupt xml file
			XDocument doc;
			using(var reader = XmlReader.Create(dataPath+"drugs.xml")) {
				doc=XDocument.Load(reader);
				doc.Root.Add(new XElement("Drug2"));
				doc.Save(dataPath+"tmp.xml");
			}
			copy = xmlSerial.DeserializeFromFile(dataPath+"tmp.xml");
			// Xml format error discovered
			Assert.That(copy == null);

			var jsonSerial = new DrugJSONSerializer();
			var json = jsonSerial.Serialize(dC);
			copy = jsonSerial.Deserialize(json);
			Assert.That(copy, Is.EquivalentTo(dC));

			jsonSerial.SerializeToFile(dC, dataPath+"data.json");
			copy = jsonSerial.DeserializeFromFile(dataPath+"data.json");
			Assert.That(copy, Is.EquivalentTo(dC));

			var drug = jsonSerial.DeserializeDrug(@"
				{
				  ""$type"": ""lab2.ChemicalDescriptor, ConsoleApplication"",
				  ""chemName"": ""CDP870"",
				  ""INN"": ""Certolizumab pegol""
				}
			");
			Assert.That(new UnifiedDescriptor("Certolizumab pegol").Equals(drug));

			var binSerial = new DrugBinSerializer();
			binSerial.SerializeToFile(dC, dataPath+"drugs.bin");
			copy = binSerial.DeserializeFromFile(dataPath+"drugs.bin");
			Assert.That(copy, Is.EquivalentTo(dC));
		}
		[Test]
		public void CheckFunds() {
			var budg = new BudgetaryFund(balance: 1000);
			var sh = new Shipment<IDrug>(new UnifiedDescriptor("atenolol"), 50, 20);

			var dep = new Department("Dep1", budg:false);
			Assert.That( !budg.provideFunding(sh, dep));

			var depBudg = new Department("Dep2", budg: true);
			Assert.That(budg.provideFunding(sh, depBudg));

			sh.decreaseVolume(50);
			Assert.That(budg.provideFunding(sh, depBudg));

			var sh2 = new Shipment<IDrug>(new UnifiedDescriptor("atenolol"), 1, 1);
			Assert.IsFalse(budg.provideFunding(sh2, depBudg));

			var targ = new TargetedFund(new List<IDrug> {
				new UnifiedDescriptor("atenolol"), new UnifiedDescriptor("procaine")
			}, balance: 10000);

			sh=new Shipment<IDrug>(new TrademarkDescriptor("A","AProd","atenolol"), 1, 200);
			Assert.That(targ.provideFunding(sh, dep));

			sh2=new Shipment<IDrug>(new UnifiedDescriptor("cocaine"), 1, 500);
			Assert.IsFalse(targ.provideFunding(sh2, dep));
		}
		[Test]
		public void CheckPharmData() {
			Assert.That(PharmData.Code(
				new UnifiedDescriptor("lidocaine")
			), Is.EqualTo("lidocaine"));

			var CP = new CompoundedDrug("lidocaine.procaine.Certolizumab pegol", true);
			
			Assert.That(
				PharmData.Code(CP),
			Is.EqualTo("lidocaine.procaine.Certolizumab pegol"));

			Assert.That(CP.isNarcotic());
			Assert.That(!CP.requiresFridge());

			var ex = Assert.Throws<DrugAccountException>(()=>
				new UnifiedDescriptor("lidocaine2")
			);
			Assert.That(ex.Message, Is.EqualTo("Unknown INN: lidocaine2"));
		}

		[Test]
		public void CheckShipments() {
			var aten = new UnifiedDescriptor("atenolol");
			var sh = new Shipment<IDrug>(aten, 50, 2.5);
			Assert.AreEqual(sh.Cost, 125);

			sh.decreaseVolume(10);
			Assert.AreEqual(sh.Cost, 100);

			Assert.AreEqual(sh.getVolume(), 40);
			Assert.AreEqual(sh.getPrice(), 2.5);

			Assert.That(sh.getDrug().Equals(new UnifiedDescriptor("atenolol")));

			var ex = Assert.Throws<DrugAccountException>(() => 
				new Shipment<IDrug>(aten, 0, 2.5)
			);
			Assert.That(ex.Message, Is.EqualTo("Zero shipment volume"));

			ex = Assert.Throws<DrugAccountException>(() => 
				new Shipment<IDrug>(aten, -10, 2.5)
			);
			Assert.That(ex.Message, Is.EqualTo("Negative shipment volume=-10"));

			ex = Assert.Throws<DrugAccountException>(() => 
				new Shipment<IDrug>(aten, 10, 0.0)
			);
			Assert.That(ex.Message, Is.EqualTo("Zero shipment price"));

			ex = Assert.Throws<DrugAccountException>(() => 
				new Shipment<IDrug>(aten, 10, -2.5)
			);
			Assert.That(ex.Message, Is.EqualTo("Negative shipment price=-2.5"));
		}
		[Test]
		public void CheckWarehouses() {
			var d1 = new UnifiedDescriptor("lidocaine");
			var d2 = new UnifiedDescriptor("procaine");

			var w1 = new CommonWarehouse(capacity: 40);
			Assert.That(w1.storeShipment(
				new Shipment<IDrug>(d1, 15, 200)
			));
			Assert.That(!w1.storeShipment(
				new Shipment<IDrug>(d1, 50, 200)
			));
			Assert.That(!w1.storeShipment(
				new Shipment<IDrug>(d2, 5, 200)
			));
			w1.storeShipment(new Shipment<IDrug>(d1, 10, 40));
			w1.distributeDrug(d1, 5);
			Assert.AreEqual(w1.getBalance(), 2400);
			w1.distributeDrug(d1, 15);
			Assert.AreEqual(w1.getBalance(), 200);

			var w2 = new SafeWarehouse(space: 40, safeSpace: 10);
			Assert.That(w2.storeShipment(
				new Shipment<IDrug>(d2, 5, 200)
			));
			Assert.That(!w2.storeShipment(
				new Shipment<IDrug>(d2, 15, 200)
			));

			var w3 = new ComboWarehouse(space: 200, comboSpace: 100);
			var path = dataPath+"shipments.xml";
			var res = w3.loadShipments(path);
			Assert.That(res);

			var ex = Assert.Throws<ArgumentException>(() => {
				w3.SpecialSpace+=150;
			});
			Assert.That(ex.Message, Is.EqualTo(
				"Warehouse space is not sufficient to include new special space"
			));
			Assert.DoesNotThrow(()=> {
				w3.SpecialSpace+=50;
			});

			var w4 = new FridgeWarehouse(space: 200, fridgeSpace: 50);
			WarehouseLogger<IWarehouse> l = new StringWarehouseLogger<IWarehouse>(w4);
			l.OnLog+=WarehouseEventHandler;

			w4.storeShipment(new Shipment<IDrug>(d1, 50, 200));
			Assert.AreEqual(w4.getBalance(), 10000); 
			
			var cold = new UnifiedDescriptor("insulin");
			var coldSh = new Shipment<IDrug>(cold, _volume: 60, _price: 80);
			
			Assert.IsFalse(w4.storeShipment(coldSh));
			w4.SpecialSpace += 50;
			Assert.That(w4.storeShipment(coldSh));

			ex = Assert.Throws<ArgumentException>(()=> {
				w4.SpecialSpace -= 50;
			});
			Assert.That(ex.Message, Is.EqualTo(
				"New space not allowes to store all shipments"
			));
			Assert.That(w4.distributeDrug(d1, 20));
			Assert.IsFalse(w4.distributeDrug(cold, 80));
			Assert.That(w4.getBalance(), Is.EqualTo(6000));

			Assert.That(
				(l as StringWarehouseLogger<IWarehouse>).getLog(),
				Is.EqualTo(
@"Shipment of lidocaine volume=50 cost=10000 stored successfully.
Balance checked: balance=10000
Shipment of insulin volume=60 cost=4800 couldn't be stored.
Shipment of insulin volume=60 cost=4800 stored successfully.
20 of lidocaine distributed successfully.
20 of insulin couldn't be provided.
Balance checked: balance=6000
"));
			l.StopLogging();
		}
		public void WarehouseEventHandler(TextWriter writer, WarehouseEventArgs args) {
			var balArgs = args as BalanceCheckArgs;
			if(balArgs != null) {
				writer.WriteLine("Balance checked: balance={0}",balArgs.balance);
				return;
			}
			var storeArgs = args as ShipmentStoreArgs;
			if(storeArgs != null) {
				var shipment = storeArgs.shipment;
				var result = (storeArgs.success) ? "stored successfully." : "couldn't be stored.";

				writer.WriteLine("Shipment of {0} volume={1} cost={2} {3}",
					shipment.getDrug(), shipment.getVolume(), shipment.Cost, result);
				return;
			}
			var distArgs = args as DrugDistributionArgs;
			if(distArgs != null) {
				var drug = distArgs.drug;
				var num = distArgs.distributed;
				var result = (distArgs.success) ? "distributed successfully." : "couldn't be provided.";
				writer.WriteLine("{0} of {1} {2}", num, drug, result);
			}
		}
	}
}
