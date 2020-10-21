using Primitives;

using Sparkler.Editor.Components.SizeAnalysis;
using Sparkler.Utility;
using Sparkler.Utility.Editor;

using System;
using System.Linq;

using UnityEditor;
using UnityEditor.Compilation;

using UnityEngine;

namespace Sparkler.Editor.CodeGens
{
	public class ComponentCreatorWindow : EditorWindow
	{
		private const string NESTED_STRUCTS_WARNING = "\nBE AWARE:\nYou have nested structures so there is possibility you are not able to achieve best size";
		private static readonly GUIContent s_titleContent = new GUIContent("Component creator");
		private static readonly Type s_primitiveType = typeof(IPrimitiveType);

		private static string[] _namespaces;
		private SerializedObject _serializedObject;
		[SerializeField] private ComponentDefinition _componentDefinition;

		[MenuItem( "DOTS/Sparkler/Component creator", priority = 100 )]
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
			DrawSizeAnalysis();

			_serializedObject.Update();

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

		private void DrawSizeAnalysis()
		{
			var fields = _componentDefinition.Fields
					.Select(f => f.type.Type)
					.Where(t => t != null)
					.SelectMany(t => StructTypeSize.CollectFields(t))
					.ToArray();

			bool hasNested = _componentDefinition.Fields.Any(f => !((f.type.Type?.IsPrimitive ?? true) || s_primitiveType.IsAssignableFrom(f.type.Type)));

			var currentSize = StructTypeSize.GetCurrentStructSize( fields );
			var possibleSize = StructTypeSize.GetPossibleStructSize( fields );

			using ( new GUIEnabledScope( false ) )
			{
				EditorGUILayout.TextArea(
					S.Concat() + _componentDefinition.Fields.Length + " fields with size: " + currentSize
					+ "B, when possible " + possibleSize + " B" + ( hasNested ? NESTED_STRUCTS_WARNING : string.Empty ),
					EditorStyles.helpBox );
			}

			if ( possibleSize < currentSize && GUILayout.Button( "Try to reduce size" ) )
			{
				if ( _componentDefinition.Fields.Any( f => f.type.Type == null ) )
				{
					EditorUtility.DisplayDialog( "Invalid setup", "Component have filed without type, fix it", "OK" );
				}
				_componentDefinition.Fields = _componentDefinition
					.Fields
					.OrderByDescending( f => f.type.Type != null ? StructTypeSize.GetTypeSize( f.type.Type ) : 0 )
					.ToArray();
			}
			EditorGUILayout.Space();
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