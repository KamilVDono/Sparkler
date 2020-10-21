using System;

using Unity.Entities;



namespace Sparkler.AI.States.Components
{
	[Serializable]
	public struct WorkSpeed : IComponentData
	{
		public float Value;

	}
}