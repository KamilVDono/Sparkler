using Unity.Entities;

namespace Sparkler.Example.Systems
{
	[UpdateInGroup( typeof( SimulationSystemGroup ), OrderFirst = true )]
	[UpdateBefore( typeof( BeginSimulationEntityCommandBufferSystem ) )]
	public class TimeUpdateGroup : ComponentSystemGroup
	{
	}
}