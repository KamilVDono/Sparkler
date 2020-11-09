using Sparkler.Components;
using Sparkler.Editor.SharedComponentFilters;
using Sparkler.Utility.Editor;

using System.Collections.Generic;

using UnityEditor;

using UnityEngine;

using static Sparkler.Utility.Editor.GUIContentExtension;
using static Sparkler.Utility.SerializedPropertyExtension;

namespace Sparkler.Editor
{
	[CustomPropertyDrawer( typeof( SystemLambdaAction ) )]
	public class SystemLambdaActionDrawer : PropertyDrawer
	{
		#region GUIContent
		private static readonly GUIContent s_parallelContent = new GUIContent("Parallel schedule");
		private static readonly GUIContent s_structuralChangesContent = new GUIContent("Structural changes");
		private static readonly GUIContent s_queryFieldContent = new GUIContent("Query field");
		private static readonly GUIContent s_sharedFilterContent = new GUIContent("Shared filter");
		#endregion GUIContent

		#region Styles
		private static GUIStyle s_boldLabelStyle = null;

		private static GUIStyle s_BoldLabelStyle => s_boldLabelStyle ?? ( s_boldLabelStyle = new GUIStyle( EditorStyles.boldLabel )
		{
			wordWrap = true,
			richText = true,
		} );

		#endregion Styles

		private static List<ComponentLink> s_cache = new List<ComponentLink>();

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
			s_cache.Clear();
			GUIDrawers.DrawArray( ref propertyRect, componentsProp, s_cache );

			// Draw additional settings
			propertyRect.AllocateLine();
			var parallelScheduling = property.FindPropertyRelative("_parallelSchedule");
			EditorGUI.LabelField( propertyRect.AllocateWidthFlat( 105 ), s_parallelContent );
			EditorGUI.PropertyField( propertyRect.AllocateWidthFlat( 25 ), parallelScheduling, EmptyContent );
			var structuralChanges = property.FindPropertyRelative("_hasStructuralChanges");
			EditorGUI.LabelField( propertyRect.AllocateWidthFlat( 115 ), s_structuralChangesContent );
			EditorGUI.PropertyField( propertyRect.AllocateWidthFlat( 25 ), structuralChanges, EmptyContent );

			propertyRect.AllocateLine();
			var queryField = property.FindPropertyRelative("_queryField");
			EditorGUI.LabelField( propertyRect.AllocateWidthFlat( 75 ), s_queryFieldContent );
			EditorGUI.PropertyField( propertyRect.AllocateRestOfLine(), queryField, EmptyContent );

			propertyRect.AllocateLine();
			var sharedFilter = property.FindPropertyRelative("_sharedFilter").GetPropertyValue<SharedComponentFilter>();
			EditorGUI.LabelField( propertyRect.AllocateWidthFlat( 75 ), s_sharedFilterContent );

			if ( sharedFilter.IsValid )
			{
				EditorGUI.LabelField( propertyRect.AllocateWidthWithAscesorFlat( 25 ), sharedFilter.FilterName, s_BoldLabelStyle );
			}
			else
			{
				EditorGUI.LabelField( propertyRect.AllocateWidthWithAscesorFlat( 25 ), EmptyContent );
			}

			if ( !sharedFilter.IsValid && GUI.Button( propertyRect.AllocateRestOfLine(), PlusContent ) )
			{
				SharedComponentFilterWindow.ShowWindow( ( name, declaration ) =>
				{
					property.serializedObject.ApplyModifiedProperties();
					sharedFilter.FilterName = name;
					sharedFilter.ComponentDeclaration = declaration;
					property.serializedObject.Update();
				} );
			}
			else if ( sharedFilter.IsValid && GUI.Button( propertyRect.AllocateRestOfLine(), MinusContent ) )
			{
				sharedFilter.Invalid();
			}

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
			// parallel scheduling and structural changes
			height += EditorGUIUtility.singleLineHeight;
			// query field
			height += EditorGUIUtility.singleLineHeight;
			// shared filter
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
	}
}