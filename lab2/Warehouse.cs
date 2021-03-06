﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace DrugAccount {
	/// <summary>
	/// Интерфейс для складов
	/// </summary>
	public interface IWarehouse {
		/// <summary>
		/// Свойство для доступа к свободному пространству на складе
		/// </summary>
		int Space {
			get; set;
		}
		/// <summary>
		/// Отпустить требуемое число медикаментов со склада. 
		/// </summary>
		/// <param name="drug">Требуемый медикамент</param>
		/// <param name="requiredNumber">Требуемое число</param>
		/// <returns>Были ли медикаменты успешно выданы</returns>
		bool distributeDrug(IDrug drug, int requiredNumber);
		/// <returns>Общую стоимость партий на балансе склада</returns>
		double getBalance();
		/// <summary>
		/// Загрузить данные о партиях медикаментов на складе из файла конфигурации.
		/// </summary>
		/// <param name="path">Путь к файлу</param>
		/// <returns>Удалось ли разместить на складе указанные в файле партии</returns>
		bool loadShipments(string path);
		/// <summary>
		/// Выгрузить данные о партиях медикаментов в файл конфигурации.
		/// </summary>
		/// <param name="path">Путь к файлу</param>
		void saveShipments(string path);
		/// <summary>
		/// Загрузить партию медикаментов на склад. Если на складе отстуствуют необходимые 
		/// для партии условия хранения или недостаточно место, партия на складе не размещается.
		/// </summary>
		/// <param name="shipment">Партия медикаментов</param>
		/// <returns>Удалось ли успешно разместить партию</returns>
		bool storeShipment(IShipment<IDrug> shipment);
		/// <summary>
		/// Событие подсчёта баланса склада. Обработчику передаётся подсчитанный баланс. 
		/// </summary>
		event Action<WarehouseEventArgs> OnBalanceCount;
		/// <summary>
		/// Событие отпуска медикаментов со склада. Обработчику передаётся отпускаемый медикамент, 
		/// число отпущенных упаковок и логическое значение: удалось ли отпустить требуемое
		/// количество.
		/// </summary>
		event Action<WarehouseEventArgs> OnDrugDistribution;
		/// <summary>
		/// Событие размещения партии на складе. Обработчику передаются размещаемая партия
		/// и логическое значение: удалось ли успешно разместить партию
		/// </summary>
		event Action<WarehouseEventArgs> OnShipmentStore;
	}
	/// <summary>
	/// Интерфейс для складов с внутренними хранилищами для медикаментов.
	/// </summary>
	public interface ISpecialWarehouse : IWarehouse{
		/// <summary>
		/// Свойство, предоставляющее доступ к свободному пространству во внутреннем хранилища
		/// </summary>
		int SpecialSpace {
			get; set;
		}
	}
	/// <summary>
	/// Класс Склад служит основой для различных типов складов в предметной области и используется для
	/// представления хранилищ лекарств (холодильников, сейфов).
	/// </summary>
	public class Warehouse : IWarehouse {
		/// <summary>
		/// Партии медикаментов на складе
		/// </summary>
		private List<IShipment<IDrug>> shipments;
		/// <summary>
		/// Позволяет избежать отправления избыточных событий при подсчёте баланса
		/// на складах с внутренним хранилищем.
		/// </summary>
		protected bool emitBalanceEvent = true;
		/// <summary>
		/// Позволяет избежать отправления избыточных событий при загрузке партий склада
		/// из файла конфигурации
		/// </summary>
		protected bool emitShipmentEvent = true;
		/// <summary>
		/// Свободное место на складе (измеряется в количестве контейнеров с медикаментами)
		/// </summary>
		protected int space;
		public int Space {
			get {
				return space;
			}
			set {
				if(value < 0) throw new System.ArgumentException(
					"New space not allowes to store all shipments"
				);
				space = value;
			}
		}
		public event Action<WarehouseEventArgs> OnBalanceCount;
		public event Action<WarehouseEventArgs> OnDrugDistribution;
		public event Action<WarehouseEventArgs> OnShipmentStore;
		// Methods for raising events in derived classes
		protected void raiseOnBalanceCount(double balance) {
			if(emitBalanceEvent) OnBalanceCount(new BalanceCheckArgs(this, balance));
		}
		protected void raiseOnBalanceCount(WarehouseEventArgs args) {
			if(emitBalanceEvent) OnBalanceCount(args);
		}
		protected void raiseOnDrugDistribution(IDrug drug, int distributed, bool success) {
			OnDrugDistribution(new DrugDistributionArgs(this, drug, distributed, success));
		}
		protected void raiseOnDrugDistribution(WarehouseEventArgs args) {
			OnDrugDistribution(args);
		}
		protected void raiseOnShipmentStore(IShipment<IDrug> shipment, bool success) {
			if(emitShipmentEvent) OnShipmentStore(new ShipmentStoreArgs(this, shipment, success));
		}
		protected void raiseOnShipmentStore(WarehouseEventArgs args) {
			if(emitShipmentEvent) OnShipmentStore(args);
		}

		/// <summary>
		/// Область видимости конструктора ограничена, т.к. класс не пригоден для самостоятельного 
		/// использования (отсутствуют проверки на условия хранения медикаментов).
		/// </summary>
		/// <param name="_space">Начальная вместимость</param>
		protected Warehouse(int _space) {
			space = _space;
			shipments = new List<IShipment<IDrug>>();
			// subscribe noop to events to prevent NullPointerException when event emitted
			OnBalanceCount = delegate { };
			OnDrugDistribution = delegate { };
			OnShipmentStore = delegate { };
		}
		/// <summary>
		/// Позволяет дочерним классам получить экземпляр родительского класса
		/// в обход защищённого конструктора.
		/// </summary>
		/// <param name="space">Начальная вместимость</param>
		/// <returns>Экземпляр класса Склад</returns>
		protected static Warehouse Instance(int space) {
			return new Warehouse(space);
		}
		public virtual bool distributeDrug(IDrug drug, int requiredNumber) {
			var proper = shipments.Where(
				shipment=>shipment.getDrug().Equals(drug)
			).ToList<IShipment<IDrug>>();
			int requiredCopy = requiredNumber;
			while(requiredNumber > 0) {
				if(proper.Count == 0) {
					raiseOnDrugDistribution(drug, requiredNumber, false);
					return false;
				}
				var used = proper.First();
				int dec = used.decreaseVolume(requiredNumber);
				requiredNumber -= dec;
				space += dec;
				proper.Remove(used);
				if(used.getVolume() == 0) shipments.Remove(used);
			}
			raiseOnDrugDistribution(drug, requiredCopy, true);
			return true;
		}
		public virtual double getBalance() {
			double balance = 0;
			foreach(var shipment in shipments) {
				balance += shipment.Cost;
			}
			raiseOnBalanceCount(balance);
			return balance;
		}
		public virtual bool loadShipments(string path) {
			XDocument doc = XDocument.Load(path);
			bool s = true;
			try {
				var list = doc.Root.Elements("Shipment").Select(x=>new {
					code = (string) (x.Element("INN") ?? x.Element("CompoundCode")),
					codeType = x.Element("INN")?.Name ?? "CompoundCode",
					volume = (int) x.Element("Volume"),
					price =(double)x.Element("Price")
				});

				emitShipmentEvent = false;
				foreach(var sh in list) {
					if(sh.code == null) throw new XmlException("Shipment config file contains entity without INN && CompoundCode");

					s = storeShipment(new Shipment<IDrug>(
						(sh.codeType=="INN") ? new UnifiedDescriptor(sh.code) as IDrug
						                     : new CompoundedDrug(sh.code) as IDrug, 
						sh.volume, sh.price)) && s;
				}
				emitShipmentEvent = true;
			}
			catch(ArgumentNullException e) {
				throw new XmlException("Bad-formatted shipment config file", e);
			}
			return s;
		}
		public void saveShipments(string path) {
			XElement all = new XElement("Shipments");
			foreach(var sh in shipments) {
				var drug = sh.getDrug();
				all.Add(new XElement("Shipment", 
					new XElement((drug is AbstractManufacturedDrug) ? "INN" : "CompoundCode", PharmData.Code(drug)),
					new XElement("Volume", sh.getVolume()),
					new XElement("Price", sh.getPrice())
				));
			}
			all.Save(path);
		}
		public virtual bool storeShipment(IShipment<IDrug> shipment) {
			if(space >= shipment.getVolume()) {
				space -= shipment.getVolume();
				shipments.Add(shipment);
				raiseOnShipmentStore(shipment, true);
				return true;
			}
			raiseOnShipmentStore(shipment, false);
			return false;
		}
	}
	/// <summary>
	/// Обычный склад не позволяет хранить медикаменты, требующие охлаждения или защиты 
	/// </summary>
	public class CommonWarehouse : Warehouse {
		public CommonWarehouse(int capacity) : base(capacity) { }
		public override bool storeShipment(IShipment<IDrug> shipment) {
			var drug = shipment.getDrug();
			if(drug.isNarcotic() || drug.requiresFridge()) {
				raiseOnShipmentStore(shipment, false);
				return false;
			}
			else return base.storeShipment(shipment);
		}
	}
	/// <summary>
	/// Склад, содержащий внутренние хранилища для медикаментов. Вместимость внутренних хранилищ 
	/// допустимо изменять, при это общая вместимость обычных помещений и внутренних хранилищ постоянна.
	/// </summary>
	public abstract class SpecialWarehouse: Warehouse, ISpecialWarehouse {
		/// <summary>
		/// Внутреннее хранилище
		/// </summary>
		protected Warehouse specialStore;
		public int SpecialSpace {
			get {
				return specialStore.Space;
			}
			set {
				int diff = value - specialStore.Space;
				specialStore.Space = value;
				if(space < diff) throw new System.ArgumentException(
					"Warehouse space is not sufficient to include new special space"
				);
				space -= diff;
			}
		}
		/// <summary>
		/// Конструктор инициализирует внутреннее хранилище с заданной вместимостью.
		/// </summary>
		/// <param name="space">Вместимость обычных помещений</param>
		/// <param name="specialSpace">Вместимость внутреннего хранилища</param>
		protected SpecialWarehouse(int space, int specialSpace) : base(space){
			specialStore = Warehouse.Instance(specialSpace);
			// redirecting events
			specialStore.OnBalanceCount += raiseOnBalanceCount;
			specialStore.OnDrugDistribution += raiseOnDrugDistribution;
			specialStore.OnShipmentStore += raiseOnShipmentStore;
		}
		public override double getBalance() {
			// Don't emit OnBalanceCount event in getBalance()
			emitBalanceEvent = false;
			var balance = base.getBalance() + specialStore.getBalance();
			emitBalanceEvent = true;
			raiseOnBalanceCount(balance);
			return balance;
		}
	}
	/// <summary>
	/// Склад с холодильниками позволяет хранить препараты в охлаждённом состоянии
	/// </summary>
	public class FridgeWarehouse : SpecialWarehouse {
		public FridgeWarehouse(int space, int fridgeSpace) : base(space, fridgeSpace){}
		public override bool distributeDrug(IDrug drug, int requiredNumber) {
			if(drug.requiresFridge()) {
				return specialStore.distributeDrug(drug, requiredNumber);
			}else {
				return base.distributeDrug(drug, requiredNumber);
			}
		}
		public override bool storeShipment(IShipment<IDrug> shipment) {
			var drug = shipment.getDrug();
			if(drug.isNarcotic()) {
				raiseOnShipmentStore(shipment, false);
				return false;
			}else if(drug.requiresFridge()) {
				return specialStore.storeShipment(shipment);
			}else {
				return base.storeShipment(shipment);
			}
		}
	}
	/// <summary>
	/// Склад с сейфами позволяет хранить наркотические препараты
	/// </summary>
	public class SafeWarehouse : SpecialWarehouse {
		public SafeWarehouse(int space, int safeSpace) : base(space, safeSpace){}
		public override bool distributeDrug(IDrug drug, int requiredNumber) {
			if(drug.isNarcotic()) {
				return specialStore.distributeDrug(drug, requiredNumber);
			}else {
				return base.distributeDrug(drug, requiredNumber);
			}
		}
		public override bool storeShipment(IShipment<IDrug> shipment) {
			var drug = shipment.getDrug();
			if(drug.isNarcotic()) {
				return specialStore.storeShipment(shipment);
			}else if(drug.requiresFridge()) {
				raiseOnShipmentStore(shipment, false);
				return false;
			}else {
				return base.storeShipment(shipment);
			}
		}
	}
	/// <summary>
	/// Склад с комбинированным хранилищем позволяет хранить наркотические препараты в охлаждённом состоянии
	/// </summary>
	public class ComboWarehouse : SpecialWarehouse {
		public ComboWarehouse(int space, int comboSpace) : base(space, comboSpace){}
		public override bool distributeDrug(IDrug drug, int requiredNumber) {
			if(drug.isNarcotic() || drug.requiresFridge()) {
				return specialStore.distributeDrug(drug, requiredNumber);
			}else {
				return base.distributeDrug(drug, requiredNumber);
			}
		}
		public override bool storeShipment(IShipment<IDrug> shipment) {
			var drug = shipment.getDrug();
			if(drug.isNarcotic() || drug.requiresFridge()) {
				return specialStore.storeShipment(shipment);
			}else {
				return base.storeShipment(shipment);
			}
		}
	}
}

