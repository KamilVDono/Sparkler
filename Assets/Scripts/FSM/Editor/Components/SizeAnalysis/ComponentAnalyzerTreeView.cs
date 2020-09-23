///
/// -- Code adopted from https://gist.github.com/mzaks/ec261ac853621af8503b73391ebd18f1
///
using System;
using System.Collections.Generic;
using System.Reflection;

using Unity.Entities;

using UnityEditor.IMGUI.Controls;

using UnityEngine;

namespace FSM.Editor.Components.SizeAnalysis
{
	public class ComponentAnalyzerTreeView : TreeView
	{
		private bool _showOnlyProblematicComponents;
		private string _excludeString = "";

		public ComponentAnalyzerTreeView( TreeViewState treeViewState )
				: base( treeViewState ) => Reload();

		public void ShowOnlyProblematic( bool value )
		{
			if ( _showOnlyProblematicComponents != value )
			{
				_showOnlyProblematicComponents = value;
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
			var root = new TreeViewItem      { id = 0, depth = -1, displayName = "Root" };

			var id = 1;
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();

			foreach ( var assembly in assemblies )
			{
				if ( IsExcluded( assembly.GetName().Name ) )
				{
					continue;
				}
				var assemblyItem = new TreeViewItem   { id = id, displayName = assembly.GetName().Name };
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
					if ( typeof( IComponentData ).IsAssignableFrom( type ) )
					{
						var size = TypeSize.GetTypeSize(type);
						id++;

						var warnings = new List<string>();
						var possibleSize = TypeSize.GetStructSize(type, warnings);
						if ( warnings.Count > 0 )
						{
							Debug.LogWarning( $"{string.Join( ",\n", warnings.ToArray() )}" );
						}
						var prefix = size <= possibleSize ? "✔︎" : "✘️";
						if ( possibleSize < size )
						{
							problems++;
						}

						var show = !_showOnlyProblematicComponents || size > possibleSize;

						if ( show )
						{
							var fields = new List<FieldInfo>();
							TypeSize.CollectFields( type, fields );

							var text = $"{prefix} {type.Name} holds {fields.Count} values";
							if ( fields.Count > 0 )
							{
								text += $" in {size} bytes";
							}
							if ( size > possibleSize )
							{
								text += $", where {possibleSize} bytes is possible";
							}
							var componentItem = new TreeViewItem   { id = id, displayName = text };
							assemblyItem.AddChild( componentItem );
						}
					}
				}

				if ( problems > 0 )
				{
					assemblyItem.displayName = $"{assemblyItem.displayName} [{problems}]";
				}
				if ( assemblyItem.hasChildren )
				{
					root.AddChild( assemblyItem );
				}
				id++;
			}

			if ( root.hasChildren == false )
			{
				root.AddChild( new TreeViewItem( 1, 1, "No components were found" ) );
			}

			SetupDepthsFromParentsAndChildren( root );
			return root;
		}

		protected override void SelectionChanged( IList<int> selectedIds )
		{
			base.SelectionChanged( selectedIds );
			var rows = FindRows(selectedIds);
		}

		private bool IsExcluded( string value )
		{
			if ( _excludeString == null || _excludeString.Trim().Length == 0 )
			{
				return false;
			}
			foreach ( var exclude in _excludeString.Split( ',' ) )
			{
				var trimmedExclude = exclude.Trim();
				if ( trimmedExclude.Length == 0 )
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