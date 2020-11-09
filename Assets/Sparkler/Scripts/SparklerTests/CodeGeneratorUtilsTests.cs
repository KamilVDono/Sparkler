using NUnit.Framework;

using Sparkler.Editor.CodeGeneration;

using System.Text;

namespace Sparkler.Tests
{
	public class CodeGeneratorUtilsTests
	{
		[Test]
		public void ConditionalText_True()
		{
			string text = new StringBuilder()
				.AppendLine("#$TAG")
				.AppendLine("True")
				.AppendLine("$#TAG")
				.AppendLine("#$!TAG")
				.AppendLine("False")
				.AppendLine("$#!TAG")
				.AppendLine("Other part").ToString();

			var afterText = CodeGeneratorUtils.ConditionalText( true, "TAG", text );
			Assert.IsTrue( afterText.Contains( "True" ) );
			Assert.IsTrue( afterText.Contains( "Other part" ) );
			Assert.IsFalse( afterText.Contains( "False" ) );
		}

		[Test]
		public void ConditionalText_False()
		{
			string text = new StringBuilder()
				.AppendLine("#$TAG")
				.AppendLine("True")
				.AppendLine("$#TAG")
				.AppendLine("#$!TAG")
				.AppendLine("False")
				.AppendLine("$#!TAG")
				.AppendLine("Other part").ToString();

			var afterText = CodeGeneratorUtils.ConditionalText( false, "TAG", text );
			Assert.IsFalse( afterText.Contains( "True" ) );
			Assert.IsTrue( afterText.Contains( "Other part" ) );
			Assert.IsTrue( afterText.Contains( "False" ) );
		}

		[Test]
		public void ConditionalText_True_InvalidName()
		{
			string text = new StringBuilder()
				.AppendLine("#$TAG")
				.AppendLine("True")
				.AppendLine("$#TAG")
				.AppendLine("#$!TAG")
				.AppendLine("False")
				.AppendLine("$#!TAG")
				.AppendLine("Other part").ToString();

			var afterText = CodeGeneratorUtils.ConditionalText( true, "tag", text );
			Assert.IsTrue( afterText.Contains( "True" ) );
			Assert.IsTrue( afterText.Contains( "Other part" ) );
			Assert.IsTrue( afterText.Contains( "False" ) );
			Assert.IsTrue( afterText.Contains( "#$TAG" ) );
			Assert.IsTrue( afterText.Contains( "#$!TAG" ) );
		}

		[Test]
		public void ConditionalText_Flase_InvalidName()
		{
			string text = new StringBuilder()
				.AppendLine("#$TAG")
				.AppendLine("True")
				.AppendLine("$#TAG")
				.AppendLine("#$!TAG")
				.AppendLine("False")
				.AppendLine("$#!TAG")
				.AppendLine("Other part").ToString();

			var afterText = CodeGeneratorUtils.ConditionalText( false, "tag", text );
			Assert.IsTrue( afterText.Contains( "True" ) );
			Assert.IsTrue( afterText.Contains( "Other part" ) );
			Assert.IsTrue( afterText.Contains( "False" ) );
			Assert.IsTrue( afterText.Contains( "#$TAG" ) );
			Assert.IsTrue( afterText.Contains( "#$!TAG" ) );
		}

		[Test]
		public void ConditionalText_True_OnlyTrue()
		{
			string text = new StringBuilder()
				.AppendLine("#$TAG")
				.AppendLine("True")
				.AppendLine("$#TAG")
				.AppendLine("Other part").ToString();

			var afterText = CodeGeneratorUtils.ConditionalText( true, "TAG", text );
			Assert.IsTrue( afterText.Contains( "True" ) );
			Assert.IsTrue( afterText.Contains( "Other part" ) );
			Assert.IsFalse( afterText.Contains( "False" ) );
		}

		[Test]
		public void ConditionalText_False_OnlyTrue()
		{
			string text = new StringBuilder()
				.AppendLine("#$TAG")
				.AppendLine("True")
				.AppendLine("$#TAG")
				.AppendLine("Other part").ToString();

			var afterText = CodeGeneratorUtils.ConditionalText( false, "TAG", text );
			Assert.IsFalse( afterText.Contains( "True" ) );
			Assert.IsTrue( afterText.Contains( "Other part" ) );
			Assert.IsFalse( afterText.Contains( "False" ) );
		}

		[Test]
		public void ConditionalText_True_OnlyFalse()
		{
			string text = new StringBuilder()
				.AppendLine("#$!TAG")
				.AppendLine("False")
				.AppendLine("$#!TAG")
				.AppendLine("Other part").ToString();

			var afterText = CodeGeneratorUtils.ConditionalText( true, "TAG", text );
			Assert.IsFalse( afterText.Contains( "True" ) );
			Assert.IsTrue( afterText.Contains( "Other part" ) );
			Assert.IsFalse( afterText.Contains( "False" ) );
		}

		[Test]
		public void ConditionalText_False_OnlyFalse()
		{
			string text = new StringBuilder()
				.AppendLine("#$!TAG")
				.AppendLine("False")
				.AppendLine("$#!TAG")
				.AppendLine("Other part").ToString();

			var afterText = CodeGeneratorUtils.ConditionalText( false, "TAG", text );
			Assert.IsFalse( afterText.Contains( "True" ) );
			Assert.IsTrue( afterText.Contains( "Other part" ) );
			Assert.IsTrue( afterText.Contains( "False" ) );
		}
	}
}