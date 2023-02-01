using System.Linq;
using Assets.GamePlay.Cameras;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraUtils : MonoBehaviour
{
    public static RenderTexture DepthTexture;

    public static float GetDistanceToIncludeBounds(Bounds bounds, float fov, float multiplier = 1)
    {
        var sizes = bounds.max - bounds.min;
        var size = Mathf.Max(sizes.x, sizes.y, sizes.z) * multiplier;
        var cameraView = 2.0f * Mathf.Tan(0.5f * Mathf.Deg2Rad * fov); // Visible height 1 meter in front
        var distance = size / cameraView; // Combined wanted distance from the object
        distance += 0.5f * size; // Estimated offset from the center to the outside of the object
        return distance;
    }

    public static Entity GetClosestEntityWithComponent<T>(Vector3 position, float minDistance = 100) where T : IComponentData
    {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        var q = em.CreateEntityQuery(typeof(LocalToWorld), typeof(T));
        var entities = q.ToEntityArray(Unity.Collections.Allocator.Temp);

        var closest = Entity.Null;
        foreach (var e in entities)
        {
            var l2w = em.GetComponentData<LocalToWorld>(e);
            var dist = Vector3.Distance(position, l2w.Position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = e;
            }
        }

        return closest;
    }
    public static Entity GetParentEntityWithComponent<T>(Entity entity) where T : IComponentData
    {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        if (em.HasComponent<T>(entity)) return entity;
        if (em.HasComponent<Parent>(entity)) return GetParentEntityWithComponent<T>(em.GetComponentData<Parent>(entity).Value);
        return Entity.Null;
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

    public static void SetState(CameraState end)
    {
        var camera = end.Camera.GetComponent<Camera>();
        end.Camera.transform.parent = end.CameraParent;
        end.Focus.parent = end.FocusParent;
        end.Camera.transform.localPosition = end.CameraLocalPosition;
        end.Camera.transform.localRotation = end.CameraLocalRotation;
        end.Focus.localPosition = end.FocusLocalPosition;
        end.Focus.localRotation = end.FocusLocalRotation;
        camera.fieldOfView = end.FieldOfView;
        camera.nearClipPlane = end.NearClip;
        camera.farClipPlane = end.FarClip;
        Cursor.lockState = end.Cursor;
    }

    public static float GetScreenDepthAtCursor(float maxDepth = 10000) => math.min(DepthTexture.Sample(Mouse.current.position.ReadValue().x / Screen.width, Mouse.current.position.ReadValue().y / Screen.height).r, maxDepth);
    public static Vector3 GetCursorWorldPosition(float maxDepth = 10000) => 
        Camera.main.transform.position 
        + Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()).direction 
        * GetScreenDepthAtCursor(maxDepth);

    public static void SetOutline(GameObject gameObject, bool active)
    {
        gameObject.layer = active ? LayerMask.NameToLayer("OutlinedGroup") : LayerMask.NameToLayer("Default");
    }
    public static void SetEntityOutline(Entity entity, bool active)
    {
        SetLayer(entity, active ? LayerMask.NameToLayer("OutlinedGroup") : LayerMask.NameToLayer("Default"));
    }
    public static void SetLayer(Entity entity, int layer)
    {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        if (em.HasComponent<Child>(entity))
        {
            var children = em.GetBuffer<Child>(entity).ToNativeArray(Allocator.Temp).ToArray().Select(x => x.Value);
            foreach (var child in children)
            {
                SetLayer(child, layer);
            }
        }

        //TODO: Set Layer
    }

}

