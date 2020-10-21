///
/// -- Code adopted from https://gist.github.com/mzaks/ec261ac853621af8503b73391ebd18f1
///
using Sparkler.Utility;

using System;
using System.Collections.Generic;

using Unity.Entities;

using UnityEditor.IMGUI.Controls;

namespace Sparkler.Editor.Components.SizeAnalysis
{
	public class ComponentAnalyzerTreeView : TreeView
	{
		private bool _showOnlyProblematicComponents;
		private bool _showEnums;
		private string _excludeString = "";

		public ComponentAnalyzerTreeView( TreeViewState treeViewState ) : base( treeViewState ) => Reload();

		public void ShowOnlyProblematic( bool value )
		{
			if ( _showOnlyProblematicComponents != value )
			{
				_showOnlyProblematicComponents = value;
				Reload();
			}
		}

		public void ShowEnums( bool value )
		{
			if ( _showEnums != value )
			{
				_showEnums = value;
				Reload();
			}
		}

		public void Exclude( string excludeString )
		{
			if ( _excludeString == excludeString )
			{
				return;
			}

			_excludeString = excludeString;
			Reload();
		}

		protected override TreeViewItem BuildRoot()
		{
			var root = new TreeViewItem{ id = 0, depth = -1, displayName = "Root" };
			var id = 0;
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();

			foreach ( var assembly in assemblies )
			{
				if ( IsExcluded( assembly.GetName().Name ) )
				{
					continue;
				}
				var assemblyItem = new TreeViewItem { id = ++id, displayName = assembly.GetName().Name };
				var enums = new TreeViewItem{ id = ++id, depth = -1, displayName = "Enums" };
				var problems = 0;
				foreach ( var type in assembly.DefinedTypes )
				{
					if ( IsExcluded( type.Name ) )
					{
						continue;
					}
					if ( type.IsAbstract )
					{
						continue;
					}

					// Enums
					if ( _showEnums && type.IsEnum && !type.IsGenericType )
					{
						var enumSize = StructTypeSize.GetTypeSize(type);
						var valueCount = Enum.GetValues(type).Length;
						if ( valueCount <= byte.MaxValue && enumSize > sizeof( byte ) )
						{
							var componentItem = new TreeViewItem
							{
								id = ++id,
								displayName = S.Concat(type.Name) + " has size " + enumSize + " but can be " + sizeof(byte)
								+ " if you use: \"" + type.Name + " : byte\"",
							};
							enums.AddChild( componentItem );
						}
						else if ( valueCount <= ushort.MaxValue && enumSize > sizeof( ushort ) )
						{
							var componentItem = new TreeViewItem
							{
								id = ++id,
								displayName = S.Concat(type.Name) + " has size " + enumSize + " but can be " + sizeof(ushort)
								+ " if you use: \"" + type.Name + " : ushort\"",
							};
							enums.AddChild( componentItem );
						}
					}

					// Structs
					if ( !( type.IsValueType && !type.IsEnum ) )
					{
						continue;
					}

					var interfaces = new HashSet<Type>(type.GetInterfaces());
					bool isValidComponent = interfaces.Contains( typeof( IComponentData ) )
						|| interfaces.Contains( typeof( ISharedComponentData ) )
						|| interfaces.Contains( typeof( IBufferElementData ) )
						|| interfaces.Contains( typeof( ISystemStateComponentData ) )
						|| interfaces.Contains( typeof( ISystemStateSharedComponentData ) )
						|| interfaces.Contains( typeof( ISystemStateBufferElementData ) );

					if ( isValidComponent )
					{
						var size = StructTypeSize.GetTypeSize(type);

						var possibleSize = StructTypeSize.GetPossibleStructSize(type);
						var prefix = size <= possibleSize ? "✔︎" : "✘️";
						if ( possibleSize < size )
						{
							problems++;
						}

						var show = !_showOnlyProblematicComponents || size > possibleSize;

						if ( show )
						{
							var fields = new List<Type>();
							StructTypeSize.CollectFields( type, fields );

							var text = S.Concat(prefix) + ' ' + type.Name + " holds " + fields.Count + " values";
							if ( fields.Count > 0 )
							{
								text = text + " in " + size + " bytes";
							}
							if ( size > possibleSize )
							{
								text = text + ", where " + possibleSize + " bytes is possible";
							}
							var componentItem = new TreeViewItem   { id = ++id, displayName = text };
							assemblyItem.AddChild( componentItem );
						}
					}
				}

				if ( problems > 0 )
				{
					assemblyItem.displayName = S.Concat( assemblyItem.displayName ) + '[' + problems + ']';
				}
				if ( enums.hasChildren )
				{
					assemblyItem.AddChild( enums );
				}
				if ( assemblyItem.hasChildren )
				{
					root.AddChild( assemblyItem );
				}
			}

			if ( !root.hasChildren )
			{
				root.AddChild( new TreeViewItem { id = 1, displayName = "Nothing to display" } );
			}

			SetupDepthsFromParentsAndChildren( root );
			return root;
		}

		private bool IsExcluded( string value )
		{
			if ( string.IsNullOrWhiteSpace( _excludeString ) )
			{
				return false;
			}
			foreach ( var exclude in _excludeString.Split( ',' ) )
			{
				var trimmedExclude = exclude.Trim();
				if ( string.IsNullOrWhiteSpace( trimmedExclude ) )
				{
					continue;
				}
				if ( value.StartsWith( trimmedExclude ) )
				{
					return true;
				}
			}

			return false;
		}
	}
}