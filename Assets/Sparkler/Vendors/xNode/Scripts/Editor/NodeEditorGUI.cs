﻿using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;

using UnityEngine;

namespace Sparkler.XNode.Editor
{
	/// <summary>
	/// Contains GUI methods
	/// </summary>
	public partial class NodeEditorWindow
	{
		public NodeGraphEditor graphEditor;
		private List<UnityEngine.Object> selectionCache;
		private List<XNode.Node> culledNodes;
		private Matrix4x4 prevGuiMatrix;

		/// <summary>
		/// 19 if docked, 22 if not
		/// </summary>
		private int topPadding => isDocked() ? 19 : 22;

		/// <summary>
		/// Executed after all other window GUI. Useful if Zoom is ruining your day. Automatically
		/// resets after being run.
		/// </summary>
		public event Action onLateGUI;

		public static Rect ScaleSizeBy( Rect rect, float scale, Vector2 pivotPoint )
		{
			Rect result = rect;
			result.x -= pivotPoint.x;
			result.y -= pivotPoint.y;
			result.xMin *= scale;
			result.xMax *= scale;
			result.yMin *= scale;
			result.yMax *= scale;
			result.x += pivotPoint.x;
			result.y += pivotPoint.y;
			return result;
		}

		public static bool DropdownButton( string name, float width ) => GUILayout.Button( name, EditorStyles.toolbarDropDown, GUILayout.Width( width ) );

		public void BeginZoomed()
		{
			GUI.EndClip();

			GUIUtility.ScaleAroundPivot( Vector2.one / zoom, position.size * 0.5f );
			Vector4 padding = new Vector4(0, topPadding, 0, 0);
			padding *= zoom;
			GUI.BeginClip( new Rect( -( ( position.width * zoom ) - position.width ) * 0.5f, -( ( ( position.height * zoom ) - position.height ) * 0.5f ) + ( topPadding * zoom ),
					position.width * zoom,
					position.height * zoom ) );
		}

		public void EndZoomed()
		{
			GUIUtility.ScaleAroundPivot( Vector2.one * zoom, position.size * 0.5f );
			Vector3 offset = new Vector3(
								(((position.width * zoom) - position.width) * 0.5f),
								(((position.height * zoom) - position.height) * 0.5f) + (-topPadding * zoom) + topPadding,
								0);
			GUI.matrix = Matrix4x4.TRS( offset, Quaternion.identity, Vector3.one );
		}

		public void DrawGrid( Rect rect, float zoom, Vector2 panOffset )
		{
			rect.position = Vector2.zero;

			Vector2 center = rect.size / 2f;
			Texture2D gridTex = graphEditor.GetGridTexture();
			Texture2D crossTex = graphEditor.GetSecondaryGridTexture();

			// Offset from origin in tile units
			float xOffset = -(center.x * zoom + panOffset.x) / gridTex.width;
			float yOffset = ((center.y - rect.size.y) * zoom + panOffset.y) / gridTex.height;

			Vector2 tileOffset = new Vector2(xOffset, yOffset);

			// Amount of tiles
			float tileAmountX = Mathf.Round(rect.size.x * zoom) / gridTex.width;
			float tileAmountY = Mathf.Round(rect.size.y * zoom) / gridTex.height;

			Vector2 tileAmount = new Vector2(tileAmountX, tileAmountY);

			// Draw tiled background
			GUI.DrawTextureWithTexCoords( rect, gridTex, new Rect( tileOffset, tileAmount ) );
			GUI.DrawTextureWithTexCoords( rect, crossTex, new Rect( tileOffset + new Vector2( 0.5f, 0.5f ), tileAmount ) );
		}

		public void DrawSelectionBox()
		{
			if ( currentActivity == NodeActivity.DragGrid )
			{
				Vector2 curPos = WindowToGridPosition(Event.current.mousePosition);
				Vector2 size = curPos - dragBoxStart;
				Rect r = new Rect(dragBoxStart, size);
				r.position = GridToWindowPosition( r.position );
				r.size /= zoom;
				Handles.DrawSolidRectangleWithOutline( r, new Color( 0, 0, 0, 0.1f ), new Color( 1, 1, 1, 0.6f ) );
			}
		}

