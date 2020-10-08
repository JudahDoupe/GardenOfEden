using Unity.Entities;

namespace Assets.Scripts.Plants.ECS.Components
{
    public struct Internode : IComponentData
    {
        public Entity HeadNode { get; set; }
        public Entity TailNode { get; set; }
        public float Length;
        public float Radius;
    }

    public struct InternodeReference : IComponentData
    {
        public Entity Internode { get; set; }
    }
}
