using System;
using System.Collections.Generic;

namespace lab2
{
    class Program
    {
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
			// ICollection
			ICollection<IDrug> dC = new DrugCollection<IDrug>(new List<IDrug> {
				new UnifiedDescriptor("procaine"),
				new TrademarkDescriptor("procaine-A","Pharm#","procaine")
			});
			dC.Add(new ChemicalDescriptor("CDP870","Certolizumab pegol"));
			// IEnumerable, implicit IEnumerator
			foreach(var drug in dC) {
				Console.WriteLine("{0}",drug);
			}
			// IClonable
			IDrug clone = (dC as DrugCollection<IDrug>)[2].Clone() as IDrug;
			Console.WriteLine("Clone {0}",clone);

			// contravariance
			FridgeWarehouse fw =new FridgeWarehouse(200, 40);  
			IBalance<IWarehouse> wBalance = new WarehouseBalance<IWarehouse>(fw);
			IBalance<ISpecialWarehouse> swBalance = wBalance;
			Console.WriteLine("{0}",swBalance.checkBalance(0));

			var castedCollection =  (dC as DrugCollection<IDrug>)
				.Downcast<IManufacturedDrug>();
			var comp = new CompoundedDrug(castedCollection);
			//covariance
			IShipment<IDrug> sh = new Shipment<ICompoundedDrug>(comp, 100, 0.3);
			Console.WriteLine("{0}",sh.Cost);
				 
			Console.ReadKey();
        }
    }
}
