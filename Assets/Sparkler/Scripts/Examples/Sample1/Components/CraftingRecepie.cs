using System;

using Unity.Entities;



namespace Sparkler.AI.States.Components
{
	[Serializable]
	public struct CraftingRecepie : IComponentData
	{
		public int RequiredWork;
		public float WorkDone;

	}
}