using System;

using UnityEngine;

namespace Sparkler.Utility.Editor
{
	public struct GUIEnabledScope : IDisposable
	{
		private bool _oldEnable;

		public GUIEnabledScope( bool enable, bool force = false )
		{
			_oldEnable = GUI.enabled;
			if ( force )
			{
				GUI.enabled = enable;
			}
			else
			{
				GUI.enabled &= enable;
			}
		}

		public void Dispose() => GUI.enabled = _oldEnable;
	}
}