using Unity.Entities;
using Unity.Transforms;

[WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.Editor | WorldSystemFilterFlags.ThinClientSimulation)]
[UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
[UpdateBefore(typeof(TransformSystemGroup))]
public class PlantSimulationGroup : ComponentSystemGroup { }

[WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.Editor)]
[UpdateInGroup(typeof(PlantSimulationGroup), OrderFirst = true)]
public class BeginPlantSimulationEntityCommandBufferSystem : EntityCommandBufferSystem { }

[WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.Editor)]
[UpdateInGroup(typeof(PlantSimulationGroup), OrderLast = true)]
public class EndPlantSimulationEntityCommandBufferSystem : EntityCommandBufferSystem { }