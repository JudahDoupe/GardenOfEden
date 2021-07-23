using Assets.GamePlay.Cameras;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class CirclingCamera : CameraController
{
    public float LerpSpeed = 5f;
    public float RotationSpeed = 10f;
    public float Fov = 60;

    private Entity _focusedEntity;

    public void Enable(CameraState currentState, Entity focusedEntity)
    {
        _focusedEntity = focusedEntity;
        CurrentState = currentState;

        if (!World.DefaultGameObjectInjectionWorld.EntityManager.Exists(focusedEntity))
        {
            Singleton.PerspectiveController.ZoomOut();
        }

        IsActive = true;
    }

    public void Disable()
    {
        CameraUtils.SetEntityOutline(_focusedEntity, false);
        IsActive = false;
    }

    private void LateUpdate()
    {
        if (!IsActive) return;
        if (!World.DefaultGameObjectInjectionWorld.EntityManager.Exists(_focusedEntity))
        {
            Singleton.PerspectiveController.ZoomOut();
            return;
        }

        CameraUtils.SetState(GetTargetState(CurrentState, _focusedEntity));
        CurrentState.Focus.Rotate(Vector3.up, RotationSpeed * Time.deltaTime, Space.Self);

        CameraUtils.SetEntityOutline(_focusedEntity, true);

        if (Input.mouseScrollDelta.y < 0)
        {
            Singleton.PerspectiveController.ZoomOut();
        }
    }

    public CameraState GetTargetState(CameraState currentState, Entity focusedEntity)
    {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        var bounds = CameraUtils.EncapsulateChildren(focusedEntity);
        var plantCoord = em.GetComponentData<Coordinate>(focusedEntity);
        var backCoord = new Coordinate(currentState.Camera.position, Planet.LocalToWorld);
        var focusCoord = new Coordinate(bounds.center, Planet.LocalToWorld);
        backCoord.Altitude = plantCoord.Altitude;
        var distance = math.max(CameraUtils.GetDistanceToIncludeBounds(bounds, Fov), 2);
        var focusRot = quaternion.LookRotation(math.normalize(plantCoord.LocalPlanet - backCoord.LocalPlanet), math.normalize(plantCoord.LocalPlanet));
        var cameraPos = Vector3.up * distance * 2f / 3f - Vector3.forward * distance;
        var cameraCoord = new Coordinate(focusCoord.LocalPlanet + ((Quaternion) focusRot * cameraPos).ToFloat3());
        cameraPos +=  Vector3.up * (CameraUtils.ClampAboveTerrain(cameraCoord).Altitude - cameraCoord.Altitude);

        return new CameraState(currentState.Camera, currentState.Focus)
        {
            CameraLocalPosition = IsActive 
                ? Vector3.Lerp(currentState.Camera.localPosition, cameraPos, LerpSpeed * Time.deltaTime)
                : cameraPos,
            CameraLocalRotation = quaternion.LookRotation(math.normalize(-cameraPos), Vector3.up),
            CameraParent = currentState.Focus,
            FocusLocalPosition = IsActive
                ? Vector3.Lerp(currentState.Focus.localPosition, focusCoord.LocalPlanet, LerpSpeed * Time.deltaTime)
                : (Vector3)focusCoord.LocalPlanet,
            FocusLocalRotation = focusRot,
            FocusParent = Planet.Transform,
            FieldOfView = Fov,
        };
    }

}