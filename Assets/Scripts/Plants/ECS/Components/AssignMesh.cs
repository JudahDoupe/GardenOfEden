using Unity.Collections;
using Unity.Entities;

namespace Assets.Scripts.Plants.ECS.Components
{
    public struct AssignMesh : IComponentData
    {
        public FixedString64 MeshName { get; set; }
    }
}
