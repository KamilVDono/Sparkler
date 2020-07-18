// Copyright (c) Rotorz Limited. All rights reserved. Licensed under the MIT license. See LICENSE
// file in the project root.

using System;

using UnityEngine;

namespace Rotorz.Games
{
	/// <summary>
	/// Reference to a class <see cref="System.Type"/> with support for Unity serialization.
	/// </summary>
	[Serializable]
	public sealed class ClassTypeReference : ISerializationCallbackReceiver, IEquatable<ClassTypeReference>
	{
		[SerializeField]
		private string classRef;

		[SerializeField]
		private string name;

		private Type type;
		public string Name => name;

		/// <summary>
		/// Gets or sets type of class reference.
		/// </summary>
		/// <exception cref="System.ArgumentException">
		/// If <paramref name="value"/> is not a class type.
		/// </exception>
		public Type Type
		{
			get => this.type;
			set
			{
				if ( value != null && !( value.IsClass || value.IsValueType ) )
				{
					throw new ArgumentException( string.Format( "'{0}' is not a class type.", value.FullName ), "value" );
				}

				this.type = value;
				this.classRef = GetClassRef( value );
				this.name = value?.Name;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ClassTypeReference"/> class.
		/// </summary>
		public ClassTypeReference()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ClassTypeReference"/> class.
		/// </summary>
		/// <param name="assemblyQualifiedClassName">Assembly qualified class name.</param>
		public ClassTypeReference( string assemblyQualifiedClassName )
		{
			this.Type = !string.IsNullOrEmpty( assemblyQualifiedClassName )
				? Type.GetType( assemblyQualifiedClassName )
				: null;
			this.name = this.Type?.Name;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ClassTypeReference"/> class.
		/// </summary>
		/// <param name="type">Class type.</param>
		/// <exception cref="System.ArgumentException">
		/// If <paramref name="type"/> is not a class type.
		/// </exception>
		public ClassTypeReference( Type type )
		{
			Type = type;
			this.name = this.Type?.Name;
		}

		public static string GetClassRef( Type type )
		{
			return type != null
				? type.FullName + ", " + type.Assembly.GetName().Name
				: "";
		}

		public static implicit operator string( ClassTypeReference typeReference ) => typeReference.classRef;

		public static implicit operator Type( ClassTypeReference typeReference ) => typeReference.Type;

		public static implicit operator ClassTypeReference( Type type ) => new ClassTypeReference( type );

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			if ( !string.IsNullOrEmpty( this.classRef ) )
			{
				this.type = System.Type.GetType( this.classRef );

				if ( this.type == null )
				{
					Debug.LogWarning( string.Format( "'{0}' was referenced but class type was not found.", this.classRef ) );
				}
			}
			else
			{
				this.type = null;
			}
		}

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
			//HACK: If we null Type then we need null also name
			if ( Type == null )
			{
				name = "";
			}
		}

		public override string ToString() => this.Type != null ? this.Type.FullName : "(None)";

		#region Equality

		public static bool operator ==( ClassTypeReference left, ClassTypeReference right ) => left.Equals( right );

		public static bool operator !=( ClassTypeReference left, ClassTypeReference right ) => !left.Equals( right );

		public override bool Equals( object obj ) => Equals( obj as ClassTypeReference );

		public bool Equals( ClassTypeReference other ) => !ReferenceEquals( other, null ) && classRef == other.classRef;

		public override int GetHashCode() => 1504390128 + classRef.GetHashCode();

		#endregion Equality
	}
}