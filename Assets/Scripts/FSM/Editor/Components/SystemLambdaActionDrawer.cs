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
		private static readonly GUIContent s_foldedButtonContent = new GUIContent("\u25B6");
		private static readonly GUIContent s_expandedButtonContent = new GUIContent("\u25BC");

		private static List<ComponentLink> s_cache = new List<ComponentLink>();
		private static List<int> s_indexesToDelete = new List<int>();

		public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
		{
			EditorGUI.BeginProperty( position, label, property );

			var wasChanged = GUI.changed;
			GUI.changed = false;

			PropertyRect propertyRect = new PropertyRect(position);

			// Draw name
			var nameProp = property.FindPropertyRelative("_name");
			EditorGUI.PropertyField( propertyRect.AllocateLine(), nameProp );

			// Draw array
			var componentsProp = property.FindPropertyRelative("_components");
			DrawArray( propertyRect, componentsProp );

			EditorGUI.EndProperty();

			if ( GUI.changed )
			{
				// We can have some structural changes so we need to sync with Unity serialized side
				property.serializedObject.ApplyModifiedProperties();
				SystemLambdaAction systemLambdaAction = property.GetPropertyValue() as SystemLambdaAction;
				systemLambdaAction?.PropertiesChanged( s_cache );
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
			if ( components.isExpanded )
			{
				for ( int i = 0; i < components.arraySize; i++ )
				{
					height += EditorGUI.GetPropertyHeight( components.GetArrayElementAtIndex( i ) );
				}
			}

			return height;
		}

		private void DrawArray( PropertyRect propertyRect, SerializedProperty property )
		{
			s_cache.Clear();
			s_indexesToDelete.Clear();

			int addNewElementCount = 0;

			// -- Drawing
			// Header [Label - size - plus button]
			propertyRect.AllocateLine();
			property.isExpanded ^= GUI.Button( propertyRect.AllocateWidthFlat( 15 ), !property.isExpanded ? s_foldedButtonContent : s_expandedButtonContent, EditorStyles.boldLabel );
			EditorGUI.LabelField( propertyRect.AlocateWidthWithAscesorFlat( 75 ), property.displayName, EditorStyles.boldLabel );

			int newSize = EditorGUI.IntField( propertyRect.AllocateWidthFlat(50), property.arraySize );

			if ( GUI.Button( propertyRect.RestOfLine(), "+" ) )
			{
				++addNewElementCount;
			}

			if ( !property.isExpanded )
			{
				return;
			}

			// Draw content
			for ( int i = 0; i < property.arraySize; i++ )
			{
				var componentProp = property.GetArrayElementAtIndex(i);
				propertyRect.AllocateLine( EditorGUI.GetPropertyHeight( componentProp ) );

				EditorGUI.BeginChangeCheck();
				EditorGUI.PropertyField( propertyRect.AlocateWidthWithAscesorFlat( 0 ), componentProp );
				if ( EditorGUI.EndChangeCheck() )
				{
					s_cache.Add( componentProp.GetPropertyValue<ComponentLink>() );
				}

				if ( GUI.Button( propertyRect.AllocateWidthFlat( 25 ), "-" ) )
				{
					s_indexesToDelete.Add( i );
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
					s_indexesToDelete.Add( property.arraySize - 1 - i );
				}
			}
			else if ( newSize > property.arraySize )
			{
				addNewElementCount += newSize - property.arraySize;
			}

			// Do remove and add operations
			for ( int i = s_indexesToDelete.Count - 1; i >= 0; i-- )
			{
				property.DeleteArrayElementAtIndex( s_indexesToDelete[i] );
				GUI.changed = true;
			}
			s_indexesToDelete.Clear();

			for ( int i = 0; i < addNewElementCount; i++ )
			{
				property.InsertArrayElementAtIndex( property.arraySize );
				GUI.changed = true;
			}
		}
	}
}