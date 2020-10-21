using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

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

		public static T MinBy<T, TKey>( this IEnumerable<T> elements, Func<T, TKey> key, bool safeMode = false ) where TKey : IComparable<TKey>
		{
			var enumerator = elements.GetEnumerator();
			// use the first element to initialize
			if ( !enumerator.MoveNext() )
			{
				if ( safeMode )
				{
					return default;
				}
				else
				{
					throw new ArgumentException( "Cannot find the minimum of an empty collection." );
				}
			}
			T minimumElement = enumerator.Current;
			TKey minimumKey = key(minimumElement);
			// iterate over the other elements and update
			while ( enumerator.MoveNext() )
			{
				T element = enumerator.Current;
				TKey elementKey = key(element);
				if ( elementKey.CompareTo( minimumKey ) < 0 )
				{
					minimumElement = element;
					minimumKey = elementKey;
				}
			}
			enumerator.Dispose();
			// done!
			return minimumElement;
		}

		public static T MaxBy<T, TKey>( this IEnumerable<T> elements, Func<T, TKey> key, bool safeMode = false ) where TKey : IComparable<TKey>
		{
			var enumerator = elements.GetEnumerator();
			// use the first element to initialize
			if ( !enumerator.MoveNext() )
			{
				if ( safeMode )
				{
					return default;
				}
				else
				{
					throw new ArgumentException( "Cannot find the maximum of an empty collection." );
				}
			}
			T maximumElement = enumerator.Current;
			TKey maximumKey = key(maximumElement);
			// iterate over the other elements and update
			while ( enumerator.MoveNext() )
			{
				T element = enumerator.Current;
				TKey elementKey = key(element);
				if ( elementKey.CompareTo( maximumKey ) > 0 )
				{
					maximumElement = element;
					maximumKey = elementKey;
				}
			}
			enumerator.Dispose();
			// done!
			return maximumElement;
		}

		public static void ForEach<T>( this IEnumerable<T> elements, Action<T> action )
		{
			foreach ( T element in elements )
			{
				action( element );
			}
		}

		public static bool AtLeast<T>( this IEnumerable<T> source, int minCount )
		{
			var collection = source as ICollection<T>;
			return collection == null
					? source.Skip( minCount - 1 ).Any()
					: collection.Count >= minCount;
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

		public static T FirstOrAny<T>( this IEnumerable<T> source, Func<T, bool> predicate ) where T : class
		{
			T itemThatSatisfyPredicate = source.FirstOrDefault(predicate);
			if ( itemThatSatisfyPredicate == null )
			{
				return source.FirstOrDefault();
			}
			else
			{
				return itemThatSatisfyPredicate;
			}
		}

		public static Transform FirstOrDefault( this Transform transform, Func<Transform, bool> query )
		{
			if ( query( transform ) )
			{
				return transform;
			}
			for ( int i = 0; i < transform.childCount; i++ )
			{
				var result = FirstOrDefault(transform.GetChild(i), query);
				if ( result != null )
				{
					return result;
				}
			}
			return null;
		}

		public static T FirstNotNull<T>( params T[] args ) where T : class => args.FirstOrDefault( a => a != null );

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