using System;

using UnityEditor;

using UnityEngine;

namespace XNode.Editor
{
	/// <summary>
	/// Base class to derive custom Node Graph editors from. Use this to override how graphs are drawn
	/// in the editor.
	/// </summary>
	[CustomNodeGraphEditor( typeof( XNode.NodeGraph ) )]
	public class NodeGraphEditor : XNode.Editor.NodeEditorBase<NodeGraphEditor, NodeGraphEditor.CustomNodeGraphEditorAttribute, XNode.NodeGraph>
	{
		/// <summary>
		/// Are we currently renaming a node?
		/// </summary>
		protected bool isRenaming;

		public virtual bool HasToolbar => false;

		[Obsolete( "Use window.position instead" )]
		public Rect position { get => window.position; set => window.position = value; }

		public virtual void OnToolbar()
		{
		}

		public virtual void OnGUI()
		{
		}

		/// <summary>
		/// Called when opened by NodeEditorWindow
		/// </summary>
		public virtual void OnOpen() { }

		/// <summary>
		/// Called when closed by NodeEditorWindow
		/// </summary>
		public virtual void OnClose() { }

		public virtual Texture2D GetGridTexture() => NodeEditorPreferences.GetSettings().gridTexture;

		public virtual Texture2D GetSecondaryGridTexture() => NodeEditorPreferences.GetSettings().crossTexture;

		public Color GetToolbarColor() => NodeEditorPreferences.GetSettings().toolbarColor;

		/// <summary>
		/// Return default settings for this graph type. This is the settings the user will load if no
		/// previous settings have been saved.
		/// </summary>
		public virtual NodeEditorPreferences.Settings GetDefaultPreferences() => new NodeEditorPreferences.Settings();

		/// <summary>
		/// Returns context node menu path. Null or empty strings for hidden nodes.
		/// </summary>
		public virtual string GetNodeMenuName( Type type )
		{
			//Check if type has the CreateNodeMenuAttribute
			if ( NodeEditorUtilities.GetAttrib( type, out XNode.Node.CreateNodeMenuAttribute attrib ) ) // Return custom path
			{
				return attrib.menuName;
			}
			else // Return generated path
			{
				return ObjectNames.NicifyVariableName( type.ToString().Replace( '.', '/' ) );
			}
		}

		/// <summary>
		/// Add items for the context menu when right-clicking this node. Override to add custom menu items.
		/// </summary>
		public virtual void AddContextMenuItems( GenericMenu menu )
		{
			Vector2 pos = NodeEditorWindow.current.WindowToGridPosition(Event.current.mousePosition);
			for ( int i = 0; i < NodeEditorWindow.nodeTypes.Length; i++ )
			{
				Type type = NodeEditorWindow.nodeTypes[i];

				//Get node context menu path
				string path = GetNodeMenuName(type);
				if ( string.IsNullOrEmpty( path ) )
				{
					continue;
				}

				menu.AddItem( new GUIContent( path ), false, () =>
				{
					CreateNode( type, pos );
				} );
			}
			menu.AddSeparator( "" );
			menu.AddItem( new GUIContent( "Preferences" ), false, () => NodeEditorWindow.OpenPreferences() );
			NodeEditorWindow.AddCustomContextMenuItems( menu, target );
		}

		public virtual Color GetPortColor( XNode.NodePort port ) => GetTypeColor( port.ValueType );

		public virtual Color GetTypeColor( Type type ) => NodeEditorPreferences.GetTypeColor( type );

		/// <summary>
		/// Create a node and save it in the graph asset
		/// </summary>
		public virtual void CreateNode( Type type, Vector2 position ) => CreateNode( type, position, null );

		public virtual void CreateNode( Type type, Vector2 position, Action<Node> config )
		{
			XNode.Node node = target.AddNode(type);
			node.position = position;
			if ( string.IsNullOrEmpty( node.name ) )
			{
				// Automatically remove redundant 'Node' postfix
				string typeName = type.Name;
				if ( typeName.EndsWith( "Node" ) )
				{
					typeName = typeName.Substring( 0, typeName.LastIndexOf( "Node" ) );
				}

				node.name = UnityEditor.ObjectNames.NicifyVariableName( typeName );
			}
			AssetDatabase.AddObjectToAsset( node, target );
			if ( NodeEditorPreferences.GetSettings().autoSave )
			{
				AssetDatabase.SaveAssets();
			}

			NodeEditorWindow.RepaintAll();
			config?.Invoke( node );
		}

		/// <summary>
		/// Creates a copy of the original node in the graph
		/// </summary>
		public XNode.Node CopyNode( XNode.Node original )
		{
			XNode.Node node = target.CopyNode(original);
			node.name = original.name;
			AssetDatabase.AddObjectToAsset( node, target );
			if ( NodeEditorPreferences.GetSettings().autoSave )
			{
				AssetDatabase.SaveAssets();
			}

			return node;
		}

		/// <summary>
		/// Safely remove a node and all its connections.
		/// </summary>
		public virtual void RemoveNode( XNode.Node node )
		{
			target.RemoveNode( node );
			UnityEngine.Object.DestroyImmediate( node, true );
			if ( NodeEditorPreferences.GetSettings().autoSave )
			{
				AssetDatabase.SaveAssets();
			}
		}

		[AttributeUsage( AttributeTargets.Class )]
		public class CustomNodeGraphEditorAttribute : Attribute,
			XNode.Editor.NodeEditorBase<NodeGraphEditor, NodeGraphEditor.CustomNodeGraphEditorAttribute, XNode.NodeGraph>.INodeEditorAttrib
		{
			public string editorPrefsKey;
			private Type inspectedType;

			/// <summary>
			/// Tells a NodeGraphEditor which Graph type it is an editor for
			/// </summary>
			/// <param name="inspectedType">Type that this editor can edit</param>
			/// <param name="editorPrefsKey">Define unique key for unique layout settings instance</param>
			public CustomNodeGraphEditorAttribute( Type inspectedType, string editorPrefsKey = "xNode.Settings" )
			{
				this.inspectedType = inspectedType;
				this.editorPrefsKey = editorPrefsKey;
			}

			public Type GetInspectedType() => inspectedType;
		}
	}
}