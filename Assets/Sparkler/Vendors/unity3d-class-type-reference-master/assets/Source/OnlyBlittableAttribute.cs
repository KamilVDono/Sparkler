using System;

using Unity.Collections.LowLevel.Unsafe;

namespace Sparkler.Rotorz.Games
{
	/// <summary>
	/// Constraint that allows selection of only blittable types
	/// </summary>
	[AttributeUsage( AttributeTargets.Field, AllowMultiple = false )]
	public class OnlyBlittableAttribute : ClassTypeConstraintAttribute
	{
		/// <inheritdoc/>
		public override bool IsConstraintSatisfied( Type type )
		{
			if ( !type.IsValueType )
			{
				return false;
			}
			if ( type.IsGenericType )
			{
				return false;
			}
			return UnsafeUtility.IsBlittable( type );
		}
	}
}