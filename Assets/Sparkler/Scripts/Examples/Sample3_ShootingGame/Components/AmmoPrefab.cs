using System;

using Unity.Entities;

using Unity.Entities;


namespace Sparkler.Example.Shooter.Components
{
	[Serializable]
	
	[GenerateAuthoringComponent]
	
	public struct AmmoPrefab : IComponentData
	{
		public Entity Prefab;

	}
}