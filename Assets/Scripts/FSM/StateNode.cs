using FSM.Components;
using FSM.Utility;

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

		[SerializeField] private SystemLambdaAction[] _lambdas = new SystemLambdaAction[0];

		#region Queries

		public string StateName
		{
			get
			{
				var systemName = Name;
				if ( !systemName.EndsWith( "System" ) )
				{
					systemName += "System";
				}
				return systemName;
			}
		}

		public IEnumerable<ComponentLink> AllComponents => _lambdas.SelectMany( l => l.Components );
		public IReadOnlyCollection<SystemLambdaAction> Lambdas => _lambdas;
		public bool HasStructuralChanges => DynamicOutputs.Any( o => o.IsConnected );

		public IEnumerable<StateNode> TransitionsFrom => Inputs
			.Where( o => o.fieldName == nameof( _from ) )
			.SelectMany( o => o.GetInputValues() )
			.OfType<StateNode>();

		public IEnumerable<StateNode> TransitionsTo => Outputs
			.SelectMany( o => o.GetConnections().Select( c => c.node ) )
			.OfType<StateNode>();

		public StateNode TransitionTo( SystemLambdaAction lambda )
		{
			int index = _lambdas.IndexOf(lambda);
			return GetLambdaPort( index )?.Connection?.node as StateNode;
		}

		#endregion Queries

		#region Dynamic ports

		public NodePort GetLambdaPort( int index )
		{
			string portName = ComponentPortName(index);
			return Outputs.FirstOrDefault( o => o.IsDynamic && o.fieldName == portName );
		}

		public NodePort GetOrAddLambdaPort( int index )
		{
			var port = GetLambdaPort(index);
			if ( port != null )
			{
				return port;
			}
			string portName = ComponentPortName(index);
			return AddDynamicOutput( typeof( StateNode ), ConnectionType.Override, TypeConstraint.Strict, portName );
		}

		public void RemoveLambdaPort( int index )
		{
			var port = GetLambdaPort(index);
			if ( port != null )
			{
				RemoveDynamicPort( port );
			}
		}

		private string ComponentPortName( int index )
		{
			if ( _lambdas.Length <= index || index < 0 )
			{
				return null;
			}

			return $"Component_{_lambdas[index].GetHashCode()}";
		}

		#endregion Dynamic ports

		public override object GetValue( NodePort port ) => this;

		#region Setup validation

		protected override IEnumerable<Func<(bool, string)>> ConfigurationCheckers => new Func<(bool, string)>[] {
			HasAtLeastOneLambda, HasRefInParameter, ComponentsUsageConstrains, ComponentsUniquality, LambdasHasName, ComponentsNames, LambdasUniqueName
		};

		private (bool, string) HasAtLeastOneLambda() => (_lambdas.Length > 0, "Has zero lambda actions/behaviors");

		private (bool, string) HasRefInParameter()
		{
			foreach ( var lambda in _lambdas )
			{
				bool hasRefOrIn = lambda.Components
					.Any( c => c.Usage == ComponentLinkUsageType.All && ( c.AccessType == ComponentLinkAccessType.Read || c.AccessType == ComponentLinkAccessType.ReadWrite ) );
				if ( !hasRefOrIn )
				{
					return (hasRefOrIn, $"Has zero [All][R] and [All][RW] in {lambda.Name}");
				}
			}
			return (true, "");
		}

		private (bool, string) ComponentsUsageConstrains()
		{
			foreach ( var lambda in _lambdas )
			{
				bool toManyAll = lambda.Components.Count( c => c.Usage == ComponentLinkUsageType.All && c.AccessType == ComponentLinkAccessType.Unused ) > 3;
				bool toManyAny = lambda.Components.Count( c => c.Usage == ComponentLinkUsageType.Any ) > 3;
				bool toManyNone = lambda.Components.Count( c => c.Usage == ComponentLinkUsageType.None ) > 3;

				if ( toManyAll || toManyAny || toManyNone )
				{
					StringBuilder message = new StringBuilder();
					message.Append( "Lambda: " );
					message.AppendLine( lambda.Name );

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
			}
			return (true, "");
		}

		private (bool, string) ComponentsUniquality()
		{
			foreach ( var lambda in _lambdas )
			{
				bool allUnique = lambda.Components.Select( c => c.ComponentName ).Distinct().Count() == lambda.Components.Count;
				if ( !allUnique )
				{
					return (allUnique, "Has not duplicated components");
				}
			}
			return (true, "");
		}

		private (bool, string) LambdasHasName()
		{
			bool allHasNames = _lambdas.All( c => !string.IsNullOrWhiteSpace( c.Name ) );
			return (allHasNames, "Has lambda without name");
		}

		private (bool, string) LambdasUniqueName()
		{
			bool allNamesAreUnique = _lambdas.Select(l => l.Name).Distinct().Count() == _lambdas.Length;
			return (allNamesAreUnique, "Lambdas has duplicated names");
		}

		private (bool, string) ComponentsNames()
		{
			foreach ( var lambda in _lambdas )
			{
				bool allHasNames = lambda.Components.All( c => !string.IsNullOrWhiteSpace( c.ComponentName ) );
				if ( !allHasNames )
				{
					return (allHasNames, "Has component without name");
				}
			}
			return (true, "");
		}

		#endregion Setup validation
	}
}