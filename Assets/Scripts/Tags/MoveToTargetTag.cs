
using Unity.Entities;
using Unity.Transforms;

namespace Tags
{
	public struct MoveToTargetTag : IComponentData
	{
		public Translation Target;
	}
}