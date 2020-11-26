using System;

using Unity.Entities;



namespace Sparkler.Example.Shooter.Components
{
	[Serializable]
	
	public struct ReloadingProgress : IComponentData
	{
		public float Value;

	}
}