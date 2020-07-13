﻿using System;
using System.Collections.Generic;

using UnityEditor;

using UnityEngine;

namespace XNode.Editor
{
	public static class NodeEditorPreferences
	{
		public enum NoodleType { Curve, Line, Angled }

		/// <summary>
		/// The last editor we checked. This should be the one we modify
		/// </summary>
		private static XNode.Editor.NodeGraphEditor lastEditor;

		/// <summary>
		/// The last key we checked. This should be the one we modify
		/// </summary>
		private static string lastKey = "xNode.Settings";

		private static Dictionary<Type, Color> typeColors = new Dictionary<Type, Color>();
		private static Dictionary<string, Settings> settings = new Dictionary<string, Settings>();

		[System.Serializable]
		public class Settings : ISerializationCallbackReceiver
		{
			[SerializeField] private Color32 _gridLineColor = new Color(0.45f, 0.45f, 0.45f);
			public Color32 gridLineColor { get => _gridLineColor; set { _gridLineColor = value; _gridTexture = null; _crossTexture = null; } }

			[SerializeField] private Color32 _gridBgColor = new Color(0.18f, 0.18f, 0.18f);
			public Color32 gridBgColor { get => _gridBgColor; set { _gridBgColor = value; _gridTexture = null; } }

			[SerializeField] private Color32 _toolbarColor = Color.black;
			public Color toolbarColor { get => _toolbarColor; set => _toolbarColor = value; }

			[Obsolete( "Use maxZoom instead" )]
			public float zoomOutLimit { get => maxZoom; set => maxZoom = value; }

			[UnityEngine.Serialization.FormerlySerializedAs("zoomOutLimit")]
			public float maxZoom = 5f;

			public float minZoom = 1f;
			public Color32 highlightColor = new Color32(255, 255, 255, 255);
			public bool gridSnap = true;
			public bool autoSave = true;
			public bool zoomToMouse = true;
			public bool portTooltips = true;
			[SerializeField] private string typeColorsData = "";
			[NonSerialized] public Dictionary<string, Color> typeColors = new Dictionary<string, Color>();
			public NoodleType noodleType = NoodleType.Curve;

			private Texture2D _gridTexture;

			public Texture2D gridTexture
			{
				get
				{
					if ( _gridTexture == null )
					{
						_gridTexture = NodeEditorResources.GenerateGridTexture( gridLineColor, gridBgColor );
					}

					return _gridTexture;
				}
			}

			private Texture2D _crossTexture;

			public Texture2D crossTexture
			{
				get
				{
					if ( _crossTexture == null )
					{
						_crossTexture = NodeEditorResources.GenerateCrossTexture( gridLineColor );
					}

					return _crossTexture;
				}
			}

			public void OnAfterDeserialize()
			{
				// Deserialize typeColorsData
				typeColors = new Dictionary<string, Color>();
				string[] data = typeColorsData.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
				for ( int i = 0; i < data.Length; i += 2 )
				{
					if ( ColorUtility.TryParseHtmlString( "#" + data[i + 1], out Color col ) )
					{
						typeColors.Add( data[i], col );
					}
				}
			}

			public void OnBeforeSerialize()
			{
				// Serialize typeColors
				typeColorsData = "";
				foreach ( var item in typeColors )
				{
					typeColorsData += item.Key + "," + ColorUtility.ToHtmlStringRGB( item.Value ) + ",";
				}
			}
		}

