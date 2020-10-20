using Unity.Entities;
using Unity.Mathematics;

namespace Assets.Scripts.Plants.ECS.Components
{
    public struct Node : IComponentData
    {
        public float3 Size;
    }

    public struct Internode : IComponentData
    {
        public Entity Mesh;
        public float Length;
        public float Radius;
    }
}
