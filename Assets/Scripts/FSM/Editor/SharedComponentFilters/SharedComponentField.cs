using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using Unity.Entities;

using UnityEditor;

using UnityEngine;

namespace FSM.Editor.SharedComponentFilters
{
	public abstract class SharedComponentField
	{
		#region Style
		protected static GUIStyle s_labelStyle = null;

		protected static GUIStyle s_LabelStyle => s_labelStyle ?? ( s_labelStyle = new GUIStyle( EditorStyles.label )
		{
			wordWrap = true,
			richText = true,
		} );

		#endregion Style

		protected ISharedComponentData _component;
		protected FieldInfo _fieldInfo;
		protected string _editorName;

		protected SharedComponentField( ISharedComponentData component, FieldInfo fieldInfo )
		{
			_component = component;
			_fieldInfo = fieldInfo;
			_editorName = ObjectNames.NicifyVariableName( _fieldInfo.Name );
		}

		public static SharedComponentField Create( ISharedComponentData component, FieldInfo fieldInfo )
		{
			// floating-points
			if ( fieldInfo.FieldType == typeof( float ) )
			{
				return new SharedComponentFieldFloat( component, fieldInfo );
			}
			if ( fieldInfo.FieldType == typeof( double ) )
			{
				return new SharedComponentFieldDouble( component, fieldInfo );
			}
			if ( fieldInfo.FieldType == typeof( decimal ) )
			{
				return new SharedComponentFieldDecimal( component, fieldInfo );
			}
			// integral
			if ( fieldInfo.FieldType == typeof( int ) )
			{
				return new SharedComponentFieldInt( component, fieldInfo );
			}
			if ( fieldInfo.FieldType == typeof( uint ) )
			{
				return new SharedComponentFieldUInt( component, fieldInfo );
			}
			if ( fieldInfo.FieldType == typeof( long ) )
			{
				return new SharedComponentFieldLong( component, fieldInfo );
			}
			if ( fieldInfo.FieldType == typeof( ulong ) )
			{
				return new SharedComponentFieldULong( component, fieldInfo );
			}
			if ( fieldInfo.FieldType == typeof( short ) )
			{
				return new SharedComponentFieldShort( component, fieldInfo );
			}
			if ( fieldInfo.FieldType == typeof( ushort ) )
			{
				return new SharedComponentFieldUShort( component, fieldInfo );
			}
			if ( fieldInfo.FieldType == typeof( byte ) )
			{
				return new SharedComponentFieldByte( component, fieldInfo );
			}
			if ( fieldInfo.FieldType == typeof( sbyte ) )
			{
				return new SharedComponentFieldSByte( component, fieldInfo );
			}
			// enum
			if ( fieldInfo.FieldType.IsEnum )
			{
				if ( fieldInfo.FieldType.IsDefined( typeof( FlagsAttribute ), false ) )
				{
					return new SharedComponentFieldFlagEnum( component, fieldInfo );
				}
				else
				{
					return new SharedComponentFieldEnum( component, fieldInfo );
				}
			}
			// fallback
			return new SharedComponentFieldUndefined( component, fieldInfo );
		}

		public abstract void Draw();

		public override string ToString() => $"{_fieldInfo.Name} = {_fieldInfo.GetValue( _component )}";
	}

	public abstract class SharedComponentField<T> : SharedComponentField
	{
		protected T _currentValue;

		protected SharedComponentField( ISharedComponentData component, FieldInfo fieldInfo ) : base( component, fieldInfo ) => _currentValue = (T)fieldInfo.GetValue( component );
	}

	// fallback
	public class SharedComponentFieldUndefined : SharedComponentField
	{
		public SharedComponentFieldUndefined( ISharedComponentData component, FieldInfo fieldInfo ) : base( component, fieldInfo )
		{
		}

		public override void Draw() => EditorGUILayout.LabelField( $"<color=red>{_editorName}: {_fieldInfo.FieldType}</color>", s_LabelStyle );
	}

	// floating-points
	public class SharedComponentFieldFloat : SharedComponentField<float>
	{
		public SharedComponentFieldFloat( ISharedComponentData component, FieldInfo fieldInfo ) : base( component, fieldInfo )
		{
		}

		public override void Draw()
		{
			EditorGUI.BeginChangeCheck();
			var newValue = EditorGUILayout.DelayedFloatField(_editorName, _currentValue);
			if ( EditorGUI.EndChangeCheck() && newValue != _currentValue )
			{
				_fieldInfo.SetValue( _component, newValue );
				_currentValue = newValue;
			}
		}
	}

	public class SharedComponentFieldDouble : SharedComponentField<double>
	{
		public SharedComponentFieldDouble( ISharedComponentData component, FieldInfo fieldInfo ) : base( component, fieldInfo )
		{
		}

