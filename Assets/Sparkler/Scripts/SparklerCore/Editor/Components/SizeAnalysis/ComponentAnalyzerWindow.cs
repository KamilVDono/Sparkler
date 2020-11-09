///
/// -- Code adopted from https://gist.github.com/mzaks/ec261ac853621af8503b73391ebd18f1
///
using UnityEditor;
using UnityEditor.IMGUI.Controls;

using UnityEngine;

namespace Sparkler.Editor.Components.SizeAnalysis
{
	public class ComponentAnalyzerWindow : EditorWindow
	{
		private const string TITLE = "Analyze components & enums size";
		private const string MENU_TITLE = "DOTS/Sparkler/" + TITLE;
		private static readonly GUIContent s_titleContent = new GUIContent(TITLE);

		private static TreeViewState _treeViewState;

		private ComponentAnalyzerTreeView _componentAnalyzerTreeView;

		private bool _showOnlyProblematicComponents = true;
		private bool _showEnums = true;
		private string _excludeString = " ,System,Unity,mscorlib,Mono,nunit,bee,Rotorz,Newtonsoft,Novell,xNode,SyntaxTree,";

		[MenuItem( MENU_TITLE, priority = 200 )]
		private static void ShowWindow()
		{
			var window = GetWindow<ComponentAnalyzerWindow>();
			window.titleContent = s_titleContent;
			window.Show();
		}

		private void OnGUI()
		{
			var lastRectHeight = EditorGUILayout.BeginVertical().height + 4;
			GUILayout.BeginHorizontal();
			_showOnlyProblematicComponents = EditorGUILayout.Toggle( "Show problems only:", _showOnlyProblematicComponents );
			_showEnums = EditorGUILayout.Toggle( "Show enum problems:", _showEnums );
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label( "Exclude:", GUILayout.Width( 60 ) );
			_excludeString = GUILayout.TextField( _excludeString );
			GUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();

			_componentAnalyzerTreeView.OnGUI( new Rect( 0, lastRectHeight, position.width, position.height - lastRectHeight ) );
			_componentAnalyzerTreeView.ShowOnlyProblematic( _showOnlyProblematicComponents );
			_componentAnalyzerTreeView.ShowEnums( _showEnums );
			_componentAnalyzerTreeView.Exclude( _excludeString );
		}

		private void OnEnable()
		{
			if ( _treeViewState == null )
			{
				_treeViewState = new TreeViewState();
			}

			_componentAnalyzerTreeView = new ComponentAnalyzerTreeView( _treeViewState );
		}
	}
}