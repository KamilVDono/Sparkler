using Sparkler.Example.Components;

using System;

using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Sparkler.Example.Systems
{
	public class SpawnUnitsSystem : SystemBase
	{
		private EntityArchetype _archetype;

		protected override void OnCreate()
		{
			base.OnCreate();

			_archetype = EntityManager.CreateArchetype( typeof( CharacterTag ), typeof( InPlace ), typeof( WorkStat ), typeof( EnergyStat ), typeof( EnergyRegeneration ) );

			var e = EntityManager.CreateEntity( typeof( SpawnRequest ));
			EntityManager.SetComponentData( e, new SpawnRequest() { Count = 50 } );
		}

		protected override void OnUpdate()
		{
			// -- SpawnUnitsSystem_Main
			var rng = new Unity.Mathematics.Random((uint)(DateTime.Now.ToString().GetHashCode()));

			Entities
				.WithStructuralChanges()
				.WithName( "SpawnUnitsSystem_Main" )
				.ForEach( ( Entity e, ref SpawnRequest spawnRequest ) =>
			{
				var count = spawnRequest.Count;
				var entities = EntityManager.CreateEntity( _archetype, (int)count, Allocator.Temp );

				foreach ( var entity in entities )
				{
					EntityManager.SetSharedComponentData( entity, new InPlace() { Place = Place.Home } );
					EntityManager.SetComponentData( entity, new WorkStat() { Speed = rng.NextFloat( 5f, 7f ) } );
					EntityManager.SetComponentData( entity, new EnergyStat() { Count = 100, FatiguePressure = rng.NextFloat( 15f, 25f ) } );
					EntityManager.SetComponentData( entity, new EnergyRegeneration() { Value = rng.NextFloat( 20f, 30f ) } );
				}

				entities.Dispose();
				EntityManager.DestroyEntity( e );
			} )
			.Run();
		}
	}
}