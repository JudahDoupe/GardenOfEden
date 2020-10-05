using Unity.Collections;
using Unity.Entities;

namespace Assets.Scripts.Plants.ECS.Components
{
    public struct EnergyStore : IComponentData
    {
        public float Quantity { get; set; }
        public float Capacity { get; set; }
        public float Preassure { get; set; }
    }
}
