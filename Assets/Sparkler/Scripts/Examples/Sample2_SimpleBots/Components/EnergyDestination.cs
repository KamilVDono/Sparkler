using System;

using Unity.Entities;



namespace Sparkler.Example.Components
{
	[Serializable]
	public struct EnergyDestination : IComponentData
	{
		public float Value;

	}
}