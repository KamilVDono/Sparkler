using System;

using Unity.Entities;

namespace Sparkler.Example.Shooter.Components
{
	[Serializable]
	[GenerateAuthoringComponent]
	public struct LastShootTime : IComponentData
	{
		public double Value;
	}
}