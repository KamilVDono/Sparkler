using System;

namespace FSM.Components
{
	[Serializable]
	public class SharedComponentFilter
	{
		public string FilterName;
		public string ComponentDeclaration;

		public bool IsValid => !string.IsNullOrEmpty( FilterName ) && !string.IsNullOrEmpty( ComponentDeclaration );

		public void Invalid()
		{
			FilterName = string.Empty;
			ComponentDeclaration = string.Empty;
		}
	}
}