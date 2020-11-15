using System;

using Unity.Entities;

namespace Sparkler.Example.Components
{
	[Serializable]
	public struct Heading : IComponentData
	{
		public Place Place;
		public float Progression;
	}
}