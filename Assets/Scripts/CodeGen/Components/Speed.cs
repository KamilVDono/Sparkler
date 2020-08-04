using System;

using Unity.Entities;



namespace FSM.AI.States.Components
{
	[Serializable]
	public struct Speed : IComponentData
	{
		public float Value;

	}
}