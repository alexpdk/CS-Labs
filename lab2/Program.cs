using System;
using System.Collections.Generic;

namespace lab2
{
    class Program
    {
		static int compareDrugs(IDrug d1, IDrug d2) {
			bool man1 = d1 is IManufacturedDrug, man2 = d2 is IManufacturedDrug;
			if(man1 && man2) {
				return (d1 as IManufacturedDrug).INN.CompareTo((d2 as IManufacturedDrug).INN);
			}else if(!man1 && !man2) {
				return (d1 as ICompoundedDrug).CompoundCode.CompareTo((d2 as ICompoundedDrug).CompoundCode);
			}else if(man1 && !man2) {
				return -1; //IManufacturedDrug considered less than ICompoundedDrug
			}else{
				return 1; //greater
			}
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
        static void Main(string[] args)
        {
			DrugCollection<IDrug> dC = new DrugCollection<IDrug>(new List<IDrug> {
				new UnifiedDescriptor("procaine"),
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

			var castedCollection = dC.Downcast<IManufacturedDrug>();
			var compounded = new CompoundedDrug(castedCollection);
			////covariance
			//IShipment<IDrug> sh = new Shipment<ICompoundedDrug>(compounded, 100, 0.3);
			//Console.WriteLine("{0}",sh.Cost);

			// adding drugs of different type
			dC.Add(compounded);
			dC.Add(new UnifiedDescriptor("atenolol"));

			// using Func to print array
			Func<IDrug, string> drug_str = stringifyMedication;
			Console.WriteLine("Unsorted array");
			Console.WriteLine(dC.Stringify(drug_str, " | "));

			// using delegate to sort by type/name
			DrugCollection<IDrug>.SortDrugList smoothSort = (list) => {
				new Smoothsort<IDrug>(compareDrugs, list).Sort();
			};
			dC.SpecifySortMethod(smoothSort);
			dC.Sort();
			Console.WriteLine("\nSorted array");
			Console.WriteLine(dC.Stringify(drug_str, " | "));

			// using Action to select narcotics
			DrugCollection<IDrug> narcotics = new DrugCollection<IDrug>();
			dC.ForEach(drug => {
				if(drug.isNarcotic())
					narcotics.Add(drug);
			});
			Console.WriteLine("\nNarcotics from collection:");
			Console.WriteLine(narcotics);

			//var list = new List<int> { 2, 6, 12, 9, 0, 2, 8, 15, 77, 7 };
			//Comparison<int> comp = (a, b) => {
			//	if(a>b)
			//		return 1;
			//	if(a<b)
			//		return -1;
			//	return 0;
			//};
			//var sort = new Smoothsort<int>(comp, list);
			//sort.Sort();
			//foreach(var val in list) {
			//	Console.Write(" {0}", val);
			//}

			Console.ReadKey();
        }
    }
}
