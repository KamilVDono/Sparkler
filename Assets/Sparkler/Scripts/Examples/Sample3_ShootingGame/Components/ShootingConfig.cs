using System;

using Unity.Entities;



namespace Sparkler.Example.Shooter.Components
{
	[Serializable]
	
	[GenerateAuthoringComponent]
	
	public struct ShootingConfig : IComponentData
	{
		public float SpreadAngle;
		public byte AmmoBurst;
		public byte ShootsPerSecond;

	}
}