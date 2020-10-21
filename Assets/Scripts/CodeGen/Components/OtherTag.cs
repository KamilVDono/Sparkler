using System;

using Unity.Entities;

using Unity.Mathematics;


namespace Sparkler.AI.States.Components
{
	[Serializable]
	public struct OtherTag : IComponentData
	{
		public float Value;
		public float4x4 Matrix;

	}
}