using Rotorz.Games;

using System;

using Unity.Entities;

using UnityEngine;

namespace FSM.Components
{
	[Flags]
	public enum ComponentLinkAccess
	{
		None = 0,
		Read = 1 << 0,
		Write = 1 << 1,
		ReadWrite = Read | Write,
	}

	[Serializable]
	public class ComponentLink
	{
		[SerializeField]
		[ClassImplements( typeof( IComponentData ) )]
		private ClassTypeReference _componentTypeReference = default;

		[SerializeField]
		private string _componentName;

		[SerializeField]
		private ComponentLinkAccess _accessType;
	}
}