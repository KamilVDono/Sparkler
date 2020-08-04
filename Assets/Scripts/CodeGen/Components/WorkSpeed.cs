using System;

using Unity.Entities;



namespace FSM.AI.States.Components
{
	[Serializable]
	public struct WorkSpeed : IComponentData
	{
		public float Value;

	}
}