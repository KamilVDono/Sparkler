using UnityEngine;
using UnityEngine.UI;

using SF = UnityEngine.SerializeField;

namespace Sparkler.Examples.Authoring
{
	public class HideEntitiesPreviewView : MonoBehaviour
	{
		[SF] private Button _toggleButton;
		[SF] private Text _buttonLabel;
		[SF] private GameObject _charactersView;

		private void Start()
		{
			_toggleButton.onClick.AddListener( ToggleCharactersView );
			UpdateButton();
		}

		private void OnDestroy() => _toggleButton.onClick.RemoveListener( ToggleCharactersView );

		private void ToggleCharactersView()
		{
			var active = _charactersView.activeSelf;
			_charactersView.SetActive( !active );
			UpdateButton();
		}

		private void UpdateButton()
		{
			var active = _charactersView.activeSelf;
			_buttonLabel.text = active ? "Disable characters view" : "Enable characters view";
		}
	}
}