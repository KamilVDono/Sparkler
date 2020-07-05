using UnityEngine;

using XNode.Editor;

namespace FSM.Editor
{
	[CustomNodeEditor( typeof( FSMNode ) )]
	public class FSMNodeEditor : XNode.Editor.NodeEditor
	{
		private Color? _nodeColor;
		protected FSMNode Target => target as FSMNode;

		private Color _NodeColor
		{
			get
			{
				if ( _nodeColor.HasValue )
				{
					return _nodeColor.Value;
				}

				_nodeColor = Target.Color;
				return _nodeColor.Value;
			}
		}

		public override void OnHeaderGUI() => GUILayout.Label( Target.Name, NodeEditorResources.styles.nodeHeader, GUILayout.Height( 30 ) );

		public override Color GetTint()
		{
			if ( !Target.IsRightConfigured() )
			{
				return Color.red;
			}
			return _NodeColor;
		}
	}

	public abstract class FSMNodeEditor<T> : FSMNodeEditor where T : FSMNode
	{
		protected new T Target => base.Target as T;
	}
}