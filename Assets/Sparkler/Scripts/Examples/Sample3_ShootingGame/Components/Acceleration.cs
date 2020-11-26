using System;

using Unity.Entities;

using Unity.Mathematics;


namespace Sparkler.Example.Shooter.Components
{
	[Serializable]
	
	[GenerateAuthoringComponent]
	
	public struct Acceleration : IComponentData
	{
		public float2 Value;

	}
}