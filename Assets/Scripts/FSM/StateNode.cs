using FSM.Components;

using System.Collections.Generic;

using UnityEngine;

using XNode;

namespace FSM
{
	[NodeWidth( 300 )]
	[CreateNodeMenu( "State" )]
	public class StateNode : FSMNode
	{
		[Input(ShowBackingValue.Never, connectionType = ConnectionType.Multiple)]
		[SerializeField] private StateNode _from;

		[SerializeField] private ComponentLink[] _components = default;

		[Output(ShowBackingValue.Never, connectionType = ConnectionType.Multiple)]
		[SerializeField] private StateNode _to;

		public string StateName => Name;
		public IReadOnlyCollection<ComponentLink> Components => _components;

		public override object GetValue( NodePort port ) => this;
	}
}