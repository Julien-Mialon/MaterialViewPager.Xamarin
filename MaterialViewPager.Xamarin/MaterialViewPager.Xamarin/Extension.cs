using System;
using System.Collections.Generic;

namespace MaterialViewPager
{
	public static class Extension
	{
		public static void Apply<T>(this IEnumerable<T> source, Action<T> action)
		{
			foreach (T item in source)
			{
				action(item);
			}
		}
	}
}