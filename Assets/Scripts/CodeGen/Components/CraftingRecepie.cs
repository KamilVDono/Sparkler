using System;

using Unity.Entities;



namespace FSM.AI.States.Components
{
	[Serializable]
	public struct CraftingRecepie : IComponentData
	{
		public int RequiredWork;
		public float WorkDone;

	}
}