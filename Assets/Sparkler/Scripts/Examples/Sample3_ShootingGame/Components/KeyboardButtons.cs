using System;

using Unity.Entities;

namespace Sparkler.Example.Shooter.Components
{
	[Serializable]
	public struct KeyboardButtons : IComponentData
	{
		public ButtonState RState;
	}
}