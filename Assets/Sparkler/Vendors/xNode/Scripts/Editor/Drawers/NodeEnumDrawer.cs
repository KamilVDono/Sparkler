using System;
using System.Reflection;

using UnityEditor;

using UnityEngine;

namespace Sparkler.XNode.Editor.Drawers
{
	[CustomPropertyDrawer( typeof( NodeEnumAttribute ) )]
	public class NodeEnumDrawer : PropertyDrawer
	{
		private static EventModifiers s_lastModifiers;

		public static void EnumPopup( Rect position, SerializedProperty property, GUIContent label, bool isFlag, Type enumType )
		{
			// Throw error on wrong type
			if ( property.propertyType != SerializedPropertyType.Enum )
			{
				throw new ArgumentException( "Parameter selected must be of type System.Enum" );
			}

			// Add label
			position = EditorGUI.PrefixLabel( position, GUIUtility.GetControlID( FocusType.Passive ), label );

			Array enumValues = AllValues(enumType);
			string[] enumNames = AllNames(enumType);

			// Get current enum name
			string enumName = "";
			if ( property.enumValueIndex >= 0 && property.enumValueIndex < property.enumDisplayNames.Length )
			{
				enumName = property.enumDisplayNames[property.enumValueIndex];
			}

			if ( isFlag && property.enumValueIndex == -1 )
			{
				if ( property.intValue < 1 )
				{
					enumName = "[None]";
				}
				else if ( property.intValue == int.MaxValue )
				{
					enumName = "[All]";
				}
				else
				{
					enumName = "[Mixed]";
				}
			}

			// Display dropdown
			if ( EditorGUI.DropdownButton( position, new GUIContent( enumName ), FocusType.Passive ) )
			{
				// Position is all wrong if we show the dropdown during the node draw phase. Instead, add it
				// to onLateGUI to display it later.
				s_lastModifiers = Event.current?.modifiers ?? EventModifiers.None;
				NodeEditorWindow.current.onLateGUI += () => ShowContextMenuAtMouse( property, isFlag, enumValues, enumNames );
			}
		}

		public static void ShowContextMenuAtMouse( SerializedProperty property, bool isFlag, Array enumValues, string[] enumNames )
		{
			// Initialize menu
			GenericMenu menu = new GenericMenu();

			// Add all enum display names to menu
			for ( int i = 0; i < enumValues.Length; i++ )
			{
				int index = i;
				bool selected = false;
				if ( isFlag )
				{
					selected =
							(int)enumValues.GetValue( i ) != 0 ? ( (int)enumValues.GetValue( i ) & property.intValue ) == (int)enumValues.GetValue( i ) : property.intValue == 0;
				}
				else
				{
					selected = index == property.enumValueIndex;
				}

				GUIContent contentName = new GUIContent(isFlag ? enumNames[i] : property.enumDisplayNames[i]);
				menu.AddItem( contentName, selected, () => SetEnum( property, index, isFlag, enumValues ) );
			}

			// Display at cursor position
			Rect r = new Rect(Event.current.mousePosition, new Vector2(0, 0));
			menu.DropDown( r );
		}

		public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
		{
			EditorGUI.BeginProperty( position, label, property );

			EnumPopup( position, property, label, fieldInfo.FieldType.GetCustomAttribute<FlagsAttribute>() != null, fieldInfo.FieldType );

			EditorGUI.EndProperty();
		}

		private static void SetEnum( SerializedProperty property, int index, bool isFlag, Array enumValues )
		{
			if ( isFlag )
			{
				if ( property.intValue != 0 && (int)enumValues.GetValue( index ) == 0 )
				{
					// None element
					property.intValue = 0;
				}
				else
				{
					if ( ( property.intValue & (int)enumValues.GetValue( index ) ) == (int)enumValues.GetValue( index ) )
					{
						// Unselect selected
						property.intValue ^= (int)enumValues.GetValue( index );
					}
					else
					{
						// Select selected
						if ( s_lastModifiers.HasFlag( EventModifiers.Control ) )
						{
							property.intValue = (int)enumValues.GetValue( index );
						}
						else
						{
							property.intValue |= (int)enumValues.GetValue( index );
						}
					}
				}
			}
			else
			{
				property.enumValueIndex = index;
			}

			property.serializedObject.ApplyModifiedProperties();
			property.serializedObject.Update();
		}

		private static Array AllValues( Type enumType ) => Enum.GetValues( enumType );

		private static string[] AllNames( Type enumType ) => Enum.GetNames( enumType );
	}
}