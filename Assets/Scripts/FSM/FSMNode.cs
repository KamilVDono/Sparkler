using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using XNode;

namespace FSM
{
	public abstract class FSMNode : Node
	{
		[SerializeField]
		[HideInInspector]
		private string _name;

		public string Name
		{
			get => _name;
			set => _name = value;
		}

		public override Action<string> RenameAction => ( newName ) => Name = newName;

		public virtual int ColorSeed => Name?.GetHashCode() ?? 0;

		public Color Color
		{
			get
			{
				System.Random rng = new System.Random(ColorSeed);
				return new Color( (float)rng.NextDouble(), (float)rng.NextDouble(), (float)rng.NextDouble() );
			}
		}

		protected virtual IEnumerable<Func<bool>> ConfigurationCheckers => Enumerable.Empty<Func<bool>>();

		public bool IsRightConfigured() => ConfigurationCheckers.Concat( new Func<bool>[] { () => !string.IsNullOrEmpty( Name ) } ).All( f => f?.Invoke() ?? false );
	}
}