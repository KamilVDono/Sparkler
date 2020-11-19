using Sparkler.Example.Components;

using Unity.Entities;

using UnityEngine;
using UnityEngine.UI;

using SF = UnityEngine.SerializeField;

namespace Sparkler.Examples.Authoring
{
	public class SpawnView : MonoBehaviour
	{
		[SF] private Text _spawnedText;
		[SF] private InputField _toSpawnInput;
		[SF] private Button _spawnButton;
		private EntityManager _entityManager;
		private EntityQuery _spawnedQuery;

		private void Start()
		{
			var world = World.DefaultGameObjectInjectionWorld;
			_entityManager = world.EntityManager;
			_spawnedQuery = _entityManager.CreateEntityQuery( ComponentType.ReadOnly<CharacterTag>() );

			_spawnButton.onClick.AddListener( SpawnUnits );
			UpdateSpawned();
		}

		private void Update() => UpdateSpawned();

		private void UpdateSpawned()
		{
			var spawnedCount = _spawnedQuery.CalculateEntityCount();
			_spawnedText.text = $"Spawned:\n{spawnedCount}";
		}

		private void SpawnUnits()
		{
			if ( uint.TryParse( _toSpawnInput.text, out uint spawnCount ) && spawnCount > 0 )
			{
				var request = _entityManager.CreateEntity(new ComponentType(typeof(SpawnRequest)));
				_entityManager.SetComponentData( request, new SpawnRequest() { Count = spawnCount } );
			}
			else
			{
				_toSpawnInput.text = "0";
			}
		}

		private void OnDestroy() => _spawnButton.onClick.RemoveListener( SpawnUnits );
	}
}