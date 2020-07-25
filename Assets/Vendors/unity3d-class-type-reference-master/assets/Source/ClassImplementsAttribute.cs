
// Copyright (c) Rotorz Limited. All rights reserved. Licensed under the MIT license. See LICENSE
// file in the project root.

using System;

namespace Rotorz.Games
{
	/// <summary>
	/// Constraint that allows selection of classes that implement a specific interface when
	/// selecting a <see cref="ClassTypeReference"/> with the Unity inspector.
	/// </summary>
	[AttributeUsage( AttributeTargets.Field, AllowMultiple = false )]
	public sealed class ClassImplementsAttribute : ClassTypeConstraintAttribute
	{
		/// <summary>
		/// Gets the type of interface that selectable classes must implement.
		/// </summary>
		public Type[] InterfaceTypes { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="ClassImplementsAttribute"/> class.
		/// </summary>
		public ClassImplementsAttribute()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ClassImplementsAttribute"/> class.
		/// </summary>
		/// <param name="interfaceType">Type of interface that selectable classes must implement.</param>
		public ClassImplementsAttribute( params Type[] interfaceTypes ) => this.InterfaceTypes = interfaceTypes;

		/// <inheritdoc/>
		public override bool IsConstraintSatisfied( Type type )
		{
			if ( base.IsConstraintSatisfied( type ) )
			{
				foreach ( var interfaceType in InterfaceTypes )
				{
					if ( interfaceType.IsAssignableFrom( type ) )
					{
						return true;
					}
				}
			}
			return false;
		}
	}
}