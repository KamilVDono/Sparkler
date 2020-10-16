﻿using FSM.Utility.Editor;

using Rotorz.Games;

using System;

using UnityEditor;

using UnityEngine;

namespace FSM.Editor.CodeGens
{
	public enum ComponentType
	{
		ComponentData = 1,
		SharedComponentData = 2,
		SystemStateComponent = 3,
		SystemStateSharedComponent = 4,
		BufferElementData = 5,
	}

	public enum ComponentFieldAccessType
	{
		Public = 1,
		Internal = 2,
		Private = 3,
	}

	[Serializable]
	public class ComponentDefinition
	{
		public string ComponentName = "";
		public string Namespace = "";
		public string Directory = "";
		public ComponentType ComponentType = ComponentType.ComponentData;
		public ComponentField[] Fields = new ComponentField[0];
	}

	public class ComponentFieldBlacklistedNamespacesAttribute : BlacklistedNamespacesAttribute
	{
		public ComponentFieldBlacklistedNamespacesAttribute() :
			base( true, @"Mono\..+", @"System\..+", @"JetBrains", @"Bee.", @"NUnit", @"Microsoft\..+", @"Novell\..+",
				@"ExCSS", @"NiceIO", @"ICSharpCode", @"Unity.Build", @"Newtonsoft\..+", @"Rider", @"TMPro", @"UnityEditor",
				@"Editor", @"SyntaxTree\..+", @"Unity.Profiling", @"Rotorz" )
		{
		}
	}

	[Serializable]
	public class ComponentField
	{
		public string name;

		[ClassTypeReferenceAttributes( typeof( OnlyBlittableAttribute ), typeof(ComponentFieldBlacklistedNamespacesAttribute))]
		public ClassTypeReference type;

		public ComponentFieldAccessType accessType = ComponentFieldAccessType.Public;
	}

	[CustomPropertyDrawer( typeof( ComponentField ) )]
	public class ComponentFieldDrawer : PropertyDrawer
	{
		public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
		{
			PropertyRect propertyRect = new PropertyRect(position);
			EditorGUI.BeginProperty( position, label, property );

			GUIDrawers.DrawFieldWithLabelPercentage( ref propertyRect, property.FindPropertyRelative( nameof( ComponentField.accessType ) ), labelWidth: 100, labelWidthPercentage: 0.3f );
			propertyRect.AllocateWidthPrecent( 0.05f );
			GUIDrawers.DrawFieldWithLabelPercentage( ref propertyRect, property.FindPropertyRelative( nameof( ComponentField.type ) ), false, labelWidth: 50, labelWidthPercentage: 0.65f );

			GUIDrawers.DrawFieldWithLabel( ref propertyRect, property.FindPropertyRelative( nameof( ComponentField.name ) ), labelWidth: 100 );

			EditorGUI.EndProperty();
		}

		public override float GetPropertyHeight( SerializedProperty property, GUIContent label ) => EditorGUIUtility.singleLineHeight * 2;
	}
}