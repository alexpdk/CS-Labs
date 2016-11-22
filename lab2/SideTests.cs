using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace lab2 {
	[TestFixture]
	class SideTests {
		[Test]
		public async Task CheckSmoothsort() {
			var list = new List<int> { 2, 6, 12, 9, 0, 2, 8, 15, 77, 7 };

			Comparison<int> comp = (a, b) => {
				if(a>b)
					return 1;
				if(a<b)
					return -1;
				return 0;
			};
			var sort = new Smoothsort<int>(comp, list, print: false);
			Console.WriteLine("Sort started");
			var task = sort.Sort(/*new Progress<int>(p=>Console.WriteLine("{0}%",p))*/);
			await task;

			Assert.That(sort.SortedList, Is.EquivalentTo(new int[] {
				0, 2, 2, 6, 7, 8, 9, 12, 15, 77
			}));
		}
		[Test]
		public async Task CheckCollectionSort() {
			var dC = new DrugCollection<IDrug>(new List<IDrug> {
				new UnifiedDescriptor("lidocaine"),
				new TrademarkDescriptor("procaine-A","PharmSharp","procaine"),
				new ChemicalDescriptor("CP870","Certolizumab pegol")
			});

			Smoothsort<IDrug> sort = null;
			Task task = null;
			DrugCollection<IDrug>.SortDrugList smoothSort = (_list) => {
				sort = new Smoothsort<IDrug>(CompareDrugs, _list, print: true);
				task = sort.Sort(new Progress<int>(p=>Console.WriteLine("{0}%",p)));
			};
			dC.SpecifySortMethod(smoothSort);
			dC.Sort();
			await task;
			Assert.That(
				new DrugCollection<IDrug>(sort.SortedList)
				.Stringify(StringifyMedication, " | "),
			Is.EqualTo("CP870(Certolizumab pegol) | lidocaine | PharmSharp: procaine-A(procaine)"));

			//Console.WriteLine("\nDrug collection sorted");
			//Console.WriteLine(new DrugCollection<IDrug>(sort.SortedList).
			//		Stringify(stringifyMedication, " | "));
			//Console.WriteLine();
		}
		[Test]
		public void CheckInterfacesAndGenerics() {
			var dC = new DrugCollection<IDrug>(new List<IDrug> {
				new UnifiedDescriptor("lidocaine"),
				new TrademarkDescriptor("procaine-A","PharmSharp","procaine"),
				new ChemicalDescriptor("CP870","Certolizumab pegol")
			});
			//IEnumerable, implicit IEnumerator
			int count=0;
			foreach(var drug in dC) count++;
			Assert.AreEqual(dC.Count, count);

			// IClonable
			IDrug clone = (dC as DrugCollection<IDrug>)[2].Clone() as IDrug;
			Assert.That(Object.Equals(clone, dC[2]));

			// contravariance
			FridgeWarehouse fw = new FridgeWarehouse(200, 40);
			fw.storeShipment(new Shipment<IDrug>(dC[0], 10, 10));
			IBalance<IWarehouse> wBalance = new WarehouseBalance<IWarehouse>(fw);
			IBalance<ISpecialWarehouse> swBalance = wBalance;
			Assert.That(swBalance.checkBalance(100.0));

			var castedCollection = dC.Downcast<AbstractManufacturedDrug>();
			var compounded = new CompoundedDrug(castedCollection);
			//covariance
			IShipment<IDrug> sh = new Shipment<CompoundedDrug>(compounded, 100, 0.3);
			Assert.AreEqual(sh.Cost, 30);
		}
		public int CompareDrugs(IDrug d1, IDrug d2) {
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
		public string StringifyMedication(IDrug drug) {
			return drug.ToString();
		}
		//public void ManageThreads() {
		//	List<Thread> threads = new List<Thread>();
		//	var idMap = new Dictionary<int, int>();
		//	var actionMap = new Dictionary<int, Action>();
		//	var mapLock = new Object();

		//	bool actionsReady = false;

		//	for(int i=0; i<10; i++) {
		//		threads.Add(new Thread((Object o)=> {
		//			var num = (int)o;
		//			lock(mapLock) {
		//				idMap.Add(num, Thread.CurrentThread.ManagedThreadId);
		//			}
		//			while(!actionsReady) Thread.Sleep(1000);

		//			Action action;
		//			lock(mapLock) {
		//				action = actionMap[num];
		//			}
		//			action.Invoke();
		//		}));
		//		threads[threads.Count-1].Start((Object)i);
		//	}

		//	while(idMap.Count < 10) Thread.Sleep(100);
		//	foreach(var pair in idMap) {
		//		Console.WriteLine("Thread num={0} id={1}",pair.Key, pair.Value);
		//		if(pair.Value ==  15) actionMap.Add(pair.Key, ()=> SortNumbers(false));
		//		else actionMap.Add(pair.Key, ()=> { });
		//	}
		//	actionsReady = true;
		//}
	}
}
