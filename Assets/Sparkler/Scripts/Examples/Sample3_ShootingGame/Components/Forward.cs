using System;

using Unity.Entities;

using Unity.Mathematics;


namespace Sparkler.Example.Shooter.Components
{
	[Serializable]
	
	public struct Forward : IComponentData
	{
		public float2 Value;

	}
}