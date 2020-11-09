using Sparkler.Components;
using Sparkler.Editor.CodeGeneration;
using Sparkler.Rotorz.Games;
using Sparkler.Utility;
using Sparkler.Utility.Editor;

using System;

using UnityEditor;

using UnityEngine;

using static Sparkler.Utility.Editor.GUIContentExtension;

namespace Sparkler.Editor.Components
{
	[CustomPropertyDrawer( typeof( ComponentLink ) )]
	public class ComponentLinkDrawer : PropertyDrawer
	{
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

			EditorGUI.PropertyField( propertyRect.AllocateRestOfLine(), typeProp, EmptyContent );

			// New custom component
			if ( !HasSettedType( property ) )
			{
				propertyRect.AllocateLine();
				var nameProp = property.FindPropertyRelative( "_componentName" );
				EditorGUI.PropertyField( propertyRect.AllocateWidthWithAscesorFlat( 25 ), nameProp, EmptyContent );
				using ( new GUIEnabledScope( !string.IsNullOrWhiteSpace( nameProp.stringValue ) ) )
				{
					if ( GUI.Button( propertyRect.AllocateRestOfLine(), PlusContent ) )
					{
						ShowCreateComponentWindow( property );
					}
				}
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

		private void ShowCreateComponentWindow( SerializedProperty property )
		{
			var componentLink = property.GetPropertyValue<ComponentLink>();
			SystemNode stateNode = null;
			int depth = 1;
			while ( stateNode == null )
			{
				stateNode = property.GetPropertyValue( depth ) as SystemNode;
				++depth;
			}
			var systemsGraph = (SystemsGraph)stateNode.graph;
			ComponentCreatorWindow.ShowWindow( componentLink.HandwrittenName, systemsGraph.Namespace, systemsGraph.CodeGenerationPath );
		}

		private bool HasSettedType( SerializedProperty property ) =>
			( property.FindPropertyRelative( "_componentTypeReference" ).GetPropertyValue() as ClassTypeReference )?.Type != null;
	}
}