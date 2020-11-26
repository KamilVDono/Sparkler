using Sparkler.Example.Shooter.Components;

using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

using UnityEngine;

namespace Sparkler.Example.Shooter.Systems
{
	[UpdateInGroup( typeof( SimulationSystemGroup ), OrderFirst = true )]
	[UpdateBefore( typeof( BeginSimulationEntityCommandBufferSystem ) )]
	public class InputSystem : SystemBase
	{
		protected override void OnCreate()
		{
			base.OnCreate();
			var entity = EntityManager.CreateEntity( typeof( MouseLeftButton ), typeof( MouseMove ), typeof( KeyboardButtons ) );
		}

		protected override void OnUpdate()
		{
			// -- InputSystem_MouseButtons
			bool isLeftButtonPressed = Input.GetMouseButton(0);

			Entities
				.WithName( "InputSystem_MouseButtons" )
				.ForEach( ( ref MouseLeftButton mouseLeftButton ) =>
			{
				mouseLeftButton.State = mouseLeftButton.State.Update( isLeftButtonPressed );
			} )
			.Run();

			// -- InputSystem_MouseMove
			var vectorPosition = new float3(Camera.main.ScreenToWorldPoint( Input.mousePosition ));
			var mousePosition = new float2(vectorPosition.x, vectorPosition.z);

			Entities
				.WithName( "InputSystem_MouseMove" )
				.ForEach( ( ref MouseMove mouseMove ) =>
			{
				mouseMove.PreviousPosition = mouseMove.Position;
				mouseMove.Position = mousePosition;
			} )
			.Run();

			// -- InputSystem_KeyboardButtons
			bool isRPressed = Input.GetKey(KeyCode.R);

			Entities
				.WithName( "InputSystem_KeyboardButtons" )
				.ForEach( ( ref KeyboardButtons keyboardButtons ) =>
			{
				keyboardButtons.RState = keyboardButtons.RState.Update( isRPressed );
			} )
			.Run();
		}
	}
}