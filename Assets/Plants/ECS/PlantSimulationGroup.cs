using Unity.Entities;
using Unity.Transforms;

[WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.Editor | WorldSystemFilterFlags.ThinClientSimulation)]
[UpdateInGroup(typeof(VariableRateSimulationSystemGroup))]
public partial class PlantSimulationGroup : ComponentSystemGroup { }