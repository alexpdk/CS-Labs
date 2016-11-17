using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace lab2
{
    class Program
    {
		static int compareDrugs(IDrug d1, IDrug d2) {
			bool man1 = d1 is AbstractManufacturedDrug, man2 = d2 is AbstractManufacturedDrug;
			if(man1 && man2) {
				//return (d1 as IManufacturedDrug).INN.CompareTo((d2 as IManufacturedDrug).INN);
				return d1.ToString().CompareTo(d2.ToString());
			} else if(!man1 && !man2) {
				return (d1 as AbstractCompoundedDrug).CompoundCode.CompareTo((d2 as AbstractCompoundedDrug).CompoundCode);
			}else if(man1 && !man2) {
				return -1; //IManufacturedDrug considered less than ICompoundedDrug
			}else{
				return 1; //greater
			}
		}
		static void printWarehouseEvent(TextWriter writer, WarehouseEventArgs args) {
			writer.WriteLine("Event received");
			var balArgs = args as BalanceCheckArgs;
			if(balArgs != null) {
				writer.WriteLine("Balance checked: balance={0}",balArgs.balance);
				return;
			}
			var storeArgs = args as ShipmentStoreArgs;
			if(storeArgs != null) {
				var shipment = storeArgs.shipment;
				var result = (storeArgs.success) ? " stored successfully." : "couldn't be stored.";

				writer.WriteLine("Shipment of {0} volume={1} cost={2} {3}",
					shipment.getDrug(), shipment.getVolume(), shipment.Cost, result);
				return;
			}
			var distArgs = args as DrugDistributionArgs;
			//if(distArgs != null) {
			//	var shipment = di
			//	var result = (storeArgs.success) ? " stored successfully." : "couldn't be stored.";

			//	writer.WriteLine("Shipment of {0} volume={1} cost={2} {3}",
			//		shipment.getDrug(), shipment.getVolume(), shipment.Cost, result);
			//	return;
			//}
		}
		static string stringifyMedication(IDrug drug) {
			return drug.ToString();
		}
		static bool supplyDepartment(IFund fund, IShipment<IDrug> shipment, IDepartment department, IWarehouse warehouse) {
			if(!fund.provideFunding(shipment, department)){
				Console.WriteLine("Shipment could not be ordered");
				return false;
			}
			Console.WriteLine("Shipment is payed and received");
			if(!warehouse.storeShipment(shipment)) {
				Console.WriteLine("Warehouse couldn't store the shipment");
				return false;
			}
			Console.WriteLine("Warehouse stored the shipment");
			var drug = shipment.getDrug();
			if(!warehouse.distributeDrug(drug, department.getRequiredNumber(drug))) {
				Console.WriteLine("Warehouse couldn't distribute required amount of drug");
				return false;
			}
			Console.WriteLine("Drug was successfully distributed");
			return true;
		}
		static async void SortDrugCollection(DrugCollection<IDrug> dC) {
			//// using delegate to sort by type/name
			Smoothsort<IDrug> sort = null;
			Task task = null;
			DrugCollection<IDrug>.SortDrugList smoothSort = (_list) => {
				sort = new Smoothsort<IDrug>(compareDrugs, _list);
				task = sort.Sort(new Progress<int>(p=>Console.WriteLine("{0}%",p)));
			};
			dC.SpecifySortMethod(smoothSort);
			dC.Sort();
			await task;
			Console.WriteLine("\nDrug collection sorted");
			//Console.WriteLine(new DrugCollection<IDrug>(sort.SortedList).
			//		Stringify(stringifyMedication, " | "));
			Console.WriteLine();
		}
		static async void SortNumbers(bool print) {
			var list = new List<int> { 2, 6, 12, 9, 0, 2, 8, 15, 77, 7 };
			var rnd = new Random();
			for(int i=0; i<1000000; i++) list.Add(rnd.Next(200));

			Comparison<int> comp = (a, b) => {
				if(a>b)
					return 1;
				if(a<b)
					return -1;
				return 0;
			};
			var sort = new Smoothsort<int>(comp, list, print);
			Console.WriteLine("Sort started");
			var task = sort.Sort(/*new Progress<int>(p=>Console.WriteLine("{0}%",p))*/);
			await task;

			Console.WriteLine("Sort completed");
			//foreach(var val in sort.SortedList) {
			//	Console.Write(" {0}", val);
			//}
			Console.WriteLine();  
		}
		static void executedTask() {
			var id = Thread.CurrentThread.ManagedThreadId;
		}
		static void ThreadManagement() {
			List<Thread> threads = new List<Thread>();
			var idMap = new Dictionary<int, int>();
			var actionMap = new Dictionary<int, Action>();
			var mapLock = new Object();

			bool actionsReady = false;

			for(int i=0; i<10; i++) {
				threads.Add(new Thread((Object o)=> {
					var num = (int)o;
					lock(mapLock) {
						idMap.Add(num, Thread.CurrentThread.ManagedThreadId);
					}
					while(!actionsReady) Thread.Sleep(1000);

					Action action;
					lock(mapLock) {
						action = actionMap[num];
					}
					action.Invoke();
				}));
				threads[threads.Count-1].Start((Object)i);
			}

			while(idMap.Count < 10) Thread.Sleep(100);
			foreach(var pair in idMap) {
				Console.WriteLine("Thread num={0} id={1}",pair.Key, pair.Value);
				if(pair.Value ==  15) actionMap.Add(pair.Key, ()=> SortNumbers(false));
				else actionMap.Add(pair.Key, ()=> { });
			}
			actionsReady = true;
		}

        static void Main(string[] args)
        {
			DrugCollection<IDrug> dC = new DrugCollection<IDrug>(new List<IDrug> {
				new UnifiedDescriptor("lidocaine"),
				new TrademarkDescriptor("procaine-A","PharmSharp","procaine"),
			});
			dC.Add(new ChemicalDescriptor("CDP870","Certolizumab pegol"));

			// IEnumerable, implicit IEnumerator
			//foreach(var drug in dC) {
			//	Console.WriteLine("{0}",drug);
			//}
			//// IClonable
			//IDrug clone = (dC as DrugCollection<IDrug>)[2].Clone() as IDrug;
			//Console.WriteLine("Clone {0}",clone);

			//// contravariance
			//FridgeWarehouse fw =new FridgeWarehouse(200, 40);  
			//IBalance<IWarehouse> wBalance = new WarehouseBalance<IWarehouse>(fw);
			//IBalance<ISpecialWarehouse> swBalance = wBalance;
			//Console.WriteLine("{0}",swBalance.checkBalance(0));

			var castedCollection = dC.Downcast<AbstractManufacturedDrug>();
			var compounded = new CompoundedDrug(castedCollection);
			////covariance
			//IShipment<IDrug> sh = new Shipment<ICompoundedDrug>(compounded, 100, 0.3);
			//Console.WriteLine("{0}",sh.Cost);

			// adding drugs of different type
			dC.Add(compounded);
			dC.Add(new UnifiedDescriptor("atenolol"));

			//Increase collection size
			//dC.Add(TrademarkDescriptor.GenerateIndexedList(50000, "Doxedin-","Synthes","doxorubicin"));

			//// using Func to print array
			//Func<IDrug, string> drug_str = stringifyMedication;
			//Console.WriteLine("Unsorted array");
			//Console.WriteLine(dC.Stringify(drug_str, " | "));

			//SortDrugCollection(dC);

			// using Action to select narcotics
			DrugCollection<IDrug> narcotics = new DrugCollection<IDrug>();
			dC.ForEach(drug => {
				if(drug.isNarcotic())
					narcotics.Add(drug);
			});
			Console.WriteLine("\nNarcotics from collection:");
			Console.WriteLine(narcotics);

			//Console.WriteLine();
			//SortNumbers(print: false);
			//Console.WriteLine("Main thread continues\n");

			FridgeWarehouse fw = new FridgeWarehouse(200, 40);
			//WarehouseLogger<IWarehouse> l = new FileWarehouseLogger<IWarehouse>(fw, "log.txt");
			WarehouseLogger<IWarehouse> l = new ConsoleWarehouseLogger<IWarehouse>(fw);
			l.OnLog+=printWarehouseEvent;

			var balance = fw.getBalance();

			fw.storeShipment(new Shipment<IDrug>(new UnifiedDescriptor("atenolol"), 40, 10));
			l.StopLogging();
			Console.WriteLine();

			//ConsoleExceptionLogger el = new ConsoleExceptionLogger();
			FileExceptionLogger el = new FileExceptionLogger("log.txt");

			try {
				var res = fw.loadShipments("../../data/shipments.xml");
				Console.WriteLine(res ? "Successful load" : "Load failed");
				fw.saveShipments("../../data/shipments3.xml");
				//fw.saveShipments("C:/");
			}
			catch(DrugAccountException e) {
				el.LogDrugAccountException(e);
			}
			catch(Exception e) {
				el.LogSystemException(e);
			}
			el.StopLogging();

			//ThreadManagement();

			var casted = dC.Downcast<Drug>();

			//Console.WriteLine("dC size={0}",dC.Count);
			var jsonSerial = new DrugJSONSerializer();
			var json = jsonSerial.Serialize(casted);
			jsonSerial.SerializeToFile(casted, "../../data/drugs.json");
			Console.WriteLine(json);

			var binSerial = new DrugBinSerializer();
			binSerial.SerializeToFile(casted, "../../data/drugs.bin");

			var xmlSerial = new DrugXMLSerializer();
			var xml = xmlSerial.Serialize(dC.WrapCollection());
			xmlSerial.SerializeToFile(dC.WrapCollection(), "../../data/drugs.xml");
			Console.WriteLine(xml);
			Console.WriteLine();

			
			//var uni = @"{
			//	""$type"": ""lab2.UnifiedDescriptor, ConsoleApplication"",
			//	""INN"": ""procaine""
			//}";
			//var drug1 = jsonSerial.DeserializeDrug(uni);
			//Console.WriteLine(drug1.ToString());

			//var dC2 = jsonSerial.Deserialize(json);
			//var dC2 = xmlSerial.Deserialize(xml);
			//var dC2 = jsonSerial.DeserializeFromFile("../../data/drugs.json");
			//var dC2 = binSerial.DeserializeFromFile("../../data/drugs.bin");
			//var dC2 = xmlSerial.Deserialize(xml).UnwrapCollection();
			var dC2 = xmlSerial.DeserializeFromFile("../../data/drugs.xml").UnwrapCollection();
			Console.WriteLine("dC2 size={0}",dC2.Count);
			Console.WriteLine(dC2.ToString());
			Console.ReadKey();
        }
    }
}
