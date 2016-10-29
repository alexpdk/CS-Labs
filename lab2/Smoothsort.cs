using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace lab2 {
	/// <summary>
	/// Обобщённый алгоритм плавной сортировки
	/// </summary>
	/// <typeparam name="T">Тип сортируемых данных</typeparam>
	class Smoothsort<T>{

		/// <summary>
		/// Метод сравнения сортируемых значений
		/// </summary>
		private Comparison<T> comp;
		/// <summary>
		/// Сортируемый список
		/// </summary>
		private List<T> list;
		/// <summary>
		/// Список чисел Леонардо от 1 до первого числа, превышающего размер сортируемого списка
		/// </summary>
		private List<int> leos;
		/// <summary>
		/// Номера чисел Леонардо, соответствующих размерам куч в последовательности
		/// </summary>
		private List<int> sizes;
		/// <summary>
		/// Флаг отладочной печати процесса сортировки
		/// </summary>
		private bool print;
		/// <summary>
		/// Завершена ли сортировка
		/// </summary>
		private bool completed = false;

		public List<T> SortedList {
			get { return completed ? list : null;}
		}
		public Smoothsort(Comparison<T> comp, List<T> list, bool print=false) {
			this.comp = comp;
			this.list = list.Select(v=>v).ToList<T>();
			this.print = print;
			leos = new List<int>();
			sizes = new List<int>();
		}
		/// <summary>
		/// Обеспечить для текущей кучи выполнение свойств последовательности куч: корень
		/// кучи больше или равен корню предыдущей кучи, а также левому и правому потомкам.
		/// </summary>
		/// <param name="rootIndex">Индекс корня текущей кучи</param>
		/// <param name="sizeIndex">Индекс элемента списка размеров, позволяющего узнать размер
		/// текущей кучи</param>
		private void ensureSequence(int rootIndex, int sizeIndex) {
			// skip root comparison, if we already at last root
			bool toSort = true;
			while(toSort && sizeIndex > 0) {
				int sizeKey = sizes[sizeIndex];
				int prevRootIndex = rootIndex - leos[sizeKey];
				//  element is heap root
				if(sizeKey >= 2) {
					int lCIndex = rootIndex-leos[sizeKey-2]-1;
					int rCIndex = rootIndex-1;
					if(valMore(prevRootIndex, rootIndex) && valNotLess(prevRootIndex, lCIndex)
					&& valNotLess(prevRootIndex, rCIndex)) {
						valSwap(prevRootIndex, rootIndex);
						rootIndex = prevRootIndex;
						sizeIndex--;
					}
					else toSort = false;
				// element is standalone
				}else if(valMore(prevRootIndex, rootIndex)) {
					valSwap(prevRootIndex, rootIndex);
					rootIndex = prevRootIndex;
					sizeIndex--;
				}else toSort = false;
			}
			shiftDown(rootIndex, sizes[sizeIndex]);
		}
		/// <summary>
		/// Сгенерировать числа Леонардо, используемые при сортировке (до первого числа, 
		/// превосходящего размер сортируемого списка, включительно).
		/// </summary>
		private void genLeonardo() {
			int l1 = 1;
			int l2 = 1;
			leos.Add(1);
			leos.Add(1);
			while(l2 < list.Count) {
				l2 = l2+l1+1;
				l1 = l2-l1-1;
				leos.Add(l2);
			}
			if(print) {
				Console.Write("Leonardo numbers: ");
				foreach(var val in leos) {
					Console.Write(" {0}", val);
				}
				Console.WriteLine();
			}
		}
		/// <summary>
		/// Включить элемент в последовательность куч
		/// </summary>
		/// <param name="listIndex">Номер элемента в сортируемом списке</param>
		private void insertElem(int listIndex) {
			int len = sizes.Count;
			if(len >= 2 && sizes[len-2] == sizes[len-1]+1){
				int newKey = sizes[len-2]+1;
				sizes.RemoveAt(len-1);
				sizes.RemoveAt(len-2);
				sizes.Add(newKey);
			}else {
				sizes.Add((len > 0 && sizes[len-1] == 1) ? 0 : 1);
			}
			ensureSequence(listIndex, sizes.Count-1);
			if(print) printList(listIndex);
		}
		/// <summary>
		/// Форматированная печать последовательности куч
		/// </summary>
		/// <param name="maxIndex">Номер в сортируемом списке последнего элемента,
		/// принадлежащего последовательности куч</param>
		private void printList(int maxIndex) {
			if(sizes.Count == 0) return;
			int sIndex = 0;
			int sCounter = leos[sizes[sIndex]];
			for(int i=0; i<=maxIndex; i++) {
				Console.Write(" {0}", list[i]);
				sCounter --;
				if(sCounter == 0){
					Console.Write(" |");
					sIndex++;
					if(sIndex < sizes.Count) sCounter=leos[sizes[sIndex]];
				}
			}
			Console.WriteLine();
		}
		/// <summary>
		/// Удалить максимальный элемент из последовательности куч, не нарушая свойств
		/// последовательности
		/// </summary>
		/// <param name="listIndex">Номер в сортируемом списке последнего элемента,
		/// принадлежащего последовательности куч</param>
		/// <returns>Удалённый элемент</returns>
		private T removeElem(int listIndex) {
			int len = sizes.Count;
			int sizeKey = sizes[len-1];
			sizes.RemoveAt(len-1);
			if(sizeKey >= 2) {
				int lCIndex = listIndex-leos[sizeKey-2]-1;
				int rCIndex = listIndex-1;

				sizes.Add(sizeKey-1);
				ensureSequence(lCIndex, len-1);

				sizes.Add(sizeKey-2);
				ensureSequence(rCIndex, len);
			}
			if(print) printList(listIndex);
			
			return list[listIndex];
		}
		/// <summary>
		/// Просеить элемент в куче Леонардо до достижения позиции, пока он не будет меньше
		/// своих потомков.
		/// </summary>
		/// <param name="rootIndex">Индекс просеиваемого элемента</param>
		/// <param name="sizeKey">Индекс числа Леонардо, соответствующего размеру кучи,
		/// корнем которой служит просеиваемый элемент</param>
		private void shiftDown(int rootIndex, int sizeKey) {
			// equals to heap size > 1
			if(sizeKey >= 2) {
				// indices of left && right children roots
				int lCIndex = rootIndex-leos[sizeKey-2]-1;
				int rCIndex = rootIndex-1;
				if(valMore(lCIndex, rootIndex) && valNotLess(lCIndex, rCIndex)) {
					valSwap(lCIndex, rootIndex);
					shiftDown(lCIndex, sizeKey-1);
				}
				else if(valMore(rCIndex, rootIndex) && valNotLess(rCIndex, lCIndex)) {
					valSwap(rCIndex, rootIndex);
					shiftDown(rCIndex, sizeKey-2);
				}
			}
		}
		/// <summary>
		/// Выполнить плавную сортировку списка по возрастанию
		/// </summary>
		public Task Sort(IProgress<int> progress=null) {
			return Task.Run(()=>{
				genLeonardo();
				int step = list.Count / 50;
				if(step == 0 && progress!=null) {
					//Console.WriteLine("Smootsort.Sort: Too small list to report progress");
					//Debug.WriteLine("Smootsort.Sort: Too small list to report progress");
					progress = null;
				}
				var repI = 0;
				for(int i=0; i<list.Count; i++) {
					insertElem(i);
					if(progress != null && i/step != repI) {
						repI = i / step;
						progress.Report(repI);
					}
				}
				for(int i=list.Count-1, j=0; i>=0; i--, j++) {
					removeElem(i);
					if(progress!=null && j/step+50!=repI) {
						repI = j/step+50;
						progress.Report(repI);
					}
				}
				completed = true;
			});
		}
		/// <returns>Превосходит ли элемент с индексом i1 элемент с индексом i2</returns>
		private bool valMore(int i1, int i2) {
			return comp(list[i1], list[i2])==1;
		}
		/// <returns>Не уступает ли элемент с индексом i1 элементу с индексом i2</returns>
		private bool valNotLess(int i1, int i2) {
			return comp(list[i1], list[i2])>=0;
		}
		/// <summary>
		/// Поменять в списке элементы с заданными индексами
		/// </summary>
		private void valSwap(int i1, int i2) {
			T tmp = list[i1];
			list[i1] = list[i2];
			list[i2] = tmp;
		}
	}
}
