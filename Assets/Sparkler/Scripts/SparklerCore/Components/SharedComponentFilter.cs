using System;
using System.Text;

using UnityEngine;

namespace Sparkler.Components
{
	[Serializable]
	public class SharedComponentFilter
	{
		public string FilterName;
		[SerializeField] private byte[] _serializedDeclaration;

		public string ComponentDeclaration
		{
			get
			{
				if ( ( _serializedDeclaration?.Length ?? 0 ) < 1 )
				{
					return string.Empty;
				}
				return Encoding.Unicode.GetString( _serializedDeclaration );
			}
			set
			{
				if ( string.IsNullOrWhiteSpace( value ) )
				{
					_serializedDeclaration = null;
				}
				_serializedDeclaration = Encoding.Unicode.GetBytes( value );
			}
		}

		public bool IsValid => !string.IsNullOrEmpty( FilterName ) && !string.IsNullOrEmpty( ComponentDeclaration );

		public void Invalid()
		{
			FilterName = string.Empty;
			ComponentDeclaration = string.Empty;
		}
	}
}