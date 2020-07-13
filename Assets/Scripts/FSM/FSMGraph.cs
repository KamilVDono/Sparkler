using System.Linq;

using UnityEngine;

using XNode;

namespace FSM
{
	[CreateAssetMenu( menuName = "FSM/Graph", fileName = "FSMGraph" )]
	public class FSMGraph : NodeGraph
	{
		[SerializeField] private string _codeGenerationPath;
		[SerializeField] private string _namespace;
		public string CodeGenerationPath { get => _codeGenerationPath; set => _codeGenerationPath = value; }
		public string Namespace { get => _namespace; set => _namespace = value; }

		public void GenerateCode()
		{
			string template = LoadTemplate();
			var states = nodes.OfType<StateNode>();
		}

		private string LoadTemplate() => "";
	}
}