		/// <summary>
		/// Get settings of current active editor
		/// </summary>
		public static Settings GetSettings()
		{
			if ( lastEditor != XNode.Editor.NodeEditorWindow.current.graphEditor )
			{
				object[] attribs = XNode.Editor.NodeEditorWindow.current.graphEditor.GetType().GetCustomAttributes(typeof(XNode.Editor.NodeGraphEditor.CustomNodeGraphEditorAttribute), true);
				if ( attribs.Length == 1 )
				{
					XNode.Editor.NodeGraphEditor.CustomNodeGraphEditorAttribute attrib = attribs[0] as XNode.Editor.NodeGraphEditor.CustomNodeGraphEditorAttribute;
					lastEditor = XNode.Editor.NodeEditorWindow.current.graphEditor;
					lastKey = attrib.editorPrefsKey;
				}
				else
				{
					return null;
				}
			}
			if ( !settings.ContainsKey( lastKey ) )
			{
				VerifyLoaded();
			}

			return settings[lastKey];
		}

#if UNITY_2019_1_OR_NEWER

		[SettingsProvider]
		public static SettingsProvider CreateXNodeSettingsProvider()
		{
			SettingsProvider provider = new SettingsProvider("Preferences/Node Editor", SettingsScope.User)
			{
				guiHandler = (searchContext) => { XNode.Editor.NodeEditorPreferences.PreferencesGUI(); },
				keywords = new HashSet<string>(new [] { "xNode", "node", "editor", "graph", "connections", "noodles", "ports" })
			};
			return provider;
		}

#endif

#if !UNITY_2019_1_OR_NEWER
				[PreferenceItem("Node Editor")]
#endif

		private static void PreferencesGUI()
		{
			VerifyLoaded();
			Settings settings = NodeEditorPreferences.settings[lastKey];

			NodeSettingsGUI( lastKey, settings );
			GridSettingsGUI( lastKey, settings );
			SystemSettingsGUI( lastKey, settings );
			TypeColorsGUI( lastKey, settings );
			if ( GUILayout.Button( new GUIContent( "Set Default", "Reset all values to default" ), GUILayout.Width( 120 ) ) )
			{
				ResetPrefs();
			}
		}

		private static void GridSettingsGUI( string key, Settings settings )
		{
			//Label
			EditorGUILayout.LabelField( "Grid", EditorStyles.boldLabel );
			settings.gridSnap = EditorGUILayout.Toggle( new GUIContent( "Snap", "Hold CTRL in editor to invert" ), settings.gridSnap );
			settings.zoomToMouse = EditorGUILayout.Toggle( new GUIContent( "Zoom to Mouse", "Zooms towards mouse position" ), settings.zoomToMouse );
			EditorGUILayout.LabelField( "Zoom" );
			EditorGUI.indentLevel++;
			settings.maxZoom = EditorGUILayout.FloatField( new GUIContent( "Max", "Upper limit to zoom" ), settings.maxZoom );
			settings.minZoom = EditorGUILayout.FloatField( new GUIContent( "Min", "Lower limit to zoom" ), settings.minZoom );
			EditorGUI.indentLevel--;
			settings.gridLineColor = EditorGUILayout.ColorField( "Color", settings.gridLineColor );
			settings.gridBgColor = EditorGUILayout.ColorField( " ", settings.gridBgColor );
			settings.toolbarColor = EditorGUILayout.ColorField( "Toolbar Color", settings.toolbarColor );
			if ( GUI.changed )
			{
				SavePrefs( key, settings );

				NodeEditorWindow.RepaintAll();
			}
			EditorGUILayout.Space();
		}

		private static void SystemSettingsGUI( string key, Settings settings )
		{
			//Label
			EditorGUILayout.LabelField( "System", EditorStyles.boldLabel );
			settings.autoSave = EditorGUILayout.Toggle( new GUIContent( "Autosave", "Disable for better editor performance" ), settings.autoSave );
			if ( GUI.changed )
			{
				SavePrefs( key, settings );
			}

			EditorGUILayout.Space();
		}

