using UnityEngine;

namespace Sparkler.Utility.Editor
{
	public static class GUIContentExtension
	{
		public static readonly GUIContent FoldedButtonContent = new GUIContent("\u25B6");
		public static readonly GUIContent ExpandedButtonContent = new GUIContent("\u25BC");
		public static readonly GUIContent MoveDownContent = new GUIContent("\u25BC");
		public static readonly GUIContent MoveUpContent = new GUIContent("\u25B2");
		public static readonly GUIContent MinusContent = new GUIContent("-");
		public static readonly GUIContent PlusContent = new GUIContent("+");
		public static readonly GUIContent EmptyContent = GUIContent.none;
	}
}