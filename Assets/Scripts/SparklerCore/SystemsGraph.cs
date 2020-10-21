using UnityEngine;

using XNode;

namespace Sparkler
{
	[CreateAssetMenu( menuName = "Sparkler/SystemsGraph", fileName = "SystemsGraph" )]
	public class SystemsGraph : NodeGraph
	{
		[SerializeField] private string _codeGenerationPath;
		[SerializeField] private string _namespace;
		[SerializeField] private bool _stateEditing;
		public string CodeGenerationPath { get => _codeGenerationPath; set => _codeGenerationPath = value; }
		public string Namespace { get => _namespace; set => _namespace = value; }
		public bool StateEditing { get => _stateEditing; set => _stateEditing = value; }
	}
}