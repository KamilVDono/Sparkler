using Sparkler.Example.Components;

using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace Sparkler.Example.Systems
{
	[DisableAutoCreation]
	[UpdateInGroup( typeof( TimeUpdateGroup ) )]
	public class TimeOfDaySystem : SystemBase
	{
		protected override void OnCreate()
		{
			base.OnCreate();
			EntityManager.CreateEntity( ComponentType.ReadOnly<TimeSpeed>(), new ComponentType( typeof( GameTime ) ) );
		}

		protected override void OnUpdate()
		{
			// -- TimeOfDaySystem_Main
			float deltaTime = Time.DeltaTime;

			Entities
				.WithName( "TimeOfDaySystem_Main" )
				.ForEach( ( ref GameTime gameTime, in TimeSpeed timeSpeed ) =>
			{
				float delta = deltaTime * timeSpeed.Value;
				gameTime.Elapsed += delta;
				gameTime.Delta = delta;
				var hours = (uint)math.floor( gameTime.Elapsed );
				gameTime.Hours = (byte)( hours % 24 );
				var days = hours / 24;
				gameTime.Days = (byte)( ( days % 30 ) + 1 );
				var months = days / 30;
				gameTime.Months = (byte)( ( months % 12 ) + 1 );
				gameTime.Years = (byte)( ( months / 12 ) + 1 );
			} )
			.Run();
		}
	}
}