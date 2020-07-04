using FSM;

using Unity.Entities;

namespace Tags
{
	public struct EatTag : IStateTag
	{
		public Entity Food;
	}
}