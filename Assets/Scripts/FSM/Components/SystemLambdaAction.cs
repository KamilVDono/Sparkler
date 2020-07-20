using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace FSM.Components
{
	[Serializable]
	public class SystemLambdaAction
	{
		[SerializeField] private string _name;
		[SerializeField] private ComponentLink[] _components;

		public void PropertiesChanged( List<ComponentLink> changedComponents )
		{
			foreach ( var changedComponent in changedComponents )
			{
				bool isRefIn = changedComponent.Usage == ComponentLinkUsageType.All && (changedComponent.AccessType & ComponentLinkAccessType.R) != 0;
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
			_components = _components.OrderBy( c => c.Usage ).ThenBy( c => c.AccessType ).ToArray();
		}

		private static bool IsSameTypeComponent( ComponentLink c, ComponentLink changedComponent ) => c.Usage == changedComponent.Usage && ( changedComponent.Usage == ComponentLinkUsageType.All || c.AccessType == changedComponent.AccessType );
	}
}