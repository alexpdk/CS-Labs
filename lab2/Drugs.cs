using System;
using System.Collections;
using System.Collections.Generic;

namespace lab2 { 
	/// <summary>
	/// Интерфейс для медикаментов
	/// </summary>
	public interface IDrug : ICloneable, IEquatable<IDrug>{
		/// <returns>Является ли медикамент наркотическим</returns>
		bool isNarcotic();
		/// <returns>Требует ли медикамент хранения в охлаждённом состоянии</returns>
		bool requiresFridge();
	}
	/// <summary>
	/// Интерфейс для медикаментов фабричного производства, учитываемых по МНН
	/// </summary>
	public interface IManufacturedDrug : IDrug{
		/// <summary>
		/// Доступный для чтения МНН медикамента
		/// </summary>
		string INN {
			get;
		}
	}
	/// <summary>
	/// Интерфейс для экстемпоральных медикаментов, учитываемых по коду ингредиентов.
	/// </summary>
	public interface ICompoundedDrug : IDrug {
		/// <summary>
		/// Доступный для чтения код ингредиентов
		/// </summary>
		string CompoundCode {
			get;
		}
		/// <returns>Сертифицировано ли производство препарата</returns>
		bool isSertified();
	}
	/// <summary>
	/// Экстемпоральный медикамент
	/// </summary>
	public class CompoundedDrug : ICompoundedDrug {
		/// <summary>
		/// Входят ли в состав наркотики
		/// </summary>
		bool has_narcotic;
		/// <summary>
		/// Требует ли хранения в охлаждённом состоянии
		/// </summary>
		bool keep_cold;
		/// <summary>
		/// Сертифицирован ли препарат
		/// </summary>
		bool sertified;
		/// <summary>
		/// Код ингредиентов
		/// </summary>
		string code;

		public string CompoundCode {
			get {
				return code;
			}
		}
		public CompoundedDrug(ICollection<IManufacturedDrug> ingredients, bool sertified = false) {
			this.sertified = sertified;
			code = "";
			foreach(var drug in ingredients) {
				code += drug.INN+'.';
				if(drug.isNarcotic()) has_narcotic = true;
				if(drug.requiresFridge()) keep_cold = true;
			}
		}
		public object Clone() {
			return MemberwiseClone();
		}
		public bool Equals(IDrug other) {
			var cd = other as ICompoundedDrug;
			return (cd != null) && (code == cd.CompoundCode);
		}
		public override bool Equals(object obj) {
			IDrug drug = obj as IDrug;
			return (drug != null) && Equals(drug);
		}
		public override int GetHashCode() {
			return code.GetHashCode();
		}
		public bool isNarcotic() {
			return has_narcotic;
		}
		public bool isSertified() {
			return sertified;
		}
		public bool requiresFridge() {
			return keep_cold;
		}
	}

	/// <summary>
	/// Описание препарата, позволяющее получать его свойства при известном МНН. Не определяет
	/// способ идентификации препарата.
	/// </summary>
	public abstract class DrugDescriptor : IManufacturedDrug {
		public abstract string INN {
			get;
		}
		public object Clone() {
			return MemberwiseClone();
		}
		public bool Equals(IDrug other) {
			var md = other as IManufacturedDrug;
			return (md != null) && (INN == md.INN); 
		}
		public override bool Equals(object obj) {
			var drug = obj as IDrug;
			return (drug != null) && Equals(drug); 
		}
		public override int GetHashCode() {
			return INN.GetHashCode();
		}
		public bool isNarcotic() {
			return PharmData.conditions[INN] % PharmData.NARCOTIC == 1;
		}
		public bool requiresFridge() {
			return PharmData.conditions[INN] % PharmData.KEEP_COLD == 1;
		}
	}
	/// <summary>
	/// Унифицированное описание - описание препарата посредством МНН.
	/// </summary>
	public class UnifiedDescriptor : DrugDescriptor {
		private string inn;
		public override string INN {
			get {
				return inn;
			}
		}
		public UnifiedDescriptor(string _inn){
			inn = _inn;
		}
		public override string ToString() {
			return inn;
		}
	}
	/// <summary>
	/// Неунифицированное описание препарата(через товарное или химическое наименование)
	/// </summary>
	public abstract class NonUnifiedDescriptor : DrugDescriptor {
		/// <summary>
		/// Ссылка на описание посредством МНН
		/// </summary>
		private UnifiedDescriptor unified;
		public override string INN{
			get{
				return unified.INN;
			}
		}
		/// <summary>
		/// Коструктор формирует внутреннюю ссылку на описание 
		/// </summary>
		/// <param name="_inn">МНН</param>
		protected NonUnifiedDescriptor(string _inn){
			unified = new UnifiedDescriptor(_inn);
		}
	}
	/// <summary>
	/// Описание препарата посредством товарного имени и производителя
	/// </summary>
	public class TrademarkDescriptor : NonUnifiedDescriptor {
		private string trademark;
		private string company; 
		public TrademarkDescriptor(string _trademark, string _company, string inn) : base(inn) {
			trademark = _trademark;
			company = _company;
		}
		public override string ToString() {
			return string.Format("{0}: {1}({2})",company,trademark,INN);
		}
	}
	/// <summary>
	///  Описание препарата посредством научного наименования действующего вещества
	/// </summary>
	public class ChemicalDescriptor : NonUnifiedDescriptor {
		private string chemName;

		public ChemicalDescriptor(string name, string inn) : base(inn) {
			chemName = name;
		}
		public override string ToString() {
			return string.Format("{0}({1})",chemName,INN);
		}
	}
	/// <summary>
	/// Фармакологические данные об используемых медикаментах
	/// </summary>
	public class PharmData {
		/// <summary>
		/// Коды условий хранения
		/// </summary>
		public const int USUAL_CONDITIONS = 0;
		public const int NARCOTIC = 1;
		public const int KEEP_COLD = 2;

		/// <summary>
		/// Условия хранения препаратов
		/// </summary>
		public static Dictionary<string, int> conditions = new Dictionary<string, int> {
			{"pazopanib", KEEP_COLD},
			{"atenolol", USUAL_CONDITIONS},
			{"procaine", NARCOTIC},
			{"cocaine", NARCOTIC},
			{"Certolizumab pegol", USUAL_CONDITIONS}
		};
		/// <summary>
		/// Список МНН закупаемых медикаментов
		/// </summary>
		public static List<string> INNList = new List<string>(conditions.Keys);
	}
}
