using System;

using UnityEditor;

using UnityEngine;

namespace Sparkler.Utility.Editor
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
			if ( height <= 0 )
			{
				throw new ArgumentOutOfRangeException( nameof( height ), $"Parameter has to be grater than zero" );
			}
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
			if ( _currentRect == default )
			{
				throw new InvalidOperationException( $"Call {nameof( AllocateLine )} before." );
			}
			_currentRect.width = width;
			var rect = _currentRect;
			_currentRect.x += width;
			return rect;
		}

		public Rect AllocateWidthPrecent( float width )
		{
			if ( _currentRect == default )
			{
				throw new InvalidOperationException( $"Call {nameof( AllocateLine )} before." );
			}
			if ( width <= 0 )
			{
				throw new ArgumentOutOfRangeException( nameof( width ), "Parameter has to be grater than zero" );
			}
			if ( width > 1 )
			{
				throw new ArgumentOutOfRangeException( nameof( width ), "Parameter has to be less or equal to one" );
			}

			float flatWidth = _currentLine.width * width;
			return AllocateWidthFlat( flatWidth );
		}

		public Rect AllocateRestOfLine()
		{
			if ( _currentRect == default )
			{
				throw new InvalidOperationException( $"Call {nameof( AllocateLine )} before." );
			}
			_currentRect.width = _currentLine.width - ( _currentRect.x - _currentLine.x );
			return _currentRect;
		}

		public Rect AllocateWidthWithAscesorFlat( float widthOfAscesor )
		{
			if ( _currentRect == default )
			{
				throw new InvalidOperationException( $"Call {nameof( AllocateLine )} before." );
			}
			if ( widthOfAscesor <= 0 )
			{
				throw new ArgumentOutOfRangeException( nameof( widthOfAscesor ), "Parameter has to be grater than zero" );
			}

			var leftWidth = _currentLine.width - (_currentRect.x - _currentLine.x);
			var width = leftWidth - widthOfAscesor;
			return AllocateWidthFlat( width );
		}
	}
}