using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace lab2 { 
	/// <summary>
	/// Интерфейс для медикаментов
	/// </summary>
	public interface IDrug : ICloneable, IEquatable<IDrug> {

		/// <returns>Является ли медикамент наркотическим</returns>
		bool isNarcotic();
		/// <returns>Требует ли медикамент хранения в охлаждённом состоянии</returns>
		bool requiresFridge();
	}
	[Serializable]
	public abstract class Drug: IDrug {
		public abstract object Clone();
		public abstract bool Equals(IDrug other);
		public abstract bool isNarcotic();
		public abstract bool requiresFridge();
	}
	/// <summary>
	/// Интерфейс для медикаментов фабричного производства, учитываемых по МНН
	/// </summary>
	[Serializable]
	public abstract class AbstractManufacturedDrug : Drug{
		/// <summary>
		/// Доступный для чтения МНН медикамента
		/// </summary>
		public abstract string INN {
			get; set; 
		}
	}
	/// <summary>
	/// Интерфейс для экстемпоральных медикаментов, учитываемых по коду ингредиентов.
	/// </summary>
	[Serializable]
	public abstract class AbstractCompoundedDrug : Drug {
		/// <summary>
		/// Доступный для чтения код ингредиентов
		/// </summary>
		public abstract string CompoundCode {
			get; set;
		}
		/// <returns>Сертифицировано ли производство препарата</returns>
		public abstract bool isSertified();
	}
	/// <summary>
	/// Экстемпоральный медикамент
	/// </summary>
	[Serializable]
	public class CompoundedDrug : AbstractCompoundedDrug {
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
		public bool sertified;
		/// <summary>
		/// Код ингредиентов
		/// </summary>
		string code;

		public override string CompoundCode {
			get {
				return code;
			}
			set {
				code = value;
				var ings = code.Split('.').Select(inn=>new UnifiedDescriptor(inn)).ToList<AbstractManufacturedDrug>();
				defineProperties(ings);
			}
		}
		private CompoundedDrug() { }
		public CompoundedDrug(ICollection<AbstractManufacturedDrug> ingredients, bool sertified = false) {
			this.sertified = sertified;
			code = string.Join(".",ingredients.Select(drug=>drug.INN));
			defineProperties(ingredients);
		}
		[JsonConstructor]
		public CompoundedDrug(string CompoundCode, bool sertified=false) {
			this.CompoundCode = CompoundCode;
			this.sertified = sertified;
		}
		override public object Clone() {
			return MemberwiseClone();
		}
		private void defineProperties(ICollection<AbstractManufacturedDrug> ingredients) {
			foreach(var drug in ingredients) {
				if(drug.isNarcotic()) has_narcotic = true;
				if(drug.requiresFridge()) keep_cold = true;
			}
		}
		override public bool Equals(IDrug other) {
			var cd = other as AbstractCompoundedDrug;
			return (cd != null) && (code == cd.CompoundCode);
		}
		public override bool Equals(object obj) {
			IDrug drug = obj as IDrug;
			return (drug != null) && Equals(drug);
		}
		public override int GetHashCode() {
			return code.GetHashCode();
		}
		override public bool isNarcotic() {
			return has_narcotic;
		}
		override public bool isSertified() {
			return sertified;
		}
		override public bool requiresFridge() {
			return keep_cold;
		} 
		public override string ToString() {
			return code;
			//return "Compound:"+code.Replace('.','-');
		}
	}

	/// <summary>
	/// Описание препарата, позволяющее получать его свойства при известном МНН. Не определяет
	/// способ идентификации препарата.
	/// </summary>
	[Serializable]
	public abstract class DrugDescriptor : AbstractManufacturedDrug {

		override public object Clone() {
			return MemberwiseClone();
		}
		override public bool Equals(IDrug other) {
			var md = other as AbstractManufacturedDrug;
			return (md != null) && (INN == md.INN); 
		}
		public override bool Equals(object obj) {
			var drug = obj as IDrug;
			return (drug != null) && Equals(drug); 
		}
		public override int GetHashCode() {
			return INN.GetHashCode();
		}
		override public bool isNarcotic() {
			return (PharmData.conditions[INN] & PharmData.NARCOTIC) > 0;
		}
		override public bool requiresFridge() {
			return (PharmData.conditions[INN] & PharmData.KEEP_COLD) > 0;
		}
	}
	/// <summary>
	/// Унифицированное описание - описание препарата посредством МНН.
	/// </summary>
	/// 
	[Serializable]
	public class UnifiedDescriptor : DrugDescriptor {
		private string inn;
		public override string INN {
			get {return inn;}
			set { inn = value;}
		}
		private UnifiedDescriptor() { }
		[JsonConstructor]
		public UnifiedDescriptor(string INN){
			inn = INN;
			if( !PharmData.conditions.ContainsKey(inn)) throw new DrugAccountException("Unknown INN: {0}", inn);
		}
		public override string ToString() {
			return inn;
		}
	}
	/// <summary>
	/// Неунифицированное описание препарата(через товарное или химическое наименование)
	/// </summary>
	[Serializable]
	public abstract class NonUnifiedDescriptor : DrugDescriptor {
		/// <summary>
		/// Ссылка на описание посредством МНН
		/// </summary>
		private UnifiedDescriptor unified;
		public override string INN{
			get{return unified.INN;}
			set {unified.INN = value; }
		}
		protected NonUnifiedDescriptor() {
			unified = new UnifiedDescriptor("atenolol");
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
	[Serializable]
	public class TrademarkDescriptor : NonUnifiedDescriptor {
		public string trademark;
		public string company; 
		private TrademarkDescriptor():base() { }
		[JsonConstructor]
		public TrademarkDescriptor(string trademark, string company, string INN) : base(INN) {
			this.trademark = trademark;
			this.company = company;
		}
		public static List<IDrug> GenerateIndexedList(int N, string tr_base, string company, string inn) {
			var list = new List<IDrug>();
			for(int i=0; i<N; i++) list.Add(new TrademarkDescriptor(
				tr_base+i, company, inn));
			return list;
		}
		public override string ToString() {
			return string.Format("{0}: {1}({2})",company,trademark,INN);
		}
	}
	/// <summary>
	///  Описание препарата посредством научного наименования действующего вещества
	/// </summary>
	[Serializable]
	public class ChemicalDescriptor : NonUnifiedDescriptor {
		public string chemName;
		private ChemicalDescriptor() { }
		[JsonConstructor]
		public ChemicalDescriptor(string chemName, string INN) : base(INN) {
			this.chemName = chemName;
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
			{"lidocaine", USUAL_CONDITIONS},
			{"Certolizumab pegol", USUAL_CONDITIONS},
			{"doxorubicin", USUAL_CONDITIONS},
			{"insulin", KEEP_COLD }
		};
		/// <summary>
		/// Список МНН закупаемых медикаментов
		/// </summary>
		public static List<string> INNList = new List<string>(conditions.Keys);

		public static string Code(IDrug drug) {
			return (drug is AbstractManufacturedDrug) ? (drug as AbstractManufacturedDrug).INN 
			       : (drug as AbstractCompoundedDrug).CompoundCode;
		}
	}
}
