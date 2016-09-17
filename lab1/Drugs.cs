using System;
using System.Collections;
using System.Collections.Generic;

namespace lab1 {
	/// <summary>
	/// Интерфейс для медикаментов
	/// </summary>
	public interface IDrug {
		/// <summary>
		/// Свойство, позволяющее получить МНН медикамента
		/// </summary>
		string INN {
			get;
		}
		/// <returns>Является ли медикамент наркотическим</returns>
		bool isNarcotic();
		/// <returns>Требует ли медикамент хранения в охлаждённом состоянии</returns>
		bool requiresFridge();
	}
	/// <summary>
	/// Описание препарата, позволяющее получать его свойства при известном МНН. Не определяет
	/// способ идентификации препарата.
	/// </summary>
	public abstract class DrugDescriptor : IDrug {
		public abstract string INN {
			get;
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
		//
		TrademarkDescriptor(string _trademark, string _company, string inn) : base(inn) {
			trademark = _trademark;
			company = _company;
		}
	}
	/// <summary>
	///  Описание препарата посредством научного наименования действующего вещества
	/// </summary>
	public class ChemicalDescriptor : NonUnifiedDescriptor {
		private string chemName;

		ChemicalDescriptor(string name, string inn) : base(inn) {
			chemName = name;
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
		};
		/// <summary>
		/// Список МНН закупаемых медикаментов
		/// </summary>
		public static List<string> INNList = new List<string>(conditions.Keys);
	}
}
