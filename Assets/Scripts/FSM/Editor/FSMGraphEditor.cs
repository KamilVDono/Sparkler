using UnityEngine;

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
	}
}