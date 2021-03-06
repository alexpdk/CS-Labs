﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace DrugAccount {
	/// <summary>
	/// Интерфейс для логгирования событий склада
	/// </summary>
	/// <typeparam name="T">Тип склада</typeparam>
	public interface IWarehouseLogger<in T> where T : IWarehouse {
		/// <summary>
		/// Событие, на которое должны подписываться методы логирования
		/// </summary>
		event Action<TextWriter, WarehouseEventArgs> OnLog;
	}
	/// <summary>
	/// Класс для логирования событий склада
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class WarehouseLogger<T> : IWarehouseLogger<T> where T : IWarehouse {
		public event Action<TextWriter, WarehouseEventArgs> OnLog;
		protected T warehouse;
		/// <summary>
		/// Объект, используемый для записи сообщений
		/// </summary>
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
	public class StringWarehouseLogger<T>: WarehouseLogger<T> where T: IWarehouse {
		private StringBuilder SB = new StringBuilder();

		public StringWarehouseLogger(T warehouse):base(warehouse) {
			writer = new StringWriter(SB);
		}
		/// <returns>Строку с результатами логирования</returns>
		public String getLog() {
			return SB.ToString();
		}
	}
}
