using System;

using Unity.Entities;

namespace Sparkler.Example.Components
{
	[Serializable]
	public struct GameTime : IComponentData
	{
		public float Elapsed;
		public float Delta;
		public byte Hours;
		public byte Days;
		public byte Months;
		public byte Years;
	}
}