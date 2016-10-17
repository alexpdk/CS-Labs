using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace lab2 {
	/// <summary>
	/// Класс для передачи аргументов в обработчики событий класса Склад
	/// </summary>
	public abstract class WarehouseEventArgs: EventArgs {
		public readonly Warehouse warehouse;
		public WarehouseEventArgs(Warehouse ware) {
			warehouse = ware;
		}
	}
	// Производные классы описывают аргументы для различных типов событий.
	public class BalanceCheckArgs: WarehouseEventArgs {
		public readonly double balance;
		public BalanceCheckArgs(Warehouse warehouse, double balance):base(warehouse) {
			this.balance = balance;
		}
	}
	public class DrugDistributionArgs: WarehouseEventArgs {
		public readonly IDrug drug;
		public readonly int distributed;
		public readonly bool success;
		public DrugDistributionArgs(Warehouse warehouse, IDrug drug, int distributed, bool success): base(warehouse) {
			this.drug = drug;
			this.distributed = distributed;
			this.success = success;
		}
	}
	public class ShipmentStoreArgs: WarehouseEventArgs {
		public readonly IShipment<IDrug> shipment;
		public readonly bool success;
		public ShipmentStoreArgs(Warehouse warehouse, IShipment<IDrug> shipment, bool success): base(warehouse) {
			this.shipment = shipment;
			this.success = success;
		}
	}
}
