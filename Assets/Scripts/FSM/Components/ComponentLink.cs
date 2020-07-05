using Rotorz.Games;

using System;

using Unity.Entities;

using UnityEngine;

using XNode;

namespace FSM.Components
{
	[Flags]
	public enum ComponentLinkAccessType
	{
		Unused = 0,
		Read = 1 << 0,
		Write = 1 << 1,
		ReadWrite = Read | Write,
	}

	public enum ComponentLinkUsageType
	{
		Invalid = 0,
		All = 1,
		Any = 2,
		None = 3,
	}

	[Serializable]
	public class ComponentLink
	{
		[SerializeField]
		[ClassImplements( typeof( IComponentData ) )]
		private ClassTypeReference _componentTypeReference = default;

		[SerializeField]
		private string _componentName;

		[SerializeField, NodeEnum]
		private ComponentLinkAccessType _accessType;

		[SerializeField, NodeEnum]
		private ComponentLinkUsageType _usageType;
	}
}