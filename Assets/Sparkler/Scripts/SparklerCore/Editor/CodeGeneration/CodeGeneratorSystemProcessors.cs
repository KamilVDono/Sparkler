using Sparkler.Components;

using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Sparkler.Editor.CodeGeneration
{
	#region ICodeGeneratorSystemProcessorBeforeLambda

	public class SystemAssignUsingsProcessor : ICodeGeneratorSystemProcessorBeforeLambda
	{
		private static readonly Regex s_usingsRegex = new Regex(@"\$USINGS\$");

		public string ProcessBeforeLambda( SystemsGraph systemsGraph, SystemNode system, string template )
		{
			string namespaceName = systemsGraph.Namespace;
			// Namespaces
			var usingNamespaces = system.AllComponents
				.Where( c => c.TypeReference != null )
				.Select( c => c.TypeReference.Namespace )
				.Distinct()
				.Where( c => !string.IsNullOrWhiteSpace( c ) );
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
	}

	[ProcessAfter( typeof( SystemAssignUsingsProcessor ) )]
	public class SystemAssignAssignQueriesFieldsDefinitionsProcessor : ICodeGeneratorSystemProcessorBeforeLambda
	{
		private static readonly Regex s_queryFieldsRegex = new Regex(@"\$QUERY_FIELDS\$");

		public string ProcessBeforeLambda( SystemsGraph systemsGraph, SystemNode system, string template )
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
	}

	#endregion ICodeGeneratorSystemProcessorBeforeLambda

	#region ICodeGeneratorSystemProcessorAfterLambda

	public class SystemCleanupEmptyLinesProcessor : ICodeGeneratorSystemProcessorAfterLambda
	{
		private static readonly Regex s_emptyLinesRegex = new Regex(@"^\s+$[\r\n]*", RegexOptions.Multiline);

		public string ProcessAfterLambda( SystemsGraph systemsGraph, SystemNode system, string template ) =>
			s_emptyLinesRegex.Replace( template, string.Empty );
	}

	[ProcessAfter( typeof( SystemCleanupEmptyLinesProcessor ) )]
	public class SystemAssignNamespaceProcessor : ICodeGeneratorSystemProcessorAfterLambda
	{
		private static readonly Regex s_namespaceRegex = new Regex(@"\$NAMESPACE\$");

		public string ProcessAfterLambda( SystemsGraph systemsGraph, SystemNode system, string template ) =>
			s_namespaceRegex.Replace( template, systemsGraph.Namespace );
	}

	[ProcessAfter( typeof( SystemAssignNamespaceProcessor ) )]
	public class SystemAssignNameProcessor : ICodeGeneratorSystemProcessorAfterLambda
	{
		private static readonly Regex s_nameRegex = new Regex(@"\$NAME\$");

		public string ProcessAfterLambda( SystemsGraph systemsGraph, SystemNode system, string template ) =>
			s_nameRegex.Replace( template, system.StateName );
	}

	#endregion ICodeGeneratorSystemProcessorAfterLambda

	#region ICodeGeneratorSystemProcessorLambda

	public class LambdaAssignLambdaNameProcessor : ICodeGeneratorSystemProcessorLambda
	{
		private static readonly Regex s_lambdaNameRegex = new Regex(@"\$LAMBDA_NAME\$");
		private static readonly Regex s_lambdaLowerNameRegex = new Regex(@"\$LAMBDA_NAME_LOWER\$");

		public string ProcessLambda( SystemNode system, SystemLambdaAction lambda, string currentTemplate )
		{
			currentTemplate = s_lambdaNameRegex.Replace( currentTemplate, lambda.FullName( system ) );
			return s_lambdaLowerNameRegex.Replace( currentTemplate, lambda.Name.ToLower() );
		}
	}

	[ProcessAfter( typeof( LambdaAssignLambdaNameProcessor ) )]
	public class LambdaAssignHasSharedProcessor : ICodeGeneratorSystemProcessorLambda
	{
		public string ProcessLambda( SystemNode system, SystemLambdaAction lambda, string currentTemplate )
		{
			currentTemplate = CodeGeneratorUtils.ConditionalText( lambda.HasSharedComponent, "HAS_SHARED", currentTemplate );
			return CodeGeneratorUtils.ConditionalText( lambda.HasStructuralChanges, "STRUCTURAL_CHANGES", currentTemplate );
		}
	}

	[ProcessAfter( typeof( LambdaAssignHasSharedProcessor ) )]
	public class LambdaAssignParallelProcessor : ICodeGeneratorSystemProcessorLambda
	{
		public string ProcessLambda( SystemNode system, SystemLambdaAction lambda, string currentTemplate ) =>
			CodeGeneratorUtils.ConditionalText( lambda.Parallel, "PARALLEL", currentTemplate );
	}

	[ProcessAfter( typeof( LambdaAssignParallelProcessor ) )]
	public class LambdaAssignQueryFieldProcessor : ICodeGeneratorSystemProcessorLambda
	{
		private static readonly Regex s_queryFieldRegex = new Regex(@"\$QUERY_FIELD\$");

		public string ProcessLambda( SystemNode system, SystemLambdaAction lambda, string currentTemplate )
		{
			if ( !lambda.HasQueryField )
			{
				return s_queryFieldRegex.Replace( currentTemplate, "" );
			}
			return s_queryFieldRegex.Replace( currentTemplate, $".WithStoreEntityQueryInField( ref {lambda.QueryFieldName} )" );
		}
	}

	[ProcessAfter( typeof( LambdaAssignQueryFieldProcessor ) )]
	public class LambdaAssignSharedFilterProcessor : ICodeGeneratorSystemProcessorLambda
	{
		private static readonly Regex s_sharedFilterDefinitionRegex = new Regex(@"\$SHARED_FILTER_DECLARATION\$");
		private static readonly Regex s_sharedFilterRegex = new Regex(@"\$SHARED_FILTER\$");

		public string ProcessLambda( SystemNode system, SystemLambdaAction lambda, string currentTemplate )
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
	}

	[ProcessAfter( typeof( LambdaAssignSharedFilterProcessor ) )]
	public class LambdaAssignForEachComponentsProcessor : ICodeGeneratorSystemProcessorLambda
	{
		private static readonly Regex s_foreachRegex = new Regex(@"\$FOR_EACH\$");

		public string ProcessLambda( SystemNode system, SystemLambdaAction lambda, string currentTemplate )
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

			return s_foreachRegex.Replace( currentTemplate, foreachBuilder.ToString() );
		}
	}

	[ProcessAfter( typeof( LambdaAssignForEachComponentsProcessor ) )]
	public class LambdaAssignWithAllProcessor : ICodeGeneratorSystemProcessorLambda
	{
		private static readonly Regex s_withAllRegex = new Regex(@"\$WITH_ALL\$");

		public string ProcessLambda( SystemNode system, SystemLambdaAction lambda, string currentTemplate )
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
			return s_withAllRegex.Replace( currentTemplate, withAllBuilder.ToString() );
		}
	}

	[ProcessAfter( typeof( LambdaAssignWithAllProcessor ) )]
	public class LambdaAssignWithAnyProcessor : ICodeGeneratorSystemProcessorLambda
	{
		private static readonly Regex s_withAnyRegex = new Regex(@"\$WITH_ANY\$");

		public string ProcessLambda( SystemNode system, SystemLambdaAction lambda, string currentTemplate )
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
			return s_withAnyRegex.Replace( currentTemplate, withAnyBuilder.ToString() );
		}
	}

	[ProcessAfter( typeof( LambdaAssignWithAnyProcessor ) )]
	public class LambdaAssignWithNoneProcessor : ICodeGeneratorSystemProcessorLambda
	{
		private static readonly Regex s_withNoneRegex = new Regex(@"\$WITH_NONE\$");

		public string ProcessLambda( SystemNode system, SystemLambdaAction lambda, string currentTemplate )
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
			return s_withNoneRegex.Replace( currentTemplate, withNoneBuilder.ToString() );
		}
	}

	[ProcessAfter( typeof( LambdaAssignWithNoneProcessor ) )]
	public class LambdaSetupTransitionProcessor : ICodeGeneratorSystemProcessorLambda
	{
		private static readonly Regex s_transitionToRegex = new Regex(@"\$TRANSITION_TO\$");

		public string ProcessLambda( SystemNode system, SystemLambdaAction lambda, string currentTemplate )
		{
			var transitions = system.TransitionTo(lambda).ToArray();
			bool hasTransition = (transitions?.Count() ?? -1) > 0;
			string transitionsNames = "";
			if ( hasTransition )
			{
				transitionsNames = string.Join( ", ", transitions.Select( t => t.Name ) );
			}

			currentTemplate = CodeGeneratorUtils.ConditionalText( hasTransition, "TRANSITION", currentTemplate );
			return s_transitionToRegex.Replace( currentTemplate, transitionsNames );
		}
	}

	#endregion ICodeGeneratorSystemProcessorLambda
}