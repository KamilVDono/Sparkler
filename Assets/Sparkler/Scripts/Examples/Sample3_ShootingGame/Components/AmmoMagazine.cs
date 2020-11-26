using System;

using Unity.Entities;



namespace Sparkler.Example.Shooter.Components
{
	[Serializable]
	
	[GenerateAuthoringComponent]
	
	public struct AmmoMagazine : IComponentData
	{
		public byte MagazineSize;
		public byte CurrentMagazine;

	}
}