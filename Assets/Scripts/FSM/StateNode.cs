using FSM.Components;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using XNode;

namespace FSM
{
	[NodeWidth( 300 )]
	[CreateNodeMenu( "State" )]
	public class StateNode : FSMNode
	{
		[Input(ShowBackingValue.Never, connectionType = ConnectionType.Multiple)]
		[SerializeField] private StateNode _from = null;

		[SerializeField] private ComponentLink[] _components = new ComponentLink[0];
		[SerializeField] private SystemLambdaAction[] _lambda = new SystemLambdaAction[0];

		[Output(ShowBackingValue.Never, connectionType = ConnectionType.Multiple)]
		[SerializeField] private StateNode _to = null;

		public string StateName => Name;
		public IReadOnlyCollection<ComponentLink> Components => _components;

		public override object GetValue( NodePort port ) => this;

		#region Setup validation

		protected override IEnumerable<Func<(bool, string)>> ConfigurationCheckers => new Func<(bool, string)>[] {
			HasRefInParameter, ComponentsUsageConstrains, ComponentsUniquality, ComponentsNames, ValidateTransition
		};

		private (bool, string) HasRefInParameter()
		{
			bool hasRefOrIn = _components
				.Any( c => c.Usage == ComponentLinkUsageType.All && ( c.AccessType == ComponentLinkAccessType.R || c.AccessType == ComponentLinkAccessType.RW ) );
			return (hasRefOrIn, "Has zero [All][R] and [All][RW])");
		}

		private (bool, string) ComponentsUsageConstrains()
		{
			bool toManyAll = _components.Count( c => c.Usage == ComponentLinkUsageType.All && c.AccessType == ComponentLinkAccessType.Un ) > 3;
			bool toManyAny = _components.Count( c => c.Usage == ComponentLinkUsageType.Any ) > 3;
			bool toManyNone = _components.Count( c => c.Usage == ComponentLinkUsageType.None ) > 3;

			StringBuilder message = new StringBuilder();
			if ( toManyAll )
			{
				message.AppendLine( "Has to many [All][Un] components (max 3)" );
			}
			if ( toManyAny )
			{
				message.AppendLine( "Has to many [Any] components (max 3)" );
			}
			if ( toManyNone )
			{
				message.AppendLine( "Has to many [None] components (max 3)" );
			}

			return (!toManyAll && !toManyAny && !toManyNone, message.ToString());
		}

		private (bool, string) ComponentsUniquality()
		{
			bool allUnique = _components.Select( c => c.ComponentName ).Distinct().Count() == _components.Length;
			return (allUnique, "Has not duplicated components");
		}

		private (bool, string) ComponentsNames()
		{
			bool allHasNames = _components.All( c => !string.IsNullOrWhiteSpace( c.ComponentName ) );
			return (allHasNames, "Has component without name");
		}

		private (bool, string) ValidateTransition()
		{
			var connectedNodes = Outputs.FirstOrDefault( p => p?.fieldName == nameof( _to ) ).GetConnections().Select(c => c.node).OfType<StateNode>();
			bool allValid = true;
			foreach ( var node in connectedNodes )
			{
				var withAllOthers = node._components.Where(c => c.Usage == ComponentLinkUsageType.All).Select(c => c.ComponentName).ToArray();
				var withAllMine = _components.Where( c => c.Usage == ComponentLinkUsageType.All ).Select( c => c.ComponentName ).ToArray();
				var uninoned = withAllOthers.Union(withAllMine);
				allValid = allValid && uninoned.Count() != Mathf.Max( withAllOthers.Count(), withAllMine.Count() );
			}
			return (allValid, "The state that transition to has the same state definition");
		}

		#endregion Setup validation
	}
}