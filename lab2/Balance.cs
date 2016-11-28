using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DrugAccount {
	/// <summary>
	/// Интерфейс для операции проверки баланса.
	/// </summary>
	/// <typeparam name="T">Структура, обладающая балансом</typeparam>
	public interface IBalance<in T> {
		/// <summary>
		/// Проверить баланс.
		/// </summary>
		/// <param name="expected">Ожидаемое состояние баланса</param>
		/// <returns>Пройдена ли проверка</returns>
		bool checkBalance(double expected);
	}
	/// <summary>
	/// Проверка складского баланса.
	/// </summary>
	/// <typeparam name="T">Тип склада</typeparam>
	public class WarehouseBalance<T> : IBalance<T> where T : IWarehouse {
		/// <summary>
		/// Проверяемый склад.
		/// </summary>
		private T warehouse;

		public bool checkBalance(double expected) {
			return Math.Abs(warehouse.getBalance() - expected) < 0.0001;
		}
		public WarehouseBalance(T _warehouse) {
			warehouse = _warehouse;
		}
	}
}
