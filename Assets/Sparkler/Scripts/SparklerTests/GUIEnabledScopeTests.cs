using NUnit.Framework;

using Sparkler.Utility.Editor;

using UnityEngine;

namespace Sparkler.Tests
{
	public class GUIEnabledScopeTests
	{
		#region Set true

		[Test]
		public void True_True()
		{
			var _oldEnable = GUI.enabled;
			GUI.enabled = true;

			using ( new GUIEnabledScope( true ) )
			{
				Assert.IsTrue( GUI.enabled );
			}
			Assert.IsTrue( GUI.enabled );

			GUI.enabled = _oldEnable;
		}

		[Test]
		public void True_True_Force()
		{
			var _oldEnable = GUI.enabled;
			GUI.enabled = true;

			using ( new GUIEnabledScope( true, true ) )
			{
				Assert.IsTrue( GUI.enabled );
			}
			Assert.IsTrue( GUI.enabled );

			GUI.enabled = _oldEnable;
		}

		[Test]
		public void Flase_True()
		{
			var _oldEnable = GUI.enabled;
			GUI.enabled = false;

			using ( new GUIEnabledScope( true ) )
			{
				Assert.IsFalse( GUI.enabled );
			}
			Assert.IsFalse( GUI.enabled );

			GUI.enabled = _oldEnable;
		}

		[Test]
		public void Flase_True_Force()
		{
			var _oldEnable = GUI.enabled;
			GUI.enabled = false;

			using ( new GUIEnabledScope( true, true ) )
			{
				Assert.IsTrue( GUI.enabled );
			}
			Assert.IsFalse( GUI.enabled );

			GUI.enabled = _oldEnable;
		}

		#endregion Set true

		#region Set false

		[Test]
		public void True_False()
		{
			var _oldEnable = GUI.enabled;
			GUI.enabled = true;

			using ( new GUIEnabledScope( false ) )
			{
				Assert.IsFalse( GUI.enabled );
			}
			Assert.IsTrue( GUI.enabled );

			GUI.enabled = _oldEnable;
		}

		[Test]
		public void True_False_Force()
		{
			var _oldEnable = GUI.enabled;
			GUI.enabled = true;

			using ( new GUIEnabledScope( false, true ) )
			{
				Assert.IsFalse( GUI.enabled );
			}
			Assert.IsTrue( GUI.enabled );

			GUI.enabled = _oldEnable;
		}

		[Test]
		public void Flase_False()
		{
			var _oldEnable = GUI.enabled;
			GUI.enabled = false;

			using ( new GUIEnabledScope( false, true ) )
			{
				Assert.IsFalse( GUI.enabled );
			}
			Assert.IsFalse( GUI.enabled );

			GUI.enabled = _oldEnable;
		}

		[Test]
		public void Flase_False_Force()
		{
			var _oldEnable = GUI.enabled;
			GUI.enabled = false;

			using ( new GUIEnabledScope( false, true ) )
			{
				Assert.IsFalse( GUI.enabled );
			}
			Assert.IsFalse( GUI.enabled );

			GUI.enabled = _oldEnable;
		}

		#endregion Set false
	}
}