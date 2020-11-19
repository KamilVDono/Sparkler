using Sparkler.Example.Components;

using System.Collections.Generic;

using Unity.Entities;

using UnityEngine;

using SF = UnityEngine.SerializeField;

namespace Sparkler.Examples.Authoring
{
	public class CharactersView : MonoBehaviour
	{
		[SF] private EntityDisplay _displayPrefab;
		[SF] private RectTransform _inHomeParent;
		[SF] private RectTransform _inTravelParent;
		[SF] private RectTransform _inWorkParent;
		private List<EntityDisplay> _inHomeDisplays = new List<EntityDisplay>();
		private List<EntityDisplay> _inTravelDisplays = new List<EntityDisplay>();
		private List<EntityDisplay> _inWorkDisplays = new List<EntityDisplay>();
		private EntityQuery _inHomeQuery;
		private EntityQuery _inTravelQuery;
		private EntityQuery _inWorkQuery;

		private void Start()
		{
			var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
			_inHomeQuery = manager.CreateEntityQuery( ComponentType.ReadOnly<CharacterTag>(), ComponentType.ReadOnly<InPlace>() );
			_inHomeQuery.SetSharedComponentFilter( new InPlace() { Place = Place.Home } );

			_inTravelQuery = manager.CreateEntityQuery( ComponentType.ReadOnly<CharacterTag>(), ComponentType.ReadOnly<InPlace>() );
			_inTravelQuery.SetSharedComponentFilter( new InPlace() { Place = Place.InTravel } );

			_inWorkQuery = manager.CreateEntityQuery( ComponentType.ReadOnly<CharacterTag>(), ComponentType.ReadOnly<InPlace>() );
			_inWorkQuery.SetSharedComponentFilter( new InPlace() { Place = Place.Mine } );

			ResizePool( _inHomeDisplays, _inHomeParent );
			ResizePool( _inTravelDisplays, _inTravelParent );
			ResizePool( _inWorkDisplays, _inWorkParent );
		}

		private void LateUpdate()
		{
			if ( !gameObject.activeInHierarchy )
			{
				return;
			}

			using var inHome = _inHomeQuery.ToEntityArray( Unity.Collections.Allocator.Temp );
			int i = 0;
			foreach ( var entity in inHome )
			{
				var display = Display( i++, _inHomeDisplays, _inHomeParent );
				display.SetEntity( entity );
			}
			DeactiveDisplays( i, _inHomeDisplays );

			using var inTravel = _inTravelQuery.ToEntityArray( Unity.Collections.Allocator.Temp );
			i = 0;
			foreach ( var entity in inTravel )
			{
				var display = Display( i++, _inTravelDisplays, _inTravelParent );
				display.SetEntity( entity );
			}
			DeactiveDisplays( i, _inTravelDisplays );

			using var inWork = _inWorkQuery.ToEntityArray( Unity.Collections.Allocator.Temp );
			i = 0;
			foreach ( var entity in inWork )
			{
				var display = Display( i++, _inWorkDisplays, _inWorkParent );
				display.SetEntity( entity );
			}
			DeactiveDisplays( i, _inWorkDisplays );
		}

		private void ResizePool( List<EntityDisplay> pool, RectTransform parent )
		{
			for ( int i = 0; i < 200; i++ )
			{
				var display = Instantiate( _displayPrefab, parent );
				pool.Add( display );
			}
		}

		private EntityDisplay Display( int index, List<EntityDisplay> pool, RectTransform parent )
		{
			while ( index >= pool.Count )
			{
				ResizePool( pool, parent );
			}

			return pool[index];
		}

		private void DeactiveDisplays( int startIndex, List<EntityDisplay> pool )
		{
			Entity target = default;
			for ( int i = startIndex; i < pool.Count; i++ )
			{
				pool[i].SetEntity( target );
			}
		}
	}
}