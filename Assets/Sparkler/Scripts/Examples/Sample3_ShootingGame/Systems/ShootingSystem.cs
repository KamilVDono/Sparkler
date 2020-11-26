using Sparkler.Example.Shooter.Components;

using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Sparkler.Example.Shooter.Systems
{
	public class ShootingSystem : SystemBase
	{
		private EndSimulationEntityCommandBufferSystem _endSimulationCmdBuffer;

		protected override void OnCreate()
		{
			base.OnCreate();
			_endSimulationCmdBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
			RequireSingletonForUpdate<KeyboardButtons>();
		}

		protected override void OnUpdate()
		{
			// -- ShootingSystem_Main
			var mouseState = GetSingleton<MouseLeftButton>();

			if ( mouseState.State == ButtonState.StartPressing || mouseState.State == ButtonState.Hold )
			{
				var elapsedTime = Time.ElapsedTime;
				var shootCmdBuffer = _endSimulationCmdBuffer.CreateCommandBuffer();
				var rng = new Random((uint)elapsedTime.GetHashCode());

				Entities
				.WithName( "ShootingSystem_Main" )
				.WithNone<ReloadingProgress>()
				.ForEach( ( ref LastShootTime lastShootTime, ref AmmoMagazine ammoMagazine, in AmmoPrefab ammoPrefab, in ShootingConfig shootingConfig, in Rotation rotation ) =>
				{
					if ( elapsedTime - lastShootTime.Value > ( 1f / shootingConfig.ShootsPerSecond ) && ammoMagazine.CurrentMagazine > 0 )
					{
						lastShootTime.Value = elapsedTime;
						for ( int i = 0; i < shootingConfig.AmmoBurst; i++ )
						{
							// TODO: Get angle and calculate position

							//float3 forward =

							//quaternion.LookRotationSafe()

							var e = shootCmdBuffer.Instantiate( ammoPrefab.Prefab );

							// TODO: Set position on spawned entity
							//shootCmdBuffer.SetComponent( e, new Translation() { Value = new float3( x, 1, z ) } );
						}
					}
				} )
				.Schedule();
			}

			// -- ShootingSystem_Main
			var keyboardButtons = GetSingleton<KeyboardButtons>();

			if ( keyboardButtons.RState == ButtonState.StartPressing )
			{
				var startReloadCmdBuffer = _endSimulationCmdBuffer.CreateCommandBuffer();
				Entities
					.WithName( "ShootingSystem_StartReloading" )
					.WithNone<ReloadingProgress>()
					.WithAll<AmmoMagazine, ReloadSpeed>()
					.ForEach( ( Entity entity ) =>
				{
					startReloadCmdBuffer.AddComponent<ReloadingProgress>( entity );
				} )
				.Schedule();
			}

			// -- ShootingSystem_Reloading
			var reloadingCmdBuffer = _endSimulationCmdBuffer.CreateCommandBuffer();
			float deltaTime = Time.DeltaTime;

			Entities
				.WithName( "ShootingSystem_Reloading" )
				.ForEach( ( Entity entity, ref ReloadingProgress reloadingProgress, ref AmmoMagazine ammoMagazine, in ReloadSpeed reloadSpeed ) =>
			{
				reloadingProgress.Value += deltaTime * reloadSpeed.Value;
				if ( reloadingProgress.Value >= 1 )
				{
					reloadingCmdBuffer.RemoveComponent<ReloadingProgress>( entity );
					ammoMagazine.CurrentMagazine = ammoMagazine.MagazineSize;
				}
			} )
			.Schedule();
			_endSimulationCmdBuffer.AddJobHandleForProducer( this.Dependency );
		}
	}
}