using FSM.Utility;
using FSM.Utility.Editor;

using Rotorz.Games;

using System;
using System.Linq;
using System.Reflection;
using System.Text;

using Unity.Entities;

using UnityEditor;

using UnityEngine;

namespace FSM.Editor.SharedComponentFilters
{
	public class SharedComponentFilterWindow : EditorWindow
	{
		#region Styles
		private static GUIStyle s_boldLabelStyle = null;

		private static GUIStyle s_BoldLabelStyle => s_boldLabelStyle ?? ( s_boldLabelStyle = new GUIStyle( EditorStyles.boldLabel )
		{
			wordWrap = true,
			richText = true,
		} );

		#endregion Styles

		#region Drawing state
		private Vector2 _mainScroll = Vector2.zero;
		private SerializedObject _serializedObject = null;

		private SerializedObject _SerializedObject => _serializedObject ?? ( _serializedObject = new SerializedObject( this ) );
		#endregion Drawing state

		[ClassImplements(typeof(ISharedComponentData)), SerializeField]
		private ClassTypeReference _componentType = null;

		private ISharedComponentData _component;
		private SharedComponentField[] _fields;
		private bool _allFieldsValid;
		private Action<string, string> _onCreate;
		private string FilterName => _componentType?.Type.Name.ToLowerFirstChar();

		public static void ShowWindow( Action<string, string> onCreate )
		{
			var window = EditorWindow.GetWindow<SharedComponentFilterWindow>();
			window._onCreate = onCreate;
			window.ShowUtility();
		}

		public override string ToString()
		{
			if ( _componentType?.Type == null || _component == null || ( _fields?.Length ?? 0 ) < 1 || !_allFieldsValid )
			{
				return string.Empty;
			}

			StringBuilder sb = new StringBuilder();
			sb.Append( _componentType.Type.Name );
			sb.Append( " " );
			sb.Append( FilterName );
			sb.Append( " = new " );
			sb.Append( _componentType.Type.Name );
			sb.Append( "{ " );
			foreach ( var field in _fields )
			{
				sb.Append( field.ToString() );
				sb.Append( ", " );
			}
			sb.Append( "};" );

			return sb.ToString();
		}

		private void OnGUI()
		{
			_mainScroll = EditorGUILayout.BeginScrollView( _mainScroll );

			_SerializedObject.Update();
			EditorGUILayout.PropertyField( _SerializedObject.FindProperty( nameof( _componentType ) ) );
			_SerializedObject.ApplyModifiedProperties();

			DrawProperties();

			EditorGUILayout.BeginHorizontal();
			if ( GUILayout.Button( "Close" ) )
			{
				Close();
			}

			using ( new GUIEnabledScope( _allFieldsValid ) )
			{
				if ( GUILayout.Button( "Create" ) )
				{
					_onCreate( FilterName, ToString() );
					Close();
				}
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.EndScrollView();
		}

		private void DrawProperties()
		{
			// no component type = no drawing
			if ( _componentType?.Type == null )
			{
				return;
			}

			// check component instance
			if ( _component == null || _component.GetType() != _componentType.Type )
			{
				_component = Activator.CreateInstance( _componentType.Type ) as ISharedComponentData;
				if ( _component == null )
				{
					return;
				}

				_fields = _componentType.Type.GetFields( BindingFlags.Instance | BindingFlags.Public )
					.Select( f => SharedComponentField.Create( _component, f ) )
					.ToArray();
				_allFieldsValid = !_fields.Any( f => f is SharedComponentFieldUndefined );
			}

			foreach ( var field in _fields )
			{
				field.Draw();
			}

			if ( !_allFieldsValid )
			{
				EditorGUILayout.LabelField( "Only components with primitives and enums are supported.\n" +
					"Composed data structures tends to more complex and less readable code and behavior.", s_BoldLabelStyle );
			}
			else
			{
				EditorGUILayout.LabelField( ToString(), s_BoldLabelStyle );
			}
		}
	}
}