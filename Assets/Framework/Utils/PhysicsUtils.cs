using System.Numerics;
using Unity.Mathematics;

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
    }
    
}