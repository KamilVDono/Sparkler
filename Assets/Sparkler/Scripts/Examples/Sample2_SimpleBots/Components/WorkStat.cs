using System;

using Unity.Entities;



namespace Sparkler.Example.Components
{
	[Serializable]
	public struct WorkStat : IComponentData
	{
		public float Speed;

	}
}