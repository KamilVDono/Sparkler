using Sparkler.Example.Components;

using System.Text;

using Unity.Entities;

using UnityEngine;
using UnityEngine.UI;

using SF = UnityEngine.SerializeField;

namespace Sparkler.Examples.Authoring
{
	public class EntityDisplay : MonoBehaviour
	{
		[SF] private Text _nameText;
		[SF] private Text _energyText;
		private Entity _target = default;
		private EntityManager _entityManager;
		private StringBuilder _nameBuilder = new StringBuilder();

		public void SetEntity( Entity target )
		{
			if ( _entityManager == default )
			{
				Start();
			}
			_target = target;
			Update();
		}

		private void Start()
		{
			_entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			gameObject.SetActive( false );
		}

		private void Update()
		{
			if ( !_entityManager.Exists( _target ) )
			{
				gameObject.SetActive( false );
			}
			else
			{
				gameObject.SetActive( true );
				_nameBuilder.Clear();
				_nameBuilder.Append( _entityManager.GetName( _target ) );

				if ( _entityManager.HasComponent<IsEating>( _target ) )
				{
					_nameBuilder.Append( " (eating)" );
				}
				if ( _entityManager.HasComponent<IsSleeping>( _target ) )
				{
					_nameBuilder.Append( " (sleeping)" );
				}
				if ( _entityManager.HasComponent<EnergyDestination>( _target ) )
				{
					_nameBuilder.Append( " (resting)" );
				}

				_nameText.text = _nameBuilder.ToString();
				_energyText.text = _entityManager.GetComponentData<EnergyStat>( _target ).Count.ToString( "F2" );
			}
		}
	}
}