		/// <summary>
		/// Draw a bezier from output to input in grid coordinates
		/// </summary>
		public void DrawNoodle( Color col, List<Vector2> gridPoints, NodePort output = null, NodePort input = null )
		{
			Vector2 mousePos = Event.current.mousePosition;
			Vector2[] windowPoints = gridPoints.Select(x => GridToWindowPosition(x)).ToArray();
			Handles.color = col;
			int length = gridPoints.Count;
			switch ( NodeEditorPreferences.GetSettings().noodleType )
			{
				case NodeEditorPreferences.NoodleType.Curve:
					Vector2 outputTangent = Vector2.right;
					for ( int i = 0; i < length - 1; i++ )
					{
						Vector2 inputTangent = Vector2.left;

						if ( i == 0 )
						{
							outputTangent = Vector2.right * Vector2.Distance( windowPoints[i], windowPoints[i + 1] ) * 0.01f * zoom;
						}

						if ( i < length - 2 )
						{
							Vector2 ab = (windowPoints[i + 1] - windowPoints[i]).normalized;
							Vector2 cb = (windowPoints[i + 1] - windowPoints[i + 2]).normalized;
							Vector2 ac = (windowPoints[i + 2] - windowPoints[i]).normalized;
							Vector2 p = (ab + cb) * 0.5f;
							float tangentLength = (Vector2.Distance(windowPoints[i], windowPoints[i + 1]) + Vector2.Distance(windowPoints[i + 1], windowPoints[i + 2])) * 0.005f * zoom;
							float side = ((ac.x * (windowPoints[i + 1].y - windowPoints[i].y)) - (ac.y * (windowPoints[i + 1].x - windowPoints[i].x)));

							p = new Vector2( -p.y, p.x ) * Mathf.Sign( side ) * tangentLength;
							inputTangent = p;
						}
						else
						{
							inputTangent = Vector2.left * Vector2.Distance( windowPoints[i], windowPoints[i + 1] ) * 0.01f * zoom;
						}

						// bezier fields
						var startPos = windowPoints[i];
						var endPos = windowPoints[i + 1];
						var startTangent = windowPoints[i] + ((outputTangent * 50) / zoom);
						var endTangent = windowPoints[i + 1] + ((inputTangent * 50) / zoom);
						// If connection is selected draw outline bezier
						if ( selectedConnections.Any( c => c.outputPort == output && c.inputPort == input ) )
						{
							Handles.DrawBezier( startPos, endPos, startTangent, endTangent, Color.white, null, 8 );
						}
						// Draw bezier
						Handles.DrawBezier( startPos, endPos, startTangent, endTangent, col, null, 4 );
						// Check is bezier hovered by mouse
						if ( HandleUtility.DistancePointBezier( mousePos, startPos, endPos, startTangent, endTangent ) <= 3.5f )
						{
							hoveredConnection.outputPort = output;
							hoveredConnection.inputPort = input;
						}
						outputTangent = -inputTangent;
					}
					break;

				case NodeEditorPreferences.NoodleType.Line:
					for ( int i = 0; i < length - 1; i++ )
					{
						DrawAAPolyLineSelection( mousePos, 5, windowPoints[i], windowPoints[i + 1], output, input );
					}
					break;

				case NodeEditorPreferences.NoodleType.Angled:
					for ( int i = 0; i < length - 1; i++ )
					{
						if ( i == length - 1 )
						{
							continue; // Skip last index
						}

						if ( windowPoints[i].x <= windowPoints[i + 1].x - ( 50 / zoom ) )
						{
							float midpoint = (windowPoints[i].x + windowPoints[i + 1].x) * 0.5f;
							Vector2 start_1 = windowPoints[i];
							Vector2 end_1 = windowPoints[i + 1];
							start_1.x = midpoint;
							end_1.x = midpoint;
							DrawAAPolyLineSelection( mousePos, 5, windowPoints[i], start_1, output, input );
							DrawAAPolyLineSelection( mousePos, 5, start_1, end_1, output, input );
							DrawAAPolyLineSelection( mousePos, 5, end_1, windowPoints[i + 1], output, input );
						}
						else
						{
							float midpoint = (windowPoints[i].y + windowPoints[i + 1].y) * 0.5f;
							Vector2 start_1 = windowPoints[i];
							Vector2 end_1 = windowPoints[i + 1];
							start_1.x += 25 / zoom;
							end_1.x -= 25 / zoom;
							Vector2 start_2 = start_1;
							Vector2 end_2 = end_1;
							start_2.y = midpoint;
							end_2.y = midpoint;
							DrawAAPolyLineSelection( mousePos, 5, windowPoints[i], start_1, output, input );
							DrawAAPolyLineSelection( mousePos, 5, start_1, start_2, output, input );
							DrawAAPolyLineSelection( mousePos, 5, start_2, end_2, output, input );
							DrawAAPolyLineSelection( mousePos, 5, end_2, end_1, output, input );
							DrawAAPolyLineSelection( mousePos, 5, end_1, windowPoints[i + 1], output, input );
						}
					}
					break;
			}
		}

