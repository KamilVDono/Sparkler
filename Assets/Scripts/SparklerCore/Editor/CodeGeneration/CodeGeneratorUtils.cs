using System.IO;
using System.Text.RegularExpressions;

using UnityEngine;

namespace Sparkler.Editor.CodeGeneration
{
	public static class CodeGeneratorUtils
	{
		public static string ConditionalText( bool condition, string conditionalName, string input )
		{
			if ( !condition )
			{
				input = Regex.Replace( input, $"(#\\${conditionalName})(?s)(.+?)(\\$#{conditionalName})", "" );
				return Regex.Replace( input, $"(#\\$!{conditionalName})(?s)(.+?)(\\$#!{conditionalName})", m => m.Groups[2].Value );
			}
			else
			{
				input = Regex.Replace( input, $"(#\\$!{conditionalName})(?s)(.+?)(\\$#!{conditionalName})", "" );
				return Regex.Replace( input, $"(#\\${conditionalName})(?s)(.+?)(\\$#{conditionalName})", m => m.Groups[2].Value );
			}
		}

		public static string LoadTemplate( string name )
		{
			var filePath = Path.Combine( Application.dataPath, "Templates", $"{name}.txt" );
			return File.ReadAllText( filePath );
		}
	}
}