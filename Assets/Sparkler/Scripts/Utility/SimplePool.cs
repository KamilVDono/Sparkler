using System;
using System.Collections.Generic;

namespace Sparkler.Utility
{
	public class SimplePool<T>
	{
		private const int INCREASE_SIZE = 10;

		private Queue<T> _pool = new Queue<T>();
		private Func<T> _createNewFunc;

		public SimplePool( Func<T> createNew ) => _createNewFunc = createNew;

		public T Get()
		{
			if ( _pool.Count > 0 )
			{
				return _pool.Dequeue();
			}
			IncreasePool();
			return _pool.Dequeue();
		}

		public void Release( T item ) => _pool.Enqueue( item );

		private void IncreasePool()
		{
			for ( int i = 0; i < INCREASE_SIZE; i++ )
			{
				_pool.Enqueue( _createNewFunc() );
			}
		}
	}
}