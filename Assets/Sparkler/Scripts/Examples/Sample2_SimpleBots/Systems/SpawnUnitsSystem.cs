using Sparkler.Example.Components;

using System;
using System.Collections.Generic;
using System.Linq;

using Unity.Collections;
using Unity.Entities;

namespace Sparkler.Example.Systems
{
	[DisableAutoCreation]
	public class SpawnUnitsSystem : SystemBase
	{
		private static readonly string[] Names = new string[]{
			"Bill", "Tob", "Bob", "Alex", "Marie", "Ola", "Ala", "Olek", "Max", "Rex", "Tik", "Rick", "John", "Nina", "Aga"
		};

		private EntityArchetype _archetype;
		private Dictionary<string, int> _countByName;

		protected override void OnCreate()
		{
			base.OnCreate();

			_archetype = EntityManager.CreateArchetype( typeof( CharacterTag ), typeof( InPlace ), typeof( WorkStat ), typeof( EnergyStat ), typeof( EnergyRegeneration ) );

			_countByName = Names.ToDictionary( n => n, n => -1 );
		}

		protected override void OnUpdate()
		{
			// -- SpawnUnitsSystem_Main
			var rng = new Unity.Mathematics.Random((uint)(DateTime.Now.ToString().GetHashCode()));

			Entities
				.WithStructuralChanges()
				.WithoutBurst()
				.WithName( "SpawnUnitsSystem_Main" )
				.ForEach( ( Entity e, ref SpawnRequest spawnRequest ) =>
			{
				var count = spawnRequest.Count;
				var entities = EntityManager.CreateEntity( _archetype, (int)count, Allocator.Temp );

				foreach ( var entity in entities )
				{
					string name = Names[rng.NextInt( 0, Names.Length-1 )];
					var nameCount = ++_countByName[name];
					_countByName[name] = nameCount;

					EntityManager.SetName( entity, name + nameCount );
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