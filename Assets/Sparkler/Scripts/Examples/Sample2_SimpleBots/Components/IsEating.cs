using System;

using Unity.Entities;



namespace Sparkler.Example.Components
{
	[Serializable]
	public struct IsEating : IComponentData
	{
		public byte Value;
	}
}