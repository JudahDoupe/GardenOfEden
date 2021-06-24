using System;
using System.Collections;
using System.Linq;
using Assets.Scripts.Plants.Setup;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class CameraUtils : MonoBehaviour
{
  
    public static float GetDistanceToIncludeBounds(Bounds bounds, float fov, float multiplier = 1)
    {
        var sizes = bounds.max - bounds.min;
        var size = Mathf.Max(sizes.x, sizes.y, sizes.z) * multiplier;
        var cameraView = 2.0f * Mathf.Tan(0.5f * Mathf.Deg2Rad * fov); // Visible height 1 meter in front
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

    public static Coordinate ClampAboveTerrain(Coordinate coord, float minDistance = 1)
    {
        var minAltitude = math.max(Singleton.Land.SampleHeight(coord), Singleton.Water.SampleHeight(coord)) + minDistance;
        coord.Altitude = coord.Altitude < minAltitude ? minAltitude : coord.Altitude;
        return coord;
    }
    public static Coordinate ClampToTerrain(Coordinate coord)
    {
        coord.Altitude = math.max(Singleton.Land.SampleHeight(coord), Singleton.Water.SampleHeight(coord));
        return coord;
    }

    public static void Transition(CameraState end, Action callback = null, float transitionSpeed = 1)
    {
        end.Camera.parent = end.CameraParent;
        end.Focus.parent = end.FocusParent;
        var start = new CameraState(end.Camera, end.Focus);
        var speed = new []
        {
            GetTransitionTime(start.CameraLocalPosition, end.CameraLocalPosition, transitionSpeed), 
            GetTransitionTime(start.CameraLocalRotation, end.CameraLocalRotation, transitionSpeed),
            GetTransitionTime(start.FocusLocalPosition, end.FocusLocalPosition, transitionSpeed),
            GetTransitionTime(start.FocusLocalRotation, end.FocusLocalRotation, transitionSpeed),
            GetTransitionTime(start.FieldOfView, end.FieldOfView, transitionSpeed),
        }.Max();

        Singleton.Instance.StartCoroutine(AnimateTransition(speed, start, end, callback));
    }
    private static IEnumerator AnimateTransition(float seconds, CameraState start, CameraState end, Action callback = null)
    {
        var remainingSeconds = seconds;
        var t = 0f;
        var camera = end.Camera.GetComponent<Camera>();

        while (t < 1)
        {
            yield return new WaitForEndOfFrame();

            end.Camera.localPosition = Vector3.Lerp(start.CameraLocalPosition, end.CameraLocalPosition, t);
            end.Camera.localRotation = Quaternion.Lerp(start.CameraLocalRotation, end.CameraLocalRotation, t);
            end.Focus.localPosition = Vector3.Lerp(start.FocusLocalPosition, end.FocusLocalPosition, t);
            end.Focus.localRotation = Quaternion.Lerp(start.FocusLocalRotation, end.FocusLocalRotation, t);
            camera.fieldOfView = math.lerp(start.FieldOfView, end.FieldOfView, t);

            remainingSeconds -= Time.deltaTime;
            t = 1 - (remainingSeconds / seconds);
        }

        end.Camera.localPosition = end.CameraLocalPosition;
        end.Camera.localRotation = end.CameraLocalRotation;
        end.Focus.localPosition = end.FocusLocalPosition;
        end.Focus.localRotation = end.FocusLocalRotation;
        camera.fieldOfView = end.FieldOfView;

        callback?.Invoke();
    }
    public static float GetTransitionTime(Vector3 start, Vector3 end, float transitionSpeed = 1)
    {
        return math.sqrt(Vector3.Distance(start, end)) * 0.05f / transitionSpeed;
    }
    public static float GetTransitionTime(Quaternion start, Quaternion end, float transitionSpeed = 1)
    {
        return math.sqrt(Quaternion.Angle(start, end)) * 0.05f / transitionSpeed;
    }
    public static float GetTransitionTime(float start, float end, float transitionSpeed = 1)
    {
        return math.sqrt(math.abs(start - end)) * 0.05f / transitionSpeed;
    }
}