		/// <summary>
		/// Draws all connections
		/// </summary>
		public void DrawConnections()
		{
			Vector2 mousePos = Event.current.mousePosition;
			List<RerouteReference> selection = preBoxSelectionReroute != null ? new List<RerouteReference>(preBoxSelectionReroute) : new List<RerouteReference>();
			hoveredReroute = new RerouteReference();
			hoveredConnection = new ConnectionReference();

			Color col = GUI.color;
			foreach ( XNode.Node node in graph.nodes )
			{
				//If a null node is found, return. This can happen if the nodes associated script is deleted. It is currently not possible in Unity to delete a null asset.
				if ( node == null )
				{
					continue;
				}

				// Draw full connections and output > reroute
				foreach ( XNode.NodePort output in node.Outputs )
				{
					//Needs cleanup. Null checks are ugly
					if ( !_portConnectionPoints.TryGetValue( output, out Rect fromRect ) )
					{
						continue;
					}

					Color connectionColor = graphEditor.GetPortColor(output);

					for ( int k = 0; k < output.ConnectionCount; k++ )
					{
						XNode.NodePort input = output.GetConnection(k);

						// Error handling
						if ( input == null )
						{
							continue; //If a script has been updated and the port doesn't exist, it is removed and null is returned. If this happens, return.
						}

						if ( !input.IsConnectedTo( output ) )
						{
							input.Connect( output );
						}

						if ( !_portConnectionPoints.TryGetValue( input, out Rect toRect ) )
						{
							continue;
						}

						List<Vector2> reroutePoints = output.GetReroutePoints(k);

						List<Vector2> gridPoints = new List<Vector2>
						{
							fromRect.center
						};
						gridPoints.AddRange( reroutePoints );
						gridPoints.Add( toRect.center );
						DrawNoodle( connectionColor, gridPoints, output, input );

						// Loop through reroute points again and draw the points
						for ( int i = 0; i < reroutePoints.Count; i++ )
						{
							RerouteReference rerouteRef = new RerouteReference(output, k, i);
							// Draw reroute point at position
							Rect rect = new Rect(reroutePoints[i], new Vector2(12, 12));
							rect.position = new Vector2( rect.position.x - 6, rect.position.y - 6 );
							rect = GridToWindowRect( rect );

							// Draw selected reroute points with an outline
							if ( selectedReroutes.Contains( rerouteRef ) )
							{
								GUI.color = NodeEditorPreferences.GetSettings().highlightColor;
								GUI.DrawTexture( rect, NodeEditorResources.dotOuter );
							}

							GUI.color = connectionColor;
							GUI.DrawTexture( rect, NodeEditorResources.dot );
							if ( rect.Overlaps( selectionBox ) )
							{
								selection.Add( rerouteRef );
							}

							if ( rect.Contains( mousePos ) )
							{
								hoveredReroute = rerouteRef;
								hoveredConnection.outputPort = null;
							}
						}
					}
				}
			}
			GUI.color = col;
			if ( Event.current.type != EventType.Layout && currentActivity == NodeActivity.DragGrid )
			{
				selectedReroutes = selection;
			}
		}

