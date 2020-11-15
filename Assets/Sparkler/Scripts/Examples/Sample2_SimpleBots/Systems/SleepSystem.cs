using Sparkler.Example.Components;

using Unity.Entities;

namespace Sparkler.Example.Systems
{
	public class SleepSystem : SystemBase
	{
		protected override void OnCreate() => base.OnCreate();

		protected override void OnUpdate()
		{
			// -- SleepSystem_Sleeping
			InPlace inPlace = new InPlace{ Place = Place.Home, };
			Entities
				.WithName( "SleepSystem_Sleeping" )
				.WithSharedComponentFilter( inPlace )
				.ForEach( ( Entity e, ref IsSleeping isSleeping ) =>
			{
			} )
			.ScheduleParallel();
		}
	}
}