using Sparkler.Example.Components;

using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace Sparkler.Example.Systems
{
	[UpdateInGroup( typeof( TimeUpdateGroup ) )]
	public class TimeOfDaySystem : SystemBase
	{
		public float Speedup = 1.5f;

		protected override void OnCreate()
		{
			base.OnCreate();
			EntityManager.CreateEntity( new ComponentType( typeof( GameTime ) ) );
		}

		protected override void OnUpdate()
		{
			// -- TimeOfDaySystem_Main
			float deltaTime = Time.DeltaTime * Speedup;

			Entities
				.WithName( "TimeOfDaySystem_Main" )
				.ForEach( ( ref GameTime gameTime ) =>
			{
				gameTime.Elapsed += deltaTime;
				gameTime.Delta = deltaTime;
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