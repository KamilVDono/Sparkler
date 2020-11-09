using Sparkler.Rotorz.Games;

using System;
using System.Linq;
using System.Reflection;

using Unity.Entities;

#if UNITY_EDITOR

#endif

using UnityEngine;

namespace Sparkler.Components
{
	[Flags]
	public enum ComponentLinkAccessType
	{
		[InspectorName("Un")]
		Unused = 0,

		[InspectorName("R")]
		Read = 1 << 0,

		[InspectorName("RW")]
		ReadWrite = Read | 1 << 1,

		JustForUnity = 1 << 2,
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
		[ClassImplements( typeof( IComponentData ), typeof( ISharedComponentData ), typeof(IBufferElementData), typeof(ISystemStateComponentData), typeof(ISystemStateSharedComponentData) )]
		private ClassTypeReference _componentTypeReference = default;

		[SerializeField]
		private string _componentName;

		[SerializeField]
		private ComponentLinkAccessType _accessType = ComponentLinkAccessType.Unused;

		[SerializeField]
		private ComponentLinkUsageType _usageType = ComponentLinkUsageType.Invalid;

		#region Queries
		public Type TypeReference { get => _componentTypeReference?.Type; set => _componentTypeReference = new ClassTypeReference( value ); }
		public string HandwrittenName => _componentName;

		public string ComponentName => TypeReference?.Name;

		public ComponentLinkAccessType AccessType { get => _accessType; set => _accessType = value; }
		public ComponentLinkUsageType Usage { get => _usageType; set => _usageType = value; }

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

		#region Editor
#if UNITY_EDITOR

		private static Type[] _allTypes;

		static ComponentLink() => _allTypes = AppDomain.CurrentDomain.GetAssemblies().Reverse().SelectMany( a => a.GetTypes() ).ToArray();

		public bool Validate()
		{
			if ( TypeReference == null && !string.IsNullOrWhiteSpace( HandwrittenName ) )
			{
				var myType = GetType();
				var field = myType.GetField( "_componentTypeReference", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				var implementationFilter = field.GetCustomAttribute<ClassImplementsAttribute>();
				var type = _allTypes.FirstOrDefault( t => t.Name == HandwrittenName && implementationFilter.IsConstraintSatisfied(t) );
				if ( type != null )
				{
					_componentTypeReference = new ClassTypeReference( type );
					_componentName = "";
					return true;
				}
			}
			if ( TypeReference == null )
			{
				if ( TypeReference == null && !string.IsNullOrWhiteSpace( _componentTypeReference.Name ) )
				{
					_componentName = _componentTypeReference.Name;
					_componentTypeReference = new ClassTypeReference();
					return true;
				}
			}

			return false;
		}

#endif
		#endregion Editor
	}
}