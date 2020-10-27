using Sparkler.Utility;

using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using UnityEditor;

namespace Sparkler.Editor.CodeGeneration
{
	public class CodeGenerator
	{
		private static readonly Regex s_lambdaRegex = new Regex(@"(#\$LAMBDA)(?s)(.+?)(\$#LAMBDA)");

		public static void Generate( SystemsGraph systemsGraph )
		{
			GenerateSystems( systemsGraph );
			AssetDatabase.Refresh();
		}

		public static void Generate( ComponentDefinition componentDefinition )
		{
			GenerateComponent( componentDefinition );
			AssetDatabase.Refresh();
		}

		#region Components

		private static void GenerateComponent( ComponentDefinition componentDefinition )
		{
			// Paths
			var componentsPath = componentDefinition.Directory;
			if ( componentsPath.EndsWith( "Components" ) )
			{
				componentsPath = PathExtension.SystemPath( componentsPath );
			}
			else
			{
				componentsPath = PathExtension.SystemPath( Path.Combine( componentsPath, "Components" ) );
			}
			if ( !Directory.Exists( componentsPath ) )
			{
				Directory.CreateDirectory( componentsPath );
			}

			var template = CodeGeneratorUtils.LoadTemplate("Component");

			var processors = ProcessorsSelector.Selectors<ICodeGeneratorComponentProcessors>();
			foreach ( var processor in processors )
			{
				template = processor.Process( componentDefinition, template );
			}

			var upperedComponentName = componentDefinition.ComponentName.ToUpperFirstChar();
			var componentPath = Path.Combine( componentsPath, $"{upperedComponentName}.cs");
			GenerateComponentFile( upperedComponentName, componentPath, template );
		}

		private static void GenerateComponentFile( string componentName, string componentPath, string template )
		{
			var shouldProceed = !File.Exists( componentPath ) || EditorUtility.DisplayDialog(
						"Create component file",
						$"Component {componentName} at path {componentPath} already exists. Do you want regenerate this file?", "Yes", "No"
						);
			if ( shouldProceed )
			{
				File.WriteAllText( componentPath, template );
			}
		}

		#endregion Components

		#region Systems

		private static void GenerateSystems( SystemsGraph systemsGraph )
		{
			var states = systemsGraph.nodes.OfType<SystemNode>().Where( s => s.Editable ).ToList();

			var systemsPath = systemsGraph.CodeGenerationPath;
			if ( systemsPath.EndsWith( "Systems" ) )
			{
				systemsPath = PathExtension.SystemPath( systemsPath );
			}
			else
			{
				systemsPath = PathExtension.SystemPath( Path.Combine( systemsPath, "Systems" ) );
			}

			if ( !Directory.Exists( systemsPath ) )
			{
				Directory.CreateDirectory( systemsPath );
			}

			var loadedTemplate = CodeGeneratorUtils.LoadTemplate("System");

			foreach ( var system in states )
			{
				var template = loadedTemplate;
				var systemName = system.StateName;

				// ICodeGeneratorSystemProcessorBeforeLambda
				var beforeLambdaProcessors = ProcessorsSelector.Selectors<ICodeGeneratorSystemProcessorBeforeLambda>();
				foreach ( var processor in beforeLambdaProcessors )
				{
					template = processor.ProcessBeforeLambda( systemsGraph, system, template );
				}

				// Process lambdas
				var lambdaTemplate = s_lambdaRegex.Match( template ).Groups[2].Value;
				StringBuilder lambdaTemplateBuilder = new StringBuilder();
				foreach ( var lambda in system.Lambdas )
				{
					var currentTemplate = lambdaTemplate;

					// ICodeGeneratorSystemProcessorLambda
					var lambdaProcessors = ProcessorsSelector.Selectors<ICodeGeneratorSystemProcessorLambda>();
					foreach ( var processor in lambdaProcessors )
					{
						currentTemplate = processor.ProcessLambda( system, lambda, currentTemplate );
					}

					// Add lambda to others
					lambdaTemplateBuilder.AppendLine( currentTemplate );
				}

				template = CodeGeneratorUtils.ConditionalText( system.HasStructuralChanges, "STRUCTURAL_CHANGES", template );

				template = s_lambdaRegex.Replace( template, lambdaTemplateBuilder.ToString() );

				// ICodeGeneratorSystemProcessorAfterLambda
				var afterLambdaProcessors = ProcessorsSelector.Selectors<ICodeGeneratorSystemProcessorAfterLambda>();
				foreach ( var processor in afterLambdaProcessors )
				{
					template = processor.ProcessAfterLambda( systemsGraph, system, template );
				}

				// Generate file
				var systemPath = Path.Combine( systemsPath, $"{systemName}.cs");
				GenerateSystemFile( systemName, systemPath, template );
			}
		}

		private static void GenerateSystemFile( string systemName, string systemPath, string template )
		{
			var shouldProceed = !File.Exists( systemPath ) || EditorUtility.DisplayDialog(
						"Create system file",
						$"System {systemName} at path {systemPath} already exists. Do you want regenerate this file?", "Yes", "No"
						);

			if ( shouldProceed )
			{
				File.WriteAllText( systemPath, template );
			}
		}

		#endregion Systems
	}
}