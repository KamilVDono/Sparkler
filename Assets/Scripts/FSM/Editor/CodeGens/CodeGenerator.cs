using FSM.Components;
using FSM.Utility;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using UnityEditor;

using UnityEngine;

namespace FSM.Editor
{
	public class CodeGenerator
	{
		private static readonly Regex s_lambdaRegex = new Regex(@"(#\$LAMBDA)(?s)(.+?)(\$#LAMBDA)");

		private FSMGraph _fsmGraph;
		private IReadOnlyCollection<StateNode> _states;
		private IReadOnlyCollection<ComponentLink> _components;

		private CodeGenerator()
		{
		}

		public static void Generate( FSMGraph fsmGraph )
		{
			var states = fsmGraph.nodes.OfType<StateNode>().ToList();
			var components = states.SelectMany(s => s.AllComponents).Distinct().ToList();

			CodeGenerator codeGenerator = new CodeGenerator()
			{
				_fsmGraph = fsmGraph,
				_states = states,
				_components = components
			};
			codeGenerator.Generate();
		}

		#region Generation

		private void Generate()
		{
			GenerateComponents();

			GenerateSystems();

			AssetDatabase.Refresh();
		}

		#region Components

		private void GenerateComponents()
		{
			var componentsPath = PathExtension.SystemPath( Path.Combine( _fsmGraph.CodeGenerationPath, "Components") );
			if ( !Directory.Exists( componentsPath ) )
			{
				Directory.CreateDirectory( componentsPath );
			}

			var unavailableComponents = _components
				.Where(c => c.IsHandWrited)
				.Select(c => c.HandwrittenName)
				.Distinct();

			var template = LoadTemplate("Component");

			foreach ( var componentName in unavailableComponents )
			{
				var upperedComponentName = char.ToUpperInvariant( componentName[0] ) + componentName.Substring( 1 );
				var componentPath = Path.Combine( componentsPath, $"{upperedComponentName}.cs");
				GenerateComponentFile( upperedComponentName, componentPath, template );
			}
		}

		private void GenerateComponentFile( string componentName, string componentPath, string template )
		{
			var shouldProceed = !File.Exists( componentPath ) || EditorUtility.DisplayDialog(
						"Create component file",
						$"Component {componentName} at path {componentPath} already exists. Do you want regenerate this file?", "Yes", "No"
						);
			if ( shouldProceed )
			{
				var code = template;
				code = Regex.Replace( code, @"\$NAMESPACE\$", _fsmGraph.Namespace );
				code = Regex.Replace( code, @"\$NAME\$", componentName );

				File.WriteAllText( componentPath, code );
			}
		}

		#endregion Components

		#region Systems

		private void GenerateSystems()
		{
			var systemsPath = PathExtension.SystemPath(Path.Combine( _fsmGraph.CodeGenerationPath, "Systems"));
			if ( !Directory.Exists( systemsPath ) )
			{
				Directory.CreateDirectory( systemsPath );
			}

			IEnumerable<(StateNode s, string)> systemsData = _states.Select(s => (s, s.StateName));

			var loadedTemplate = LoadTemplate("System");

			foreach ( var system in _states )
			{
				var template = loadedTemplate;
				var systemName = system.StateName;

				template = AssignUsings( system, _fsmGraph.Namespace, template );

				var lambdaTemplate = s_lambdaRegex.Match(template).Groups[2].Value;
				StringBuilder lambdaTemplateBuilder = new StringBuilder();
				foreach ( var lambda in system.Lambdas )
				{
					var currentTemplate = lambdaTemplate;
					currentTemplate = AssignLambdaName( system, lambda, currentTemplate );

					currentTemplate = AssignForEachComponents( lambda, currentTemplate );

					currentTemplate = AssignWithAll( lambda, currentTemplate );

					currentTemplate = AssignWithAny( lambda, currentTemplate );

					currentTemplate = AssignWithNone( lambda, currentTemplate );

					currentTemplate = SetupTransition( system, lambda, currentTemplate );

					lambdaTemplateBuilder.AppendLine( currentTemplate );
				}
				template = s_lambdaRegex.Replace( template, lambdaTemplateBuilder.ToString() );

				template = ConditionalText( system.HasStructuralChanges, "STRUCTURAL_CHANGES", template );

				template = Regex.Replace( template, @"^\s+$[\r\n]*", string.Empty, RegexOptions.Multiline );

				var systemPath = Path.Combine( systemsPath, $"{systemName}.cs");
				GenerateSystemFile( systemName, systemPath, template );
			}
		}

		private void GenerateSystemFile( string systemName, string systemPath, string template )
		{
			var shouldProceed = !File.Exists( systemPath ) || EditorUtility.DisplayDialog(
						"Create system file",
						$"System {systemName} at path {systemPath} already exists. Do you want regenerate this file?", "Yes", "No"
						);

			if ( shouldProceed )
			{
				var code = template;
				code = Regex.Replace( code, @"\$NAMESPACE\$", _fsmGraph.Namespace );
				code = Regex.Replace( code, @"\$NAME\$", systemName );

				File.WriteAllText( systemPath, code );
			}
		}

		#region Helpers

