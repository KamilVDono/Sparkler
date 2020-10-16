using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEngine;

namespace FSM.Utility
{
	public static class ReflectionExtensions
	{
		/// <summary>
		/// Copy field value from source field(sourceFieldName) to target field(targetFieldName) with
		/// possible object conversion via fieldValueConverter
		/// </summary>
		/// <param name="source">Source object to copy from</param>
		/// <param name="sourceFieldName">Name of field to copy from</param>
		/// <param name="target">Target object to paste to</param>
		/// <param name="targetFieldName">Name of field to paste to</param>
		/// <param name="fieldValueConverter">Function to convert source value to proper target value</param>
		public static void CopyField( object source, string sourceFieldName, object target, string targetFieldName, Func<object, object> fieldValueConverter = null )
		{
			var sourceField = source.GetType().GetFieldRecursive(sourceFieldName);
			if ( sourceField == null )
			{
				Debug.LogWarning( $"Object: {source} of type {source.GetType()} has not field named {sourceFieldName}" );
				return;
			}

			var targetField = target.GetType().GetFieldRecursive(targetFieldName);
			if ( targetField == null )
			{
				Debug.LogWarning( $"Object: {target} of type {target.GetType()} has not field named {targetFieldName}" );
				return;
			}

			if ( fieldValueConverter == null )
			{
				fieldValueConverter = ( x ) => x;
			}

			try
			{
				targetField.SetValue( target, fieldValueConverter( sourceField.GetValue( source ) ) );
			}
			catch ( Exception e )
			{
				Debug.LogError( $"On CopyField  {sourceFieldName}({source.GetType()}) => {targetFieldName}({target.GetType()}). Error {e.GetType()} with message {e.Message}" );
			}
		}

		/// <summary>
		/// Set field value in target object
		/// </summary>
		/// <param name="target">Target object</param>
		/// <param name="fieldName">Field to paste to</param>
		/// <param name="value">New field value</param>
		public static void SetField( object target, string fieldName, object value )
		{
			var field = target.GetType().GetFieldRecursive(fieldName);
			if ( field == null )
			{
				Debug.LogWarning( $"Object: {target} of type {target.GetType()} has not field named {fieldName}" );
				return;
			}
			try
			{
				field.SetValue( target, value );
			}
			catch ( Exception e )
			{
				Debug.LogError( $"On SetPrivateField  {fieldName}({target.GetType()}) => ({value}). Error {e.GetType()} with message {e.Message}" );
			}
		}

		/// <summary>
		/// Obtain FieldInfo of field with fieldName Work even for private fields from base classes
		/// </summary>
		/// <param name="sourceType">Object type</param>
		/// <param name="fieldName">Field name</param>
		/// <returns>If found FieldInfo, otherwise null</returns>
		public static FieldInfo GetFieldRecursive( this Type sourceType, string fieldName )
		{
			FieldInfo field = null;
			Type type = sourceType;
			do
			{
				field = type.GetField( fieldName, BindingFlags.Instance | BindingFlags.Public
												| BindingFlags.NonPublic | BindingFlags.GetField );
				type = type.BaseType;
			} while ( field == null && type != null );

			return field;
		}

		public static IEnumerable<T> FieldsOfType<T>( object obj )
		{
			Type type = typeof(T);

			foreach ( var field in obj.GetType().GetFields( BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly ) )
			{
				if ( type.IsAssignableFrom( field.FieldType ) )
				{
					// normal field
					T temp = (T)field.GetValue(obj);
					yield return temp;
				}
				else if ( field.FieldType.IsArray && type.IsAssignableFrom( field.FieldType.GetElementType() ) )
				{
					// array
					foreach ( T temp in (T[])field.GetValue( obj ) )
					{
						yield return temp;
					}
				}
				else if ( typeof( IEnumerable<T> ).IsAssignableFrom( field.FieldType ) && type.IsAssignableFrom( field.FieldType.GenericTypeArguments[0] ) )
				{
					// enumerable
					foreach ( T temp in (IEnumerable<T>)field.GetValue( obj ) )
					{
						yield return temp;
					}
				}
			}
		}