		/// <summary>
		/// Do <see cref="Handles.DrawAAPolyLine"/> but cares about selection
		/// </summary>
		private void DrawAAPolyLineSelection( Vector2 mousePos, float width, Vector3 startPoint, Vector3 endPoint, NodePort output, NodePort input )
		{
			if ( selectedConnections.Any( c => c.outputPort == output && c.inputPort == input ) )
			{
				Handles.DrawAAPolyLine( width * 2, startPoint, endPoint );
			}
			Handles.DrawAAPolyLine( width, startPoint, endPoint );
			if ( HandleUtility.DistancePointLine( mousePos, startPoint, endPoint ) <= 3.5f )
			{
				hoveredConnection.outputPort = output;
				hoveredConnection.inputPort = input;
			}
		}

		private void OnGUI()
		{
			Event e = Event.current;
			Matrix4x4 m = GUI.matrix;
			if ( graph == null )
			{
				return;
			}

			ValidateGraphEditor();
			Controls();

			DrawGrid( position, zoom, panOffset );

			DrawConnections();
			DrawDraggedConnection();
			DrawNodes();
			DrawSelectionBox();
			DrawTooltip();
			if ( graphEditor.HasToolbar )
			{
				DrawToolbar();
			}
			graphEditor.OnGUI();

			// Run and reset onLateGUI
			if ( onLateGUI != null )
			{
				onLateGUI();
				onLateGUI = null;
			}

			GUI.matrix = m;
		}

		private void DrawToolbar()
		{
			var rect = EditorGUILayout.BeginVertical(GUILayout.Height(EditorGUIUtility.singleLineHeight + 8), GUILayout.ExpandWidth(true));
			EditorGUI.DrawRect( rect, graphEditor.GetToolbarColor() );
			EditorGUILayout.Space( 2 );
			EditorGUILayout.BeginHorizontal();

			graphEditor.OnToolbar();

			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();
		}

		/// <summary>
		/// Show right-click context menu for hovered reroute
		/// </summary>
		private void ShowRerouteContextMenu( RerouteReference reroute )
		{
			GenericMenu contextMenu = new GenericMenu();
			contextMenu.AddItem( new GUIContent( "Remove" ), false, () => reroute.RemovePoint() );
			contextMenu.DropDown( new Rect( Event.current.mousePosition, Vector2.zero ) );
			if ( NodeEditorPreferences.GetSettings().autoSave )
			{
				AssetDatabase.SaveAssets();
			}
		}

		/// <summary>
		/// Show right-click context menu for hovered port
		/// </summary>
		private void ShowPortContextMenu( XNode.NodePort hoveredPort )
		{
			GenericMenu contextMenu = new GenericMenu();
			contextMenu.AddItem( new GUIContent( "Clear Connections" ), false, () => hoveredPort.ClearConnections() );
			contextMenu.DropDown( new Rect( Event.current.mousePosition, Vector2.zero ) );
			if ( NodeEditorPreferences.GetSettings().autoSave )
			{
				AssetDatabase.SaveAssets();
			}
		}

