﻿using System;

using Unity.Entities;

namespace Sparkler.Example.Components
{
	[Serializable]
	public struct TimeSpeed : IComponentData
	{
		public float Value;
	}
}