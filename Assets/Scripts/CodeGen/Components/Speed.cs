using System;

using Unity.Entities;



namespace Sparkler.AI.States.Components
{
	[Serializable]
	public struct Speed : IComponentData
	{
		public float Value;

	}
}