		public override void Draw()
		{
			EditorGUI.BeginChangeCheck();
			var newStringValue = EditorGUILayout.DelayedTextField( _editorName, _currentValue.ToString() );
			if ( EditorGUI.EndChangeCheck() && double.TryParse( newStringValue, out var newValue ) && newValue != _currentValue )
			{
				_fieldInfo.SetValue( _component, newValue );
				_currentValue = newValue;
			}
		}
	}

	public class SharedComponentFieldDecimal : SharedComponentField<decimal>
	{
		public SharedComponentFieldDecimal( ISharedComponentData component, FieldInfo fieldInfo ) : base( component, fieldInfo )
		{
		}

		public override void Draw()
		{
			EditorGUI.BeginChangeCheck();
			var newStringValue = EditorGUILayout.DelayedTextField( _editorName, _currentValue.ToString() );
			if ( EditorGUI.EndChangeCheck() && decimal.TryParse( newStringValue, out var newValue ) && newValue != _currentValue )
			{
				_fieldInfo.SetValue( _component, newValue );
				_currentValue = newValue;
			}
		}
	}

	// integral
	public class SharedComponentFieldInt : SharedComponentField<int>
	{
		public SharedComponentFieldInt( ISharedComponentData component, FieldInfo fieldInfo ) : base( component, fieldInfo )
		{
		}

		public override void Draw()
		{
			EditorGUI.BeginChangeCheck();
			var newValue = EditorGUILayout.DelayedIntField(_editorName, _currentValue);
			if ( EditorGUI.EndChangeCheck() && newValue != _currentValue )
			{
				_fieldInfo.SetValue( _component, newValue );
				_currentValue = newValue;
			}
		}
	}

	public class SharedComponentFieldUInt : SharedComponentField<uint>
	{
		public SharedComponentFieldUInt( ISharedComponentData component, FieldInfo fieldInfo ) : base( component, fieldInfo )
		{
		}

		public override void Draw()
		{
			EditorGUI.BeginChangeCheck();
			var newStringValue = EditorGUILayout.DelayedTextField( _editorName, _currentValue.ToString() );
			if ( EditorGUI.EndChangeCheck() && uint.TryParse( newStringValue, out uint newValue ) && newValue != _currentValue )
			{
				_fieldInfo.SetValue( _component, newValue );
				_currentValue = newValue;
			}
		}
	}

	public class SharedComponentFieldLong : SharedComponentField<long>
	{
		public SharedComponentFieldLong( ISharedComponentData component, FieldInfo fieldInfo ) : base( component, fieldInfo )
		{
		}

		public override void Draw()
		{
			EditorGUI.BeginChangeCheck();
			var newStringValue = EditorGUILayout.DelayedTextField( _editorName, _currentValue.ToString() );
			if ( EditorGUI.EndChangeCheck() && long.TryParse( newStringValue, out var newValue ) && newValue != _currentValue )
			{
				_fieldInfo.SetValue( _component, newValue );
				_currentValue = newValue;
			}
		}
	}

	public class SharedComponentFieldULong : SharedComponentField<ulong>
	{
		public SharedComponentFieldULong( ISharedComponentData component, FieldInfo fieldInfo ) : base( component, fieldInfo )
		{
		}

		public override void Draw()
		{
			EditorGUI.BeginChangeCheck();
			var newStringValue = EditorGUILayout.DelayedTextField( _editorName, _currentValue.ToString() );
			if ( EditorGUI.EndChangeCheck() && ulong.TryParse( newStringValue, out var newValue ) && newValue != _currentValue )
			{
				_fieldInfo.SetValue( _component, newValue );
				_currentValue = newValue;
			}
		}
	}

	public class SharedComponentFieldShort : SharedComponentField<short>
	{
		public SharedComponentFieldShort( ISharedComponentData component, FieldInfo fieldInfo ) : base( component, fieldInfo )
		{
		}

		public override void Draw()
		{
			EditorGUI.BeginChangeCheck();
			var newIntValue = EditorGUILayout.DelayedIntField(_editorName, _currentValue);
			if ( EditorGUI.EndChangeCheck() && newIntValue != _currentValue )
			{
				if ( newIntValue < short.MinValue )
				{
					newIntValue = short.MinValue;
				}
				else if ( newIntValue > short.MaxValue )
				{
					newIntValue = short.MaxValue;
				}
				var newValue = (short)newIntValue;
				_fieldInfo.SetValue( _component, newValue );
				_currentValue = newValue;
			}
		}
	}

	public class SharedComponentFieldUShort : SharedComponentField<ushort>
	{
		public SharedComponentFieldUShort( ISharedComponentData component, FieldInfo fieldInfo ) : base( component, fieldInfo )
		{
		}

