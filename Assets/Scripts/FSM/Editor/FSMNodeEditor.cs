using UnityEngine;

using XNode.Editor;

namespace FSM.Editor
{
	[CustomNodeEditor( typeof( FSMNode ) )]
	public class FSMNodeEditor : XNode.Editor.NodeEditor
	{
		protected FSMNode Target => target as FSMNode;

		public override void OnHeaderGUI() => GUILayout.Label( Target.Name, NodeEditorResources.styles.nodeHeader, GUILayout.Height( 30 ) );

		public override Color GetTint()
		{
			if ( !Target.IsRightConfigured() )
			{
				return Color.red;
			}
			return base.GetTint();
		}
	}

	public abstract class FSMNodeEditor<T> : FSMNodeEditor where T : FSMNode
	{
		protected new T Target => base.Target as T;
	}
}