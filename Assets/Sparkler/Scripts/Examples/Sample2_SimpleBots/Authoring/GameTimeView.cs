using Sparkler.Example.Components;

using Unity.Entities;

using UnityEngine;
using UnityEngine.UI;

using SF = UnityEngine.SerializeField;

namespace Sparkler.Examples.Authoring
{
	public class GameTimeView : MonoBehaviour
	{
		[SF] private Text _hoursText;
		[SF] private Text _daysText;
		[SF] private Text _monthsText;
		[SF] private Text _yearsText;
		[SF] private Slider _timeSpeedSlider;
		[SF] private Text _timeSpeedLabel;

		private EntityManager _entityManager;
		private EntityQuery _gameTimeQuery;
		private EntityQuery _timeSpeedQuery;

		private void Start()
		{
			var world = World.DefaultGameObjectInjectionWorld;
			_entityManager = world.EntityManager;

			_gameTimeQuery = _entityManager.CreateEntityQuery( ComponentType.ReadOnly<GameTime>() );
			_timeSpeedQuery = _entityManager.CreateEntityQuery( ComponentType.ReadOnly<TimeSpeed>() );
			_timeSpeedSlider.onValueChanged.AddListener( SpeedChanged );
			_timeSpeedSlider.value = 0;
			SpeedChanged( 0 );
		}

		private void SpeedChanged( float newValue )
		{
			using var entities = _timeSpeedQuery.ToEntityArray( Unity.Collections.Allocator.Temp );
			foreach ( var entity in entities )
			{
				_entityManager.SetComponentData( entity, new TimeSpeed() { Value = newValue } );
			}
			_timeSpeedLabel.text = newValue.ToString( "f2" );
		}

		private void LateUpdate()
		{
			if ( !GetGameTime( out var gameTime ) )
			{
				return;
			}

			_hoursText.text = $"Hour: {gameTime.Hours}";
			_daysText.text = $"Day: {gameTime.Days}";
			_monthsText.text = $"Month: {gameTime.Months}";
			_yearsText.text = $"Year: {gameTime.Years}";
		}

		private bool GetGameTime( out GameTime gameTime )
		{
			using var components = _gameTimeQuery.ToComponentDataArray<GameTime>( Unity.Collections.Allocator.Temp );
			if ( components.Length == 1 )
			{
				gameTime = components[0];
				return true;
			}
			gameTime = default;
			return false;
		}

		private void OnDestroy() => _timeSpeedSlider.onValueChanged.RemoveListener( SpeedChanged );
	}
}