using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEditor;

namespace Sparkler.Editor.CodeGeneration
{
	public static class ProcessorsSelector
	{
		public static List<T> Selectors<T>()
		{
			var allSelectorsTypes = TypeCache.GetTypesDerivedFrom<T>();

			var unusedSelectors = new HashSet<T>( allSelectorsTypes.Select(t => (T)Activator.CreateInstance(t)));
			Dictionary<Type, T> selectorsMapping = unusedSelectors.ToDictionary(s => s.GetType());

			HashSet<T> leftSelectors = new HashSet<T>(unusedSelectors);

			List<T> selectors = new List<T>();

			Dictionary<T, HashSet<T>> pairs = new Dictionary<T, HashSet<T>>();

			// Find pairs
			foreach ( var selector in unusedSelectors )
			{
				// Process after
				var processAfter = selector.GetType().GetCustomAttribute<ProcessAfter>();
				if ( processAfter?.ReferenceType != null )
				{
					selectorsMapping.TryGetValue( processAfter.ReferenceType, out var leftSelector );
					if ( leftSelector != null )
					{
						if ( !pairs.TryGetValue( leftSelector, out var dependencies ) )
						{
							dependencies = new HashSet<T>();
							pairs[leftSelector] = dependencies;
						}
						dependencies.Add( selector );
					}
				}

				// Process before
				var processBefore = selector.GetType().GetCustomAttribute<ProcessBefore>();
				if ( processBefore?.ReferenceType != null )
				{
					selectorsMapping.TryGetValue( processBefore.ReferenceType, out var dependent );
					if ( dependent != null )
					{
						if ( !pairs.TryGetValue( selector, out var dependencies ) )
						{
							dependencies = new HashSet<T>();
							pairs[selector] = dependencies;
						}
						dependencies.Add( dependent );
					}
				}
			}

			Queue<T> selectorsQueue = new Queue<T>( unusedSelectors.Except( pairs.SelectMany( p => p.Value ) ) );

			// Find chain
			while ( selectorsQueue.Count > 0 )
			{
				var selector = selectorsQueue.Dequeue();
				if ( unusedSelectors.Contains( selector ) )
				{
					selectors.Add( selector );
					unusedSelectors.Remove( selector );
					if ( pairs.TryGetValue( selector, out var dependencies ) )
					{
						foreach ( var dependent in dependencies )
						{
							if ( unusedSelectors.Contains( dependent ) )
							{
								selectorsQueue.Enqueue( dependent );
							}
						}
					}
				}
			}

			if ( unusedSelectors.Count > 1 )
			{
				throw new Exception( $"For type {typeof( T )} there is impossible dependencies chain" );
			}

			// Check constrains
			for ( int i = 0; i < selectors.Count; i++ )
			{
				var selector = selectors[i];
				if ( pairs.TryGetValue( selector, out var dependencies ) )
				{
					if ( dependencies.Except( selectors.Skip( i ) ).Count() > 0 )
					{
						throw new Exception( $"For type {typeof( T )} there is impossible dependencies chain" );
					}
				}
			}

			return selectors;
		}
	}
}