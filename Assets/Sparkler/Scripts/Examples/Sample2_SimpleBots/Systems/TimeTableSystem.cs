using Sparkler.Example.Components;

using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Sparkler.Example.Systems
{
	[UpdateInGroup( typeof( TimeUpdateGroup ) )]
	[UpdateAfter( typeof( TimeOfDaySystem ) )]
	public class TimeTableSystem : SystemBase
	{
		private BeginSimulationEntityCommandBufferSystem _beginSimulationCmdBuffer;
		private EntityQuery _sleepQuery;
		private EntityQuery _removeEnergyDestination;
		private EntityQuery _eatQuery;
		private ComponentType _isSleepType = typeof(IsSleeping);
		private ComponentType _isEatingType = typeof(IsEating);
		private ComponentType _energyDestinationType = typeof(EnergyDestination);
		private bool _regeneratedEnergy = false;

		protected override void OnCreate()
		{
			base.OnCreate();
			_beginSimulationCmdBuffer = World.GetExistingSystem<BeginSimulationEntityCommandBufferSystem>();
			var sleepQuery = new EntityQueryDesc
			{
				All = new ComponentType[]{typeof( CharacterTag )},
				None = new ComponentType[]{ _isSleepType }
			};
			_sleepQuery = GetEntityQuery( sleepQuery );

			var eatQuery = new EntityQueryDesc
			{
				All = new ComponentType[]{typeof( CharacterTag )},
				None = new ComponentType[]{ _isEatingType }
			};
			_eatQuery = GetEntityQuery( eatQuery );

			_removeEnergyDestination = GetEntityQuery( _energyDestinationType );
		}

		protected override void OnUpdate()
		{
			var gameTime = GetSingleton<GameTime>();
			var hour = gameTime.Hours;

			bool shouldAddDependencies = false;

			if ( hour == 5 )
			{
				_regeneratedEnergy = false;
			}
			else if ( !_regeneratedEnergy && hour == 6 )
			{
				EntityCommandBuffer entityCmdBuffer = new EntityCommandBuffer(Allocator.TempJob, PlaybackPolicy.SinglePlayback);
				var cmdBuffer = entityCmdBuffer.AsParallelWriter();
				var handle = Entities
					.ForEach( ( Entity e, int entityInQueryIndex, ref EnergyStat energyStat ) =>
					{
						energyStat.Count = 100;
						cmdBuffer.RemoveComponent<IsSleeping>(entityInQueryIndex, e);
					} )
					.ScheduleParallel( Dependency );
				_regeneratedEnergy = true;
				handle.Complete();
				entityCmdBuffer.Playback( EntityManager );
				entityCmdBuffer.Dispose();

				Dependency = default;
			}

			if ( hour >= 23 || hour < 6 )
			{
				EntityManager.AddComponent( _sleepQuery, _isSleepType );
			}
			else if ( ( hour >= 6 && hour < 7 ) || ( hour >= 20 && hour < 21 ) )
			{
				EntityManager.AddComponent( _eatQuery, _isEatingType );
			}
			else if ( hour == 7 )
			{
				var cmdBuffer = _beginSimulationCmdBuffer.CreateCommandBuffer().AsParallelWriter();
				var isEatingType = _isEatingType;
				InPlace filter = new InPlace(){Place = Place.Home};

				Entities
					.WithNone<Heading>()
					.WithAll<IsEating>()
					.WithSharedComponentFilter( filter )
					.WithoutBurst()
					.ForEach( ( Entity e, int entityInQueryIndex ) =>
					{
						cmdBuffer.AddComponent( entityInQueryIndex, e, new Heading() { Place = Place.Mine } );
						cmdBuffer.SetSharedComponent( entityInQueryIndex, e, new InPlace() { Place = Place.InTravel } );
						cmdBuffer.RemoveComponent( entityInQueryIndex, e, isEatingType );
					} )
					.ScheduleParallel();
				shouldAddDependencies = true;
			}
			else if ( hour == 18 )
			{
				var mainCmdBuffer = _beginSimulationCmdBuffer.CreateCommandBuffer().AsParallelWriter();
				InPlace filter = new InPlace(){Place = Place.Mine};

				EntityManager.RemoveComponent( _removeEnergyDestination, _energyDestinationType );

				Entities
					.WithNone<Heading>()
					.WithSharedComponentFilter( filter )
					.WithoutBurst()
					.ForEach( ( Entity e, int entityInQueryIndex ) =>
					{
						mainCmdBuffer.AddComponent( entityInQueryIndex, e, new Heading() { Place = Place.Home } );
						mainCmdBuffer.SetSharedComponent( entityInQueryIndex, e, new InPlace() { Place = Place.InTravel } );
					} )
					.ScheduleParallel();
				shouldAddDependencies = true;
			}
			else if ( hour == 21 )
			{
				var cmdBuffer = _beginSimulationCmdBuffer.CreateCommandBuffer().AsParallelWriter();
				var isEatingType = _isEatingType;
				Entities
					.WithAll<IsEating>()
					.ForEach( ( Entity e, int entityInQueryIndex ) =>
					{
						cmdBuffer.RemoveComponent( entityInQueryIndex, e, isEatingType );
					} )
					.ScheduleParallel();
				shouldAddDependencies = true;
			}

			if ( shouldAddDependencies )
			{
				_beginSimulationCmdBuffer.AddJobHandleForProducer( Dependency );
			}
		}
	}
}