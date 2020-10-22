using System.ComponentModel;
using Unity.Entities;
using Unity.Mathematics;

namespace Assets.Scripts.Plants.ECS.Components
{
    public struct Node : IComponentData
    {
        public float3 Size;
        public float Volume => Size.x * Size.y * Size.z * 1.333f * math.PI;
    }

    public struct Internode : IComponentData
    {
        public float Length;
        public float Radius;
        public float Volume => Length * Radius * Radius * math.PI;
    }
}
