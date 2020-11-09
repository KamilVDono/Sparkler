using NUnit.Framework;

using Sparkler.Editor.CodeGeneration;

using System;

namespace Sparkler.Tests
{
	public class ProcessorsSelectorTests
	{
		[Test]
		public void NoClassesImplementsInterface()
		{
			var selectors = ProcessorsSelector.Selectors<ITest0>();
			Assert.NotNull( selectors );
			Assert.Zero( selectors.Count );
		}

		[Test]
		public void NoOrderingAttributtes()
		{
			var selectors = ProcessorsSelector.Selectors<ITest1>();
			Assert.NotNull( selectors );
			Assert.AreEqual( selectors.Count, 3 );
		}

		[Test]
		public void ProcessAfter()
		{
			var selectors = ProcessorsSelector.Selectors<ITest2>();
			Assert.NotNull( selectors );
			Assert.AreEqual( selectors.Count, 3 );
			Assert.AreEqual( selectors[0].GetType(), typeof( ClassTest2_1 ) );
			Assert.AreEqual( selectors[1].GetType(), typeof( ClassTest2_2 ) );
			Assert.AreEqual( selectors[2].GetType(), typeof( ClassTest2_3 ) );
		}

		[Test]
		public void ProcessBefore()
		{
			var selectors = ProcessorsSelector.Selectors<ITest3>();
			Assert.NotNull( selectors );
			Assert.AreEqual( selectors.Count, 3 );
			Assert.AreEqual( selectors[0].GetType(), typeof( ClassTest3_1 ) );
			Assert.AreEqual( selectors[1].GetType(), typeof( ClassTest3_2 ) );
			Assert.AreEqual( selectors[2].GetType(), typeof( ClassTest3_3 ) );
		}

		[Test]
		public void ProcessBefore_And_ProcessAfter()
		{
			var selectors = ProcessorsSelector.Selectors<ITest4>();
			Assert.NotNull( selectors );
			Assert.AreEqual( selectors.Count, 3 );
			Assert.AreEqual( selectors[0].GetType(), typeof( ClassTest4_1 ) );
			Assert.AreEqual( selectors[1].GetType(), typeof( ClassTest4_2 ) );
			Assert.AreEqual( selectors[2].GetType(), typeof( ClassTest4_3 ) );
		}

		[Test]
		public void Cyclic_ProcessAfter()
		{
			Assert.Throws<Exception>( () =>
			 {
				 ProcessorsSelector.Selectors<ITest5>();
			 } );
		}

		[Test]
		public void Cyclic_ProcessBefore()
		{
			Assert.Throws<Exception>( () =>
			{
				ProcessorsSelector.Selectors<ITest6>();
			} );
		}

		[Test]
		public void Cyclic_Mixed()
		{
			Assert.Throws<Exception>( () =>
			{
				ProcessorsSelector.Selectors<ITest7>();
			} );
		}

		[Test]
		public void ProcessAfter_Double()
		{
			var selectors = ProcessorsSelector.Selectors<ITest8>();
			Assert.NotNull( selectors );
			Assert.AreEqual( selectors.Count, 3 );
			Assert.AreEqual( selectors[0].GetType(), typeof( ClassTest8_2 ) );
		}

		#region Test classes and interfaces

		#region Test NoClassesImplementsInterface

		private interface ITest0 { }

		#endregion Test NoClassesImplementsInterface

		#region Test NoOrderingAttributtes

		private interface ITest1 { }

		private class ClassTest1_1 : ITest1 { }

		private class ClassTest1_2 : ITest1 { }

		private class ClassTest1_3 : ITest1 { }

		#endregion Test NoOrderingAttributtes

		#region Test ProcessAfter

		private interface ITest2 { }

		private class ClassTest2_1 : ITest2 { }

		[ProcessAfter( typeof( ClassTest2_1 ) )]
		private class ClassTest2_2 : ITest2 { }

		[ProcessAfter( typeof( ClassTest2_2 ) )]
		private class ClassTest2_3 : ITest2 { }

		#endregion Test ProcessAfter

		#region Test ProcessBefore

		private interface ITest3 { }

		[ProcessBefore( typeof( ClassTest3_2 ) )]
		private class ClassTest3_1 : ITest3 { }

		[ProcessBefore( typeof( ClassTest3_3 ) )]
		private class ClassTest3_2 : ITest3 { }

		private class ClassTest3_3 : ITest3 { }

		#endregion Test ProcessBefore

		#region Test ProcessBefore_And_ProcessAfter

		private interface ITest4 { }

		private class ClassTest4_1 : ITest4 { }

		[ProcessAfter( typeof( ClassTest4_1 ) )]
		[ProcessBefore( typeof( ClassTest4_3 ) )]
		private class ClassTest4_2 : ITest4 { }

		private class ClassTest4_3 : ITest4 { }

		#endregion Test ProcessBefore_And_ProcessAfter

		#region Cyclic_ProcessAfter

		private interface ITest5 { }

		[ProcessAfter( typeof( ClassTest5_3 ) )]
		private class ClassTest5_1 : ITest5 { }

		[ProcessAfter( typeof( ClassTest5_1 ) )]
		private class ClassTest5_2 : ITest5 { }

		[ProcessAfter( typeof( ClassTest5_2 ) )]
		private class ClassTest5_3 : ITest5 { }

		#endregion Cyclic_ProcessAfter

		#region Cyclic_ProcessBefore

		private interface ITest6 { }

		[ProcessBefore( typeof( ClassTest6_2 ) )]
		private class ClassTest6_1 : ITest6 { }

		[ProcessBefore( typeof( ClassTest6_3 ) )]
		private class ClassTest6_2 : ITest6 { }

		[ProcessBefore( typeof( ClassTest6_1 ) )]
		private class ClassTest6_3 : ITest6 { }

		#endregion Cyclic_ProcessBefore

		#region Cyclic_Mixed

		private interface ITest7 { }

		[ProcessBefore( typeof( ClassTest7_2 ) )]
		private class ClassTest7_1 : ITest7 { }

		[ProcessBefore( typeof( ClassTest7_3 ) )]
		private class ClassTest7_2 : ITest7 { }

		[ProcessBefore( typeof( ClassTest7_2 ) )]
		private class ClassTest7_3 : ITest7 { }

		#endregion Cyclic_Mixed

		#region ProcessAfter_Double

		private interface ITest8 { }

		[ProcessAfter( typeof( ClassTest8_2 ) )]
		private class ClassTest8_1 : ITest8 { }

		[ProcessBefore( typeof( ClassTest8_1 ) )]
		private class ClassTest8_2 : ITest8 { }

		[ProcessAfter( typeof( ClassTest8_2 ) )]
		private class ClassTest8_3 : ITest8 { }

		#endregion ProcessAfter_Double

		#endregion Test classes and interfaces
	}
}