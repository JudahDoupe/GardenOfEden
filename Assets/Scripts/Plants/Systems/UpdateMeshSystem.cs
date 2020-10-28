using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.Scripts.Plants.Systems
{
    class UpdateMeshSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities
                .WithNone<Dormant, LocalToParent>()
                .ForEach(
                    (ref Rotation rotation, ref Translation translation, ref NonUniformScale scale, ref InternodeReference internodeRef, in LocalToWorld l2w) =>
                    {
                        var l2wQuery = GetComponentDataFromEntity<LocalToWorld>(true);
                        var parentQuery = GetComponentDataFromEntity<Parent>(true);
                        var internodeQuery = GetComponentDataFromEntity<Internode>(true);

                        var internode = internodeQuery[internodeRef.Entity];
                        var headPos = l2wQuery[internodeRef.Entity].Position;
                        var tailPos = l2wQuery[parentQuery[internodeRef.Entity].Value].Position;
                        float3 vector = headPos - tailPos + new float3(0,0,0.00001f);

                        translation.Value = headPos;
                        rotation.Value = UnityEngine.Quaternion.LookRotation(vector);
                        scale.Value = new float3(internode.Radius, internode.Radius, internode.Length);
                    })
                .WithName("UpdateInternodeMesh")
                .ScheduleParallel();

            Entities
                .WithNone<Dormant, LocalToParent>()
                .ForEach(
                    (ref Rotation rotation, ref Translation translation, ref NonUniformScale scale, ref NodeReference nodeRef, in LocalToWorld l2w) =>
                    {
                        var l2wQuery = GetComponentDataFromEntity<LocalToWorld>(true);
                        var nodeQuery = GetComponentDataFromEntity<Node>(true);

                        var node = nodeQuery[nodeRef.Entity];
                        translation.Value = l2wQuery[nodeRef.Entity].Position;
                        rotation.Value = l2wQuery[nodeRef.Entity].Rotation;
                        scale.Value = node.Size;
                    })
                .WithName("UpdateNodeMesh")
                .ScheduleParallel();
        }

    }
}
