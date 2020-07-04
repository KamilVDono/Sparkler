using XNode;

namespace FSM
{
	public class StateNode : FSMNode
	{
		[Input(ShowBackingValue.Never, connectionType = ConnectionType.Multiple)]
		public StateNode from;

		[Output(ShowBackingValue.Never, connectionType = ConnectionType.Multiple)]
		public StateNode to;

		public override object GetValue( NodePort port ) => this;
	}
}