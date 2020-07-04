using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEditor.Callbacks;

using UnityEngine;

using Object = UnityEngine.Object;

namespace XNode.Editor
{
	[InitializeOnLoad]
	public partial class NodeEditorWindow : EditorWindow
	{
		public static NodeEditorWindow current;

		public XNode.NodeGraph graph;

		private Dictionary<XNode.NodePort, Rect> _portConnectionPoints = new Dictionary<XNode.NodePort, Rect>();

		[SerializeField] private NodePortReference[] _references = new NodePortReference[0];

		[SerializeField] private Rect[] _rects = new Rect[0];

		private Dictionary<XNode.Node, Vector2> _nodeSizes = new Dictionary<XNode.Node, Vector2>();

		private Vector2 _panOffset;

		private float _zoom = 1;

		/// <summary>
		/// Stores node positions for all nodePorts.
		/// </summary>
		public Dictionary<XNode.NodePort, Rect> portConnectionPoints => _portConnectionPoints;

		public Dictionary<XNode.Node, Vector2> nodeSizes => _nodeSizes;

		public Vector2 panOffset { get => _panOffset; set { _panOffset = value; Repaint(); } }

		public float zoom { get => _zoom; set { _zoom = Mathf.Clamp( value, NodeEditorPreferences.GetSettings().minZoom, NodeEditorPreferences.GetSettings().maxZoom ); Repaint(); } }

		/// <summary>
		/// Create editor window
		/// </summary>
		public static NodeEditorWindow Init()
		{
			NodeEditorWindow w = CreateInstance<NodeEditorWindow>();
			w.titleContent = new GUIContent( "xNode" );
			w.wantsMouseMove = true;
			w.Show();
			return w;
		}

		[OnOpenAsset( 0 )]
		public static bool OnOpen( int instanceID, int line )
		{
			XNode.NodeGraph nodeGraph = EditorUtility.InstanceIDToObject(instanceID) as XNode.NodeGraph;
			if ( nodeGraph != null )
			{
				Open( nodeGraph );
				return true;
			}
			return false;
		}

		/// <summary>
		/// Open the provided graph in the NodeEditor
		/// </summary>
		public static void Open( XNode.NodeGraph graph )
		{
			if ( !graph )
			{
				return;
			}

			NodeEditorWindow w = Init(graph);
			w.wantsMouseMove = true;
			w.graph = graph;
		}

		/// <summary>
		/// Create editor window
		/// </summary>
		public static NodeEditorWindow Init( XNode.NodeGraph graph )
		{
			NodeEditorWindow w = null;
			foreach ( var window in Resources.FindObjectsOfTypeAll<NodeEditorWindow>() )
			{
				if ( window.titleContent.text.IndexOf( graph.name, StringComparison.InvariantCultureIgnoreCase ) == 0 )
				{
					w = window;
				}
			}

			if ( w == null )
			{
				w = CreateWindow<NodeEditorWindow>( typeof( NodeEditorWindow ) );
				w.titleContent = new GUIContent( "xNode" );
				w.wantsMouseMove = true;
				w.Show();
			}
			else
			{
				w.Focus();
			}

			return w;
		}

		/// <summary>
		/// Repaint all open NodeEditorWindows.
		/// </summary>
		public static void RepaintAll()
		{
			NodeEditorWindow[] windows = Resources.FindObjectsOfTypeAll<NodeEditorWindow>();
			for ( int i = 0; i < windows.Length; i++ )
			{
				windows[i].Repaint();
			}
		}

		public void Save()
		{
			if ( AssetDatabase.Contains( graph ) )
			{
				EditorUtility.SetDirty( graph );
				if ( NodeEditorPreferences.GetSettings().autoSave )
				{
					AssetDatabase.SaveAssets();
				}
			}
			else
			{
				SaveAs();
			}
		}

		public void SaveAs()
		{
			string path = EditorUtility.SaveFilePanelInProject("Save NodeGraph", "NewNodeGraph", "asset", "");
			if ( string.IsNullOrEmpty( path ) )
			{
				return;
			}
			else
			{
				XNode.NodeGraph existingGraph = AssetDatabase.LoadAssetAtPath<XNode.NodeGraph>(path);
				if ( existingGraph != null )
				{
					AssetDatabase.DeleteAsset( path );
				}

				AssetDatabase.CreateAsset( graph, path );
				EditorUtility.SetDirty( graph );
				if ( NodeEditorPreferences.GetSettings().autoSave )
				{
					AssetDatabase.SaveAssets();
				}
			}
		}

