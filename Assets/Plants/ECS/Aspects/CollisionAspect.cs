using Unity.Entities;
using Unity.Mathematics;

public readonly partial struct CollisionAspect : IAspect
{
    private readonly RefRW<CollisionResponse> _response;

    public float3 PositionAdjustment => _response.ValueRO.PositionAdjustment;
    public float3 VelocityAdjustment => _response.ValueRO.VelocityAdjustment;

    public void Clear()
    {
        _response.ValueRW.PositionAdjustment = new float3(0, 0, 0);
        _response.ValueRW.VelocityAdjustment = new float3(0, 0, 0);
    }
    
    public bool AddSphereCollisionResponse(float3 myCenter, float3 otherCenter, 
                                           float myRadius, float otherRadius,
                                           float3 myVelocity, float3 otherVelocity,
                                           float myBounciness, float otherBounciness)
    {
        var collisionVector = myCenter - otherCenter;
        var collisionDistance = math.length(collisionVector);
            
        var penetrationDistance = myRadius + otherRadius - collisionDistance;
        if (penetrationDistance < 0) 
            return false;
        
        var penetrationNormal = collisionVector / collisionDistance;
        var penetrationSpeed = math.dot(myVelocity - otherVelocity, penetrationNormal);
            
        var restitution = 1 + math.max(myBounciness, otherBounciness);

        _response.ValueRW.PositionAdjustment += penetrationNormal * penetrationDistance;
        _response.ValueRW.VelocityAdjustment -= penetrationNormal * penetrationSpeed * restitution;

        return true;
    }
}