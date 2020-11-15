using System;

using Unity.Entities;



namespace Sparkler.Example.Components
{
	[Serializable]
	public struct EnergyStat : IComponentData
	{
		public float Count;
		public float FatiguePressure;

	}
}