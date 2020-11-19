using Sparkler.Example.Components;

using Unity.Entities;

namespace Sparkler.Example.Systems
{
	[DisableAutoCreation]
	public class RelaxSystem : SystemBase
	{
		private EndSimulationEntityCommandBufferSystem _endSimulationCmdBuffer;

		protected override void OnCreate()
		{
			base.OnCreate();
			_endSimulationCmdBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
		}

		protected override void OnUpdate()
		{
			// -- RelaxSystem_Main
			var mainCmdBuffer = _endSimulationCmdBuffer.CreateCommandBuffer().AsParallelWriter();
			InPlace inPlace = new InPlace{ Place = Place.Mine, };

			GameTime gameTime = GetSingleton<GameTime>();
			float deltaTime = gameTime.Delta;
			Entities
				.WithName( "RelaxSystem_Main" )
				.WithSharedComponentFilter( inPlace )
				.ForEach( ( Entity entity, int entityInQueryIndex, ref EnergyStat energyStat, in EnergyRegeneration energyRegeneration, in EnergyDestination energyDestination ) =>
			{
				energyStat.Count += energyRegeneration.Value * deltaTime;
				if ( energyStat.Count >= energyDestination.Value )
				{
					mainCmdBuffer.RemoveComponent<EnergyDestination>( entityInQueryIndex, entity );
				}
			} )
			.ScheduleParallel();
			_endSimulationCmdBuffer.AddJobHandleForProducer( this.Dependency );
		}
	}
}