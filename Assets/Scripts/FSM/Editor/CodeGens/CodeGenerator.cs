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
		private FSMGraph _fsmGraph;
		private IReadOnlyCollection<StateNode> _states;
		private IReadOnlyCollection<ComponentLink> _components;

		private CodeGenerator()
		{
		}

		public static void Generate( FSMGraph fsmGraph )
		{
			var states = fsmGraph.nodes.OfType<StateNode>().ToList();
			var components = states.SelectMany(s => s.Components).Distinct().ToList();

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
			var systemsPath =  PathExtension.SystemPath(Path.Combine( _fsmGraph.CodeGenerationPath, "Systems"));
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

				var hasForEachComponents = AssignForEachComponents( system, ref template );
				if ( !hasForEachComponents )
				{
					Debug.LogWarning( $"System {systemName} has zero 'ForEach' components so can not be created" );
					continue;
				}

				template = AssignUsings( system, _fsmGraph.Namespace, template );

				template = AssignWithAll( system, template );

				template = AssignWithAny( system, template );

				template = AssignWithNone( system, template );

				template = SetupTransition( system, template );

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

		private static bool AssignForEachComponents( StateNode system, ref string template )
		{
			//ForEach
			StringBuilder foreachBuilder = new StringBuilder();
			var refComponents = system.Components
					.Where(c => c.Usage == ComponentLinkUsageType.All && c.AccessType == ComponentLinkAccessType.RW)
					.ToArray();
			var inComponents = system.Components
					.Where(c => c.Usage == ComponentLinkUsageType.All && c.AccessType == ComponentLinkAccessType.R)
					.ToArray();

			if ( refComponents.Length > 0 || inComponents.Length > 0 )
			{
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
			}
			else
			{
				return false;
			}
			template = Regex.Replace( template, @"\$FOR_EACH\$", foreachBuilder.ToString() );
			return true;
		}

		private static string AssignUsings( StateNode system, string namespaceName, string template )
		{
			// Namespaces
			var usingNamespaces = system.Components.Where( c => c.TypeReference != null ).Select( c => c.TypeReference.Namespace ).Distinct();
			StringBuilder usingsBuilder = new StringBuilder();
			foreach ( var usingNamespace in usingNamespaces )
			{
				usingsBuilder.Append( "using " );
				usingsBuilder.Append( usingNamespace );
				usingsBuilder.AppendLine( ";" );
			}
			if ( system.Components.Any( c => c.IsHandWrited ) )
			{
				usingsBuilder.AppendLine( $"using {namespaceName}.Components;" );
			}
			template = Regex.Replace( template, @"\$USINGS\$", usingsBuilder.ToString() );
			return template;
		}

		private static string AssignWithAll( StateNode system, string template )
		{
			//With all
			StringBuilder withAllBuilder = new StringBuilder();
			var withAllComponents = system.Components
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

		private static string AssignWithAny( StateNode system, string template )
		{
			//With any
			StringBuilder withAnyBuilder = new StringBuilder();
			var withAnyComponents = system.Components
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

		private static string AssignWithNone( StateNode system, string template )
		{
			//With none
			StringBuilder withNoneBuilder = new StringBuilder();
			var withNoneComponents = system.Components
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

		private static string SetupTransition( StateNode system, string template )
		{
			var transitions = system.TransitionsTo.Select(s => s.StateName).ToArray();
			bool hasTransition = transitions.Length > 0;
			if ( !hasTransition )
			{
				return Regex.Replace( template, @"#\$TRANSITION(?s).+?#\$", "" );
			}
			template = Regex.Replace( template, @"#\$TRANSITION", "" );
			template = Regex.Replace( template, @"#\$", "" );
			template = Regex.Replace( template, @"\$TRANSITION_TO\$", string.Join( ", ", transitions ) );
			return template;
		}

		#endregion Helpers

		#endregion Systems

		#endregion Generation

		private string LoadTemplate( string name )
		{
			var filePath = Path.Combine( Application.dataPath, "Templates", $"{name}.txt" );
			return File.ReadAllText( filePath );
		}
	}
}