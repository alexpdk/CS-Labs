using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace lab2 {
	public interface IWarehouseLogger<in T> where T : IWarehouse {
		event Action<TextWriter, WarehouseEventArgs> OnLog;
	}

	public abstract class WarehouseLogger<T> : IWarehouseLogger<T> where T : IWarehouse {
		public event Action<TextWriter, WarehouseEventArgs> OnLog;
		protected T warehouse;
		protected TextWriter writer;
		public WarehouseLogger(T warehouse){
			this.warehouse = warehouse;
			// init event
			OnLog = delegate{ };
			// events subscription
			warehouse.OnBalanceCount += PrintMessage;
			warehouse.OnDrugDistribution += PrintMessage;
			warehouse.OnShipmentStore += PrintMessage;
		}

		public IWarehouse IWarehouse {
			get {
				throw new System.NotImplementedException();
			}

			set {
			}
		}

		public virtual void PrintMessage(WarehouseEventArgs args) {
			OnLog(writer, args);
		}
		public virtual void StopLogging() {
			writer.Close();
		}
	}
	public class ConsoleWarehouseLogger<T> : WarehouseLogger<T> where T : IWarehouse {
		public ConsoleWarehouseLogger(T warehouse) : base(warehouse){
			writer = Console.Out;
		}
	}
	public class FileWarehouseLogger<T> : WarehouseLogger<T> where T : IWarehouse {
		public FileWarehouseLogger(T warehouse, string path) : base(warehouse) {

			//Debug.Write(Directory.GetCurrentDirectory());
			if(!File.Exists(path)) {
				Debug.Write("Create file");
				var stream = File.Create(path);
				writer = new StreamWriter(stream);
			}else {
				writer=File.AppendText(path);
				Debug.Write("File exists");
			}
		}
	}
}
