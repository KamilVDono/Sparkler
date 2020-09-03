using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using Unity.Entities;

namespace FSM.Editor.CodeGens
{
	public static class SystemReader
	{
		public static FileSystemData Read( string systemFilePath )
		{
			var fileContent = File.ReadAllText(systemFilePath);
			var match = Regex.Match( fileContent, @"(class\s+)(\S+)" );
			if ( match?.Groups.Count != 3 )
			{
				return null;
			}
			var className = match.Groups[2].Value;
			var systemType = AppDomain.CurrentDomain.GetAssemblies()
							.SelectMany( a => a.GetTypes() )
							.First( t => t.Name == className );

			var isSystem = typeof( SystemBase ).IsAssignableFrom( systemType );
			if ( !isSystem )
			{
				return null;
			}

			var systemName = className;
			if ( systemName.EndsWith( "System" ) )
			{
				systemName = systemName.Substring( 0, systemName.Length - "system".Length );
			}

			var withoutSingleComments = Regex.Replace( fileContent, @"^\s+\/\/.+$", "", RegexOptions.Multiline );
			var withoutComments = Regex.Replace(withoutSingleComments, @"\/\*.+\*\/", "", RegexOptions.Singleline);
			var withoutWhitespaces = Regex.Replace( withoutComments, @"[\r\n\t\f\v]+", " " );
			var lambdas = Regex.Matches( withoutWhitespaces, @"(?<=;)(\s*)(Entities.+?)(\s*;)" );

			FileSystemData data = new FileSystemData()
			{
				Name = systemName,
				Lambdas = new FileSystemLambdaData[lambdas.Count],
			};

			for ( int i = 0; i < lambdas.Count; i++ )
			{
				var lambda = lambdas[i];
				var lambdaData = ProcessLambda( lambda.Groups[2].Value );
				data.Lambdas[i] = lambdaData;
			}

			return data;
		}

		private static FileSystemLambdaData ProcessLambda( string lambdaBody )
		{
			FileSystemLambdaData lambdaData = new FileSystemLambdaData();

			var hasRun = Regex.IsMatch( lambdaBody, @"\.Run\(\)$" );
			var hasSchedule = Regex.IsMatch( lambdaBody, @"\.Schedule\(\)$" );
			var hasScheduleParallel = Regex.IsMatch( lambdaBody, @"\.ScheduleParallel\(\)$" );

			lambdaData.LaunchType = hasRun ? LaunchType.Run : ( hasSchedule ? LaunchType.Schedule : LaunchType.ScheduleParallel );

			lambdaData.Name = Regex.Match( lambdaBody, @"\.\s*WithName\s*\(\s*""(.+)""\s*\)" )?.Groups[1].Value ?? "";
			lambdaData.QueryField = Regex.Match( lambdaBody, @"\.\s*WithStoreEntityQueryInField\s*\(\s*ref\s*(.+?)\s*\)" )?.Groups[1].Value ?? "";
			lambdaData.SharedComponentFilter = Regex.Match( lambdaBody, @"\.\s*WithSharedComponentFilter\s*\(\s*(.+?)\s*?\)" )?.Groups[1].Value ?? "";
			var withAny = Regex.Match( lambdaBody, @"\.\s*WithAny\s*<\s*(.+?)\s*?>" )?.Groups[1].Value.Split( ',' ).Select( s => s.Trim() ).Where( s => !string.IsNullOrWhiteSpace(s)).ToArray() ?? new string[0];
			lambdaData.WithAny.AddRange( withAny );
			var withAll = Regex.Match( lambdaBody, @"\.\s*WithAll\s*<\s*(.+?)\s*?>" )?.Groups[1].Value.Split( ',' ).Select( s => s.Trim() ).Where( s => !string.IsNullOrWhiteSpace(s)).ToArray() ?? new string[0];
			lambdaData.WithAll.AddRange( withAll );
			var withNone = Regex.Match( lambdaBody, @"\.\s*WithNone\s*<\s*(.+?)\s*?>" )?.Groups[1].Value.Split( ',' ).Select( s => s.Trim() ).Where( s => !string.IsNullOrWhiteSpace(s)).ToArray() ?? new string[0];
			lambdaData.WithNone.AddRange( withNone );

			var inParametersMateches = Regex.Matches(lambdaBody, @"in\s+(.+?)\s+(.+?)[\s,]");
			for ( int i = 0; i < inParametersMateches.Count; i++ )
			{
				var typeName = inParametersMateches[i].Groups[1].Value;
				if ( string.IsNullOrWhiteSpace( typeName.Trim() ) )
				{
					continue;
				}
				lambdaData.InParameter.Add( typeName );
			}

			var refParametersMateches = Regex.Matches(lambdaBody, @"ref\s+([^a]+?)\s+([^a]+?)[\s,]");
			for ( int i = 0; i < refParametersMateches.Count; i++ )
			{
				var typeName = refParametersMateches[i].Groups[1].Value;
				if ( string.IsNullOrWhiteSpace( typeName.Trim() ) )
				{
					continue;
				}
				lambdaData.RefParamter.Add( typeName );
			}

			return lambdaData;
		}
	}
}