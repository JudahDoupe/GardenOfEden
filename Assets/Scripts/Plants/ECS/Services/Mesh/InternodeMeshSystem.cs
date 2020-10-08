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
                    (ref Rotation rotation, ref Translation translation, ref NonUniformScale scale, ref Internode internode, in LocalToWorld l2w) =>
                    {
                        var querry = GetComponentDataFromEntity<LocalToWorld>(true);
                        var headPos = querry[internode.HeadNode].Position;
                        var tailPos = querry[internode.TailNode].Position;
                        float3 vector = tailPos - headPos + new float3(0, 0, -0.000001f);

                        rotation.Value = UnityEngine.Quaternion.LookRotation(vector);
                        translation.Value = tailPos;
                        internode.Length = math.length(vector);
                        scale.Value = new float3(internode.Radius, internode.Radius, internode.Length);
                    })
                .WithName("UpdateInternodeMesh")
                .ScheduleParallel();
        }

    }
}
