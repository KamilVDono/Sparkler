using FSM.Components;
using FSM.Utility.Editor;

using System.Collections.Generic;

using UnityEditor;

using UnityEngine;

using static FSM.Utility.SerializedPropertyExtension;

namespace FSM.Editor
{
	[CustomPropertyDrawer( typeof( SystemLambdaAction ) )]
	public class SystemLambdaActionDrawer : PropertyDrawer
	{
		public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
		{
			EditorGUI.BeginProperty( position, label, property );

			var wasChanged = GUI.changed;
			GUI.changed = false;

			PropertyRect propertyRect = new PropertyRect(position);

			// Draw name
			var nameProp = property.FindPropertyRelative("_name");
			EditorGUI.PropertyField( propertyRect.AllocateLine(), nameProp );

			var componentsProp = property.FindPropertyRelative("_components");
			DrawArray( propertyRect, componentsProp );

			EditorGUI.EndProperty();

			if ( GUI.changed )
			{
				// We can have some structural changes so we need to sync with Unity serialized side
				property.serializedObject.ApplyModifiedProperties();
				SystemLambdaAction systemLambdaAction = property.GetPropertyValue() as SystemLambdaAction;
				systemLambdaAction?.PropertiesChanged();
				property.serializedObject.Update();
			}

			GUI.changed |= wasChanged;
		}

		public override float GetPropertyHeight( SerializedProperty property, GUIContent label )
		{
			// name property
			float height = EditorGUIUtility.singleLineHeight;
			// array header
			height += EditorGUIUtility.singleLineHeight;
			// array items
			var components = property.FindPropertyRelative( "_components" );
			for ( int i = 0; i < components.arraySize; i++ )
			{
				height += EditorGUI.GetPropertyHeight( components.GetArrayElementAtIndex( i ) );
			}

			return height;
		}

		private static void DrawArray( PropertyRect propertyRect, SerializedProperty property )
		{
			var indexesToDelete = new List<int>();
			int addNewElementCount = 0;

			// -- Drawing
			// Header [Label - size - plus button]
			var line = propertyRect.AllocateLine();
			EditorGUI.LabelField( propertyRect.AlocateWidthWithAscesorFlat( 75 ), property.displayName, EditorStyles.boldLabel );

			int newSize = EditorGUI.IntField( propertyRect.AllocateWidthFlat(50), property.arraySize );

			if ( GUI.Button( propertyRect.RestOfLine(), "+" ) )
			{
				++addNewElementCount;
			}

			// Draw content
			for ( int i = 0; i < property.arraySize; i++ )
			{
				var componentProp = property.GetArrayElementAtIndex(i);
				propertyRect.AllocateLine( EditorGUI.GetPropertyHeight( componentProp ) );

				EditorGUI.PropertyField( propertyRect.AlocateWidthWithAscesorFlat( 0 ), componentProp );

				if ( GUI.Button( propertyRect.AllocateWidthFlat( 25 ), "-" ) )
				{
					indexesToDelete.Add( i );
				}
			}

			// -- Logic
			// Calculate direct size change
			if ( newSize < 0 && property.arraySize != 0 )
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

			// Do remove and add operations
			for ( int i = indexesToDelete.Count - 1; i >= 0; i-- )
			{
				property.DeleteArrayElementAtIndex( indexesToDelete[i] );
				GUI.changed = true;
			}
			indexesToDelete.Clear();

			for ( int i = 0; i < addNewElementCount; i++ )
			{
				property.InsertArrayElementAtIndex( property.arraySize );
				GUI.changed = true;
			}
		}
	}
}