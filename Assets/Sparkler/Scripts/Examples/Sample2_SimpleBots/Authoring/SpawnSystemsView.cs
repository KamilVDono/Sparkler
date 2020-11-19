using Sparkler.Example.Systems;

using System;

using Unity.Entities;

using UnityEngine;
using UnityEngine.UI;

using SF = UnityEngine.SerializeField;

namespace Sparkler.Examples.Authoring
{
	public class SpawnSystemsView : MonoBehaviour
	{
		private static readonly Type[] SYSTEM_TYPES = new Type[]{
			typeof(EatSystem), typeof(RelaxSystem), typeof(SleepSystem), typeof(SpawnUnitsSystem), typeof(TravelSystem), typeof(WorkSystem)
		};

		private static readonly Type[] TIME_SYSTEM_TYPES = new Type[]{
			typeof(TimeOfDaySystem), typeof(TimeTableSystem)
		};

		[SF] private Button _startButton;
		[SF] private Text _label;

		public void Start()
		{
			_startButton.onClick.AddListener( StartSystems );
			_label.text = "Not running";
		}

		private void StartSystems()
		{
			var world = World.DefaultGameObjectInjectionWorld;
			var updateGroup = world.GetExistingSystem<SimulationSystemGroup>();
			foreach ( var system in SYSTEM_TYPES )
			{
				var createdSystem = world.CreateSystem( system );
				updateGroup.AddSystemToUpdateList( createdSystem );
				createdSystem.Enabled = true;
			}
			var timeGroup = world.GetExistingSystem<TimeUpdateGroup>();
			foreach ( var system in TIME_SYSTEM_TYPES )
			{
				var createdSystem = world.CreateSystem( system );
				timeGroup.AddSystemToUpdateList( createdSystem );
				createdSystem.Enabled = true;
			}

			timeGroup.SortSystems();
			updateGroup.SortSystems();

			_startButton.onClick.RemoveListener( StartSystems );
			_startButton.interactable = false;
			_label.text = "Running";
		}

		private void OnDestroy() => _startButton.onClick.RemoveListener( StartSystems );
	}
}