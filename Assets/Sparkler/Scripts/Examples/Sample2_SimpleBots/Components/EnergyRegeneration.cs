using System;

using Unity.Entities;



namespace Sparkler.Example.Components
{
	[Serializable]
	public struct EnergyRegeneration : IComponentData
	{
		public float Value;

	}
}