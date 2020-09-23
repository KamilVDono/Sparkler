///
/// -- Code adopted from https://gist.github.com/mzaks/ec261ac853621af8503b73391ebd18f1
///
using UnityEditor;
using UnityEditor.IMGUI.Controls;

using UnityEngine;

namespace FSM.Editor.Components.SizeAnalysis
{
	public class ComponentAnalyzerWindow : EditorWindow
	{
		private static readonly GUIContent s_titleContent = new GUIContent("Analyze components size");

		private TreeViewState _treeViewState;

		private ComponentAnalyzerTreeView _componentAnalyzerTreeView;

		[SerializeField] private bool _showOnlyProblematicComponents;
		[SerializeField] private string _excludeString;

		[MenuItem( "FSM/Analyze Components Size" )]
		private static void ShowWindow()
		{
			var window = GetWindow<ComponentAnalyzerWindow>();
			window.titleContent = s_titleContent;
			window.Show();
		}

		private void OnGUI()
		{
			_showOnlyProblematicComponents = EditorGUILayout.Toggle( "Show problems only:", _showOnlyProblematicComponents );
			GUILayout.BeginHorizontal();
			GUILayout.Label( "Exclude:", GUILayout.Width( 40 ) );
			_excludeString = GUILayout.TextField( _excludeString );
			GUILayout.EndHorizontal();
			_componentAnalyzerTreeView.OnGUI( new Rect( 0, 40, position.width, position.height - 40 ) );
			_componentAnalyzerTreeView.ShowOnlyProblematic( _showOnlyProblematicComponents );
			_componentAnalyzerTreeView.Exclude( _excludeString );
		}

		private void OnEnable()
		{
			// Check whether there is already a serialized view state (state
			// that survived assembly reloading)
			if ( _treeViewState == null )
			{
				_treeViewState = new TreeViewState();
			}

			_componentAnalyzerTreeView = new ComponentAnalyzerTreeView( _treeViewState );
		}
	}
}