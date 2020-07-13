using FSM.Components;
using FSM.Utility;

using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEditor;

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
				.Where(c => c.TypeReference == null && !string.IsNullOrWhiteSpace(c.ComponentName))
				.Select(c => c.ComponentName)
				.Distinct();

			foreach ( var componentName in unavailableComponents )
			{
				var componentPath = Path.Combine( componentsPath, $"{componentName}.cs");
				GenerateComponentFile( componentName, componentPath );
			}
		}

		private void GenerateComponentFile( string componentName, string componentPath )
		{
			var shouldProceed = !File.Exists( componentPath ) || EditorUtility.DisplayDialog(
						"Create component file",
						$"Component {componentName} at path {componentPath} already exists. Do you want regenerate this file?", "Yes", "No"
						);
			if ( shouldProceed )
			{
				File.WriteAllText( componentPath, $"class {componentName} {{}}" );
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

			var systemNames = _states.Select(s =>
			{
				if(s.StateName.EndsWith("System"))
				{
					return s.StateName;
				}
				else
				{
					return s.StateName + "System";
				}
			} );

			foreach ( var systemName in systemNames )
			{
				var systemPath = Path.Combine( systemsPath, $"{systemName}.cs");
				GenerateSystemFile( systemName, systemPath );
			}
		}

		private void GenerateSystemFile( string systemName, string systemPath )
		{
			var shouldProceed = !File.Exists( systemPath ) || EditorUtility.DisplayDialog(
						"Create system file",
						$"System {systemName} at path {systemPath} already exists. Do you want regenerate this file?", "Yes", "No"
						);

			if ( shouldProceed )
			{
				File.WriteAllText( systemPath, $"class {systemName} {{}}" );
			}
		}

		#endregion Systems

		#endregion Generation
	}
}