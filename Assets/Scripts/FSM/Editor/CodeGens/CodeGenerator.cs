using FSM.Components;
using FSM.Editor.CodeGens;
using FSM.Primitives;
using FSM.Utility;

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Unity.Entities;

using UnityEditor;

using UnityEngine;

using ComponentType = FSM.Editor.CodeGens.ComponentType;

namespace FSM.Editor
{
	public class CodeGenerator
	{
		#region Regex definitions
		private static readonly Regex s_lambdaRegex = new Regex(@"(#\$LAMBDA)(?s)(.+?)(\$#LAMBDA)");
		private static readonly Regex s_usingsRegex = new Regex(@"\$USINGS\$");
		private static readonly Regex s_componentTypesRegex = new Regex(@"\$COMPONENT_TYPE\$");
		private static readonly Regex s_fieldsRegex = new Regex(@"\$FIELDS\$");
		private static readonly Regex s_namespaceRegex = new Regex(@"\$NAMESPACE\$");
		private static readonly Regex s_nameRegex = new Regex(@"\$NAME\$");
		private static readonly Regex s_emptyLinesRegex = new Regex(@"^\s+$[\r\n]*", RegexOptions.Multiline);
		private static readonly Regex s_queryFieldsRegex = new Regex(@"\$QUERY_FIELDS\$");
		private static readonly Regex s_queryFieldRegex = new Regex(@"\$QUERY_FIELD\$");
		private static readonly Regex s_foreachRegex = new Regex(@"\$FOR_EACH\$");
		private static readonly Regex s_withAllRegex = new Regex(@"\$WITH_ALL\$");
		private static readonly Regex s_withAnyRegex = new Regex(@"\$WITH_ANY\$");
		private static readonly Regex s_withNoneRegex = new Regex(@"\$WITH_NONE\$");
		private static readonly Regex s_transitionToRegex = new Regex(@"\$TRANSITION_TO\$");
		private static readonly Regex s_lambdaNameRegex = new Regex(@"\$LAMBDA_NAME\$");
		private static readonly Regex s_lambdaLowerNameRegex = new Regex(@"\$LAMBDA_NAME_LOWER\$");
		private static readonly Regex s_sharedFilterDefinitionRegex = new Regex(@"\$SHARED_FILTER_DECLARATION\$");
		private static readonly Regex s_sharedFilterRegex = new Regex(@"\$SHARED_FILTER\$");
		#endregion Regex definitions

		public static void Generate( FSMGraph fsmGraph )
		{
			GenerateSystems( fsmGraph );
			AssetDatabase.Refresh();
		}

		public static void Generate( ComponentDefinition componentDefinition )
		{
			GenerateComponent( componentDefinition );
			AssetDatabase.Refresh();
		}

		#region Generation

		#region Components

		private static void GenerateComponent( ComponentDefinition componentDefinition )
		{
			var componentsPath = PathExtension.SystemPath( Path.Combine( componentDefinition.Directory, "Components") );
			if ( !Directory.Exists( componentsPath ) )
			{
				Directory.CreateDirectory( componentsPath );
			}

			var template = LoadTemplate("Component");

			var upperedComponentName = componentDefinition.ComponentName.ToUpperFirstChar();

			template = AssignUsings( componentDefinition.Fields, template );
			template = AssignComponentType( componentDefinition.ComponentType, template );
			template = AssignComponentFields( componentDefinition.Fields, template );

			var componentPath = Path.Combine( componentsPath, $"{upperedComponentName}.cs");
			GenerateComponentFile( upperedComponentName, componentDefinition.Namespace, componentPath, template );
		}

		#region Helpers

		private static string AssignUsings( ComponentField[] fields, string template )
		{
			// Namespaces
			var usingNamespaces = fields
				.Where( f => f.type.Type != null )
				.Select( f => f.type.Type.Namespace )
				.Distinct()
				.Except(typeof(IPrimitiveType).Namespace.Yield());

			StringBuilder usingsBuilder = new StringBuilder();
			foreach ( var usingNamespace in usingNamespaces )
			{
				usingsBuilder.Append( "using " );
				usingsBuilder.Append( usingNamespace );
				usingsBuilder.AppendLine( ";" );
			}

			template = s_usingsRegex.Replace( template, usingsBuilder.ToString() );
			return template;
		}

		private static string AssignComponentType( ComponentType componentType, string template )
		{
			var componentInterfaceName = GetInterfaceName(componentType);
			return s_componentTypesRegex.Replace( template, componentInterfaceName );
		}

		private static string AssignComponentFields( ComponentField[] fields, string template )
		{
			var fieldsBuilder = new StringBuilder();

			foreach ( var field in fields )
			{
				if ( field.type.Type != null )
				{
					if ( typeof( IPrimitiveType ).IsAssignableFrom( field.type.Type ) )
					{
						IPrimitiveType primitiveType = Activator.CreateInstance(field.type.Type) as IPrimitiveType;
						fieldsBuilder.AppendLine( primitiveType.GetFieldDeclaration( field.name, FieldAccesType( field.accessType ) ) );
					}
					else
					{
						fieldsBuilder.AppendLine( $"\t\t{FieldAccesType( field.accessType )} {field.type.Type.Name} {field.name};" );
					}
				}
			}

			return s_fieldsRegex.Replace( template, fieldsBuilder.ToString() );
		}