		private void DrawNodes()
		{
			Event e = Event.current;
			if ( e.type == EventType.Layout )
			{
				selectionCache = new List<UnityEngine.Object>( Selection.objects );
			}

			System.Reflection.MethodInfo onValidate = null;
			if ( Selection.activeObject != null && Selection.activeObject is XNode.Node )
			{
				onValidate = Selection.activeObject.GetType().GetMethod( "OnValidate" );
				if ( onValidate != null )
				{
					EditorGUI.BeginChangeCheck();
				}
			}

			BeginZoomed();

			Vector2 mousePos = Event.current.mousePosition;

			if ( e.type != EventType.Layout )
			{
				hoveredNode = null;
				hoveredPort = null;
			}

			List<UnityEngine.Object> preSelection = preBoxSelection != null ? new List<UnityEngine.Object>(preBoxSelection) : new List<UnityEngine.Object>();

			// Selection box stuff
			Vector2 boxStartPos = GridToWindowPositionNoClipped(dragBoxStart);
			Vector2 boxSize = mousePos - boxStartPos;
			if ( boxSize.x < 0 ) { boxStartPos.x += boxSize.x; boxSize.x = Mathf.Abs( boxSize.x ); }
			if ( boxSize.y < 0 ) { boxStartPos.y += boxSize.y; boxSize.y = Mathf.Abs( boxSize.y ); }
			Rect selectionBox = new Rect(boxStartPos, boxSize);

			//Save guiColor so we can revert it
			Color guiColor = GUI.color;

			if ( e.type == EventType.Layout )
			{
				culledNodes = new List<XNode.Node>();
			}

			for ( int n = 0; n < graph.nodes.Count; n++ )
			{
				// Skip null nodes. The user could be in the process of renaming scripts, so removing them
				// at this point is not advisable.
				if ( graph.nodes[n] == null )
				{
					continue;
				}

				if ( n >= graph.nodes.Count )
				{
					return;
				}

				XNode.Node node = graph.nodes[n];

				// Culling
				if ( e.type == EventType.Layout )
				{
					// Cull unselected nodes outside view
					if ( !Selection.Contains( node ) && ShouldBeCulled( node ) )
					{
						culledNodes.Add( node );
						continue;
					}
				}
				else if ( culledNodes.Contains( node ) )
				{
					continue;
				}

				if ( e.type == EventType.Repaint )
				{
					_portConnectionPoints = _portConnectionPoints.Where( x => x.Key.node != node ).ToDictionary( kvp => kvp.Key, kvp => kvp.Value );
				}

				NodeEditor nodeEditor = NodeEditor.GetEditor(node, this);

				NodeEditor.portPositions.Clear();

				//Get node position
				Vector2 nodePos = GridToWindowPositionNoClipped(node.position);

				GUILayout.BeginArea( new Rect( nodePos, new Vector2( nodeEditor.GetWidth(), 4000 ) ) );

				bool selected = selectionCache.Contains(graph.nodes[n]);

				if ( selected )
				{
					GUIStyle style = new GUIStyle(nodeEditor.GetBodyStyle());
					GUIStyle highlightStyle = new GUIStyle( NodeEditorResources.styles.nodeHighlight )
					{
						padding = style.padding
					};
					style.padding = new RectOffset();
					GUI.color = nodeEditor.GetTint();
					GUILayout.BeginVertical( style );
					GUI.color = NodeEditorPreferences.GetSettings().highlightColor;
					GUILayout.BeginVertical( new GUIStyle( highlightStyle ) );
				}
				else
				{
					GUIStyle style = new GUIStyle(nodeEditor.GetBodyStyle());
					GUI.color = nodeEditor.GetTint();
					GUILayout.BeginVertical( style );
				}

				GUI.color = guiColor;
				EditorGUI.BeginChangeCheck();

				//Draw node contents
				nodeEditor.OnHeaderGUI();
				nodeEditor.OnBodyGUI();

				//If user changed a value, notify other scripts through onUpdateNode
				if ( EditorGUI.EndChangeCheck() )
				{
					if ( NodeEditor.onUpdateNode != null )
					{
						NodeEditor.onUpdateNode( node );
					}

					EditorUtility.SetDirty( node );
					nodeEditor.serializedObject.ApplyModifiedProperties();
				}

				GUILayout.EndVertical();

				//Cache data about the node for next frame
				if ( e.type == EventType.Repaint )
				{
					Vector2 size = GUILayoutUtility.GetLastRect().size;
					if ( nodeSizes.ContainsKey( node ) )
					{
						nodeSizes[node] = size;
					}
					else
					{
						nodeSizes.Add( node, size );
					}

					foreach ( var kvp in NodeEditor.portPositions )
					{
						Vector2 portHandlePos = kvp.Value;
						portHandlePos += node.position;
						Rect rect = new Rect(portHandlePos.x - 12, portHandlePos.y - 12, 24, 24);
						portConnectionPoints[kvp.Key] = rect;
					}
				}

				if ( selected )
				{
					GUILayout.EndVertical();
				}

				if ( e.type != EventType.Layout )
				{
					//Check if we are hovering this node
					Vector2 nodeSize = GUILayoutUtility.GetLastRect().size;
					Rect windowRect = new Rect(nodePos, nodeSize);
					if ( windowRect.Contains( mousePos ) )
					{
						hoveredNode = node;
					}

					//If dragging a selection box, add nodes inside to selection
					if ( currentActivity == NodeActivity.DragGrid )
					{
						if ( windowRect.Overlaps( selectionBox ) )
						{
							preSelection.Add( node );
						}
					}

					//Check if we are hovering any of this nodes ports
					//Check input ports
					foreach ( XNode.NodePort input in node.Inputs )
					{
						//Check if port rect is available
						if ( !portConnectionPoints.ContainsKey( input ) )
						{
							continue;
						}

						Rect r = GridToWindowRectNoClipped(portConnectionPoints[input]);
						if ( r.Contains( mousePos ) )
						{
							hoveredPort = input;
						}
					}
					//Check all output ports
					foreach ( XNode.NodePort output in node.Outputs )
					{
						//Check if port rect is available
						if ( !portConnectionPoints.ContainsKey( output ) )
						{
							continue;
						}

						Rect r = GridToWindowRectNoClipped(portConnectionPoints[output]);
						if ( r.Contains( mousePos ) )
						{
							hoveredPort = output;
						}
					}
				}

				GUILayout.EndArea();
			}

			if ( e.type != EventType.Layout && currentActivity == NodeActivity.DragGrid )
			{
				Selection.objects = preSelection.ToArray();
			}

			EndZoomed();

			//If a change in is detected in the selected node, call OnValidate method.
			//This is done through reflection because OnValidate is only relevant in editor,
			//and thus, the code should not be included in build.
			if ( onValidate != null && EditorGUI.EndChangeCheck() )
			{
				onValidate.Invoke( Selection.activeObject, null );
			}
		}

