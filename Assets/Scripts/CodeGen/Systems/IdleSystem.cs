using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using FSM.AI.States.Components;


namespace FSM.AI.States.Systems
{
	public class IdleSystem : SystemBase
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

			Entities
				
				
				
				.ForEach( ( in IdleTag idleTag ) =>
			{
				//TODO: Implement state behavior
			} ).Schedule();

			
			var transitionCmdBuffer = _endSimulationCmdBuffer.CreateCommandBuffer();
			Entities
				.WithName( "Transition" )
				
				
				
				.ForEach( ( Entity e, in IdleTag idleTag ) =>
			{
				//TODO: Make transition to one of the following state:
				//WalkSystem, EatSystem
			} ).Schedule();

			_endSimulationCmdBuffer.AddJobHandleForProducer( this.Dependency );
			
		}
	}
}