		public Vector2 WindowToGridPosition( Vector2 windowPosition ) => ( windowPosition - ( position.size * 0.5f ) - ( panOffset / zoom ) ) * zoom;

		public Vector2 GridToWindowPosition( Vector2 gridPosition ) => ( position.size * 0.5f ) + ( panOffset / zoom ) + ( gridPosition / zoom );

		public Rect GridToWindowRectNoClipped( Rect gridRect )
		{
			gridRect.position = GridToWindowPositionNoClipped( gridRect.position );
			return gridRect;
		}

		public Rect GridToWindowRect( Rect gridRect )
		{
			gridRect.position = GridToWindowPosition( gridRect.position );
			gridRect.size /= zoom;
			return gridRect;
		}

		public Vector2 GridToWindowPositionNoClipped( Vector2 gridPosition )
		{
			Vector2 center = position.size * 0.5f;
			// UI Sharpness complete fix - Round final offset not panOffset
			float xOffset = Mathf.Round(center.x * zoom + (panOffset.x + gridPosition.x));
			float yOffset = Mathf.Round(center.y * zoom + (panOffset.y + gridPosition.y));
			return new Vector2( xOffset, yOffset );
		}

		public void SelectNode( XNode.Node node, bool add )
		{
			if ( add )
			{
				List<Object> selection = new List<Object>( Selection.objects )
				{
					node
				};
				Selection.objects = selection.ToArray();
			}
			else
			{
				Selection.objects = new Object[] { node };
			}
		}

		public T[] Selected<T>() where T : XNode.Node => Selection.objects.OfType<T>().ToArray();

		public void DeselectNode( XNode.Node node )
		{
			List<Object> selection = new List<Object>(Selection.objects);
			selection.Remove( node );
			Selection.objects = selection.ToArray();
		}

		[InitializeOnLoadMethod]
		private static void OnLoad()
		{
			Selection.selectionChanged -= OnSelectionChanged;
			Selection.selectionChanged += OnSelectionChanged;
		}

		/// <summary>
		/// Handle Selection Change events
		/// </summary>
		private static void OnSelectionChanged()
		{
			XNode.NodeGraph nodeGraph = Selection.activeObject as XNode.NodeGraph;
			if ( nodeGraph && !AssetDatabase.Contains( nodeGraph ) )
			{
				Open( nodeGraph );
			}
		}

		private void OnDisable()
		{
			// Cache portConnectionPoints before serialization starts
			int count = portConnectionPoints.Count;
			_references = new NodePortReference[count];
			_rects = new Rect[count];
			int index = 0;
			foreach ( var portConnectionPoint in portConnectionPoints )
			{
				_references[index] = new NodePortReference( portConnectionPoint.Key );
				_rects[index] = portConnectionPoint.Value;
				index++;
			}
		}

		private void OnEnable()
		{
			// Reload portConnectionPoints if there are any
			int length = _references.Length;
			if ( length == _rects.Length )
			{
				for ( int i = 0; i < length; i++ )
				{
					XNode.NodePort nodePort = _references[i].GetNodePort();
					if ( nodePort != null )
					{
						_portConnectionPoints.Add( nodePort, _rects[i] );
					}
				}
			}
		}

		private void OnFocus()
		{
			current = this;
			ValidateGraphEditor();
			if ( graphEditor != null && NodeEditorPreferences.GetSettings().autoSave )
			{
				AssetDatabase.SaveAssets();
			}
		}

		/// <summary>
		/// Make sure the graph editor is assigned and to the right object
		/// </summary>
		private void ValidateGraphEditor()
		{
			NodeGraphEditor graphEditor = NodeGraphEditor.GetEditor(graph, this);
			if ( this.graphEditor != graphEditor )
			{
				this.graphEditor = graphEditor;
				graphEditor.OnOpen();
			}
		}

		private void DraggableWindow( int windowID ) => GUI.DragWindow();

		[System.Serializable]
		private class NodePortReference
		{
			[SerializeField] private XNode.Node _node;
			[SerializeField] private string _name;

			public NodePortReference( XNode.NodePort nodePort )
			{
				_node = nodePort.node;
				_name = nodePort.fieldName;
			}

			public XNode.NodePort GetNodePort()
			{
				if ( _node == null )
				{
					return null;
				}
				return _node.GetPort( _name );
			}
		}
	}
}