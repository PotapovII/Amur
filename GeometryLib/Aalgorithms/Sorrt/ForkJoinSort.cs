namespace GeometryLib.Aalgorithms
{
	using System;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Collections.Generic;
	/// <summary>
	/// Метод сортировки разделяй и властвуй
	/// https://makolyte.com/multithreaded-quicksort-in-csharp/
	/// </summary>
	/// <typeparam name="Tp"></typeparam>
	public class ForkJoinSort<Tp> where Tp : IComparable<Tp>
	{
		/// <summary>
		/// Метод сортировки разделяй и властвуй
		/// </summary>
		/// <param name="a"></param>
		/// <returns></returns>
		public async Task Sort(Tp[] a)
		{
			var arrs = Divide(a);
			List<Task> tasks = new List<Task>();
			foreach (var arr in arrs)
			{
				var tmp = arr;
				tasks.Add(Task.Run(() => { Array.Sort(tmp); }));
			}
			await Task.WhenAll(tasks.ToArray()).ConfigureAwait(false);
			Merge(a, new List<Arr>
			{
				new Arr() { a = arrs[0], ptr = 0 },
				new Arr() { a = arrs[1], ptr = 0 },
				new Arr() { a = arrs[2], ptr = 0 },
				new Arr() { a = arrs[3], ptr = 0 },
			});
		}
		private class Arr
		{
			public Tp[] a;
			public int ptr;
		}
		private static void Merge(Tp[] destArr, List<Arr> arrs)
		{
			Tp minValue;
			Arr min;
			for (int i = 0; i < destArr.Length; i++)
			{
				var firstArr = arrs.First();
				minValue = firstArr.a[firstArr.ptr];
				min = firstArr;
				for (int j = 1; j < arrs.Count; j++)
				{
					if (arrs[j].a[arrs[j].ptr].CompareTo(minValue) < 0)
					{
						minValue = arrs[j].a[arrs[j].ptr];
						min = arrs[j];
					}
				}
				destArr[i] = minValue;
				min.ptr++;
				if (min.ptr >= min.a.Length)
				{
					arrs.Remove(min);
				}
			}
		}

		private List<Tp[]> Divide(Tp[] a)
		{
			List<Tp[]> arrs = new List<Tp[]>();
			int divisionSize = a.Length / 4;
			var a1 = new Tp[divisionSize];
			var a2 = new Tp[divisionSize];
			var a3 = new Tp[divisionSize];
			var a4 = new Tp[a.Length - (divisionSize * 3)];
			Array.Copy(a, 0, a1, 0, a1.Length);
			Array.Copy(a, divisionSize, a2, 0, a2.Length);
			Array.Copy(a, divisionSize * 2, a3, 0, a3.Length);
			Array.Copy(a, divisionSize * 3, a4, 0, a4.Length);
			return new List<Tp[]>()
			{
				a1, a3, a2, a4
			};
		}
	}
}
