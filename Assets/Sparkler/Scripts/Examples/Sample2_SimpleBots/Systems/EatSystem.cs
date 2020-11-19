using Sparkler.Example.Components;

using Unity.Entities;

namespace Sparkler.Example.Systems
{
	[DisableAutoCreation]
	public class EatSystem : SystemBase
	{
		protected override void OnCreate() => base.OnCreate();

		protected override void OnUpdate()
		{
			// -- EatSystem_Eating
			InPlace inPlace = new InPlace{ Place = Place.Home, };
			Entities
				.WithName( "EatSystem_Eating" )
				.WithSharedComponentFilter( inPlace )
				.ForEach( ( Entity e, ref IsEating isEating ) =>
			{
			} )
			.ScheduleParallel();
		}
	}
}