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
		Un = 0,
		R = 1 << 0,
		RW = R | 1 << 1,
	}

	public enum ComponentLinkUsageType
	{
		Invalid = 0,
		All = 1,
		Any = 2,
		None = 3,
	}

	[Serializable]
	public class ComponentLink : IEquatable<ComponentLink>
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

		#region Queries
		public Type TypeReference => _componentTypeReference?.Type;
		public string HandwrittenName => _componentName;
		public string ComponentName => TypeReference?.Name ?? HandwrittenName;

		public ComponentLinkAccessType AccessType => _accessType;
		public ComponentLinkUsageType Usage => _usageType;

		public bool IsHandWrited => TypeReference == null && !string.IsNullOrWhiteSpace( HandwrittenName );
		#endregion Queries

		#region Equality

		public static bool operator ==( ComponentLink left, ComponentLink right ) => left.Equals( right );

		public static bool operator !=( ComponentLink left, ComponentLink right ) => !( left == right );

		public override bool Equals( object obj ) => Equals( obj as ComponentLink );

		public bool Equals( ComponentLink other )
		{
			bool hasEqualType = _componentTypeReference?.Type == other?._componentTypeReference?.Type;
			bool hasEqualName = _componentTypeReference == null && hasEqualType && _componentName == other?._componentName;
			return ReferenceEquals( other, null ) && ( hasEqualType || hasEqualName ) && _accessType == other._accessType && _usageType == other._usageType;
		}

		public override int GetHashCode()
		{
			var hashCode = 213946169;
			if ( _componentTypeReference?.Type != null )
			{
				hashCode = hashCode * -1521134295 + _componentTypeReference.Type.Name.GetHashCode();
			}
			else
			{
				hashCode = hashCode * -1521134295 + _componentName.GetHashCode();
			}

			hashCode = hashCode * -1521134295 + _accessType.GetHashCode();
			hashCode = hashCode * -1521134295 + _usageType.GetHashCode();
			return hashCode;
		}

		#endregion Equality
	}
}