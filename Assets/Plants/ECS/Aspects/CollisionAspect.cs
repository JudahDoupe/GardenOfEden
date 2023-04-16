using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Framework.Utils;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

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

    public bool AddSphereToSphereCollisionResponse(float3 myCenter, float3 otherCenter,
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
    
    public bool AddSphereToGroundCollisionResponse(float3 myCenter, 
                                                   float3 groundPosition,
                                                   float myRadius, 
                                                   float3 myVelocity, 
                                                   float myBounciness)
    {
        
        var penetrationDistance = 0.5f - myCenter.y + myRadius;
        if (penetrationDistance < 0)
            return false;

        var penetrationNormal = new float3(0, 1, 0);
        var penetrationSpeed = math.dot(myVelocity, penetrationNormal);
        var restitution = 1 + myBounciness;

        _response.ValueRW.PositionAdjustment += penetrationNormal * penetrationDistance;
        _response.ValueRW.VelocityAdjustment -= penetrationNormal * penetrationSpeed * restitution;

        return true;
    }

    public (float3 myClosestPoint, float3 otherClosestPoint) ClosestPointsOnLineSegments(float3 myStart, float3 otherStart,
                                                                                         float3 myEnd, float3 otherEnd)
    {
        var v0 = otherStart - myStart;
        var v1 = otherEnd - myStart;
        var v2 = otherStart - myEnd;
        var v3 = otherEnd - myEnd;

        var d0 = math.dot(v0, v0);
        var d1 = math.dot(v1, v1);
        var d2 = math.dot(v2, v2);
        var d3 = math.dot(v3, v3);

        var myClosestPoint = d2 < d0 || d2 < d1 || d3 < d0 || d3 < d1 ? myEnd : myStart;
        var otherClosestPoint = myClosestPoint.ClosestPointOnLineSegment(otherStart, otherEnd);
        myClosestPoint = otherClosestPoint.ClosestPointOnLineSegment(myStart, myEnd);
        return (myClosestPoint, otherClosestPoint);
    }
    
    public bool ShouldCollide(Entity me, Entity other, ComponentLookup<Parent> parentLookup, BufferLookup<Child> childrenLookup)
    {
        var collide = other != me;

        if (parentLookup.TryGetComponent(me, out var parent))
        {
            collide = collide && other != parent.Value;
            
            if (childrenLookup.TryGetBuffer(parent.Value, out var siblings))
            {
                for (var i = 0; i < siblings.Length; i++)
                {
                    collide &= other != siblings[i].Value;
                }
            }
        }
        
        if (childrenLookup.TryGetBuffer(me, out var children))
        {
            for (var i = 0; i < children.Length; i++)
            {
                collide = collide && other != children[i].Value;
            }
        }

        return collide;
    }
}