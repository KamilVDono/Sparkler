using NUnit.Framework;

using Sparkler.Utility.Editor;

using System;

using UnityEditor;

using UnityEngine;

namespace Sparkler.Tests
{
	public class PropertyRectTests
	{
		#region AllocateLine

		[Test]
		public void AllocateLine_Default()
		{
			int width = 512;
			int heigth = 512;
			Rect originalRect = new Rect(0, 0, width, heigth);
			PropertyRect propertyRect = new PropertyRect(originalRect);
			var line = propertyRect.AllocateLine();
			Assert.AreEqual( line.height, EditorGUIUtility.singleLineHeight );
			Assert.AreEqual( line.y, 0 );
			Assert.AreEqual( line.width, width );
		}

		[Test]
		public void AllocateLine_DefaultDouble()
		{
			int width = 512;
			int heigth = 512;
			Rect originalRect = new Rect(0, 0, width, heigth);
			PropertyRect propertyRect = new PropertyRect(originalRect);
			propertyRect.AllocateLine();
			var line = propertyRect.AllocateLine();
			Assert.AreEqual( line.height, EditorGUIUtility.singleLineHeight );
			Assert.AreEqual( line.y, EditorGUIUtility.singleLineHeight );
			Assert.AreEqual( line.width, width );
		}

		[Test]
		public void AllocateLine_Minus()
		{
			int width = 512;
			int heigth = 512;
			Rect originalRect = new Rect(0, 0, width, heigth);
			PropertyRect propertyRect = new PropertyRect(originalRect);
			Assert.Throws<ArgumentOutOfRangeException>( () => propertyRect.AllocateLine( -5 ) );
		}

		[Test]
		public void AllocateLine_Zero()
		{
			int width = 512;
			int heigth = 512;
			Rect originalRect = new Rect(0, 0, width, heigth);
			PropertyRect propertyRect = new PropertyRect(originalRect);
			Assert.Throws<ArgumentOutOfRangeException>( () => propertyRect.AllocateLine( 0 ) );
		}

		#endregion AllocateLine

		#region AllocateWidthFlat

		[Test]
		public void AllocateWidthFlat_WithoutLine()
		{
			int width = 512;
			int heigth = 512;
			Rect originalRect = new Rect(0, 0, width, heigth);
			PropertyRect propertyRect = new PropertyRect(originalRect);
			Assert.Throws<InvalidOperationException>( () => propertyRect.AllocateWidthFlat( 10 ) );
		}

		[Test]
		public void AllocateWidthFlat()
		{
			int width = 512;
			int heigth = 512;
			Rect originalRect = new Rect(0, 0, width, heigth);
			PropertyRect propertyRect = new PropertyRect(originalRect);
			propertyRect.AllocateLine();
			var rect = propertyRect.AllocateWidthFlat( 10 );
			Assert.AreEqual( rect.height, EditorGUIUtility.singleLineHeight );
			Assert.AreEqual( rect.width, 10 );
		}

		#endregion AllocateWidthFlat

		#region AllocateWidthPrecent

		[Test]
		public void AllocateWidthPrecent_WithoutLine()
		{
			int width = 512;
			int heigth = 512;
			Rect originalRect = new Rect(0, 0, width, heigth);
			PropertyRect propertyRect = new PropertyRect(originalRect);
			Assert.Throws<InvalidOperationException>( () => propertyRect.AllocateWidthPrecent( 0.5f ) );
		}

		[Test]
		public void AllocateWidthPrecent_Minus()
		{
			int width = 512;
			int heigth = 512;
			Rect originalRect = new Rect(0, 0, width, heigth);
			PropertyRect propertyRect = new PropertyRect(originalRect);
			propertyRect.AllocateLine();
			Assert.Throws<ArgumentOutOfRangeException>( () => propertyRect.AllocateWidthPrecent( -1 ) );
		}

		[Test]
		public void AllocateWidthPrecent_Zero()
		{
			int width = 512;
			int heigth = 512;
			Rect originalRect = new Rect(0, 0, width, heigth);
			PropertyRect propertyRect = new PropertyRect(originalRect);
			propertyRect.AllocateLine();
			Assert.Throws<ArgumentOutOfRangeException>( () => propertyRect.AllocateWidthPrecent( 0 ) );
		}

		[Test]
		public void AllocateWidthPrecent_OverOne()
		{
			int width = 512;
			int heigth = 512;
			Rect originalRect = new Rect(0, 0, width, heigth);
			PropertyRect propertyRect = new PropertyRect(originalRect);
			propertyRect.AllocateLine();
			Assert.Throws<ArgumentOutOfRangeException>( () => propertyRect.AllocateWidthPrecent( 1.1f ) );
		}

