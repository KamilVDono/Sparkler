using UnityEditor;

using UnityEngine;

namespace FSM.Utility.Editor
{
	public struct PropertyRect
	{
		private Rect _originalRect;
		private Rect _currentLine;
		private Rect _currentRect;

		public PropertyRect( Rect originalRect ) : this() => _originalRect = originalRect;

		public Rect AllocateLine() => AllocateLine( EditorGUIUtility.singleLineHeight );

		public Rect AllocateLine( float height )
		{
			if ( _currentRect == default )
			{
				_currentRect = _originalRect;
				_currentRect.height = height;
			}
			else
			{
				_currentRect.width = _originalRect.width;
				_currentRect.x = _originalRect.x;
				_currentRect.y += _currentRect.height;
				_currentRect.height = height;
			}
			_currentLine = _currentRect;
			return _currentRect;
		}

		public Rect AllocateWidthFlat( float width )
		{
			_currentRect.width = width;
			var rect = _currentRect;
			_currentRect.x += width;
			return rect;
		}

		public Rect AllocateWidthPrecent( float width )
		{
			float flatWidth = _currentLine.width * width;
			return AllocateWidthFlat( flatWidth );
		}

		public Rect RestOfLine()
		{
			_currentRect.width = _currentLine.width - ( _currentRect.x - _currentLine.x );
			return _currentRect;
		}

		public Rect AlocateWidthWithAscesorFlat( float widthOfAscesor )
		{
			var width = _currentRect.width - widthOfAscesor;
			_currentRect.width = width;
			var rect = _currentRect;
			_currentRect.x += width;
			return rect;
		}
	}
}