		public static Dictionary<MemberInfo, object> GetAllFieldsValues( this object source )
		{
			Dictionary<MemberInfo, object> valuesDictionary = new Dictionary<MemberInfo, object>();

			var distinctFields = source.AllFields().GroupBy(f => f.Name).Select(gr => gr.Last());
			foreach ( MemberInfo distinctField in distinctFields )
			{
				if ( distinctField is FieldInfo fieldInfo )
				{
					valuesDictionary[distinctField] = fieldInfo.GetValue( source );
				}
				else if ( distinctField is MethodInfo getterInfo )
				{
					valuesDictionary[distinctField] = getterInfo.Invoke( source, null );
				}
			}

			return valuesDictionary;
		}

		public static IEnumerable<MemberInfo> AllFields( this object source ) => source.GetType().AllFields();

		public static IEnumerable<MemberInfo> AllFields( this Type type )
		{
			HashSet<MemberInfo> fieldsSet = new HashSet<MemberInfo>();
			Type currentType = type;
			while ( currentType != null )
			{
				var fields = currentType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField).Where( f => f.GetCustomAttributes(typeof(ObsoleteAttribute), true).Length < 1 );
				fieldsSet.UnionWith( fields );
				var getters = currentType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty)
					.Where(prop => prop.GetMethod != null).Where( f => f.GetCustomAttributes(typeof(ObsoleteAttribute), true).Length < 1 ).Select(prop => prop.GetMethod);
				fieldsSet.UnionWith( getters );
				currentType = currentType.BaseType;
			}

			return fieldsSet;
		}

		public static IEnumerable<MethodInfo> AllMethods( this object source ) => source.GetType().AllMethods();

		public static IEnumerable<MethodInfo> AllMethods( this Type type )
		{
			HashSet<MethodInfo> methodsSet = new HashSet<MethodInfo>();
			Type currentType = type;
			while ( currentType != null )
			{
				var fields = currentType
					.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
					.Where( m => !(m.IsSpecialName || m.Name.StartsWith("set_") ||  m.Name.StartsWith("get_")) );
				methodsSet.UnionWith( fields );
				currentType = currentType.BaseType;
			}

			return methodsSet;
		}

		public static object MemberValue( this MemberInfo memberInfo, object relatedObject )
		{
			if ( memberInfo is FieldInfo fieldInfo )
			{
				return fieldInfo.GetValue( relatedObject );
			}
			else if ( memberInfo is MethodInfo getterInfo )
			{
				try
				{
					return getterInfo.Invoke( relatedObject, null );
				}
				catch
				{
					return null;
				}
			}
			return null;
		}

		/// <summary>
		/// Sets the member's value on the target object.
		/// </summary>
		/// <param name="member">The member.</param>
		/// <param name="target">The target.</param>
		/// <param name="value">The value.</param>
		public static void SetMemberValue( this MemberInfo member, object target, object value )
		{
			try
			{
				if ( member is FieldInfo fieldInfo )
				{
					fieldInfo.SetValue( target, value );
				}
				else if ( member.MemberType == MemberTypes.Property )
				{
					( (PropertyInfo)member ).SetValue( target, value, null );
				}
			}
			catch ( Exception e )
			{
				Debug.LogException( e );
			}
		}

		public static IEnumerable<MethodInfo> AllMethods( Func<MethodInfo, bool> filter )
		{
			var currentDomain = AppDomain.CurrentDomain;
			var assemblies = currentDomain.GetAssemblies();
			var allTypes = assemblies.SelectMany(asm => asm.GetTypes());
			var allMethods = allTypes.SelectMany(t => t.GetMethods(
				BindingFlags.Public | BindingFlags.NonPublic |
				BindingFlags.Instance | BindingFlags.Static));
			return allMethods.Where( filter );
		}

		/// <summary>
		/// All classes inheriting from TBaseType
		/// </summary>
		public static IEnumerable<Type> SubClassesOf<TBaseType>()
		{
			var baseType = typeof(TBaseType);
			return SubClassesOf( baseType );
		}

		/// <summary>
		/// All classes inheriting from TBaseType
		/// </summary>
		public static IEnumerable<Type> SubClassesOf( Type baseType )
		{
			var assembly = baseType.Assembly;

			return assembly.GetTypes().Where( t => t.IsSubclassOf( baseType ) );
		}

		public static IEnumerable<Type> SubClassesWithBaseOf( Type baseType ) => SubClassesOf( baseType ).Append( baseType );

		public static bool Implements( this Type type, Type interfaceType ) => interfaceType.IsAssignableFrom( type );
	}
}