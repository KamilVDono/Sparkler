using System;
using System.Collections.Generic;

namespace Sparkler.Utility
{
	public static class LinqExtensions
	{
		public static int IndexOf<T>( this IEnumerable<T> elements, T element )
		{
			int index = 0;
			// search linearly
			foreach ( T e in elements )
			{
				if ( e.Equals( element ) )
				{
					return index;
				}

				index++;
			}
			// nothing found
			return -1;
		}

		public static void ForEach<T>( this IEnumerable<T> elements, Action<T> action )
		{
			foreach ( T element in elements )
			{
				action( element );
			}
		}

		public static IEnumerable<T> SkipLastN<T>( this IEnumerable<T> source, int n )
		{
			var it = source.GetEnumerator();
			bool hasRemainingItems;
			var cache = new Queue<T>(n + 1);

			do
			{
				hasRemainingItems = it.MoveNext();
				if ( hasRemainingItems )
				{
					cache.Enqueue( it.Current );
					if ( cache.Count > n )
					{
						yield return cache.Dequeue();
					}
				}
			} while ( hasRemainingItems );

			it.Dispose();
		}

		/// <summary>
		/// Wraps this object instance into an IEnumerable&lt;T&gt; consisting of a single item.
		/// </summary>
		/// <typeparam name="T">Type of the object.</typeparam>
		/// <param name="item">The instance that will be wrapped.</param>
		/// <returns>An IEnumerable&lt;T&gt; consisting of a single item.</returns>
		public static IEnumerable<T> Yield<T>( this T item )
		{
			yield return item;
		}
	}
}