using FSM.Components;
using FSM.Utility;
using FSM.Utility.Editor;

using Rotorz.Games;

using System;

using UnityEditor;

using UnityEngine;

namespace FSM.Editor.Components
{
	[CustomPropertyDrawer( typeof( ComponentLink ) )]
	public class ComponentLinkDrawer : PropertyDrawer
	{
		public static readonly GUIContent EmptyContent = new GUIContent("");

		public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
		{
			EditorGUI.BeginProperty( position, label, property );

			// Draw label
			position = EditorGUI.PrefixLabel( position, GUIUtility.GetControlID( FocusType.Passive ), EmptyContent );
			var propertyRect = new PropertyRect(position);
			var indent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = Math.Max( 0, indent - 1 );

			var usageProp = property.FindPropertyRelative( "_usageType" );
			var accessProp = property.FindPropertyRelative( "_accessType" );
			var typeProp = property.FindPropertyRelative( "_componentTypeReference" );

			propertyRect.AllocateLine();

			EditorGUI.PropertyField( propertyRect.AllocateWidthFlat( 50 ), usageProp, EmptyContent );

			if ( ( (ComponentLinkUsageType)usageProp.GetPropertyValue() ) == ComponentLinkUsageType.All )
			{
				EditorGUI.PropertyField( propertyRect.AllocateWidthFlat( 40 ), accessProp, EmptyContent );
			}

			EditorGUI.PropertyField( propertyRect.RestOfLine(), typeProp, EmptyContent );

			if ( !HasSettedType( property ) )
			{
				propertyRect.AllocateLine();
				var nameProp = property.FindPropertyRelative( "_componentName" );
				EditorGUI.PropertyField( propertyRect.AllocateWidthPrecent( 1f ), nameProp, EmptyContent );
			}

			EditorGUI.indentLevel = indent;
			EditorGUI.EndProperty();
		}

		public override float GetPropertyHeight( SerializedProperty property, GUIContent label )
		{
			var height = EditorGUIUtility.singleLineHeight;
			if ( !HasSettedType( property ) )
			{
				height += EditorGUIUtility.singleLineHeight;
			}
			return height;
		}

		private bool HasSettedType( SerializedProperty property ) =>
			( property.FindPropertyRelative( "_componentTypeReference" ).GetPropertyValue() as ClassTypeReference )?.Type != null;
	}
}