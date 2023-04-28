using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace Framework.Jobs
{
    [BurstCompile]
    public partial struct RecalculateLocalToWorld : IJobEntity
    {
        [ReadOnly] public ComponentLookup<LocalTransform> LocalTransformLookup;
        [ReadOnly] public ComponentLookup<Parent> ParentLookup;
        [ReadOnly] public ComponentLookup<PostTransformMatrix> PostTransformMatrixLookup;

        [BurstCompile]
        private void Execute(Entity e, RefRW<LocalToWorld> l2w)
        {
            Helpers.ComputeWorldTransformMatrix(e, out l2w.ValueRW.Value, ref LocalTransformLookup, ref ParentLookup, ref PostTransformMatrixLookup);
        }
    }
}