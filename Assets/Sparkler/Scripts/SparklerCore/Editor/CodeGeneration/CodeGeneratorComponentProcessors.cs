using Primitives;

using Sparkler.Utility;

using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Unity.Entities;

namespace Sparkler.Editor.CodeGeneration
{
	public class ComponentAssignUsingsProcessor : ICodeGeneratorComponentProcessors
	{
		private static readonly Regex s_usingsRegex = new Regex(@"\$USINGS\$");

		public string Process( ComponentDefinition componentDefinition, string template )
		{
			// Namespaces
			var usingNamespaces = componentDefinition.Fields
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
	}

	[ProcessAfter( typeof( ComponentAssignUsingsProcessor ) )]
	public class ComponentAssignComponentTypeProcessor : ICodeGeneratorComponentProcessors
	{
		private static readonly Regex s_componentTypesRegex = new Regex(@"\$COMPONENT_TYPE\$");

		public string Process( ComponentDefinition componentDefinition, string template )
		{
			var componentInterfaceName = GetInterfaceName(componentDefinition.ComponentType);
			return s_componentTypesRegex.Replace( template, componentInterfaceName );
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
			else if ( componentType == ComponentType.BufferElementData )
			{
				return typeof( IBufferElementData ).Name;
			}
			else if ( componentType == ComponentType.SystemStateComponent )
			{
				return typeof( ISystemStateComponentData ).Name;
			}
			else if ( componentType == ComponentType.SystemStateSharedComponent )
			{
				return typeof( ISystemStateSharedComponentData ).Name;
			}
			else if ( componentType == ComponentType.SystemStateBufferElementData )
			{
				return typeof( ISystemStateBufferElementData ).Name;
			}
			return typeof( IComponentData ).Name;
		}
	}

	[ProcessAfter( typeof( ComponentAssignComponentTypeProcessor ) )]
	public class ComponentAssignComponentFieldsProcessor : ICodeGeneratorComponentProcessors
	{
		private static readonly Regex s_fieldsRegex = new Regex(@"\$FIELDS\$");

		public string Process( ComponentDefinition componentDefinition, string template )
		{
			var fieldsBuilder = new StringBuilder();

			foreach ( var field in componentDefinition.Fields )
			{
				if ( field.type.Type != null )
				{
					if ( typeof( IPrimitiveType ).IsAssignableFrom( field.type.Type ) )
					{
						IPrimitiveType primitiveType = Activator.CreateInstance(field.type.Type) as IPrimitiveType;
						fieldsBuilder.AppendLine( primitiveType.GetFieldDeclaration( field.name, FieldAccessType( field.accessType ) ) );
					}
					else
					{
						fieldsBuilder.AppendLine( $"\t\t{FieldAccessType( field.accessType )} {field.type.Type.Name} {field.name};" );
					}
				}
			}

			return s_fieldsRegex.Replace( template, fieldsBuilder.ToString() );
		}

		private static string FieldAccessType( ComponentFieldAccessType accessType )
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
	}

	[ProcessAfter( typeof( ComponentAssignComponentFieldsProcessor ) )]
	public class ComponentAssignNamespaceProcessor : ICodeGeneratorComponentProcessors
	{
		private static readonly Regex s_namespaceRegex = new Regex(@"\$NAMESPACE\$");

		public string Process( ComponentDefinition componentDefinition, string template )
		{
			var dotCompoennts = ".Components";
			var namespaceName = componentDefinition.Namespace;
			if ( namespaceName.EndsWith( dotCompoennts ) )
			{
				namespaceName = namespaceName.Substring( 0, namespaceName.Length - dotCompoennts.Length );
			}
			return s_namespaceRegex.Replace( template, namespaceName );
		}
	}

	[ProcessAfter( typeof( ComponentAssignNamespaceProcessor ) )]
	public class ComponentAssignNameProcessor : ICodeGeneratorComponentProcessors
	{
		private static readonly Regex s_nameRegex = new Regex(@"\$NAME\$");

		public string Process( ComponentDefinition componentDefinition, string template )
		{
			var upperedComponentName = componentDefinition.ComponentName.ToUpperFirstChar();
			return s_nameRegex.Replace( template, upperedComponentName );
		}
	}
}