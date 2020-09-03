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
		[SerializeField] private SharedComponentFilter _sharedFilter = new SharedComponentFilter();
		[SerializeField] private ComponentLink[] _components = new ComponentLink[0];

		#region Queries
		public IReadOnlyCollection<ComponentLink> Components => _components;
		public string Name => _name;
		public bool HasStructuralChanges => _hasStructuralChanges && !HasSharedComponent;
		public bool HasSharedComponent => _components.Any( c => c.TypeReference.Implements( typeof( ISharedComponentData ) ) || c.TypeReference.Implements( typeof( ISystemStateSharedComponentData ) ) );
		public bool Parallel => _parallelSchedule && !HasSharedComponent;

		public bool HasQueryField => !string.IsNullOrWhiteSpace( _queryField );
		public string QueryFieldName => _queryField;

		public bool HasSharedFilter => _sharedFilter.IsValid;
		public string SharedFilterName => _sharedFilter.FilterName;
		public string SharedFilterDeclaration => _sharedFilter.ComponentDeclaration;

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

		#region Creation

		public static SystemLambdaAction FromFile( FileSystemLambdaData data )
		{
			SystemLambdaAction lambda = new SystemLambdaAction()
			{
				_name = data.Name,
				_parallelSchedule = data.LaunchType == LaunchType.ScheduleParallel,
				_hasStructuralChanges = data.LaunchType == LaunchType.Run,
				_queryField = data.QueryField,
			};

			if ( !string.IsNullOrWhiteSpace( data.SharedComponentFilter ) )
			{
				lambda._sharedFilter = new SharedComponentFilter() { FilterName = data.SharedComponentFilter };
			}

			var parametersCount = data.WithAll.Count + data.WithAny.Count + data.WithNone.Count + data.RefParamter.Count + data.InParameter.Count;
			lambda._components = new ComponentLink[parametersCount];

			var allTypes = AppDomain.CurrentDomain.GetAssemblies().Reverse().SelectMany( a => a.GetTypes() ).ToArray();

			int index = 0;
			for ( int i = 0; i < data.WithAll.Count; i++ )
			{
				var type = allTypes.First(t => t.Name.Equals(data.WithAll[i]));
				lambda._components[index++] = new ComponentLink() { AccessType = ComponentLinkAccessType.Unused, Usage = ComponentLinkUsageType.All, TypeReference = type };
			}
			for ( int i = 0; i < data.WithAny.Count; i++ )
			{
				var type = allTypes.First(t => t.Name.Equals(data.WithAny[i]));
				lambda._components[index++] = new ComponentLink() { AccessType = ComponentLinkAccessType.Unused, Usage = ComponentLinkUsageType.Any, TypeReference = type };
			}
			for ( int i = 0; i < data.WithNone.Count; i++ )
			{
				var type = allTypes.First(t => t.Name.Equals(data.WithNone[i]));
				lambda._components[index++] = new ComponentLink() { AccessType = ComponentLinkAccessType.Unused, Usage = ComponentLinkUsageType.None, TypeReference = type };
			}
			for ( int i = 0; i < data.RefParamter.Count; i++ )
			{
				var type = allTypes.First(t => t.Name.Equals(data.RefParamter[i]));
				lambda._components[index++] = new ComponentLink() { AccessType = ComponentLinkAccessType.ReadWrite, Usage = ComponentLinkUsageType.All, TypeReference = type };
			}
			for ( int i = 0; i < data.InParameter.Count; i++ )
			{
				var type = allTypes.First(t => t.Name.Equals(data.InParameter[i]));
				lambda._components[index++] = new ComponentLink() { AccessType = ComponentLinkAccessType.Read, Usage = ComponentLinkUsageType.All, TypeReference = type };
			}

			lambda.Initialize();

			return lambda;
		}

		public void Initialize() => _guid = Guid.NewGuid().GetHashCode();

		#endregion Creation

		#region Operations

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