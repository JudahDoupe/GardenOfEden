using Assets.Scripts.Plants.ECS.Components;
using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Mathematics;

namespace Assets.Scripts.Plants.ECS.Services
{
    class InternodeMeshSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities
                .WithNone<LocalToParent>()
                .ForEach(
                    (ref Rotation rotation, ref Translation translation, ref NonUniformScale scale, in LocalToWorld l2w, in Internode internode) =>
                    {
                        var querry = GetComponentDataFromEntity<LocalToWorld>(true);
                        var headPos = querry[internode.HeadNode].Position;
                        var tailPos = querry[internode.TailNode].Position;
                        float3 vector = tailPos - headPos + new float3(0, 0, -0.000001f);

                        rotation.Value = UnityEngine.Quaternion.LookRotation(vector);
                        translation.Value = tailPos;
                        scale.Value.z = math.length(vector);
                    })
                .WithBurst()
                .WithName("UpdateInternodeMesh")
                .ScheduleParallel();
        }

    }
}