		public override void Draw()
		{
			EditorGUI.BeginChangeCheck();
			var newIntValue = EditorGUILayout.DelayedIntField(_editorName, _currentValue);
			if ( EditorGUI.EndChangeCheck() && newIntValue != _currentValue )
			{
				if ( newIntValue < ushort.MinValue )
				{
					newIntValue = ushort.MinValue;
				}
				else if ( newIntValue > ushort.MaxValue )
				{
					newIntValue = ushort.MaxValue;
				}
				var newValue = (ushort)newIntValue;
				_fieldInfo.SetValue( _component, newValue );
				_currentValue = newValue;
			}
		}
	}

	public class SharedComponentFieldByte : SharedComponentField<byte>
	{
		public SharedComponentFieldByte( ISharedComponentData component, FieldInfo fieldInfo ) : base( component, fieldInfo )
		{
		}

		public override void Draw()
		{
			EditorGUI.BeginChangeCheck();
			var newIntValue = EditorGUILayout.DelayedIntField(_editorName, _currentValue);
			if ( EditorGUI.EndChangeCheck() && newIntValue != _currentValue )
			{
				if ( newIntValue < byte.MinValue )
				{
					newIntValue = byte.MinValue;
				}
				else if ( newIntValue > byte.MaxValue )
				{
					newIntValue = byte.MaxValue;
				}
				var newValue = (byte)newIntValue;
				_fieldInfo.SetValue( _component, newValue );
				_currentValue = newValue;
			}
		}
	}

	public class SharedComponentFieldSByte : SharedComponentField<sbyte>
	{
		public SharedComponentFieldSByte( ISharedComponentData component, FieldInfo fieldInfo ) : base( component, fieldInfo )
		{
		}

		public override void Draw()
		{
			EditorGUI.BeginChangeCheck();
			var newIntValue = EditorGUILayout.DelayedIntField(_editorName, _currentValue);
			if ( EditorGUI.EndChangeCheck() && newIntValue != _currentValue )
			{
				if ( newIntValue < sbyte.MinValue )
				{
					newIntValue = sbyte.MinValue;
				}
				else if ( newIntValue > sbyte.MaxValue )
				{
					newIntValue = sbyte.MaxValue;
				}
				var newValue = (sbyte)newIntValue;
				_fieldInfo.SetValue( _component, newValue );
				_currentValue = newValue;
			}
		}
	}

	// enum
	public class SharedComponentFieldEnum : SharedComponentField<Enum>
	{
		public SharedComponentFieldEnum( ISharedComponentData component, FieldInfo fieldInfo ) : base( component, fieldInfo )
		{
		}

		public override void Draw()
		{
			EditorGUI.BeginChangeCheck();
			var newValue = EditorGUILayout.EnumPopup(_editorName, _currentValue);
			if ( EditorGUI.EndChangeCheck() && newValue != _currentValue )
			{
				_fieldInfo.SetValue( _component, newValue );
				_currentValue = newValue;
			}
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append( _fieldInfo.Name );
			sb.Append( " = " );
			Stack<string> declaringTypes = new Stack<string>();
			var declaringType = _fieldInfo.FieldType.DeclaringType;
			while ( declaringType != null )
			{
				declaringTypes.Push( declaringType.Name );
				declaringType = declaringType.DeclaringType;
			}
			while ( declaringTypes.Count > 0 )
			{
				sb.Append( declaringTypes.Pop() );
				sb.Append( "." );
			}
			sb.Append( _fieldInfo.FieldType.Name );
			sb.Append( "." );
			sb.Append( _fieldInfo.GetValue( _component ) );
			return sb.ToString();
		}
	}

	public class SharedComponentFieldFlagEnum : SharedComponentField<Enum>
	{
		public SharedComponentFieldFlagEnum( ISharedComponentData component, FieldInfo fieldInfo ) : base( component, fieldInfo )
		{
		}

		public override void Draw()
		{
			EditorGUI.BeginChangeCheck();
			var newValue = EditorGUILayout.EnumFlagsField(_editorName, _currentValue);
			if ( EditorGUI.EndChangeCheck() && newValue != _currentValue )
			{
				_fieldInfo.SetValue( _component, newValue );
				_currentValue = newValue;
			}
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append( _fieldInfo.Name );
			sb.Append( " = " );
			sb.Append( "(" );
			Stack<string> declaringTypes = new Stack<string>();
			var declaringType = _fieldInfo.FieldType.DeclaringType;
			while ( declaringType != null )
			{
				declaringTypes.Push( declaringType.Name );
				declaringType = declaringType.DeclaringType;
			}
			while ( declaringTypes.Count > 0 )
			{
				sb.Append( declaringTypes.Pop() );
				sb.Append( "." );
			}
			sb.Append( _fieldInfo.FieldType.Name );
			sb.Append( ")" );
			sb.Append( "(" );
			sb.Append( (int)_fieldInfo.GetValue( _component ) );
			sb.Append( ")" );
			return sb.ToString();
		}
	}
}