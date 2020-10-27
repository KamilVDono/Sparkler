using Sparkler.Editor.CodeGeneration;
using Sparkler.Utility;
using Sparkler.Utility.Editor;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Unity.Entities;

using UnityEditor;

using UnityEngine;

using XNode;
using XNode.Editor;

namespace Sparkler.Editor
{
	[CustomNodeGraphEditor( typeof( SystemsGraph ) )]
	public class SystemsGraphEditor : XNode.Editor.NodeGraphEditor
	{
		#region Consts
		private const int RefreshRate = 10;
		private static readonly GUIContent s_emptyContent = new GUIContent("");
		private static readonly GUIContent s_horizontalLine = new GUIContent("|");
		private static readonly GUIContent s_namespaceContent = new GUIContent("Namespace");
		private static readonly GUIContent s_generateContent = new GUIContent("Code generation path");
		private static readonly GUIContent s_stateEditigContent = new GUIContent("State editing");
		#endregion Consts

		private int _counter = 0;

		private SystemsGraph Target => (SystemsGraph)target;

		public override void OnOpen() => window.titleContent = new GUIContent( $"{target.name} - graph" );

		public override void OnGUI()
		{
			base.OnGUI();
			--_counter;
			if ( _counter > 0 )
			{
				return;
			}
			_counter = RefreshRate;

			bool anyChange = false;
			Target.nodes.OfType<SystemNode>().SelectMany( n => n.AllComponents ).ForEach( c => anyChange |= c.Validate() );
			if ( anyChange )
			{
				EditorUtility.SetDirty( Target );
				Target.nodes.ForEach( EditorUtility.SetDirty );
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
			}
		}

		#region Graph operations

		public override void AddContextMenuItems( GenericMenu menu )
		{
			NodePort draggedOutput = window.draggedOutput;

			Type[] parentTypes = {typeof(SystemNode)};

			if ( draggedOutput != null )
			{
				parentTypes = draggedOutput.AllowedTypes;
			}

			// Collect valid types
			HashSet<Type> availableTypes = new HashSet<Type>();
			foreach ( Type parentType in parentTypes )
			{
				foreach ( Type type in ReflectionExtensions.SubClassesWithBaseOf( parentType ) )
				{
					if ( type.IsAbstract )
					{
						continue;
					}
					availableTypes.Add( type );
				}
			}

			// If only one valid type just create it
			if ( availableTypes.Count == 1 )
			{
				Vector2 pos = NodeEditorWindow.current.WindowToGridPosition(Event.current.mousePosition);
				CreateNodeFromMenu( availableTypes.First(), pos, draggedOutput );
				return;
			}
			else
			{
				// Sort menu
				var options = availableTypes.Select(at => (at, NodeMenuPath(at))).Where( p => !string.IsNullOrWhiteSpace(p.Item2) ).OrderBy(op => op.Item2);
				// Add menu items
				foreach ( var typePathPair in options )
				{
					Vector2 pos = NodeEditorWindow.current.WindowToGridPosition(Event.current.mousePosition);
					menu.AddItem( new GUIContent( $"{typePathPair.Item2}" ), false, () => CreateNodeFromMenu( typePathPair.Item1, pos, draggedOutput ) );
				}
			}

			// Show only nodes menu if dropped output
			if ( draggedOutput == null )
			{
				base.AddContextMenuItems( menu );
			}
		}

		public override Color GetPortColor( NodePort port )
		{
			if ( port.node is SystemNode systemNode )
			{
				return systemNode.Color;
			}
			return base.GetPortColor( port );
		}

		private void CreateNodeFromMenu( Type type, Vector2 pos, NodePort draggedOutput )
		{
			CreateNode( type, pos, node =>
			{
				node.name = NodeName( type );
				// Try connect created node to current dragged output
				if ( draggedOutput != null )
				{
					foreach ( NodePort input in node.Inputs )
					{
						if ( draggedOutput.CanConnectTo( input ) )
						{
							draggedOutput.Connect( input );
							break;
						}
					}
				}
			} );
		}

		private string NodeName( Type nodeType )
		{
			if ( nodeType.GetCustomAttribute( typeof( Node.CreateNodeMenuAttribute ) ) is Node.CreateNodeMenuAttribute attr )
			{
				int slashIndex = attr.menuName.LastIndexOf('/');
				return attr.menuName.Substring( slashIndex + 1 );
			}
			return nodeType.Name.Replace( "Node", "" );
		}

		private string NodeMenuPath( Type nodeType )
		{
			if ( nodeType.GetCustomAttribute( typeof( Node.CreateNodeMenuAttribute ) ) is Node.CreateNodeMenuAttribute attr )
			{
				return attr.menuName;
			}
			return nodeType.Name.Replace( "Node", "" );
		}

		#endregion Graph operations

		#region Toolbar
		public override bool HasToolbar => true;

		public override void OnToolbar()
		{
			EditorGUILayout.LabelField( s_stateEditigContent, GUILayout.Width( 75 ) );
			Target.StateEditing = EditorGUILayout.Toggle( Target.StateEditing, GUILayout.Width( 15 ) );

			ToolbarSpace();

			EditorGUILayout.LabelField( s_generateContent, GUILayout.ExpandWidth( false ) );
			Target.CodeGenerationPath = EditorGUILayout.TextField( Target.CodeGenerationPath, GUILayout.ExpandWidth( true ) );
			if ( GUILayout.Button( "\u27b1", GUILayout.ExpandWidth( false ) ) )
			{
				var dialogPath = EditorUtility.OpenFolderPanel( "Code directory", "", "" );
				if ( !string.IsNullOrWhiteSpace( dialogPath ) )
				{
					Target.CodeGenerationPath = PathExtension.AssetsPath( dialogPath );
				}
			}

			ToolbarSpace();

			EditorStyles.label.CalcMinMaxWidth( s_namespaceContent, out var min, out var max );
			EditorGUILayout.LabelField( s_namespaceContent, GUILayout.Width( min ) );
			Target.Namespace = EditorGUILayout.TextField( Target.Namespace, GUILayout.ExpandWidth( true ) );

			ToolbarSpace();

			using ( new GUIEnabledScope( Target.StateEditing && Target.nodes.OfType<SystemNode>().All( n => n.IsRightConfigured().Item1 ) ) )
			{
				if ( GUILayout.Button( "Generate", GUILayout.Width( 120 ) ) )
				{
					CodeGenerator.Generate( Target );
				}
			}

			if ( GUILayout.Button( "Load system", GUILayout.Width( 120 ) ) )
			{
				LoadSystem();
			}
		}

		private static void ToolbarSpace()
		{
			EditorGUILayout.LabelField( s_emptyContent, GUILayout.Width( 4 ) );
			EditorGUILayout.LabelField( s_horizontalLine, GUILayout.Width( 4 ) );
			EditorGUILayout.LabelField( s_emptyContent, GUILayout.Width( 4 ) );
		}

		private void LoadSystem()
		{
			string path = EditorUtility.OpenFilePanel("Load system from c# code", Application.dataPath, "cs");
			if ( !File.Exists( path ) )
			{
				return;
			}

			var fileData = SystemReader.Read( path );
			if ( fileData == null )
			{
				EditorUtility.DisplayDialog( "Invalid file", "Can not load selected file", "OK" );
				return;
			}

			var newNode = SystemNode.FromFile( Target, fileData );
			if ( newNode == null )
			{
				EditorUtility.DisplayDialog( "Invalid file", "Selected file is already loaded", "OK" );
				return;
			}

			SetupNewNode( newNode );
		}

		#endregion Toolbar
	}
}