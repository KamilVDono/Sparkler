using System.Linq;

using UnityEditor;

using UnityEngine;

namespace FSM.Editor.CodeGens
{
	public class ComponentCreatorWindow : EditorWindow
	{
		private static readonly GUIContent s_titleContent = new GUIContent("Component creator");
		private static readonly GUIContent s_emptyContent = new GUIContent("");

		private SerializedObject _serializedObject;

		[SerializeField] private ComponentDefinition _componentDefinition;

		[MenuItem( "FSM/Component creator" )]
		public static void ShowWindow() => ShowWindow( "", "", "" );

		public static void ShowWindow( string name, string namespaceName, string directory )
		{
			var window = EditorWindow.GetWindow<ComponentCreatorWindow>();
			window.titleContent = s_titleContent;
			window._serializedObject = new SerializedObject( window );
			window._componentDefinition = new ComponentDefinition()
			{
				ComponentName = name,
				Namespace = namespaceName,
				Directory = directory,
			};
			window.ShowUtility();
		}

		private void OnGUI()
		{
			_serializedObject.Update();
			string[] excludes = { "m_Script" };

			// Iterate through serialized properties and draw them like the Inspector (But with ports)
			SerializedProperty iterator = _serializedObject.FindProperty(nameof(_componentDefinition));
			bool enterChildren = true;
			EditorGUIUtility.labelWidth = 84;
			while ( iterator.NextVisible( enterChildren ) )
			{
				enterChildren = false;
				if ( excludes.Contains( iterator.name ) )
				{
					continue;
				}

				DrawProperty( iterator );
			}
			_serializedObject.ApplyModifiedProperties();

			DrawActionButtons();
		}

		private void DrawActionButtons()
		{
			EditorGUILayout.BeginHorizontal();

			if ( GUILayout.Button( "Close" ) )
			{
				var close = EditorUtility.DisplayDialog("Close", "Do you really want close component creator window?", "Yes. Close!", "No. Stay!");
				if ( close )
				{
					Close();
				}
			}

			if ( GUILayout.Button( "Create" ) )
			{
				CodeGenerator.Generate( _componentDefinition );
				Close();
			}

			EditorGUILayout.EndHorizontal();
		}

		private void DrawProperty( SerializedProperty serializedProperty )
		{
			if ( serializedProperty.isArray && serializedProperty.propertyType != SerializedPropertyType.String )
			{
				EditorGUILayout.PropertyField( serializedProperty );
			}
			else
			{
				EditorGUILayout.BeginHorizontal();

				EditorGUILayout.LabelField( serializedProperty.displayName, GUILayout.Width( 150 ) );
				EditorGUILayout.PropertyField( serializedProperty, s_emptyContent );

				EditorGUILayout.EndHorizontal();
			}
		}
	}
}