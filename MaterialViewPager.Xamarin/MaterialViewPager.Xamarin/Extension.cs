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

		public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> source, TKey key, TValue value)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}

			if (source.ContainsKey(key))
			{
				source[key] = value;
			}
			else
			{
				source.Add(key, value);
			}
		}
	}
}