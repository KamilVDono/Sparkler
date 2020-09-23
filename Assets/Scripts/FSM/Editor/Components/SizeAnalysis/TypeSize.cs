///
/// -- Code adopted from https://gist.github.com/mzaks/ec261ac853621af8503b73391ebd18f1
///
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

using Unity.Collections.LowLevel.Unsafe;

using UnityEngine;

namespace FSM.Editor.Components.SizeAnalysis
{
	public static class TypeSize
	{
		private static readonly ConcurrentDictionary<Type, int> Cache = new ConcurrentDictionary<Type, int>();

		public static int GetTypeSize( Type type ) => Cache.GetOrAdd( type, _ => UnsafeUtility.SizeOf( type ) );

		public static int GetStructSize( Type type, List<string> warnings )
		{
			var fields = new List<FieldInfo>();
			CollectFields( type, fields );
			var biggestSize = 1;
			var sum = 0;
			foreach ( var field in fields )
			{
				var pType = field.FieldType;
				if ( pType.GetCustomAttribute<StructLayoutAttribute>() != null )
				{
					Debug.LogWarning( $"{pType.FullName}" );
				}
				var fSize = GetTypeSize(pType);
				if ( pType.IsEnum && fSize > 1 )
				{
					warnings.Add( $"Consider defining `enum {pType.Name}: byte` in order to reduce size from {fSize} bytes to 1" );
					fSize = 1;
				}

				sum += fSize;
				biggestSize = fSize > biggestSize ? fSize : biggestSize;
			}

			if ( ( sum % biggestSize ) == 0 )
			{
				return sum == 0 ? 1 : sum;
			}
			return sum + ( biggestSize - ( sum % biggestSize ) );
		}

		public static void CollectFields( Type type, List<FieldInfo> list )
		{
			var fields = type.GetFields(BindingFlags.Public |
																		BindingFlags.NonPublic |
																		BindingFlags.Instance);
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
					list.Add( field );
					continue;
				}
				if ( field.FieldType.IsPrimitive || field.FieldType.IsEnum )
				{
					list.Add( field );
				}
				else if ( field.FieldType.IsValueType )
				{
					CollectFields( field.FieldType, list );
				}
			}
		}
	}
}