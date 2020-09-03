using FSM.AI.States.Components;

using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;

namespace FSM.AI.States.Systems
{
	public class TestSystem : SystemBase
	{
		private EntityQuery _craftingQuery;

		protected override void OnUpdate()
		{
			// -- IdleSystem_TransitionWalk
			Entities
				.WithName( "IdleSystem_TransitionWalk" )
					.WithoutBurst()
					.ForEach( ( Entity e, int entityInQueryIndex, ref Translation translation, in RenderMesh renderMesh, in TestTag testTag ) =>
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
						.ForEach( ( Entity e, int entityInQueryIndex, in OtherTag otherTag ) =>
						{
							//TODO: Make transition to one of the following state:
							//Crafting
						} )
					.ScheduleParallel();
		}
	}
}