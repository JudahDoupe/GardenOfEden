using Assets.Scripts.Plants.Growth;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.Plants.Systems.Cleanup
{

    [UpdateInGroup(typeof(CleanupSystemGroup))]
    class UpdateMeshSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities
                .WithAll<NodeMesh>()
                .ForEach(
                    (ref NonUniformScale scale, in NodeMesh mesh, in Parent parent) =>
                    {
                        var query = GetComponentDataFromEntity<Node>(true);
                        if (query.HasComponent(parent.Value))
                        {
                            var node = query[parent.Value];
                            scale.Value = mesh.IsInternode ? new float3(node.InternodeRadius, node.InternodeRadius, -node.InternodeLength) : node.Size * 100;
                        }
                    })
                .WithName("UpdateNodeMesh")
                .ScheduleParallel();
        }

    }
}
