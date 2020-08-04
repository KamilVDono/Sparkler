using FSM.Utility;

using System;
using System.Collections.Generic;
using System.Linq;

using Unity.Entities;

using UnityEngine;

namespace FSM.Components
{
	[Serializable]
	public class SystemLambdaAction
	{
		[SerializeField] private int _guid;

		[SerializeField] private string _name = "";
		[SerializeField] private bool _parallelSchedule = true;
		[SerializeField] private bool _hasStructuralChanges = false;
		[SerializeField] private string _queryField = "";
		[SerializeField] private ComponentLink[] _components = new ComponentLink[0];

		#region Queries
		public IReadOnlyCollection<ComponentLink> Components => _components;
		public string Name => _name;
		public bool HasStructuralChanges => _hasStructuralChanges && !HasSharedComponent;
		public bool HasSharedComponent => _components.Any( c => c.TypeReference.Implements( typeof( ISharedComponentData ) ) || c.TypeReference.Implements( typeof( ISystemStateSharedComponentData ) ) );
		public bool Parallel => _parallelSchedule && !HasSharedComponent;

		public bool HasQueryField => !string.IsNullOrWhiteSpace( _queryField );
		public string QueryFieldName => _queryField;

		public string FullName( StateNode stateNode )
		{
			if ( _name.StartsWith( stateNode.StateName ) )
			{
				return Name;
			}
			else
			{
				return $"{stateNode.StateName}_{Name}";
			}
		}

		#endregion Queries

		#region Operations

		public void Initialize() => _guid = Guid.NewGuid().GetHashCode();

		public void PropertiesChanged( List<ComponentLink> changedComponents )
		{
			foreach ( var changedComponent in changedComponents )
			{
				bool isRefIn = changedComponent.Usage == ComponentLinkUsageType.All && (changedComponent.AccessType & ComponentLinkAccessType.Read) != 0;
				bool isInvalid = changedComponent.Usage == ComponentLinkUsageType.Invalid;
				if ( isRefIn || isInvalid )
				{
					continue;
				}

				var sameTypeComponents = _components.Where( c => IsSameTypeComponent( c, changedComponent ) );
				if ( sameTypeComponents.Count() > 3 )
				{
					changedComponent.Usage = ComponentLinkUsageType.Invalid;
				}
			}
			_components = _components.OrderBy( c => c.Usage ).ThenByDescending( c => c.AccessType ).ToArray();
		}

		#endregion Operations

		public override int GetHashCode() => _guid;

		private static bool IsSameTypeComponent( ComponentLink c, ComponentLink changedComponent ) => c.Usage == changedComponent.Usage && ( changedComponent.Usage == ComponentLinkUsageType.All || c.AccessType == changedComponent.AccessType );
	}
}