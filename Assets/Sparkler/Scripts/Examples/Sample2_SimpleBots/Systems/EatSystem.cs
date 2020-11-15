using Sparkler.Example.Components;

using Unity.Entities;

using UnityEngine;

namespace Sparkler.Example.Systems
{
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
				if ( isEating.Value == 0 )
				{
					Debug.Log( $"Eating: {e}" );
				}
				isEating.Value = 1;
			} )
			.ScheduleParallel();
		}
	}
}