using FSM.Components;
using FSM.Utility.Editor;

using System.Collections.Generic;
using System.Linq;

using UnityEditor;

using UnityEngine;

using XNode.Editor;

using static FSM.Utility.Editor.GUIContentExtension;
using static FSM.Utility.SerializedPropertyExtension;

namespace FSM.Editor.Assets.Scripts.FSM.Editor
{
	[CustomNodeEditor( typeof( StateNode ) )]
	public class StateNodeEditor : FSMNodeEditor<StateNode>
	{
		#region GUIContent
		private static readonly GUIContent s_summaryTitle = new GUIContent("Summary");
		private static readonly GUIContent s_summaryLambdasTitle = new GUIContent("Lambdas:");
		private static readonly GUIContent s_summaryComponentsTitle = new GUIContent("Components:");

		private static readonly GUIStyle s_wrappingLable = new GUIStyle( EditorStyles.label)
		{
			wordWrap = true,
		};

		#endregion GUIContent

		private bool _expandedSummary = false;

		public override void OnBodyGUI()
		{
			// Unity specifically requires this to save/update any serial object.
			// serializedObject.Update(); must go at the start of an inspector gui, and
			// serializedObject.ApplyModifiedProperties(); goes at the end.
			serializedObject.Update();
			string[] excludes = { "m_Script", "graph", "position", "ports", "_fromFile" };

			var enableScope = new GUIEnabledScope(!serializedObject.FindProperty( "_fromFile" ).boolValue);

			// Iterate through serialized properties and draw them like the Inspector (But with ports)
			SerializedProperty iterator = serializedObject.GetIterator();
			bool enterChildren = true;
			EditorGUIUtility.labelWidth = 84;
			while ( iterator.NextVisible( enterChildren ) )
			{
				enterChildren = false;
				if ( excludes.Contains( iterator.name ) )
				{
					continue;
				}

				if ( iterator.isArray && iterator.propertyType != SerializedPropertyType.String )
				{
					DrawArray( iterator, Target );
				}
				else
				{
					NodeEditorGUILayout.PropertyField( iterator, true, GUILayout.ExpandWidth( true ) );
				}
			}
			serializedObject.ApplyModifiedProperties();

			DrawSummary();

			enableScope.Dispose();
		}

		private static void DrawArray( SerializedProperty property, StateNode stateNode )
		{
			bool drawStateEditing = (stateNode.graph as FSMGraph).StateEditing;

			var indexesToDelete = new List<int>();
			int addNewElementCount = 0;
			int moveDownIndex = -1;
			int moveUpIndex = -1;

			// Header [Label - size - plus button]
			EditorGUILayout.Space( 8, false );
			EditorGUILayout.BeginHorizontal();
			using ( new GUIEnabledScope( true, true ) )
			{
				property.isExpanded ^= GUILayout.Button( !property.isExpanded ? FoldedButtonContent : ExpandedButtonContent, EditorStyles.boldLabel, GUILayout.Width( 15 ) );
			}
			EditorGUILayout.LabelField( property.displayName, EditorStyles.boldLabel );

			int newSize = EditorGUILayout.IntField( property.arraySize, GUILayout.Width( 50 ) );

			if ( GUILayout.Button( PlusContent, GUILayout.Width( 25 ) ) )
			{
				++addNewElementCount;
			}
			EditorGUILayout.EndHorizontal();

			// Draw content
			if ( property.isExpanded )
			{
				EditorGUILayout.BeginVertical();
				for ( int i = 0; i < property.arraySize; i++ )
				{
					var elementProperty = property.GetArrayElementAtIndex( i );
					DrawLine( 2, 2 );

					EditorGUILayout.BeginHorizontal();

					if ( drawStateEditing )
					{
						var name = elementProperty.FindPropertyRelative( "_name" )?.stringValue ?? "";
						EditorGUILayout.LabelField( $"{i}. {name}", EditorStyles.boldLabel );
					}
					else
					{
						EditorGUILayout.PropertyField( elementProperty.FindPropertyRelative( "_name" ) );
					}

					using ( new GUIEnabledScope( i < property.arraySize - 1 ) )
					{
						if ( GUILayout.Button( MoveDownContent, GUILayout.Width( 25 ) ) )
						{
							moveDownIndex = i;
						}
					}

					if ( GUILayout.Button( MinusContent, GUILayout.Width( 25 ) ) )
					{
						indexesToDelete.Add( i );
					}

					using ( new GUIEnabledScope( i > 0 ) )
					{
						if ( GUILayout.Button( MoveUpContent, GUILayout.Width( 25 ) ) )
						{
							moveUpIndex = i;
						}
					}

					EditorGUILayout.EndHorizontal();

					if ( drawStateEditing )
					{
						EditorGUILayout.PropertyField( elementProperty, true, GUILayout.ExpandWidth( true ) );
					}

					NodeEditorGUILayout.AddPortField( stateNode.GetOrAddLambdaPort( i ) );
				}
				EditorGUILayout.EndVertical();
			}

			// -- logic
			if ( newSize < 0 )
			{
				newSize = 0;
			}

			if ( newSize < property.arraySize )
			{
				for ( int i = 0; i < property.arraySize - newSize; i++ )
				{
					indexesToDelete.Add( property.arraySize - 1 - i );
				}
			}
			else if ( newSize > property.arraySize )
			{
				addNewElementCount += newSize - property.arraySize;
			}

			for ( int i = indexesToDelete.Count - 1; i >= 0; i-- )
			{
				property.DeleteArrayElementAtIndex( indexesToDelete[i] );
			}

			for ( int i = 0; i < addNewElementCount; i++ )
			{
				property.InsertArrayElementAtIndex( property.arraySize );
				property.serializedObject.ApplyModifiedProperties();

				var newElement = property.GetArrayElementAtIndex( property.arraySize - 1 ).GetPropertyValue<SystemLambdaAction>();
				newElement.Initialize();

				property.serializedObject.Update();
			}

			if ( moveDownIndex != -1 )
			{
				var newIndex = moveDownIndex+1;
				property.MoveArrayElement( moveDownIndex, newIndex );
			}

			if ( moveUpIndex != -1 )
			{
				var newIndex = moveUpIndex-1;
				property.MoveArrayElement( moveUpIndex, newIndex );
			}

			property.serializedObject.ApplyModifiedProperties();
			for ( int i = indexesToDelete.Count - 1; i >= 0; i-- )
			{
				stateNode.RemoveLambdaPort( indexesToDelete[i] );
			}
			indexesToDelete.Clear();
			property.serializedObject.Update();
		}

