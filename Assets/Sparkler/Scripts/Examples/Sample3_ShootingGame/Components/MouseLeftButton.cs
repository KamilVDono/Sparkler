using System;

using Unity.Entities;

namespace Sparkler.Example.Shooter.Components
{
	[Serializable]
	public struct MouseLeftButton : IComponentData
	{
		public ButtonState State;
	}
}