namespace GeometryLib.Aalgorithms
{
	using System;
	using System.Threading.Tasks;
	/// <summary>
	/// быстрая сортировка с разветвлением при рекурсии
	/// После разбиения на массива части выполняется быстрая сортировка 
	/// левого и правого подмассивов в отдельных потоках одновременно
	/// https://makolyte.com/multithreaded-quicksort-in-csharp/
	/// </summary>
	/// <typeparam name="Tp"></typeparam>
	public class ThreadedQuickSort<Tp> where Tp : IComparable<Tp>
	{
		/// <summary>
		/// Метод быстрой сортировки 
		/// </summary>
		/// <param name="arr"></param>
		/// <returns></returns>
		public async Task QuickSort(Tp[] arr)
		{
			await QuickSort(arr, 0, arr.Length - 1);
		}
		private async Task QuickSort(Tp[] arr, int left, int right)
		{
			if (right <= left) 
				return;
			int lt = left;
			int gt = right;
			var pivot = arr[left];
			int i = left + 1;
			while (i <= gt)
			{
				int cmp = arr[i].CompareTo(pivot);
				if (cmp < 0)
					Swap(arr, lt++, i++);
				else if (cmp > 0)
					Swap(arr, i, gt--);
				else
					i++;
			}
			var t1 = Task.Run(() => QuickSort(arr, left, lt - 1));
			var t2 = Task.Run(() => QuickSort(arr, gt + 1, right));
			await Task.WhenAll(t1, t2).ConfigureAwait(false);
		}
		private void Swap(Tp[] a, int i, int j)
		{
			var swap = a[i];
			a[i] = a[j];
			a[j] = swap;
		}
	}
}
