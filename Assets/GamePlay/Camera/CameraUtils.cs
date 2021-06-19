using Assets.Scripts.Plants.Setup;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class CameraUtils : MonoBehaviour
{
    public static Quaternion LookAtBoundsCenter(Bounds bounds)
    {
        var towardPlantDirection = (bounds.center - Camera.main.transform.position).normalized;
        return Quaternion.LookRotation(towardPlantDirection, Vector3.up);
    }
    
    public static Vector3 RotateAroundBounds(Vector3 startingPosition, Bounds bounds, float rotationAngle, float distanceMultiplier = 1)
    {
        var direction = (startingPosition - bounds.center).normalized;
        direction.Scale(new Vector3(1,0,1));
        direction.Normalize();
        var distance = GetDistanceToIncludeBounds(bounds, distanceMultiplier);
        var vector = direction * distance + new Vector3(0, bounds.extents.y, 0);
        var newVector = Quaternion.Euler(0, rotationAngle, 0) * vector;
        return bounds.center + newVector;
    }
    
    public static float GetDistanceToIncludeBounds(Bounds bounds, float multiplier = 1)
    {
        var sizes = bounds.max - bounds.min;
        var size = Mathf.Max(sizes.x, sizes.y, sizes.z) * multiplier;
        var cameraView = 2.0f * Mathf.Tan(0.5f * Mathf.Deg2Rad * Camera.main.fieldOfView); // Visible height 1 meter in front
        var distance = size / cameraView; // Combined wanted distance from the object
        distance += 0.5f * size; // Estimated offset from the center to the outside of the object
        return distance;
    }

    public static Entity GetClosestEntity(Vector3 position)
    {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        var q = em.CreateEntityQuery(typeof(Coordinate), typeof(UpdateChunk));
        q.SetSharedComponentFilter(Singleton.LoadBalancer.ActiveEntityChunk);
        var entities = q.ToEntityArray(Unity.Collections.Allocator.Temp);

        var closest = Entity.Null;
        var minDistance = Singleton.LoadBalancer.Radius;
        foreach (var e in entities)
        {
            var coord = em.GetComponentData<Coordinate>(e);
            var dist = Vector3.Distance(position, coord.Global(Planet.LocalToWorld));
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = e;
            }
        }

        return closest;
    }

    public static Bounds EncapsulateChildren(Entity entity, Bounds? bounds = null)
    {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        var l2w = em.GetComponentData<LocalToWorld>(entity);
        var newBounds = bounds.HasValue ? bounds.Value : new Bounds(l2w.Position, new Vector3(0.1f,0.1f,0.1f));
        newBounds.Encapsulate(l2w.Position);

        if (em.HasComponent<Child>(entity))
        {
            var children = em.GetBuffer<Child>(entity);
            for (int i = 0; i < children.Length; i++)
            {
                newBounds.Encapsulate(EncapsulateChildren(children[i].Value, newBounds));
            }
        }
        return newBounds;
    }

    public static float GetTransitionTime(Vector3 start, Vector3 end, float transitionSpeed = 1)
    {
        return math.sqrt(Vector3.Distance(start, end)) * 0.05f / transitionSpeed;
    }

    public static float GetTransitionTime(Quaternion start, Quaternion end, float transitionSpeed = 1)
    {
        return math.sqrt(Quaternion.Angle(start, end)) * 0.05f / transitionSpeed;
    }
}
