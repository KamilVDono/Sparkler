using Rotorz.Games;

using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using XNode;

namespace FSM
{
	[NodeWidth( 200 )]
	public class StateNode : FSMNode
	{
		[Input(ShowBackingValue.Never, connectionType = ConnectionType.Multiple)]
		[SerializeField] private StateNode _from;

		[ClassImplements(typeof(IStateTag))]
		[SerializeField] private ClassTypeReference[] _all = new ClassTypeReference[0];

		[ClassImplements(typeof(IStateTag))]
		[SerializeField] private ClassTypeReference[] _any = new ClassTypeReference[0];

		[ClassImplements(typeof(IStateTag))]
		[SerializeField] private ClassTypeReference[] _none = new ClassTypeReference[0];

		[Output(ShowBackingValue.Never, connectionType = ConnectionType.Multiple)]
		[SerializeField] private StateNode _to;

		public override object GetValue( NodePort port ) => this;

		private static void ValidateTypeArray( ClassTypeReference[] array, string arrayName, params ClassTypeReference[][] excepts )
		{
			HashSet<ClassTypeReference> set = new HashSet<ClassTypeReference>();
			for ( int i = 0; i < array.Length; i++ )
			{
				var current = array[i];
				if ( !set.Add( current ) )
				{
					array[i] = new ClassTypeReference();
					Debug.LogWarning( $"Removed value {current} from {arrayName} because already exists in this list" );
					continue;
				}

				foreach ( var except in excepts )
				{
					if ( except.Any( e => e == current ) )
					{
						array[i] = new ClassTypeReference();
						Debug.LogWarning( $"Removed value {current} from {arrayName} because already exists in other list" );
						break;
					}
				}
			}
		}

		private void OnValidate()
		{
			ValidateTypeArray( _all, nameof( _all ) );
			ValidateTypeArray( _any, nameof( _any ), _all );
			ValidateTypeArray( _none, nameof( _none ), _any, _all );
		}
	}
}