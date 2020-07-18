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

		private Func<(bool, string)>[] _allConfigurationCheckers;

		public string Name
		{
			get => _name;
			set
			{
				var nameParts = value.Split(' ', '.', ',', '_', '-');
				_name = "";
				foreach ( var namePart in nameParts )
				{
					_name += char.ToUpperInvariant( namePart[0] ) + namePart.Substring( 1 );
				}
			}
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

		protected virtual IEnumerable<Func<(bool, string)>> ConfigurationCheckers => Enumerable.Empty<Func<(bool, string)>>();

		private Func<(bool, string)>[] _AllConfigurationCheckers
		{
			get
			{
				if ( _allConfigurationCheckers == null )
				{
					_allConfigurationCheckers = ConfigurationCheckers
						.Concat( new Func<(bool, string)>[] { HasName } )
						.ToArray();
				}
				return _allConfigurationCheckers;
			}
		}

		public (bool, string) IsRightConfigured()
		{
			var messages = _AllConfigurationCheckers
				.Select( cc => cc() )
				.GroupBy( p => p.Item1 )
				.Where( g => !g.Key )
				.SelectMany( g => g).Select(g => g.Item2)
				.Distinct()
				.ToArray();

			if ( messages.Any() )
			{
				return (false, string.Join( "\n", messages ));
			}
			return (true, "");
		}

		private (bool, string) HasName() => (!string.IsNullOrEmpty( Name ), "Empty name");
	}
}