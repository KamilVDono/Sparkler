using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;

using UnityEngine;

namespace XNode.Editor
{
	/// <summary>
	/// Base class to derive custom Node editors from. Use this to create your own custom inspectors
	/// and editors for your nodes.
	/// </summary>

	[CustomNodeEditor( typeof( XNode.Node ) )]
	public class NodeEditor : XNode.Editor.NodeEditorBase<NodeEditor, NodeEditor.CustomNodeEditorAttribute, XNode.Node>
	{
		public static readonly Dictionary<XNode.NodePort, Vector2> portPositions = new Dictionary<XNode.NodePort, Vector2>();

		/// <summary>
		/// Fires every whenever a node was modified through the editor
		/// </summary>
		public static Action<XNode.Node> onUpdateNode;

		public virtual void OnHeaderGUI() => GUILayout.Label( target.name, NodeEditorResources.styles.nodeHeader, GUILayout.Height( 30 ) );

		/// <summary>
		/// Draws standard field editors for all public fields
		/// </summary>
		public virtual void OnBodyGUI()
		{
			// Unity specifically requires this to save/update any serial object.
			// serializedObject.Update(); must go at the start of an inspector gui, and
			// serializedObject.ApplyModifiedProperties(); goes at the end.
			serializedObject.Update();
			string[] excludes = { "m_Script", "graph", "position", "ports" };

			// Iterate through serialized properties and draw them like the Inspector (But with ports)
			SerializedProperty iterator = serializedObject.GetIterator();
			bool enterChildren = true;
			EditorGUIUtility.labelWidth = 84;
			while ( iterator.NextVisible( enterChildren ) )
			{
				enterChildren = false;
				if ( excludes.Contains( iterator.name ) )
				{
					continue;
				}

				NodeEditorGUILayout.PropertyField( iterator, true );
			}

			// Iterate through dynamic ports and draw them in the order in which they are serialized
			foreach ( XNode.NodePort dynamicPort in target.DynamicPorts )
			{
				if ( NodeEditorGUILayout.IsDynamicPortListPort( dynamicPort ) )
				{
					continue;
				}

				NodeEditorGUILayout.PortField( dynamicPort );
			}

			serializedObject.ApplyModifiedProperties();
		}

		public virtual int GetWidth()
		{
			Type type = target.GetType();
			if ( NodeEditorWindow.nodeWidth.TryGetValue( type, out var width ) )
			{
				return width;
			}
			else
			{
				return 208;
			}
		}

		public virtual Color GetTint()
		{
			Type type = target.GetType();
			if ( NodeEditorWindow.nodeTint.TryGetValue( type, out Color color ) )
			{
				return color;
			}
			else
			{
				return Color.white;
			}
		}

		public virtual GUIStyle GetBodyStyle() => NodeEditorResources.styles.nodeBody;

		/// <summary>
		/// Add items for the context menu when right-clicking this node. Override to add custom
		/// menu items.
		/// </summary>
		public virtual void AddContextMenuItems( GenericMenu menu )
		{
			// Actions if only one node is selected
			if ( Selection.objects.Length == 1 && Selection.activeObject is XNode.Node )
			{
				XNode.Node node = Selection.activeObject as XNode.Node;
				menu.AddItem( new GUIContent( "Move To Top" ), false, () => NodeEditorWindow.current.MoveNodeToTop( node ) );
				menu.AddItem( new GUIContent( "Rename" ), false, NodeEditorWindow.current.RenameSelectedNode );
			}

			// Add actions to any number of selected nodes
			menu.AddItem( new GUIContent( "Duplicate" ), false, NodeEditorWindow.current.DuplicateSelectedNodes );
			menu.AddItem( new GUIContent( "Remove" ), false, NodeEditorWindow.current.RemoveSelectedNodes );

			// Custom sctions if only one node is selected
			if ( Selection.objects.Length == 1 && Selection.activeObject is XNode.Node )
			{
				XNode.Node node = Selection.activeObject as XNode.Node;
				NodeEditorWindow.AddCustomContextMenuItems( menu, node );
			}
		}

		/// <summary>
		/// Rename the node asset. This will trigger a reimport of the node.
		/// </summary>
		public void Rename( string newName )
		{
			if ( newName == null || newName.Trim() == "" )
			{
				newName = UnityEditor.ObjectNames.NicifyVariableName( target.GetType().Name );
			}

			target.name = newName;
			AssetDatabase.ImportAsset( AssetDatabase.GetAssetPath( target ) );
		}

		[AttributeUsage( AttributeTargets.Class )]
		public class CustomNodeEditorAttribute : Attribute,
		XNode.Editor.NodeEditorBase<NodeEditor, NodeEditor.CustomNodeEditorAttribute, XNode.Node>.INodeEditorAttrib
		{
			private Type inspectedType;

			/// <summary>
			/// Tells a NodeEditor which Node type it is an editor for
			/// </summary>
			/// <param name="inspectedType">Type that this editor can edit</param>
			public CustomNodeEditorAttribute( Type inspectedType ) => this.inspectedType = inspectedType;

			public Type GetInspectedType() => inspectedType;
		}
	}
}