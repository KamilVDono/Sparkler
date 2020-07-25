using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Rotorz.Games
{
	[AttributeUsage( AttributeTargets.Field, AllowMultiple = false )]
	public class BlacklistedNamespacesAttribute : ClassTypeConstraintAttribute
	{
		private bool _useRegex = false;
		private string[] _blacklistStrings = new string[0];
		private Regex[] _blacklistRegex = new Regex[0];

		public BlacklistedNamespacesAttribute( bool useRegex, params string[] blacklist )
		{
			_useRegex = useRegex;
			if ( useRegex )
			{
				_blacklistRegex = blacklist.Select( entry => new Regex( entry, RegexOptions.IgnoreCase ) ).ToArray();
			}
			else
			{
				_blacklistStrings = blacklist;
			}
		}

		public override bool IsConstraintSatisfied( Type type )
		{
			if ( _useRegex )
			{
				foreach ( var entry in _blacklistRegex )
				{
					if ( !string.IsNullOrWhiteSpace( type.Namespace ) && entry.IsMatch( type.Namespace ) )
					{
						return false;
					}
				}
			}
			else
			{
				foreach ( var entry in _blacklistStrings )
				{
					if ( type.Namespace.IndexOf( entry, StringComparison.InvariantCultureIgnoreCase ) >= 0 )
					{
						return false;
					}
				}
			}

			return true;
		}
	}
}