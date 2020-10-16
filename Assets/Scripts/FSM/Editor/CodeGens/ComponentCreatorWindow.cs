using FSM.Utility;
using FSM.Utility.Editor;

using System;
using System.Linq;

using UnityEditor;
using UnityEditor.Compilation;

using UnityEngine;

namespace FSM.Editor.CodeGens
{
	public class ComponentCreatorWindow : EditorWindow
	{
		private static readonly GUIContent s_titleContent = new GUIContent("Component creator");

		private static string[] _namespaces;
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

		private void OnEnable()
		{
			if ( _namespaces != null )
			{
				return;
			}
			var unityAssemblies = CompilationPipeline
				.GetAssemblies( AssembliesType.PlayerWithoutTestAssemblies )
				.Select( ua => ua.name )
				.Where(n => !n.StartsWith("unity", StringComparison.InvariantCultureIgnoreCase) && !n.StartsWith("system", StringComparison.InvariantCultureIgnoreCase));

			_namespaces = AppDomain.CurrentDomain.GetAssemblies()
				.Where( a => unityAssemblies.Contains( a.GetName().Name ) )
				.SelectMany( a => a.GetTypes() )
				.Select( t => t.Namespace )
				.Where( n => !string.IsNullOrWhiteSpace( n ) )
				.Distinct().ToArray();
		}

		private void OnGUI()
		{
			_serializedObject.Update();
			string[] excludes = { "m_Script" };

			// Iterate through serialized properties and draw them like the Inspector (But with ports)
			SerializedProperty componentProperty = _serializedObject.FindProperty(nameof(_componentDefinition));

			// Directory
			EditorGUILayout.BeginHorizontal();
			var directoryProperty = componentProperty.FindPropertyRelative( "Directory" );
			GUIDrawers.DrawFieldWithLabel( directoryProperty );
			if ( GUILayout.Button( "\u27b1", GUILayout.Width( 30 ) ) )
			{
				var dialogPath = EditorUtility.OpenFolderPanel( "Code directory", "", "" );
				if ( !string.IsNullOrWhiteSpace( dialogPath ) )
				{
					directoryProperty.stringValue = PathExtension.AssetsPath( dialogPath );
				}
			}
			EditorGUILayout.EndHorizontal();

			// Namespace
			var namespaceProperty = componentProperty.FindPropertyRelative( "Namespace" );
			using ( new GUIEnabledScope( false ) )
			{
				if ( !namespaceProperty.stringValue.EndsWith( ".Components", StringComparison.InvariantCultureIgnoreCase ) )
				{
					EditorGUILayout.TextArea( "Remember, if namespace do not ends with \".Components\" system will automatically add it", EditorStyles.helpBox );
				}
			}
			EditorGUILayout.BeginHorizontal();
			GUIDrawers.DrawFieldWithLabel( namespaceProperty );
			var selectedNamespace = DrawNamespacesPopup( namespaceProperty.stringValue );
			if ( !string.IsNullOrWhiteSpace( selectedNamespace ) )
			{
				namespaceProperty.stringValue = selectedNamespace;
			}
			EditorGUILayout.EndHorizontal();

			// Type & Name
			EditorGUILayout.BeginHorizontal();
			GUIDrawers.DrawFieldWithLabel( componentProperty.FindPropertyRelative( "ComponentType" ) );
			GUIDrawers.DrawFieldWithLabel( componentProperty.FindPropertyRelative( "ComponentName" ) );
			EditorGUILayout.EndHorizontal();

			// Fields
			GUIDrawers.DrawArray<object>( componentProperty.FindPropertyRelative( "Fields" ) );

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

		private string DrawNamespacesPopup( string oldOption )
		{
			var oldIndex =_namespaces.IndexOf(oldOption);
			oldIndex = Mathf.Clamp( oldIndex, -1, _namespaces.Length - 1 );
			var newIndex = EditorGUILayout.Popup(oldIndex, _namespaces);
			if ( oldIndex != newIndex && newIndex >= 0 )
			{
				return _namespaces[newIndex];
			}
			return string.Empty;
		}
	}
}