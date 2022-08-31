using Assets.GamePlay.Cameras;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class CirclingCamera : CameraPerspective
{
    public float LerpSpeed = 5f;
    public float RotationSpeed = 10f;
    public float Fov = 60;

    public Entity FocusedEntity { get; set; }

    public override void Disable()
    {
        CameraUtils.SetEntityOutline(FocusedEntity, false);
        IsActive = false;
    }
    public override CameraState TransitionToState() => GetTargetState(FocusedEntity);

    private void LateUpdate()
    {
        if (!IsActive) return;
        if (!World.DefaultGameObjectInjectionWorld.EntityManager.Exists(FocusedEntity))
        {
            Debug.LogError("No focused Entity for the circling camera");
            return;
        }

        CameraUtils.SetState(GetTargetState(FocusedEntity));
        CameraController.CurrentState.Focus.Rotate(Vector3.up, RotationSpeed * Time.deltaTime, Space.Self);

        CameraUtils.SetEntityOutline(FocusedEntity, true);
    }

    public CameraState GetTargetState(Entity focusedEntity)
    {
        var currentState = CameraController.CurrentState;
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        var bounds = CameraUtils.EncapsulateChildren(focusedEntity);
        var plantCoord = em.GetComponentData<Coordinate>(focusedEntity);
        var backCoord = new Coordinate(currentState.Camera.transform.position, Planet.LocalToWorld);
        var focusCoord = new Coordinate(bounds.center, Planet.LocalToWorld);
        backCoord.Altitude = plantCoord.Altitude;
        var distance = math.max(CameraUtils.GetDistanceToIncludeBounds(bounds, Fov), 2);
        var focusRot = quaternion.LookRotation(math.normalize(plantCoord.LocalPlanet - backCoord.LocalPlanet), math.normalize(plantCoord.LocalPlanet));
        var cameraPos = Vector3.up * distance * 2f / 3f - Vector3.forward * distance;
        var cameraCoord = new Coordinate(focusCoord.LocalPlanet + ((Quaternion) focusRot * cameraPos).ToFloat3());
        cameraPos +=  Vector3.up * (CameraUtils.ClampAboveTerrain(cameraCoord).Altitude - cameraCoord.Altitude);

        var targetState = new CameraState(currentState.Camera, currentState.Focus)
        {
            CameraLocalPosition = IsActive 
                ? Vector3.Lerp(currentState.Camera.transform.localPosition, cameraPos, LerpSpeed * Time.deltaTime)
                : cameraPos,
            CameraLocalRotation = quaternion.LookRotation(math.normalize(-cameraPos), Vector3.up),
            CameraParent = currentState.Focus,
            FocusLocalPosition = IsActive
                ? Vector3.Lerp(currentState.Focus.localPosition, focusCoord.LocalPlanet, LerpSpeed * Time.deltaTime)
                : (Vector3)focusCoord.LocalPlanet,
            FocusLocalRotation = focusRot,
            FocusParent = Planet.Transform,
            FieldOfView = Fov,
            NearClip = 0.01f,
            FarClip = Coordinate.PlanetRadius * 1.5f,
        };

        if(targetState.CameraLocalPosition.x == float.NaN)
        {
            targetState = new CameraState();
        }

        return targetState;
    }

}