		private static void NodeSettingsGUI( string key, Settings settings )
		{
			//Label
			EditorGUILayout.LabelField( "Node", EditorStyles.boldLabel );
			settings.highlightColor = EditorGUILayout.ColorField( "Selection", settings.highlightColor );
			settings.noodleType = (NoodleType)EditorGUILayout.EnumPopup( "Noodle type", (Enum)settings.noodleType );
			settings.portTooltips = EditorGUILayout.Toggle( "Port Tooltips", settings.portTooltips );
			if ( GUI.changed )
			{
				SavePrefs( key, settings );
				NodeEditorWindow.RepaintAll();
			}
			EditorGUILayout.Space();
		}

		private static void TypeColorsGUI( string key, Settings settings )
		{
			//Label
			EditorGUILayout.LabelField( "Types", EditorStyles.boldLabel );

			//Clone keys so we can enumerate the dictionary and make changes.
			var typeColorKeys = new List<Type>(typeColors.Keys);

			//Display type colors. Save them if they are edited by the user
			foreach ( var type in typeColorKeys )
			{
				string typeColorKey = NodeEditorUtilities.PrettyName(type);
				Color col = typeColors[type];
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.BeginHorizontal();
				col = EditorGUILayout.ColorField( typeColorKey, col );
				EditorGUILayout.EndHorizontal();
				if ( EditorGUI.EndChangeCheck() )
				{
					typeColors[type] = col;
					if ( settings.typeColors.ContainsKey( typeColorKey ) )
					{
						settings.typeColors[typeColorKey] = col;
					}
					else
					{
						settings.typeColors.Add( typeColorKey, col );
					}

					SavePrefs( key, settings );
					NodeEditorWindow.RepaintAll();
				}
			}
		}

		/// <summary>
		/// Load prefs if they exist. Create if they don't
		/// </summary>
		private static Settings LoadPrefs()
		{
			// Create settings if it doesn't exist
			if ( !EditorPrefs.HasKey( lastKey ) )
			{
				if ( lastEditor != null )
				{
					EditorPrefs.SetString( lastKey, JsonUtility.ToJson( lastEditor.GetDefaultPreferences() ) );
				}
				else
				{
					EditorPrefs.SetString( lastKey, JsonUtility.ToJson( new Settings() ) );
				}
			}
			return JsonUtility.FromJson<Settings>( EditorPrefs.GetString( lastKey ) );
		}

		/// <summary>
		/// Delete all prefs
		/// </summary>
		public static void ResetPrefs()
		{
			if ( EditorPrefs.HasKey( lastKey ) )
			{
				EditorPrefs.DeleteKey( lastKey );
			}

			if ( settings.ContainsKey( lastKey ) )
			{
				settings.Remove( lastKey );
			}

			typeColors = new Dictionary<Type, Color>();
			VerifyLoaded();
			NodeEditorWindow.RepaintAll();
		}

		/// <summary>
		/// Save preferences in EditorPrefs
		/// </summary>
		private static void SavePrefs( string key, Settings settings ) => EditorPrefs.SetString( key, JsonUtility.ToJson( settings ) );

		/// <summary>
		/// Check if we have loaded settings for given key. If not, load them
		/// </summary>
		private static void VerifyLoaded()
		{
			if ( !settings.ContainsKey( lastKey ) )
			{
				settings.Add( lastKey, LoadPrefs() );
			}
		}

		/// <summary>
		/// Return color based on type
		/// </summary>
		public static Color GetTypeColor( System.Type type )
		{
			VerifyLoaded();
			if ( type == null )
			{
				return Color.gray;
			}

			if ( !typeColors.TryGetValue( type, out Color col ) )
			{
				string typeName = type.PrettyName();
				if ( settings[lastKey].typeColors.ContainsKey( typeName ) )
				{
					typeColors.Add( type, settings[lastKey].typeColors[typeName] );
				}
				else
				{
#if UNITY_5_4_OR_NEWER
					UnityEngine.Random.InitState( typeName.GetHashCode() );
#else
										UnityEngine.Random.seed = typeName.GetHashCode();
#endif
					col = new Color( UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value );
					typeColors.Add( type, col );
				}
			}
			return col;
		}
	}
}