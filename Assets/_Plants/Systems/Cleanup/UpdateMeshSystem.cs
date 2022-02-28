using Assets.Scripts.Plants.Growth;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Assets.Plants.Systems.Cleanup
{

    [UpdateInGroup(typeof(CleanupSystemGroup))]
    class UpdateMeshSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities
                .ForEach(
                    (ref NonUniformScale scale, ref Rotation rotation, in NodeMesh mesh) =>
                    {
                        var nodeQuery = GetComponentDataFromEntity<Node>(true);
                        if (nodeQuery.HasComponent(mesh.Node))
                        {
                            var node = nodeQuery[mesh.Node];
                            if (mesh.IsInternode)
                            {
                                var up = GetComponentDataFromEntity<LocalToParent>(true)[mesh.Node].Up;
                                var forward = math.normalize(GetComponentDataFromEntity<Translation>(true)[mesh.Node].Value);
                                rotation.Value = quaternion.LookRotation(forward, up);
                                scale.Value = new float3(node.InternodeRadius, node.InternodeRadius, -node.InternodeLength);
                            }
                            else
                            {
                                scale.Value = node.Size;
                            }
                        }
                    })
                .WithName("UpdateNodeMesh")
                .ScheduleParallel();
        }

    }
}