		[Test]
		public void AllocateWidthPrecent()
		{
			int width = 512;
			int heigth = 512;
			Rect originalRect = new Rect(0, 0, width, heigth);
			PropertyRect propertyRect = new PropertyRect(originalRect);
			propertyRect.AllocateLine();
			var rect = propertyRect.AllocateWidthPrecent( 0.5f );
			Assert.AreEqual( rect.height, EditorGUIUtility.singleLineHeight );
			Assert.AreEqual( rect.width, width * 0.5f );
		}

		#endregion AllocateWidthPrecent

		#region AllocateWidthWithAscesorFlat

		[Test]
		public void AllocateWidthWithAscesorFlat_WithoutLine()
		{
			int width = 512;
			int heigth = 512;
			Rect originalRect = new Rect(0, 0, width, heigth);
			PropertyRect propertyRect = new PropertyRect(originalRect);
			Assert.Throws<InvalidOperationException>( () => propertyRect.AllocateWidthWithAscesorFlat( 12 ) );
		}

		[Test]
		public void AllocateWidthWithAscesorFlat_Minus()
		{
			int width = 512;
			int heigth = 512;
			Rect originalRect = new Rect(0, 0, width, heigth);
			PropertyRect propertyRect = new PropertyRect(originalRect);
			propertyRect.AllocateLine();
			Assert.Throws<ArgumentOutOfRangeException>( () => propertyRect.AllocateWidthWithAscesorFlat( -1 ) );
		}

		[Test]
		public void AllocateWidthWithAscesorFlat_Zero()
		{
			int width = 512;
			int heigth = 512;
			Rect originalRect = new Rect(0, 0, width, heigth);
			PropertyRect propertyRect = new PropertyRect(originalRect);
			propertyRect.AllocateLine();
			Assert.Throws<ArgumentOutOfRangeException>( () => propertyRect.AllocateWidthWithAscesorFlat( 0 ) );
		}

		[Test]
		public void AllocateWidthWithAscesorFlat()
		{
			int width = 512;
			int heigth = 512;
			Rect originalRect = new Rect(0, 0, width, heigth);
			PropertyRect propertyRect = new PropertyRect(originalRect);
			propertyRect.AllocateLine();
			var rect = propertyRect.AllocateWidthWithAscesorFlat( 12 );
			Assert.AreEqual( rect.height, EditorGUIUtility.singleLineHeight );
			Assert.AreEqual( rect.width, width - 12 );
		}

		#endregion AllocateWidthWithAscesorFlat

		#region AllocateRestOfLine

		[Test]
		public void AllocateRestOfLine_WithoutLine()
		{
			int width = 512;
			int heigth = 512;
			Rect originalRect = new Rect(0, 0, width, heigth);
			PropertyRect propertyRect = new PropertyRect(originalRect);
			Assert.Throws<InvalidOperationException>( () => propertyRect.AllocateRestOfLine() );
		}

		[Test]
		public void AllocateRestOfLine_WholeLine()
		{
			int width = 512;
			int heigth = 512;
			Rect originalRect = new Rect(0, 0, width, heigth);
			PropertyRect propertyRect = new PropertyRect(originalRect);
			propertyRect.AllocateLine();
			var rect = propertyRect.AllocateRestOfLine();
			Assert.AreEqual( rect.height, EditorGUIUtility.singleLineHeight );
			Assert.AreEqual( rect.width, width );
		}

		[Test]
		public void AllocateRestOfLine_With_AllocateWidthFlat()
		{
			int width = 512;
			int heigth = 512;
			Rect originalRect = new Rect(0, 0, width, heigth);
			PropertyRect propertyRect = new PropertyRect(originalRect);
			propertyRect.AllocateLine();
			propertyRect.AllocateWidthFlat( 500 );
			var rect = propertyRect.AllocateRestOfLine();
			Assert.AreEqual( rect.height, EditorGUIUtility.singleLineHeight );
			Assert.AreEqual( rect.width, width - 500 );
		}

		[Test]
		public void AllocateRestOfLine_With_AllocateWidthWithAscesorFlat()
		{
			int width = 512;
			int heigth = 512;
			Rect originalRect = new Rect(0, 0, width, heigth);
			PropertyRect propertyRect = new PropertyRect(originalRect);
			propertyRect.AllocateLine();
			propertyRect.AllocateWidthWithAscesorFlat( 12 );
			var rect = propertyRect.AllocateRestOfLine();
			Assert.AreEqual( rect.height, EditorGUIUtility.singleLineHeight );
			Assert.AreEqual( rect.width, 12 );
		}

		#endregion AllocateRestOfLine
	}
}