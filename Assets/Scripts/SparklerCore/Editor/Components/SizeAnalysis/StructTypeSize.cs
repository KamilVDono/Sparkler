///
/// -- Code adopted from https://gist.github.com/mzaks/ec261ac853621af8503b73391ebd18f1
///
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

using Unity.Collections.LowLevel.Unsafe;

namespace Sparkler.Editor.Components.SizeAnalysis
{
	public static class StructTypeSize
	{
		private static readonly ConcurrentDictionary<Type, int> s_Cache = new ConcurrentDictionary<Type, int>();

		public static int GetTypeSize( Type type ) => s_Cache.GetOrAdd( type, _ => UnsafeUtility.SizeOf( type ) );

		public static int GetCurrentStructSize( IEnumerable<Type> fieldTypes )
		{
			int Offset( int currentOffset, int size ) => ( currentOffset % size ) == 0 ? 0 : size - ( currentOffset % size );

			var sum = 0;
			var biggestSize = 1;
			foreach ( var field in fieldTypes )
			{
				var fieldSize = GetTypeSize(field);
				var offset = Offset(sum, fieldSize);
				sum += offset + fieldSize;
				biggestSize = fieldSize > biggestSize ? fieldSize : biggestSize;
			}
			return sum + Offset( sum, biggestSize );
		}

		public static int GetPossibleStructSize( IEnumerable<Type> fieldTypes )
		{
			var biggestSize = 1;
			var sum = 0;
			foreach ( var fieldType in fieldTypes )
			{
				var fieldSize = GetTypeSize(fieldType);
				sum += fieldSize;
				biggestSize = fieldSize > biggestSize ? fieldSize : biggestSize;
			}

			if ( ( sum % biggestSize ) == 0 )
			{
				return sum == 0 ? 1 : sum;
			}
			return sum + ( biggestSize - ( sum % biggestSize ) );
		}

		public static int GetPossibleStructSize( Type type )
		{
			var fields = new List<Type>();
			CollectFields( type, fields );
			return GetPossibleStructSize( fields );
		}

		public static List<Type> CollectFields( Type type )
		{
			List<Type> types = new List<Type>();
			CollectFields( type, types );
			return types;
		}

		public static void CollectFields( Type type, List<Type> list )
		{
			if ( type.IsPrimitive )
			{
				list.Add( type );
				return;
			}
			var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			foreach ( var field in fields )
			{
				if ( field.FieldType == type )
				{
					continue;
				}
				if ( field.IsStatic )
				{
					continue;
				}

				if ( field.FieldType.IsExplicitLayout )
				{
					list.Add( field.FieldType );
					continue;
				}
				if ( field.FieldType.IsPrimitive || field.FieldType.IsEnum )
				{
					list.Add( field.FieldType );
				}
				else if ( field.FieldType.IsValueType )
				{
					CollectFields( field.FieldType, list );
				}
			}
		}
	}
}