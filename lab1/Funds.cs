using System;
using System.Collections.Generic;

namespace lab1 {
	/// <summary>
	/// Интерфейс для источников финансирования
	/// </summary>
	public interface IFund
	{
		/// <param name="drug">Тип медикамента</param>
		/// <param name="department">Отделение, которому будет передан медикамент</param>
		/// <returns>Отпустит ли источник финансирования средства 
		/// на закупку медикаментов для отдела</returns>
		bool approveFunding(IDrug drug, IDepartment department);
		/// <summary>
		/// Выделить средства на закупку партии медикаментов для отдела.
		/// </summary>
		/// <param name="shipment">Партия</param>
		/// <param name="department">Отдел</param>
		/// <returns>Выделены ли средства</returns>
		bool provideFunding(IShipment shipment, IDepartment department);
	}
	/// <summary>
	/// Источник общего назначения отпускает средства на любые цели
	/// </summary>
	public class Fund : IFund {
		/// <summary>
		/// Баланс на счёте источника
		/// </summary>
		double balance;
		public Fund(double _balance) {
			balance = _balance;
		}
		public virtual bool approveFunding(IDrug drug, IDepartment department) {
			return true;
		}
		public virtual bool provideFunding(IShipment shipment, IDepartment department) {
			if(approveFunding(shipment.getDrug(), department)) {
				if(balance >= shipment.Cost) {
					balance -= shipment.Cost;
					return true;
				}
			}
			return false;
		}
	}
	/// <summary>
	/// Бюджетный источник отпускает средства только на поставки бюджетным отеделениям
	/// </summary>
	public class BudgetaryFund : Fund {
		public BudgetaryFund(double balance) : base(balance) { }
		public override bool approveFunding(IDrug drug, IDepartment department) {
			return department.isBudgetary();
		}
	}
	/// <summary>
	/// Целевой источник выделяет средства на медикаменты из конкретного списка
	/// </summary>
	public class TargetedFund : Fund {
		/// <summary>
		/// Список оплачиваемых медикаментов
		/// </summary>
		private List<IDrug> targets;
		public TargetedFund(List<IDrug> _targets, double balance) : base(balance) {
			targets = _targets;
		} 
		public override bool approveFunding(IDrug drug, IDepartment department) {
			return targets.Exists(d=>d.INN == drug.INN);
		}
	}
	/// <summary>
	/// Фонд отделения выделяет средства на закупки для конкретного отделения
	/// </summary>
	public class DepartmentFund : Fund {
		/// <summary>
		/// Финансируемое отделение
		/// </summary>
		private IDepartment department;
		public DepartmentFund(IDepartment _department, double balance) : base(balance) {
			department = _department;
		}
		public override bool approveFunding(IDrug drug, IDepartment _department) {
			return department == _department;
		}
	}
}



