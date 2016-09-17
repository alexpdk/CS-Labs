using System;
using System.Collections.Generic;
using System.Linq;

namespace lab1 {
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
		/// Загрузить партию медикаментов на склад. Если на складе отстуствуют необходимые 
		/// для партии условия хранения или недостаточно место, партия на складе не размещается.
		/// </summary>
		/// <param name="shipment">Партия медикаментов</param>
		/// <returns>Удалось ли успешно разместить партию</returns>
		bool storeShipment(IShipment shipment);
		/// <summary>
		/// Отпустить требуемое число медикаментов со склада. 
		/// </summary>
		/// <param name="_inn">МНН медикамента</param>
		/// <param name="requiredNumber">Требуемое число</param>
		/// <returns>Были ли медикаменты успешно выданы</returns>
		bool distributeDrug(string _inn, int requiredNumber);
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
		private List<IShipment> shipments;
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
					"New space not allowes to store all shipments", "newSpace"
				);
				space = value;
			}
		}
		/// <summary>
		/// Область видимости конструктора ограничена, т.к. класс не пригоден для самостоятельного 
		/// использования (отсутствуют проверки на условия хранения медикаментов).
		/// </summary>
		/// <param name="_space">Начальная вместимость</param>
		protected Warehouse(int _space) {
			space = _space;
			shipments = new List<IShipment>();
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
		public virtual bool storeShipment(IShipment shipment) {
			if(space >= shipment.getVolume()) {
				space -= shipment.getVolume();
				shipments.Add(shipment);
				return true;
			}
			else return false;
		}
		public virtual bool distributeDrug(string _inn, int requiredNumber) {
			var proper = (List<IShipment>)shipments.Where(shipment=>shipment.getDrug().INN == _inn);
			while(requiredNumber > 0) {
				if(proper.Count == 0) return false;

				IShipment used = proper.First();
				int dec = used.decreaseVolume(requiredNumber);
				requiredNumber -= dec;
				space += dec;
				proper.Remove(used);
				if(used.getVolume() == 0) shipments.Remove(used);
			}
			return true;
		}
	}
	/// <summary>
	/// Обычный склад не позволяет хранить медикаменты, требующие охлаждения или защиты 
	/// </summary>
	public class CommonWarehouse : Warehouse {
		public CommonWarehouse(int capacity) : base(capacity) { }
		public override bool storeShipment(IShipment shipment) {
			var drug = shipment.getDrug();
			if(drug.isNarcotic() || drug.requiresFridge()) {
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
		}
	}
	/// <summary>
	/// Склад с холодильниками позволяет хранить препараты в охлаждённом состоянии
	/// </summary>
	public class FridgeWarehouse : SpecialWarehouse {
		public FridgeWarehouse(int space, int fridgeSpace) : base(space, fridgeSpace){}
		public override bool storeShipment(IShipment shipment) {
			var drug = shipment.getDrug();
			if(drug.isNarcotic()) {
				return false;
			}else if(drug.requiresFridge()) {
				return specialStore.storeShipment(shipment);
			}else {
				return base.storeShipment(shipment);
			}
		}
		public override bool distributeDrug(string _inn, int requiredNumber) {
			var drug = new UnifiedDescriptor(_inn);
			if(drug.requiresFridge()) {
				return specialStore.distributeDrug(_inn, requiredNumber);
			}else {
				return base.distributeDrug(_inn, requiredNumber);
			}
		}
	}
	/// <summary>
	/// Склад с сейфами позволяет хранить наркотические препараты
	/// </summary>
	public class SafeWarehouse : SpecialWarehouse {
		public SafeWarehouse(int space, int safeSpace) : base(space, safeSpace){}
		public override bool storeShipment(IShipment shipment) {
			var drug = shipment.getDrug();
			if(drug.isNarcotic()) {
				return specialStore.storeShipment(shipment);
			}else if(drug.requiresFridge()) {
				return false;
			}else {
				return base.storeShipment(shipment);
			}
		}
		public override bool distributeDrug(string _inn, int requiredNumber) {
			var drug = new UnifiedDescriptor(_inn);
			if(drug.isNarcotic()) {
				return specialStore.distributeDrug(_inn, requiredNumber);
			}else {
				return base.distributeDrug(_inn, requiredNumber);
			}
		}
	}
	/// <summary>
	/// Склад с комбинированным хранилищем позволяет хранить наркотические препараты в охлаждённом состоянии
	/// </summary>
	public class ComboWarehouse : SpecialWarehouse {
		public ComboWarehouse(int space, int comboSpace) : base(space, comboSpace){}
		public override bool storeShipment(IShipment shipment) {
			var drug = shipment.getDrug();
			if(drug.isNarcotic() || drug.requiresFridge()) {
				return specialStore.storeShipment(shipment);
			}else {
				return base.storeShipment(shipment);
			}
		}
		public override bool distributeDrug(string _inn, int requiredNumber) {
			var drug = new UnifiedDescriptor(_inn);
			if(drug.isNarcotic() || drug.requiresFridge()) {
				return specialStore.distributeDrug(_inn, requiredNumber);
			}else {
				return base.distributeDrug(_inn, requiredNumber);
			}
		}
	}
}

