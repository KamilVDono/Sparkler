using Sparkler.Example.Components;

using Unity.Entities;

using UnityEngine;

namespace Sparkler.Example.Systems
{
	public class WorkSystem : SystemBase
	{
		private EndSimulationEntityCommandBufferSystem _endSimulationCmdBuffer;

		protected override void OnCreate()
		{
			base.OnCreate();
			_endSimulationCmdBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
		}

		protected override void OnUpdate()
		{
			// Assign values to local variables captured in your job here, so that it has everything it
			// needs to do its work when it runs later. For example, float deltaTime = Time.DeltaTime;
			// This declares a new kind of job, which is a unit of work to do. The job is declared as an
			// Entities.ForEach with the target components as parameters, meaning it will process all
			// entities in the world that have both Translation and Rotation components. Change it to
			// process the component types you want.
			// -- WorkSystem_Main
			var mainCmdBuffer = _endSimulationCmdBuffer.CreateCommandBuffer().AsParallelWriter();
			InPlace inPlace = new InPlace{ Place = Place.Mine, };

			GameTime gameTime = GetSingleton<GameTime>();
			float deltaTime = gameTime.Delta;
			Entities
				.WithName( "WorkSystem_Main" )
				.WithSharedComponentFilter( inPlace )
				.WithNone<EnergyDestination>()
				.ForEach( ( Entity entity, int entityInQueryIndex, ref EnergyStat energyStat, in WorkStat workStat ) =>
			{
				if ( energyStat.Count == 100 )
				{
					Debug.Log( $"Start mining: {entity}" );
				}
				energyStat.Count -= deltaTime * workStat.Speed * energyStat.FatiguePressure;

				if ( energyStat.Count <= 10 )
				{
					mainCmdBuffer.AddComponent<EnergyDestination>( entityInQueryIndex, entity, new EnergyDestination() { Value = 25 } );
				}
			} )
				.ScheduleParallel();
			_endSimulationCmdBuffer.AddJobHandleForProducer( this.Dependency );
		}
	}
}