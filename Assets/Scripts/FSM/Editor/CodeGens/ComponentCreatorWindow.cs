using Rotorz.Games;

using System;
using System.Linq;

using UnityEditor;

using UnityEngine;

namespace FSM.Editor.Assets.Scripts.FSM.Editor.CodeGens
{
	public class ComponentCreatorWindow : EditorWindow
	{
		private static readonly GUIContent s_titleContent = new GUIContent("Component creator");
		private static readonly GUIContent s_emptyContent = new GUIContent("");

		private SerializedObject _serializedObject;

		[SerializeField] private string _componentName = "";
		[SerializeField] private string _namespace = "";
		[SerializeField] private ComponentType _componentType = ComponentType.ComponentData;
		[SerializeField] private ComponentField[] _fields = new ComponentField[0];

		[MenuItem( "FSM/Component creator" )]
		public static void ShowWindow()
		{
			var window =  ScriptableObject.CreateInstance<ComponentCreatorWindow>();
			window.titleContent = s_titleContent;
			window._serializedObject = new SerializedObject( window );
			window.ShowModalUtility();
		}

		private void OnGUI()
		{
			_serializedObject.Update();
			string[] excludes = { "m_Script" };

			// Iterate through serialized properties and draw them like the Inspector (But with ports)
			SerializedProperty iterator = _serializedObject.GetIterator();
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

		private enum ComponentType
		{
			ComponentData = 1,
			SharedComponentData = 2
		}

		public class ComponentFieldBlacklistedNamespacesAttribute : BlacklistedNamespacesAttribute
		{
			public ComponentFieldBlacklistedNamespacesAttribute() :
				base( true, @"Mono\..+", @"System\..+", @"JetBrains", @"Bee.", @"NUnit", @"Microsoft\..+", @"Novell\..+",
					@"ExCSS", @"NiceIO", @"ICSharpCode", @"Unity.Build", @"Newtonsoft\..+", @"Rider", @"TMPro", @"UnityEditor",
					@"Editor", @"SyntaxTree\..+", @"Unity.Profiling", @"Rotorz" )
			{
			}
		}

		[Serializable]
		private class ComponentField
		{
			public string name;

			[ClassTypeReferenceAttributes( typeof( OnlyBlittableAttribute ), typeof(ComponentFieldBlacklistedNamespacesAttribute))]
			public ClassTypeReference type;
		}
	}
}