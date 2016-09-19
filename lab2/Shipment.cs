using System;

namespace lab2 {
	/// <summary>
	/// Интерфейс для партий медикаментов
	/// </summary>
	/// <typeparam name="T">Тип медикамента</typeparam>
	public interface IShipment<out T> where T : IDrug{
		/// <summary>
		/// Получить медикамент, поставляемый в партии
		/// </summary>
		/// <returns>Медикамент</returns>
		T getDrug();
		/// <summary>
		/// Изъять часть контейнеров из партии.
		/// </summary>
		/// <param name="dec">Число изымаемых контейнеров. Если оно больше размера партии,
		/// из партии изымаются все контейнеры.</param>
		/// <returns>Число изятых контейнеров</returns>
		int decreaseVolume(int dec);
		/// <summary></summary>
		/// <returns>Размер партии (число контейнеров)</returns>
		int getVolume();
		/// <summary>
		/// Стоимость партии
		/// </summary>
		double Cost {
			get;
		}
	}
	/// <summary>
	/// Партия медикаментов
	/// </summary>
	public class Shipment<T> : IShipment<T> where T : IDrug{
		/// <summary>
		/// Поставляемый медикамент
		/// </summary>
		private T drug;
		/// <summary>
		/// Размер партии
		/// </summary>
		private int volume;
		/// <summary>
		/// Закупочная цена контейнера
		/// </summary>
		private double price; 
		/// <summary>
		/// Стоимость партии: произведение числа контейнеров на закупочную цену
		/// </summary>
		public double Cost {
			get {
				return volume * price;
			}
		}
		/// <summary></summary>
		/// <param name="_drug">Препарат</param>
		/// <param name="_volume">Размер партии</param>
		/// <param name="_price">Закупочная стоимость</param>
		public Shipment(T _drug, int _volume, double _price){
			drug = _drug;
			volume = _volume;
			price = _price;
		}
		public T getDrug() {
			return drug;
		}
		public int decreaseVolume(int dec) {
			dec = Math.Min(dec, volume);
			volume -= dec;
			return dec;
		}
		public int getVolume() {
			return volume;
		}
	}
}