		private static void GenerateComponentFile( string componentName, string namespaceName, string componentPath, string template )
		{
			var dotCompoennts = ".Components";
			var shouldProceed = !File.Exists( componentPath ) || EditorUtility.DisplayDialog(
						"Create component file",
						$"Component {componentName} at path {componentPath} already exists. Do you want regenerate this file?", "Yes", "No"
						);
			if ( shouldProceed )
			{
				if ( namespaceName.EndsWith( dotCompoennts ) )
				{
					namespaceName = namespaceName.Substring( 0, namespaceName.Length - dotCompoennts.Length );
				}
				var code = template;
				code = s_namespaceRegex.Replace( code, namespaceName );
				code = s_nameRegex.Replace( code, componentName );

				File.WriteAllText( componentPath, code );
			}
		}

		private static string GetInterfaceName( ComponentType componentType )
		{
			if ( componentType == ComponentType.ComponentData )
			{
				return typeof( IComponentData ).Name;
			}
			else if ( componentType == ComponentType.SharedComponentData )
			{
				return typeof( ISharedComponentData ).Name;
			}
			else if ( componentType == ComponentType.SystemStateComponent )
			{
				return typeof( ISystemStateComponentData ).Name;
			}
			else if ( componentType == ComponentType.SystemStateSharedComponent )
			{
				return typeof( ISystemStateSharedComponentData ).Name;
			}
			else if ( componentType == ComponentType.BufferElementData )
			{
				return typeof( IBufferElementData ).Name;
			}
			return typeof( IComponentData ).Name;
		}

		private static string FieldAccesType( ComponentFieldAccessType accessType )
		{
			if ( accessType == ComponentFieldAccessType.Public )
			{
				return "public";
			}
			else if ( accessType == ComponentFieldAccessType.Internal )
			{
				return "internal";
			}
			else if ( accessType == ComponentFieldAccessType.Private )
			{
				return "private";
			}
			return "public";
		}

		#endregion Helpers

		#endregion Components

		#region Systems

		private static void GenerateSystems( FSMGraph fsmGraph )
		{
			var states = fsmGraph.nodes.OfType<StateNode>().Where( s => s.Editable ).ToList();

			var systemsPath = PathExtension.SystemPath(Path.Combine( fsmGraph.CodeGenerationPath, "Systems"));
			if ( !Directory.Exists( systemsPath ) )
			{
				Directory.CreateDirectory( systemsPath );
			}

			var loadedTemplate = LoadTemplate("System");

			foreach ( var system in states )
			{
				var template = loadedTemplate;
				var systemName = system.StateName;

				template = AssignUsings( system, fsmGraph.Namespace, template );

				template = AssignQueriesFieldsDefinitions( system, template );

				var lambdaTemplate = s_lambdaRegex.Match( template ).Groups[2].Value;
				StringBuilder lambdaTemplateBuilder = new StringBuilder();

				foreach ( var lambda in system.Lambdas )
				{
					var currentTemplate = lambdaTemplate;
					currentTemplate = AssignLambdaName( system, lambda, currentTemplate );

					currentTemplate = AssignHasShared( lambda, currentTemplate );

					currentTemplate = AssignParallel( lambda, currentTemplate );

					currentTemplate = AssignQueryField( lambda, currentTemplate );

					currentTemplate = AssignSharedFilter( lambda, currentTemplate );

					currentTemplate = AssignForEachComponents( lambda, currentTemplate );

					currentTemplate = AssignWithAll( lambda, currentTemplate );

					currentTemplate = AssignWithAny( lambda, currentTemplate );

					currentTemplate = AssignWithNone( lambda, currentTemplate );

					currentTemplate = SetupTransition( system, lambda, currentTemplate );

					lambdaTemplateBuilder.AppendLine( currentTemplate );
				}

				template = ConditionalText( system.HasStructuralChanges, "STRUCTURAL_CHANGES", template );

				template = s_lambdaRegex.Replace( template, lambdaTemplateBuilder.ToString() );

				template = s_emptyLinesRegex.Replace( template, string.Empty );

				var systemPath = Path.Combine( systemsPath, $"{systemName}.cs");
				GenerateSystemFile( systemName, fsmGraph.Namespace, systemPath, template );
			}
		}

		private static void GenerateSystemFile( string systemName, string namespaceName, string systemPath, string template )
		{
			var shouldProceed = !File.Exists( systemPath ) || EditorUtility.DisplayDialog(
						"Create system file",
						$"System {systemName} at path {systemPath} already exists. Do you want regenerate this file?", "Yes", "No"
						);

			if ( shouldProceed )
			{
				var code = template;
				code = s_namespaceRegex.Replace( code, namespaceName );
				code = s_nameRegex.Replace( code, systemName );

				File.WriteAllText( systemPath, code );
			}
		}

