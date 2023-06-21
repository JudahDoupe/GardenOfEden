using Unity.Mathematics;
using Unity.Transforms;

namespace Framework.Utils
{
    public static class PhysicsUtils
    {
        public static float3 ClosestPointOnLineSegment(this float3 point, float3 start, float3 end)
        {
            var vector = end - start;
            var t = math.dot(point - start, vector) / math.dot(vector, vector);
            return start + math.saturate(t) * vector;
        }
        
        public static float3 WorldToLocal(this LocalToWorld transform, float3 point) 
            => math.transform(math.inverse(transform.Value), point);
        
        public static float3 LocalToWorld(this LocalToWorld transform, float3 point) 
            => math.transform(transform.Value, point);
        
        public static float3 LocalToWorldVector(this LocalToWorld transform, float3 vector) 
            => math.rotate(transform.Rotation, vector);

        public static float3 WorldToLocalVector(this LocalToWorld transform, float3 vector) 
            => math.rotate(math.conjugate(transform.Rotation), vector);

        public static void TranslateWorld(this ref LocalTransform localTransform, LocalToWorld worldTransform, float3 vector) 
            => localTransform.Position += math.rotate(localTransform.Rotation, math.rotate(math.conjugate(worldTransform.Rotation), vector));

        public static float3 TransformVector(this LocalTransform transform, float3 vector) 
            => math.rotate(transform.Rotation, vector) * transform.Scale;

        public static float3 InverseTransformVector(this LocalTransform transform, float3 vector) 
            => math.rotate(math.conjugate(transform.Rotation), vector) / transform.Scale;
    }
    
}