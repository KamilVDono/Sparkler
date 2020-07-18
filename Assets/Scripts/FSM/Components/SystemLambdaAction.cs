using System;
using System.Linq;

using UnityEngine;

namespace FSM.Components
{
	[Serializable]
	public class SystemLambdaAction
	{
		[SerializeField] private string _name;
		[SerializeField] private ComponentLink[] _components;

		public void PropertiesChanged()
		{
			Debug.Log( "Some thing changed" );
			_components = _components.OrderBy( c => c.Usage ).ThenBy( c => c.AccessType ).ToArray();
		}
	}
}