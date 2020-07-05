using FSM.Utility;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEditor;

using UnityEngine;

using XNode;
using XNode.Editor;

namespace FSM.Editor
{
	[CustomNodeGraphEditor( typeof( FSMGraph ) )]
	public class FSMGraphEditor : XNode.Editor.NodeGraphEditor
	{
		public override void OnOpen()
		{
			base.OnOpen();
			window.titleContent = new GUIContent( $"{target.name} - FSM" );
		}

		public override void AddContextMenuItems( GenericMenu menu )
		{
			NodePort draggedOutput = window.draggedOutput;

			Type[] parentTypes = {typeof(FSMNode)};

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
			if ( port.node is FSMNode fsmNode )
			{
				return fsmNode.Color;
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
	}
}