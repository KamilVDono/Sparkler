using Sparkler.Example.Shooter.Components;

using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Sparkler.Example.Shooter.Systems
{
	public class PlayerRotationSystem : SystemBase
	{
		private static readonly float3 UP = new float3(0, 1, 0);

		protected override void OnCreate()
		{
			base.OnCreate();
			RequireSingletonForUpdate<MouseMove>();
		}

		protected override void OnUpdate()
		{
			// -- PlayerRotationSystem_Main

			var mouseMove = GetSingleton<MouseMove>();

			Entities
				.WithName( "PlayerRotationSystem_Main" )
				.WithAll<PlayerTag>()
				.ForEach( ( ref Rotation rotation ) =>
			{
				var direction = math.normalize( mouseMove.Position);
				rotation.Value = quaternion.LookRotation( new float3( direction.x, 0, direction.y ), UP );
			} )
			.Schedule();
		}
	}
}