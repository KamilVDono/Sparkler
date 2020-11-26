using System;
using System.Collections.Generic;

using UnityEditor;

using UnityEngine;

using static Sparkler.Utility.Editor.GUIContentExtension;

namespace Sparkler.Utility.Editor
{
	public static class GUIDrawers
	{
		#region Array
		private static readonly SimplePool<List<int>> s_indexesPool = new SimplePool<List<int>>(() => new List<int>());

		public static void DrawArray<T>( ref PropertyRect propertyRect, SerializedProperty property, List<T> cache = null )
		{
			var indexesToDelete = s_indexesPool.Get();
			indexesToDelete.Clear();

			int addNewElementCount = 0;

			// -- Drawing
			// Header [Label - size - plus button]
			propertyRect.AllocateLine();
			using ( new GUIEnabledScope( true, true ) )
			{
				property.isExpanded ^= GUI.Button( propertyRect.AllocateWidthFlat( 15 ), !property.isExpanded ? FoldedButtonContent : ExpandedButtonContent, EditorStyles.boldLabel );
			}
			EditorGUI.LabelField( propertyRect.AllocateWidthWithAscesorFlat( 75 ), property.displayName, EditorStyles.boldLabel );

			int newSize = EditorGUI.IntField( propertyRect.AllocateWidthFlat(50), property.arraySize );

			if ( GUI.Button( propertyRect.AllocateRestOfLine(), PlusContent ) )
			{
				++addNewElementCount;
			}

			// Draw content
			if ( property.isExpanded )
			{
				for ( int i = 0; i < property.arraySize; i++ )
				{
					var componentProp = property.GetArrayElementAtIndex(i);
					propertyRect.AllocateLine( EditorGUI.GetPropertyHeight( componentProp ) );

					EditorGUI.BeginChangeCheck();
					EditorGUI.PropertyField( propertyRect.AllocateWidthWithAscesorFlat( 25 ), componentProp );
					if ( EditorGUI.EndChangeCheck() && cache != null )
					{
						cache.Add( componentProp.GetPropertyValue<T>() );
					}

					if ( GUI.Button( propertyRect.AllocateWidthFlat( 25 ), MinusContent ) )
					{
						indexesToDelete.Add( i );
					}
				}
			}

			ArrayOperations( property, indexesToDelete, addNewElementCount, newSize );
		}

		public static void DrawArray<T>( SerializedProperty property, List<T> cache = null )
		{
			var indexesToDelete = s_indexesPool.Get();
			indexesToDelete.Clear();

			int addNewElementCount = 0;

			// -- Drawing
			// Header [Label - size - plus button]
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
				for ( int i = 0; i < property.arraySize; i++ )
				{
					EditorGUILayout.BeginHorizontal();
					var componentProp = property.GetArrayElementAtIndex(i);

					EditorGUI.BeginChangeCheck();
					EditorGUILayout.PropertyField( componentProp );
					if ( EditorGUI.EndChangeCheck() && cache != null )
					{
						cache.Add( componentProp.GetPropertyValue<T>() );
					}

					if ( GUILayout.Button( MinusContent, GUILayout.Width( 25 ) ) )
					{
						indexesToDelete.Add( i );
					}
					EditorGUILayout.EndHorizontal();
				}
			}

			ArrayOperations( property, indexesToDelete, addNewElementCount, newSize );
		}

		private static void ArrayOperations( SerializedProperty property, List<int> indexesToDelete, int addNewElementCount, int newSize )
		{
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
			}
			if ( addNewElementCount > 0 )
			{
				property.serializedObject.ApplyModifiedProperties();
				try
				{
					for ( int i = property.arraySize - addNewElementCount; i < property.arraySize; i++ )
					{
						var newItemProp = property.GetArrayElementAtIndex( i );
						var itemType = newItemProp.GetPropertyType();
						var newItem = newItemProp.GetPropertyValue();
						var defaultItem = Activator.CreateInstance(itemType);
						foreach ( var fieldValue in defaultItem.GetAllFieldsValues() )
						{
							fieldValue.Key.SetMemberValue( newItem, fieldValue.Value );
						}
					}
				}
				finally
				{
					property.serializedObject.Update();
					GUI.changed = true;
				}
			}
		}

		#endregion Array

		public static void DrawFieldWithLabel( ref PropertyRect propertyRect, SerializedProperty property, bool startNewLine = true, int labelWidth = 150 )
		{
			if ( startNewLine )
			{
				propertyRect.AllocateLine();
			}
			EditorGUI.LabelField( propertyRect.AllocateWidthFlat( labelWidth ), property.displayName );
			EditorGUI.PropertyField( propertyRect.AllocateRestOfLine(), property, EmptyContent );
		}

		public static void DrawFieldWithLabelPercentage( ref PropertyRect propertyRect, SerializedProperty property, bool startNewLine = true, int labelWidth = 150, float labelWidthPercentage = 0.5f )
		{
			if ( startNewLine )
			{
				propertyRect.AllocateLine();
			}
			var halfRect = new PropertyRect(propertyRect.AllocateWidthPrecent( labelWidthPercentage ));
			halfRect.AllocateLine();
			EditorGUI.LabelField( halfRect.AllocateWidthFlat( labelWidth ), property.displayName );
			EditorGUI.PropertyField( halfRect.AllocateRestOfLine(), property, EmptyContent );
		}

		public static void DrawFieldWithLabel( SerializedProperty property, int labelWidth = 150 )
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField( property.displayName, GUILayout.Width( labelWidth ) );
			EditorGUILayout.PropertyField( property, EmptyContent );
			EditorGUILayout.EndHorizontal();
		}

		public static void DrawFieldWithLabelWidth( SerializedProperty property, int labelWidth = 150, int fullWidth = 300 )
		{
			EditorGUILayout.BeginHorizontal( GUILayout.Width( fullWidth ) );
			EditorGUILayout.LabelField( property.displayName, GUILayout.Width( labelWidth ) );
			EditorGUILayout.PropertyField( property, EmptyContent, GUILayout.MaxWidth( fullWidth - labelWidth ) );
			EditorGUILayout.EndHorizontal();
		}
	}
}