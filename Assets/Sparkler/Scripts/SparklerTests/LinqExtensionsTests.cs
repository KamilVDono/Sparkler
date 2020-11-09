using NUnit.Framework;

using Sparkler.Utility;

using System.Linq;

namespace Sparkler.Tests
{
	public class LinqExtensionsTests
	{
		#region IndexOf

		[Test]
		public void IndexOf_Empty() => Assert.AreEqual( Empty<int>().IndexOf( 1 ), -1 );

		[Test]
		public void IndexOf_NotPresent() => Assert.AreEqual( Enumerable.Repeat( 0, 5 ).IndexOf( 1 ), -1 );

		[Test]
		public void IndexOf_Only() => Assert.AreEqual( ( new int[] { 1 } ).IndexOf( 1 ), 0 );

		[Test]
		public void IndexOf_Multiple() => Assert.AreEqual( ( new int[] { 1, 1, 1, 1 } ).IndexOf( 1 ), 0 );

		#endregion IndexOf

		#region SkipLastN

		[Test]
		public void SkipLastN()
		{
			var range = Enumerable.Range( 1, 10 );
			var skipped = range.SkipLastN(3).ToArray();
			Assert.AreEqual( skipped.Length, 7 );
			Assert.AreEqual( skipped.Last(), 7 );
		}

		[Test]
		public void SkipLastN_NotEnought()
		{
			var range = Enumerable.Range( 1, 2 );
			var skipped = range.SkipLastN(3).ToArray();
			Assert.AreEqual( skipped.Length, 0 );
		}

		#endregion SkipLastN

		private T[] Empty<T>() => new T[0];
	}
}