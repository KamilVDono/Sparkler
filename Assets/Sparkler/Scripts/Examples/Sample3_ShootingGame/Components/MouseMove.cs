using System;

using Unity.Entities;

using Unity.Mathematics;

namespace Sparkler.Example.Shooter.Components
{
	[Serializable]
	public struct MouseMove : IComponentData
	{
		public float2 Position;
		public float2 PreviousPosition;
	}
}