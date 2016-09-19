using System;
using System.Collections.Generic;

namespace lab2 {
	/// <summary>
	/// Интерфейс для отделов медучреждения
	/// </summary>
	public interface IDepartment {
		/// <returns>Требуемое количество контейнеров с медикаментом</returns>
		int getRequiredNumber(IDrug drug);
		/// <returns>Является ли отдел бюджетным</returns>
		bool isBudgetary();
	}
	/// <summary>
	/// Отдел медучреждения
	/// </summary>
	class Department : IDepartment{
		public string name;
		public bool budgetary;
		/// <summary>
		/// Потребность отдела в медикаментах
		/// </summary>
		public Dictionary<string, int> needs;

		public Department(string _name, bool budg = true) {
			name = _name;
			budgetary = budg;
			needs = new Dictionary<string, int>();

			Random rnd = new Random();
			int needsSize = rnd.Next(1, 6);
			for(int i=0; i<needsSize; i++) {
				int j = rnd.Next(0, PharmData.INNList.Count);
				needs[PharmData.INNList[j] ] = rnd.Next(1, 100);
			}
		}
		public int getRequiredNumber(IDrug drug) {
			var key = (drug is IManufacturedDrug) ? (drug as IManufacturedDrug).INN
				: (drug as ICompoundedDrug).CompoundCode;
			if(needs.ContainsKey(key)) {
				return needs[key];
			}
			else return 0;
		}
		public bool isBudgetary() {
			return budgetary;
		}
	}
}
