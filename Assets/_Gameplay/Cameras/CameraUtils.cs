using System;
using System.Collections;
using System.Linq;
using Assets.GamePlay.Cameras;
using Assets.Scripts.Plants.Setup;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

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

    public static Coordinate ClampAboveTerrain(Coordinate coord, float minDistance = 1)
    {
        if (Planet.Data == null)
            return coord;

        var minAltitude = math.max(Planet.Data.PlateTectonics.LandHeightMap.Sample(coord).r, Planet.Data.Water.WaterMap.Sample(coord).a) + minDistance;
        coord.Altitude = coord.Altitude < minAltitude ? minAltitude : coord.Altitude;
        return coord;
    }
    public static Coordinate ClampToTerrain(Coordinate coord)
    {
        if (Planet.Data == null)
            return coord;

        coord.Altitude = math.max(Planet.Data.PlateTectonics.LandHeightMap.Sample(coord).r, Planet.Data.Water.WaterMap.Sample(coord).a);
        return coord;
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
    public static void TransitionState(CameraState end, CameraTransition transition, Action callback = null)
    {
        end.Camera.transform.parent = end.CameraParent;
        end.Focus.parent = end.FocusParent;
        Cursor.lockState = end.Cursor;
        var start = new CameraState(end.Camera, end.Focus);
        var speeds = new []
        {
            GetTransitionTime(start.CameraLocalPosition, end.CameraLocalPosition, transition.Speed), 
            GetTransitionTime(start.CameraLocalRotation, end.CameraLocalRotation, transition.Speed),
            GetTransitionTime(start.FocusLocalPosition, end.FocusLocalPosition, transition.Speed),
            GetTransitionTime(start.FocusLocalRotation, end.FocusLocalRotation, transition.Speed),
            GetTransitionTime(start.FieldOfView, end.FieldOfView, transition.Speed),
        };

        CameraController.Instance.StartCoroutine(AnimateTransition(speeds.Max(), start, end, callback, transition.Ease));
    }
    private static IEnumerator AnimateTransition(float seconds, CameraState start, CameraState end, Action callback, EaseType ease)
    {
        var remainingSeconds = seconds;
        var t = 0f;
        var camera = end.Camera.GetComponent<Camera>();

        while (t < 1)
        {
            yield return new WaitForEndOfFrame();

            var lerp = ease.LerpValue(t);
            end.Camera.transform.localPosition = Vector3.Lerp(start.CameraLocalPosition, end.CameraLocalPosition, lerp);
            end.Camera.transform.position = ClampAboveTerrain(new Coordinate(end.Camera.transform.position, Planet.LocalToWorld)).Global(Planet.LocalToWorld);
            end.Camera.transform.localRotation = Quaternion.Lerp(start.CameraLocalRotation, end.CameraLocalRotation, lerp);
            end.Focus.localPosition = Vector3.Lerp(start.FocusLocalPosition, end.FocusLocalPosition, lerp);
            end.Focus.localRotation = Quaternion.Lerp(start.FocusLocalRotation, end.FocusLocalRotation, lerp);
            camera.fieldOfView = math.lerp(start.FieldOfView, end.FieldOfView, lerp);

            remainingSeconds -= Time.deltaTime;
            t = 1 - (remainingSeconds / seconds);
        }

        SetState(end);

        callback?.Invoke();
    }
    public static float GetTransitionTime(Vector3 start, Vector3 end, float transitionSpeed = 1) => math.sqrt(Vector3.Distance(start, end)) * 0.05f / transitionSpeed;
    public static float GetTransitionTime(Quaternion start, Quaternion end, float transitionSpeed = 1) => math.sqrt(Quaternion.Angle(start, end)) * 0.05f / transitionSpeed;
    public static float GetTransitionTime(float start, float end, float transitionSpeed = 1) => math.sqrt(math.abs(start - end)) * 0.05f / transitionSpeed;

    public static float GetScreenDepthAtCursor(float maxDepth = 10000) => math.min(DepthTexture.Sample(Input.mousePosition.x / Screen.width, Input.mousePosition.y / Screen.height).r, maxDepth);
    public static Vector3 GetCursorWorldPosition(float maxDepth = 10000) => 
        Camera.main.transform.position 
        + Camera.main.ScreenPointToRay(Input.mousePosition).direction 
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

        if (em.HasComponent<RenderMesh>(entity))
        {
            var mesh = em.GetSharedComponentData<RenderMesh>(entity);
            if (mesh.layer != layer)
            {
                mesh.layer = layer;
                em.SetSharedComponentData(entity, mesh);
            }
        }
    }

}

