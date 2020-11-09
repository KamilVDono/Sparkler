using Sparkler.Components;
using Sparkler.Utility;
using Sparkler.XNode;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Unity.Entities;

using UnityEngine;

namespace Sparkler
{
	[NodeWidth( 300 )]
	[CreateNodeMenu( "System" )]
	public class SystemNode : Node
	{
		#region Name & Color

		[SerializeField]
		[HideInInspector]
		private string _name;

		public string Name
		{
			get => _name;
			set
			{
				var nameParts = value.Split(' ', '.', ',', '_', '-');
				_name = "";
				foreach ( var namePart in nameParts )
				{
					_name += char.ToUpperInvariant( namePart[0] ) + namePart.Substring( 1 );
				}
			}
		}

		public override Action<string> RenameAction => ( newName ) =>
		{
			if ( !_fromFile )
			{
				Name = newName;
			}
		};

		public int ColorSeed => Name?.GetHashCode() ?? 0;

		public Color Color
		{
			get
			{
				System.Random rng = new System.Random(ColorSeed);
				var color = RandomColor();
				while ( color.r > 0.9 && color.g < 0.2f && color.b < 0.2f )
				{
					color = RandomColor();
				}
				return color;

				Color RandomColor() => new Color( (float)rng.NextDouble(), (float)rng.NextDouble(), (float)rng.NextDouble() );
			}
		}

		#endregion Name & Color

		[SerializeField] private bool _fromFile = false;

		[Input(ShowBackingValue.Never, connectionType = ConnectionType.Multiple)]
		[SerializeField] private SystemNode _from = null;

		[SerializeField] private SystemLambdaAction[] _lambdas = new SystemLambdaAction[0];

		#region Creation

		public static SystemNode FromFile( SystemsGraph graph, FileSystemData data )
		{
			if ( graph.nodes.OfType<SystemNode>().Any( n => n.Name == data.Name ) )
			{
				return null;
			}

			var node = graph.AddNode<SystemNode>();
			node.Name = data.Name;

			node._lambdas = new SystemLambdaAction[data.Lambdas.Length];
			for ( var i = 0; i < data.Lambdas.Length; i++ )
			{
				var lambda = data.Lambdas[i];
				node._lambdas[i] = SystemLambdaAction.FromFile( lambda );
			}

			node._fromFile = true;

			return node;
		}

		#endregion Creation

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

		public bool Editable => !_fromFile;

		public IEnumerable<ComponentLink> AllComponents => _lambdas.SelectMany( l => l.Components );
		public IReadOnlyCollection<SystemLambdaAction> Lambdas => _lambdas;
		public bool HasStructuralChanges => DynamicOutputs.Any( o => o.IsConnected ) || _lambdas.Any( l => l.HasStructuralChanges );

		public IEnumerable<SystemNode> TransitionsFrom => Inputs
			.Where( o => o.fieldName == nameof( _from ) )
			.SelectMany( o => o.GetInputValues() )
			.OfType<SystemNode>();

		public IEnumerable<SystemNode> TransitionsTo => Outputs
			.SelectMany( o => o.GetConnections().Select( c => c.node ) )
			.OfType<SystemNode>();

		public SystemNode TransitionTo( SystemLambdaAction lambda )
		{
			int index = _lambdas.IndexOf(lambda);
			return GetLambdaPort( index )?.Connection?.node as SystemNode;
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
			return AddDynamicOutput( typeof( SystemNode ), ConnectionType.Override, TypeConstraint.Strict, portName );
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

		protected IEnumerable<Func<(bool, string)>> ConfigurationCheckers => new Func<(bool, string)>[] {
			HasAtLeastOneLambda, HasRefInParameter, ComponentsUsageConstrains, ComponentsUniquality, LambdasHasName, ComponentsTypes, LambdasUniqueName,
			HasValidComponentsUsage, NoneSharedRef, HasName
		};

		public (bool, string) IsRightConfigured()
		{
			if ( !Editable )
			{
				return (true, "");
			}
			var messages = ConfigurationCheckers
				.Select( cc => cc() )
				.GroupBy( p => p.Item1 )
				.Where( g => !g.Key )
				.SelectMany( g => g).Select(g => g.Item2)
				.Distinct()
				.ToArray();

			if ( messages.Any() )
			{
				return (false, string.Join( "\n", messages ));
			}
			return (true, "");
		}

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

		private (bool, string) ComponentsTypes()
		{
			foreach ( var lambda in _lambdas )
			{
				bool allHasTypes = lambda.Components.All( c => c.TypeReference != null );
				if ( !allHasTypes )
				{
					return (allHasTypes, "Has component without type");
				}
			}
			return (true, "");
		}

		private (bool, string) HasValidComponentsUsage()
		{
			foreach ( var lambda in _lambdas )
			{
				bool hasInvalid = lambda.Components
					.Any( c => c.Usage == ComponentLinkUsageType.Invalid);
				if ( hasInvalid )
				{
					return (false, $"Has invalid component usage in {lambda.Name}");
				}
			}
			return (true, "");
		}

		private (bool, string) NoneSharedRef()
		{
			var allRef = _lambdas
				.SelectMany(l => l.Components)
				.Where(c => c.Usage == ComponentLinkUsageType.All && c.AccessType == ComponentLinkAccessType.ReadWrite).ToArray();
			var anySharedInRef = allRef
				.Any(r => r.TypeReference.Implements(typeof(ISharedComponentData)) || r.TypeReference.Implements(typeof(ISystemStateSharedComponentData)));
			return (!anySharedInRef, "Shared components can not be write in ref mode");
		}

		private (bool, string) HasName() => (!string.IsNullOrEmpty( Name ), "Empty name");

		#endregion Setup validation

		public override string ToString() => $"{GetType().Name} - {Name}";
	}
}