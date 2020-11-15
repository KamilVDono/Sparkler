using System;

using Unity.Entities;

namespace Sparkler.Example.Components
{
	public enum Place : byte
	{
		InTravel,
		Home,
		Mine
	}

	[Serializable]
	public struct InPlace : ISharedComponentData
	{
		public Place Place;
	}
}