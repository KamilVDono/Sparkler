using System;

using UnityEngine;

using XNode;

namespace FSM
{
	public abstract class FSMNode : Node
	{
		[SerializeField][HideInInspector] private string _name;

		public string Name
		{
			get => _name;
			set => _name = value;
		}

		public override Action<string> RenameAction => ( newName ) => Name = newName;
	}
}