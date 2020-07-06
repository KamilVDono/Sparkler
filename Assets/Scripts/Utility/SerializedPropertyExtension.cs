using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using UnityEditor;

namespace FSM.Utility
{
	public static class SerializedPropertyExtension
	{
		private const BindingFlags BindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;

		private static readonly Regex DataIndexExtractRegex = new Regex(@"(?<=data\[)\d+(?=\])");

		public static T[] ExtractAttributes<T>( this SerializedProperty serializedProperty ) where T : Attribute
		{
			FieldInfo targetFieldInfo = serializedProperty.FieldInfo();
			return (T[])targetFieldInfo.GetCustomAttributes( typeof( T ), true );
		}

		public static T ExtractAttribute<T>( this SerializedProperty serializedProperty ) where T : Attribute
		{
			FieldInfo targetFieldInfo = serializedProperty.FieldInfoArrayAware();
			return (T)targetFieldInfo?.GetCustomAttribute( typeof( T ), true );
		}

		public static FieldInfo FieldInfo( this SerializedProperty serializedProperty )
		{
			Type targetType = GetParentType(serializedProperty);
			return targetType.GetFields( BindingFlags ).FirstOrDefault( fi => fi.Name.Equals( serializedProperty.name ) );
		}

		public static FieldInfo FieldInfoArrayAware( this SerializedProperty serializedProperty )
		{
			Type targetType = GetParentType(serializedProperty, serializedProperty.name.Equals("data") ? 3 : 1);
			string propertyName = serializedProperty.name.Equals("data") ? serializedProperty.propertyPath.Split('.').SkipLastN(2).Last() : serializedProperty.name;
			return targetType.GetField( propertyName, BindingFlags );
		}

		public static float Height( this SerializedProperty serializedProperty )
		{
			var children = serializedProperty.GetChildren();
			return children.Sum( child => EditorGUI.GetPropertyHeight( child, true ) );
		}

		public static IEnumerable<SerializedProperty> GetChildren( this SerializedProperty property )
		{
			property = property.Copy();
			var nextElement = property.Copy();
			bool hasNextElement = nextElement.NextVisible(false);
			if ( !hasNextElement )
			{
				nextElement = null;
			}

			property.NextVisible( true );
			while ( true )
			{
				if ( ( SerializedProperty.EqualContents( property, nextElement ) ) )
				{
					yield break;
				}

				yield return property;

				bool hasNext = property.NextVisible(false);
				if ( !hasNext )
				{
					break;
				}
			}
		}

		/// <summary>
		/// Work only on first depth
		/// </summary>
		public static object PropertyTargetObject( this SerializedProperty serializedProperty )
		{
			var targetObject = serializedProperty.serializedObject.targetObject;
			var targetObjectClassType = targetObject.GetType();
			var field = targetObjectClassType.GetField(serializedProperty.propertyPath);
			if ( field != null )
			{
				return field.GetValue( targetObject );
			}

			return null;
		}

		/// <summary>
		/// Get value of property (or property parent)
		/// </summary>
		/// <param name="serializedProperty">Property which value from we want</param>
		/// <param name="ofUpper">
		/// Set to 1 if want parent class instance, set to 2 if parent parent ... If just property value
		/// leave as 0
		/// </param>
		/// <returns>Real value of property</returns>
		public static object GetPropertyValue( this SerializedProperty serializedProperty, int ofUpper = 0 )
		{
			string[] slices = serializedProperty.propertyPath.Split('.');
			Type type = serializedProperty.serializedObject.targetObject.GetType();
			object currentValue = serializedProperty.serializedObject.targetObject;

			for ( int i = 0; i < slices.Length - ofUpper; i++ )
			{
				if ( slices[i] == "Array" )
				{
					//go to 'data[x]'
					i++;
					// extract x
					var index = int.Parse( DataIndexExtractRegex.Match(slices[i]).Value );

					var currentArray = currentValue as IEnumerable;
					var enumerator = currentArray.GetEnumerator();
					enumerator.MoveNext();

					for ( int j = 0; j < index; j++ )
					{
						enumerator.MoveNext();
					}

					currentValue = enumerator.Current;

					if ( type.IsArray )
					{
						type = type.GetElementType(); //gets info on array elements
					}
					else
					{
						type = type.GetGenericArguments()[0];
					}
				}
				else
				{
					var fieldInfo = type.GetField(slices[i], BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance);
					currentValue = fieldInfo.GetValue( currentValue );

					type = fieldInfo.FieldType;
				}
			}

			return currentValue;
		}

		public static Type GetPropertyType( this SerializedProperty serializedProperty )
		{
			string[] slices = serializedProperty.propertyPath.Split('.');
			Type type = serializedProperty.serializedObject.targetObject.GetType();

			for ( int i = 0; i < slices.Length; i++ )
			{
				if ( slices[i] == "Array" )
				{
					i++; //skips "data[x]"
					if ( type.IsArray )
					{
						type = type.GetElementType(); //gets info on array elements
					}
					else
					{
						type = type.GetGenericArguments()[0];
					}
				}
				else
				{
					type = type.GetField( slices[i], BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance ).FieldType;
				}
			}

			return type;
		}

		private static Type GetParentType( SerializedProperty serializedProperty, int parentDepth = 1 )
		{
			var targetObject = serializedProperty.serializedObject.targetObject;
			var targetObjectType = targetObject.GetType();
			if ( serializedProperty.depth > 0 )
			{
				var path = serializedProperty.propertyPath.Split('.');
				Type currentType = targetObjectType;
				int i = 0;
				while ( i < path.Length - parentDepth )
				{
					if ( path[i] == "Array" )
					{
						i++; //skips "data[x]"
						if ( currentType.IsArray )
						{
							currentType = currentType.GetElementType(); //gets info on array elements
						}
						else
						{
							currentType = currentType.GetGenericArguments()[0];
						}
					}
					else
					{
						currentType = currentType.GetField( path[i], BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance ).FieldType;
					}
					i++;
				}
				return currentType;
			}
			else
			{
				return targetObjectType;
			}
		}
	}
}