		private bool ShouldBeCulled( XNode.Node node )
		{
			Vector2 nodePos = GridToWindowPositionNoClipped(node.position);
			if ( nodePos.x / _zoom > position.width )
			{
				return true; // Right
			}
			else if ( nodePos.y / _zoom > position.height )
			{
				return true; // Bottom
			}
			else if ( nodeSizes.ContainsKey( node ) )
			{
				Vector2 size = nodeSizes[node];
				if ( nodePos.x + size.x < 0 )
				{
					return true; // Left
				}
				else if ( nodePos.y + size.y < 0 )
				{
					return true; // Top
				}
			}
			return false;
		}

		private void DrawTooltip()
		{
			if ( hoveredPort != null && NodeEditorPreferences.GetSettings().portTooltips )
			{
				Type type = hoveredPort.ValueType;
				GUIContent content = new GUIContent
				{
					text = type.PrettyName()
				};
				if ( hoveredPort.IsOutput )
				{
					object obj = hoveredPort.node.GetValue(hoveredPort);
					content.text += " = " + ( obj != null ? obj.ToString() : "null" );
				}
				Vector2 size = NodeEditorResources.styles.tooltip.CalcSize(content);
				Rect rect = new Rect(Event.current.mousePosition - (size), size);
				EditorGUI.LabelField( rect, content, NodeEditorResources.styles.tooltip );
				Repaint();
			}
		}

		private Rect TooltipRect()
		{
			Rect tooltipRect = new Rect(0, 0, position.width, EditorGUIUtility.singleLineHeight);
			return tooltipRect;
		}
	}
}