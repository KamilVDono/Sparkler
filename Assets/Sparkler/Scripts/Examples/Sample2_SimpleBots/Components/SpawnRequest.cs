using System;

using Unity.Entities;



namespace Sparkler.Example.Components
{
	[Serializable]
	public struct SpawnRequest : IComponentData
	{
		public uint Count;

	}
}