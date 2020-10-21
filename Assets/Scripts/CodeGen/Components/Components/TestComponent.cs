using System;

using Unity.Entities;



namespace Sparkler.Components
{
	[Serializable]
	public struct TestComponent : IComponentData
	{
		public byte ByteValue;
		public float FloatValue;

	}
}