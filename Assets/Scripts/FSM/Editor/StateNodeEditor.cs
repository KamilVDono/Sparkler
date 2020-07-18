using System.Collections.Generic;
using System.Linq;

using UnityEditor;

using UnityEngine;

using XNode.Editor;

namespace FSM.Editor.Assets.Scripts.FSM.Editor
{
	[CustomNodeEditor( typeof( StateNode ) )]
	public class StateNodeEditor : FSMNodeEditor
	{
		private Dictionary<string, bool> _folded = new Dictionary<string, bool>();

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

				if ( iterator.isArray )
				{
					if ( !_folded.TryGetValue( iterator.propertyPath, out var folded ) )
					{
						folded = true;
					}
					DrawArray( iterator, ref folded );
					_folded[iterator.propertyPath] = folded;
				}
				else
				{
					NodeEditorGUILayout.PropertyField( iterator, true, GUILayout.ExpandWidth( true ) );
				}
			}
			serializedObject.ApplyModifiedProperties();
		}

		private static void DrawArray( SerializedProperty property, ref bool folded )
		{
			var indexesToDelete = new List<int>();
			int addNewElementCount = 0;

			// Header [Label - size - plus button]
			EditorGUILayout.Space( 8, false );
			EditorGUILayout.BeginHorizontal();
			folded = EditorGUILayout.Foldout( folded, property.displayName, true, EditorStyles.boldLabel );

			//EditorGUILayout.LabelField( property.displayName, EditorStyles.boldLabel, GUILayout.ExpandWidth( true ) );

			int newSize = EditorGUILayout.IntField( property.arraySize, GUILayout.Width( 50 ) );

			if ( GUILayout.Button( "+", GUILayout.Width( 25 ) ) )
			{
				++addNewElementCount;
			}
			EditorGUILayout.EndHorizontal();

			if ( folded )
			{
				return;
			}

			// Draw content
			EditorGUILayout.BeginVertical();
			for ( int i = 0; i < property.arraySize; i++ )
			{
				DrawLine( 2, 2 );
				EditorGUILayout.BeginHorizontal();

				EditorGUILayout.PropertyField( property.GetArrayElementAtIndex( i ), true, GUILayout.ExpandWidth( true ) );

				if ( GUILayout.Button( "-", GUILayout.Width( 25 ) ) )
				{
					indexesToDelete.Add( i );
				}
				EditorGUILayout.EndHorizontal();
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
			indexesToDelete.Clear();

			for ( int i = 0; i < addNewElementCount; i++ )
			{
				property.InsertArrayElementAtIndex( property.arraySize );
			}
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