		private static void DrawLine( float thickness = 2, float space = 2 )
		{
			EditorGUILayout.BeginVertical();
			EditorGUILayout.Space( space / 2, false );
			var rect = EditorGUILayout.BeginVertical();
			EditorGUILayout.Space( thickness, false );
			EditorGUI.DrawRect( rect, Color.black );
			EditorGUILayout.EndVertical();
			EditorGUILayout.Space( space / 2, false );
			EditorGUILayout.EndVertical();
		}

		private void DrawSummary()
		{
			DrawLine( 1, 2 );

			EditorGUILayout.BeginHorizontal();

			using ( new GUIEnabledScope( true, true ) )
			{
				if ( GUILayout.Button( _expandedSummary ? ExpandedButtonContent : FoldedButtonContent, EditorStyles.boldLabel, GUILayout.Width( 15 ) ) )
				{
					_expandedSummary = !_expandedSummary;
				}
			}
			EditorGUILayout.LabelField( s_summaryTitle, EditorStyles.boldLabel );

			EditorGUILayout.EndHorizontal();

			if ( _expandedSummary )
			{
				EditorGUILayout.LabelField( s_summaryLambdasTitle, EditorStyles.boldLabel );

				EditorGUILayout.LabelField( $"Declares {Target.Lambdas.Count} lambda (ForEach) actions." );
				EditorGUILayout.LabelField( $"{Target.TransitionsTo.Count()}/{Target.Lambdas.Count} lambdas are transition lambda.", s_wrappingLable );
				EditorGUILayout.LabelField( $"Transition to: {string.Join( ", ", Target.TransitionsTo.Select( t => t.StateName ) )}", s_wrappingLable );
				EditorGUILayout.LabelField( $"Transition from: {string.Join( ", ", Target.TransitionsFrom.Select( t => t.StateName ) )}", s_wrappingLable );

				DrawLine( 1, 0 );
				EditorGUILayout.LabelField( s_summaryComponentsTitle, EditorStyles.boldLabel );
				EditorGUILayout.LabelField( $"Uses {Target.AllComponents.Count()} components." );

				var writeComponents = Target.AllComponents.Where( c => c.Usage == ComponentLinkUsageType.All && c.AccessType == ComponentLinkAccessType.ReadWrite ).ToArray();
				var writeComponentsNames = string.Join(", ", writeComponents.Select(c => c.ComponentName));
				EditorGUILayout.LabelField( $"Writes to {writeComponents.Count()}/{Target.AllComponents.Count()} components [{writeComponentsNames}].", s_wrappingLable );

				var readComponents = Target.AllComponents.Where( c => c.Usage == ComponentLinkUsageType.All && c.AccessType == ComponentLinkAccessType.Read ).ToArray();
				var readComponentsNames = string.Join(", ", readComponents.Select(c => c.ComponentName));
				EditorGUILayout.LabelField( $"Reads from {readComponents.Count()}/{Target.AllComponents.Count()} components [{readComponentsNames}].", s_wrappingLable );
			}
		}
	}
}