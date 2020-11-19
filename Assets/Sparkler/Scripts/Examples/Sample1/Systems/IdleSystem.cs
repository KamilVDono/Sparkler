using Sparkler.AI.States.Components;

using Unity.Entities;
using Unity.Jobs;
using Unity.Rendering;
namespace Sparkler.AI.States.Systems
{
	[DisableAutoCreation]
	public class IdleSystem : SystemBase
	{
		private EndSimulationEntityCommandBufferSystem _endSimulationCmdBuffer;
		private EntityQuery _mainQuery;
		private EntityQuery _craftingQuery;
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
			// -- IdleSystem_Main
			PrimitiveTag primitiveTag = new PrimitiveTag{ IntVal = 0, ByteVal = 0, ULongVal = 0, EnumVal = PrimitiveTag.Enum.Val1, FlagEnumVal = (PrimitiveTag.FlagEnum)(9), };
			Entities
				.WithName( "IdleSystem_Main" )
				.WithoutBurst()
				.WithSharedComponentFilter( primitiveTag )
				.WithStoreEntityQueryInField( ref _mainQuery )
				.WithAny<OtherTag>()
				.ForEach( ( in RenderMesh renderMesh, in TestTag testTag ) =>
			{
				//TODO: Implement state behavior
			} )
			.Run();
			// -- IdleSystem_TransitionWalk
			Entities
				.WithName( "IdleSystem_TransitionWalk" )
				.WithoutBurst()
				.ForEach( ( in RenderMesh renderMesh, in TestTag testTag ) =>
			{
				//TODO: Make transition to one of the following state:
				//Walk
			} )
			.Run();
			// -- IdleSystem_TransitionCrafting
			Entities
				.WithName( "IdleSystem_TransitionCrafting" )
				.WithStoreEntityQueryInField( ref _craftingQuery )
				.WithAll<WorkSpeed>()
				.ForEach( ( in OtherTag otherTag ) =>
			{
				//TODO: Make transition to one of the following state:
				//Crafting
			} )
			.ScheduleParallel();
			_endSimulationCmdBuffer.AddJobHandleForProducer( this.Dependency );
		}
	}
}