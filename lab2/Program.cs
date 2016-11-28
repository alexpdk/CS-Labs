using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DrugAccount
{
    class Program
    {
        static void Main(string[] args)
        {
			DrugCollection<IDrug> dC = new DrugCollection<IDrug>(new List<IDrug> {
				new UnifiedDescriptor("lidocaine"),
				new TrademarkDescriptor("procaine-A","PharmSharp","procaine"),
			});
			dC.Add(new ChemicalDescriptor("CDP870","Certolizumab pegol"));

			var castedCollection = dC.Downcast<AbstractManufacturedDrug>();
			var compounded = new CompoundedDrug(castedCollection);

			FridgeWarehouse fw = new FridgeWarehouse(200, 40);

			//ConsoleExceptionLogger el = new ConsoleExceptionLogger();
			FileExceptionLogger el = new FileExceptionLogger("log.txt");

			try {
				var res = fw.loadShipments("../../data/shipments.xml");
				Console.WriteLine(res ? "Successful load" : "Load failed");
				fw.saveShipments("../../../data/shipments3.xml");
				//fw.saveShipments("C:/");
			}
			catch(DrugAccountException e) {
				el.LogDrugAccountException(e);
			}
			catch(Exception e) {
				el.LogSystemException(e);
			}
			el.StopLogging();

			Console.ReadKey();
        }
    }
}
