using FSM.Components;
using FSM.Utility.Editor;

using System.Collections.Generic;
using System.Linq;

using UnityEditor;

using UnityEngine;

using XNode.Editor;

using static FSM.Utility.SerializedPropertyExtension;

namespace FSM.Editor.Assets.Scripts.FSM.Editor
{
	[CustomNodeEditor( typeof( StateNode ) )]
	public class StateNodeEditor : FSMNodeEditor<StateNode>
	{
		private static readonly GUIContent s_foldedButtonContent = new GUIContent("\u25B6");
		private static readonly GUIContent s_expandedButtonContent = new GUIContent("\u25BC");
		private static readonly GUIContent s_moveDownContent = new GUIContent("\u25BC");
		private static readonly GUIContent s_moveUpContent = new GUIContent("\u25B2");
		private static readonly GUIContent s_deleteContent = new GUIContent("-");
		private static readonly GUIContent s_addContent = new GUIContent("+");

		public override void OnBodyGUI()
		{
			// Unity specifically requires this to save/update any serial object.
			// serializedObject.Update(); must go at the start of an inspector gui, and
			// serializedObject.ApplyModifiedProperties(); goes at the end.
			serializedObject.Update();
			string[] excludes = { "m_Script", "graph", "position", "ports" };

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
		}

		private static void DrawArray( SerializedProperty property, StateNode stateNode )
		{
			var indexesToDelete = new List<int>();
			int addNewElementCount = 0;
			int moveDownIndex = -1;
			int moveUpIndex = -1;

			// Header [Label - size - plus button]
			EditorGUILayout.Space( 8, false );
			EditorGUILayout.BeginHorizontal();
			property.isExpanded ^= GUILayout.Button( !property.isExpanded ? s_foldedButtonContent : s_expandedButtonContent, EditorStyles.boldLabel, GUILayout.Width( 15 ) );
			EditorGUILayout.LabelField( property.displayName, EditorStyles.boldLabel );

			int newSize = EditorGUILayout.IntField( property.arraySize, GUILayout.Width( 50 ) );

			if ( GUILayout.Button( s_addContent, GUILayout.Width( 25 ) ) )
			{
				++addNewElementCount;
			}
			EditorGUILayout.EndHorizontal();

			if ( !property.isExpanded )
			{
				return;
			}

			// Draw content
			EditorGUILayout.BeginVertical();
			for ( int i = 0; i < property.arraySize; i++ )
			{
				var elementProperty = property.GetArrayElementAtIndex( i );
				DrawLine( 2, 2 );

				EditorGUILayout.BeginHorizontal();

				var name = elementProperty.FindPropertyRelative( "_name" )?.stringValue ?? "";
				EditorGUILayout.LabelField( $"{i}. {name}", EditorStyles.boldLabel );

				using ( new GUIEnabledScope( i < property.arraySize - 1 ) )
				{
					if ( GUILayout.Button( s_moveDownContent, GUILayout.Width( 25 ) ) )
					{
						moveDownIndex = i;
					}
				}

				if ( GUILayout.Button( s_deleteContent, GUILayout.Width( 25 ) ) )
				{
					indexesToDelete.Add( i );
				}

				using ( new GUIEnabledScope( i > 0 ) )
				{
					if ( GUILayout.Button( s_moveUpContent, GUILayout.Width( 25 ) ) )
					{
						moveUpIndex = i;
					}
				}

				EditorGUILayout.EndHorizontal();

				EditorGUILayout.PropertyField( elementProperty, true, GUILayout.ExpandWidth( true ) );

				NodeEditorGUILayout.AddPortField( stateNode.GetOrAddLambdaPort( i ) );
			}
			EditorGUILayout.EndVertical();

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
	}
}