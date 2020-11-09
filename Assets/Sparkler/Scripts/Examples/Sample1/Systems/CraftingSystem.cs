using Sparkler.AI.States.Components;

using Unity.Entities;
using Unity.Jobs;

namespace FSM.AI.States.Systems
{
	public class CraftingSystem : SystemBase
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
			// -- CraftingSystem_Main
			var mainCmdBuffer = _endSimulationCmdBuffer.CreateCommandBuffer().AsParallelWriter();
			Entities
				.WithName( "CraftingSystem_Main" )
				.ForEach( ( Entity entity, int entityInQueryIndex, ref CraftingRecepie craftingRecepie, in WorkSpeed workSpeed ) =>
			{
				//TODO: Implement state behavior
			} )
			.ScheduleParallel();
			// -- CraftingSystem_TransitionBuilding
			var transitionbuildingCmdBuffer = _endSimulationCmdBuffer.CreateCommandBuffer().AsParallelWriter();
			PrimitiveTag primitiveTag = new PrimitiveTag{ IntVal = 2, ByteVal = 0, ULongVal = 0, EnumVal = PrimitiveTag.Enum.Val1, FlagEnumVal = (PrimitiveTag.FlagEnum)(-1), };
			Entities
				.WithName( "CraftingSystem_TransitionBuilding" )
				.WithSharedComponentFilter( primitiveTag )
				.ForEach( ( Entity entity, int entityInQueryIndex, ref CraftingRecepie craftingRecepie, in WorkSpeed workSpeed ) =>
			{
				//TODO: Make transition to one of the following state:
				//Building
			} )
			.ScheduleParallel();
			_endSimulationCmdBuffer.AddJobHandleForProducer( this.Dependency );
		}
	}
}