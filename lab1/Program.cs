using System;
using System.Collections.Generic;
using System.Globalization;

namespace lab1
{
    class Program
    {
		static bool supplyDepartment(IFund fund, IShipment shipment, IDepartment department, IWarehouse warehouse) {
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
			if(!warehouse.distributeDrug(drug.INN, department.getRequiredNumber(drug))) {
				Console.WriteLine("Warehouse couldn't distribute required amount of drug");
				return false;
			}
			Console.WriteLine("Drug was successfully distributed");
			return true;
		}
        static void Main(string[] args)
        {
			IDrug atenolol = new UnifiedDescriptor("atenolol");
			
			List<IDepartment> departments = new List<IDepartment> {
				new Department("Dermatology1"),
				new Department("Dermatology2", false),
				new Department("Surgery")
			};
			
			//getter example
			IShipment S1 = new Shipment(atenolol, 10, 0.5);
			Console.WriteLine("Cost {0}", S1.Cost);

			//setter example
			try { 
				ISpecialWarehouse wh1 = new ComboWarehouse(10, 2);
				wh1.SpecialSpace-=3;
			}catch(ArgumentException e) {
				// Console.WriteLine("{0}", e);
				Console.WriteLine("Exception caught");
			}

			// polymorphism
			List<IFund> funds = new List<IFund> {
				new Fund(100.0),
				new BudgetaryFund(200.0)
			};
			funds.ForEach(f=> 
				Console.WriteLine("{0}", f.approveFunding(atenolol, departments[1]))
			);
			var str = Console.ReadLine();
			double d;
			var info = new CultureInfo("ru-RU");
			if(double.TryParse(str, NumberStyles.Any, info, out d)) {
				Console.WriteLine((d*2).ToString("f", info));
			}

			Console.ReadKey();
        }
    }
}
