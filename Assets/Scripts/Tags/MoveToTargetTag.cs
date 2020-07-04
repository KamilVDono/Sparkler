using FSM;

using Unity.Transforms;

namespace Tags
{
	public struct MoveToTargetTag : IStateTag
	{
		public Translation Target;
	}
}