		private static string AssignUsings( StateNode system, string namespaceName, string template )
		{
			// Namespaces
			var usingNamespaces = system.AllComponents.Where( c => c.TypeReference != null ).Select( c => c.TypeReference.Namespace ).Distinct();
			StringBuilder usingsBuilder = new StringBuilder();
			foreach ( var usingNamespace in usingNamespaces )
			{
				usingsBuilder.Append( "using " );
				usingsBuilder.Append( usingNamespace );
				usingsBuilder.AppendLine( ";" );
			}
			if ( system.AllComponents.Any( c => c.IsHandWrited ) )
			{
				usingsBuilder.AppendLine( $"using {namespaceName}.Components;" );
			}
			template = Regex.Replace( template, @"\$USINGS\$", usingsBuilder.ToString() );
			return template;
		}

		private static string AssignForEachComponents( SystemLambdaAction lambda, string template )
		{
			//ForEach
			StringBuilder foreachBuilder = new StringBuilder();
			var refComponents = lambda.Components
					.Where(c => c.Usage == ComponentLinkUsageType.All && c.AccessType == ComponentLinkAccessType.RW)
					.ToArray();
			var inComponents = lambda.Components
					.Where(c => c.Usage == ComponentLinkUsageType.All && c.AccessType == ComponentLinkAccessType.R)
					.ToArray();

			foreach ( var refComponent in refComponents )
			{
				var componentName = refComponent.ComponentName;
				foreachBuilder.Append( "ref " );
				foreachBuilder.Append( componentName );
				foreachBuilder.Append( " " );
				foreachBuilder.Append( char.ToLowerInvariant( componentName[0] ) );
				foreachBuilder.Append( componentName.Substring( 1 ) );
				foreachBuilder.Append( ", " );
			}

			foreach ( var inComponent in inComponents )
			{
				var componentName = inComponent.ComponentName;
				foreachBuilder.Append( "in " );
				foreachBuilder.Append( componentName );
				foreachBuilder.Append( " " );
				foreachBuilder.Append( char.ToLowerInvariant( componentName[0] ) );
				foreachBuilder.Append( componentName.Substring( 1 ) );
				foreachBuilder.Append( ", " );
			}
			// Remove space and comma
			foreachBuilder.Length -= 2;

			return Regex.Replace( template, @"\$FOR_EACH\$", foreachBuilder.ToString() );
		}

		private static string AssignWithAll( SystemLambdaAction lambda, string template )
		{
			//With all
			StringBuilder withAllBuilder = new StringBuilder();
			var withAllComponents = lambda.Components
					.Where( c => c.Usage == ComponentLinkUsageType.All && c.AccessType == ComponentLinkAccessType.Un )
					.ToArray();
			if ( withAllComponents.Length > 0 )
			{
				withAllBuilder.Append( ".WithAll<" );
				for ( var i = 0; i < withAllComponents.Length; i++ )
				{
					var component = withAllComponents[i];
					if ( i != 0 )
					{
						withAllBuilder.Append( ", " );
					}
					withAllBuilder.Append( component.ComponentName );
				}
				withAllBuilder.Append( ">()" );
			}
			template = Regex.Replace( template, @"\$WITH_ALL\$", withAllBuilder.ToString() );
			return template;
		}

		private static string AssignWithAny( SystemLambdaAction lambda, string template )
		{
			//With any
			StringBuilder withAnyBuilder = new StringBuilder();
			var withAnyComponents = lambda.Components
					.Where( c => c.Usage == ComponentLinkUsageType.Any )
					.ToArray();
			if ( withAnyComponents.Length > 0 )
			{
				withAnyBuilder.Append( ".WithAny<" );
				for ( var i = 0; i < withAnyComponents.Length; i++ )
				{
					var component = withAnyComponents[i];
					if ( i != 0 )
					{
						withAnyBuilder.Append( ", " );
					}
					withAnyBuilder.Append( component.ComponentName );
				}
				withAnyBuilder.Append( ">()" );
			}
			template = Regex.Replace( template, @"\$WITH_ANY\$", withAnyBuilder.ToString() );
			return template;
		}

		private static string AssignWithNone( SystemLambdaAction lambda, string template )
		{
			//With none
			StringBuilder withNoneBuilder = new StringBuilder();
			var withNoneComponents = lambda.Components
					.Where( c => c.Usage == ComponentLinkUsageType.None )
					.ToArray();
			if ( withNoneComponents.Length > 0 )
			{
				withNoneBuilder.Append( ".WithNone<" );
				for ( var i = 0; i < withNoneComponents.Length; i++ )
				{
					var component = withNoneComponents[i];
					if ( i != 0 )
					{
						withNoneBuilder.Append( ", " );
					}
					withNoneBuilder.Append( component.ComponentName );
				}
				withNoneBuilder.Append( ">()" );
			}
			template = Regex.Replace( template, @"\$WITH_NONE\$", withNoneBuilder.ToString() );
			return template;
		}

		private static string SetupTransition( StateNode system, SystemLambdaAction lambda, string template )
		{
			var transition = system.TransitionTo(lambda);
			bool hasTransition = transition != null;

			template = ConditionalText( hasTransition, "TRANSITION", template );
			template = Regex.Replace( template, @"\$TRANSITION_TO\$", transition?.Name ?? "" );

			return template;
		}

		private string AssignLambdaName( StateNode system, SystemLambdaAction lambda, string lambdaTemplate ) =>
			Regex.Replace( lambdaTemplate, @"\$LAMBDA_NAME\$", lambda.FullName( system ) );

		#endregion Helpers

		#endregion Systems

		#endregion Generation

		private static string ConditionalText( bool condition, string conditionalName, string input )
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

		private string LoadTemplate( string name )
		{
			var filePath = Path.Combine( Application.dataPath, "Templates", $"{name}.txt" );
			return File.ReadAllText( filePath );
		}
	}
}