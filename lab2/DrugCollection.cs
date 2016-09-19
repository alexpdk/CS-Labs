﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace lab2 {
	/// <summary>
	/// Обобщённая коллекция, позволяет хранить списки медикаментов без дупликатов.
	/// </summary>
	/// <typeparam name="T">Тип медикаментов</typeparam>
	public class DrugCollection<T> : ICollection<T> where T : IDrug{
		private List<T> list;
		private bool read_only;
		public int Count {
			get {
				return list.Count;
			}
		}
		/// <summary>
		/// Выделить среди элементов коллекции элементы производного типа.
		/// </summary>
		/// <typeparam name="T2">Производный тип</typeparam>
		/// <returns>Коллекцию производного типа</returns>
		public DrugCollection<T2> Downcast<T2>() where T2 : T {
			List<T2> l2 = new List<T2>();
			foreach(var item in list) {
				if(item is T2) l2.Add((T2)item);
			}
			return new DrugCollection<T2>(l2);
		}
		public bool IsReadOnly {
			get {
				return read_only;
			}
		}
		public DrugCollection(List<T> _list){
			list = _list;
		}
		public void Add(T drug) {
			// TODO Check, whether can be simplified with list.Contains
			if(!list.Exists(d=>d.Equals(drug))) {
				list.Add(drug);
			}
		}
		public void Clear() {
			list.Clear();
		}
		public bool Contains(T drug){
			return list.Exists(d=>d.Equals(drug));
		}
		public void CopyTo(T[] array, int index) {
			list.CopyTo(array, index);
		}
		public IEnumerator<T> GetEnumerator() {
			return new DrugEnumerator<T>(this);
		}
		IEnumerator IEnumerable.GetEnumerator() {
			return new DrugEnumerator<T>(this);
		}
		public bool Remove(T drug) {
			int index = list.FindIndex(d=>d.Equals(drug));
			if(index >= 0) list.RemoveAt(index);
			return index >= 0;
		}
		public T this[int index] {
			get {
				return list[index];
			}
			set {
				list[index] = value;
			}
		}
	}
	public class DrugEnumerator<T> : IEnumerator<T> where T : IDrug {
		private DrugCollection<T> collection;
		private int index;
		public T Current {
			get {
				return collection[index];
			}
		}
		object IEnumerator.Current {
			get {
				return collection[index];
			}
		}
		public DrugEnumerator(DrugCollection<T> col) {
			collection = col;
			index = -1;
		}
		public void Dispose() {
			collection = null;
		}
		public bool MoveNext() {
			index++;
			return index < collection.Count;
		}
		public void Reset() {
			index = -1;
		}
	}
}