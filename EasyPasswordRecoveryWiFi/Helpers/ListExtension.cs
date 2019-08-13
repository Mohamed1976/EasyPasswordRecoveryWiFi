using System;
using System.Collections.Generic;

namespace EasyPasswordRecoveryWiFi.Helpers
{
	public static class ListExtension
	{
		/// <summary>
		/// Swaps the elements at the specified positions in the specified list. 
		/// (If the specified positions are equal, invoking this method leaves the list unchanged.)
		/// </summary>
		/// <param name="list">The list in which to swap elements.</param>
		/// <param name="i">The index of one element to be swapped.</param>
		/// <param name="j">The index of the other element to be swapped.</param>
		/// <remarks>
		/// Throws:
		/// IndexOutOfRangeException - if either i or j is out of range (i < 0 || i >= list.Count ||
		/// j < 0 || j >= list.Count)
		/// </remarks>
		public static void Swap<T>(this IList<T> list, int i, int j)
		{
			if (i < 0 || i >= list.Count || j < 0 || j >= list.Count)
				throw new IndexOutOfRangeException();

			SwapElements(list, i, j);
		}

		/// <summary>
		/// Swaps the elements at the specified positions in the specified list. 
		/// (If the specified positions are equal, invoking this method leaves the list unchanged.)
		/// </summary>
		/// <param name="list">The list in which to swap elements.</param>
		/// <param name="obj1">The object to be swapped.</param>
		/// <param name="obj2">The other object to be swapped.</param>
		/// <remarks>
		/// Throws:
		/// KeyNotFoundException - if either obj1 or obj2 is not found in the list. 
		/// </remarks>
		public static void Swap<T>(this IList<T> list, T obj1, T obj2)
		{
			if (!(list.Contains(obj1) && list.Contains(obj2)))
				throw new KeyNotFoundException();

			SwapElements(list, list.IndexOf(obj1), list.IndexOf(obj2));
		}

		private static void SwapElements<T>(IList<T> list, int i, int j)
		{
			List<int> indexes = new List<int> { i, j };

			if (indexes[0] != indexes[1])
			{
				indexes.Sort();
				List<T> values = new List<T> { list[indexes[0]], list[indexes[1]] };
				list.RemoveAt(indexes[1]);
				list.RemoveAt(indexes[0]);
				list.Insert(indexes[0], values[1]);
				list.Insert(indexes[1], values[0]);
			}
		}
	}
}
