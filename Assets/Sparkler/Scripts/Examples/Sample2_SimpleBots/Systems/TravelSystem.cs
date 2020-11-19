using Sparkler.Example.Components;

using Unity.Entities;

namespace Sparkler.Example.Systems
{
	[DisableAutoCreation]
	public class TravelSystem : SystemBase
	{
		private EndSimulationEntityCommandBufferSystem _endSimulationCmdBuffer;

		protected override void OnCreate()
		{
			base.OnCreate();
			_endSimulationCmdBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
		}

		protected override void OnUpdate()
		{
			// -- TravelSystem_Progression

			GameTime gameTime = GetSingleton<GameTime>();
			float deltaTime = gameTime.Delta;
			var cmdBuffer = _endSimulationCmdBuffer.CreateCommandBuffer().AsParallelWriter();
			Entities
				.WithName( "TravelSystem_Progression" )
				.WithoutBurst()
				.ForEach( ( Entity e, int entityInQueryIndex, ref Heading heading ) =>
			{
				heading.Progression += deltaTime;

				if ( heading.Progression >= 1 )
				{
					cmdBuffer.SetSharedComponent<InPlace>( entityInQueryIndex, e, new InPlace() { Place = heading.Place } );
					cmdBuffer.RemoveComponent<Heading>( entityInQueryIndex, e );
				}
			} )
			.ScheduleParallel();

			_endSimulationCmdBuffer.AddJobHandleForProducer( this.Dependency );
		}
	}
}