		#region Helpers

		private static string AssignUsings( StateNode system, string namespaceName, string template )
		{
			// Namespaces
			var usingNamespaces = system.AllComponents
				.Where( c => c.TypeReference != null )
				.Select( c => c.TypeReference.Namespace )
				.Distinct()
				.Where( c => !string.IsNullOrWhiteSpace(c) );
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
			template = s_usingsRegex.Replace( template, usingsBuilder.ToString() );
			return template;
		}

		private static string AssignQueriesFieldsDefinitions( StateNode system, string template )
		{
			var queryFields = system.Lambdas.Where( l => l.HasQueryField ).Select( l => l.QueryFieldName ).ToArray();
			if ( !queryFields.Any() )
			{
				return s_queryFieldsRegex.Replace( template, "" );
			}
			StringBuilder fieldsBuilder = new StringBuilder();
			foreach ( var field in queryFields )
			{
				fieldsBuilder.Append( "private EntityQuery " );
				fieldsBuilder.Append( field );
				fieldsBuilder.AppendLine( ";" );
			}
			return s_queryFieldsRegex.Replace( template, fieldsBuilder.ToString() );
		}

		private static string AssignHasShared( SystemLambdaAction lambda, string currentTemplate )
		{
			currentTemplate = ConditionalText( lambda.HasSharedComponent, "HAS_SHARED", currentTemplate );
			return ConditionalText( lambda.HasStructuralChanges, "STRUCTURAL_CHANGES", currentTemplate );
		}

		private static string AssignForEachComponents( SystemLambdaAction lambda, string template )
		{
			//ForEach
			StringBuilder foreachBuilder = new StringBuilder();
			var refComponents = lambda.Components
					.Where(c => c.Usage == ComponentLinkUsageType.All && c.AccessType == ComponentLinkAccessType.ReadWrite)
					.ToArray();
			var inComponents = lambda.Components
					.Where(c => c.Usage == ComponentLinkUsageType.All && c.AccessType == ComponentLinkAccessType.Read)
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

			return s_foreachRegex.Replace( template, foreachBuilder.ToString() );
		}

		private static string AssignQueryField( SystemLambdaAction lambda, string currentTemplate )
		{
			if ( !lambda.HasQueryField )
			{
				return s_queryFieldRegex.Replace( currentTemplate, "" );
			}
			return s_queryFieldRegex.Replace( currentTemplate, $".WithStoreEntityQueryInField( ref {lambda.QueryFieldName} )" );
		}

		private static string AssignSharedFilter( SystemLambdaAction lambda, string currentTemplate )
		{
			if ( lambda.HasSharedFilter )
			{
				currentTemplate = s_sharedFilterDefinitionRegex.Replace( currentTemplate, lambda.SharedFilterDeclaration );
				return s_sharedFilterRegex.Replace( currentTemplate, $".WithSharedComponentFilter( {lambda.SharedFilterName} )" );
			}
			else
			{
				currentTemplate = s_sharedFilterDefinitionRegex.Replace( currentTemplate, string.Empty );
				return s_sharedFilterRegex.Replace( currentTemplate, string.Empty );
			}
		}

		private static string AssignWithAll( SystemLambdaAction lambda, string template )
		{
			//With all
			StringBuilder withAllBuilder = new StringBuilder();
			var withAllComponents = lambda.Components
					.Where( c => c.Usage == ComponentLinkUsageType.All && c.AccessType == ComponentLinkAccessType.Unused )
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
			template = s_withAllRegex.Replace( template, withAllBuilder.ToString() );
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
			template = s_withAnyRegex.Replace( template, withAnyBuilder.ToString() );
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
			template = s_withNoneRegex.Replace( template, withNoneBuilder.ToString() );
			return template;
		}

		private static string AssignParallel( SystemLambdaAction lambda, string template ) => ConditionalText( lambda.Parallel, "PARALLEL", template );

		private static string SetupTransition( StateNode system, SystemLambdaAction lambda, string template )
		{
			var transition = system.TransitionTo(lambda);
			bool hasTransition = transition != null;

			template = ConditionalText( hasTransition, "TRANSITION", template );
			template = s_transitionToRegex.Replace( template, transition?.Name ?? "" );

			return template;
		}

		private static string AssignLambdaName( StateNode system, SystemLambdaAction lambda, string lambdaTemplate )
		{
			lambdaTemplate = s_lambdaNameRegex.Replace( lambdaTemplate, lambda.FullName( system ) );
			return s_lambdaLowerNameRegex.Replace( lambdaTemplate, lambda.Name.ToLower() );
		}

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

		private static string LoadTemplate( string name )
		{
			var filePath = Path.Combine( Application.dataPath, "Templates", $"{name}.txt" );
			return File.ReadAllText( filePath );
		}
	}
}