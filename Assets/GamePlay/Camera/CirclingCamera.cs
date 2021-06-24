using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class CirclingCamera : MonoBehaviour
{
    public float LerpSpeed = 5f;
    public float RotationSpeed = 10f;
    public float Fov = 60;
    public bool IsActive { get; private set; }

    private Entity _focusedEntity;
    private Transform _camera;
    private Transform _focus;

    public void Enable(Transform camera, Transform focus, Entity focusedEntity)
    {
        _focusedEntity = focusedEntity;
        _focus = focus;
        _camera = camera;

        CameraUtils.Transition(GetTargetState(_camera, _focus, _focusedEntity), () => IsActive = true);
    }

    public void Disable()
    {
        IsActive = false;
    }

    private void LateUpdate()
    {
        if (!IsActive) return;

        _focus.Rotate(Vector3.up, RotationSpeed * Time.deltaTime,Space.Self);
        var state = GetTargetState(_camera, _focus, _focusedEntity);

        _camera.localPosition = Vector3.Lerp(_camera.localPosition, state.CameraLocalPosition, LerpSpeed * Time.deltaTime);
        _focus.localPosition = Vector3.Lerp(_focus.localPosition, state.FocusLocalPosition, LerpSpeed * Time.deltaTime);
    }

    public CameraState GetTargetState(Transform camera, Transform focus, Entity focusedEntity)
    {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        var bounds = CameraUtils.EncapsulateChildren(focusedEntity);
        var plantCoord = em.GetComponentData<Coordinate>(focusedEntity);
        var backCoord = new Coordinate(camera.position, Planet.LocalToWorld);
        backCoord.Altitude = plantCoord.Altitude;
        var distance = math.max(CameraUtils.GetDistanceToIncludeBounds(bounds, Fov), 1);
        var cameraPos = Vector3.up * distance * 2f / 3f - Vector3.forward * distance;
        return new CameraState(camera, focus)
        {
            CameraLocalPosition = cameraPos,
            CameraLocalRotation = quaternion.LookRotation(math.normalize(-cameraPos), Vector3.up),
            CameraParent = focus,
            FocusLocalPosition = new Coordinate(bounds.center, Planet.LocalToWorld).LocalPlanet,
            FocusLocalRotation = quaternion.LookRotation(math.normalize(plantCoord.LocalPlanet - backCoord.LocalPlanet), math.normalize(plantCoord.LocalPlanet)),
            FocusParent = Planet.Transform,
            FieldOfView = Fov,
        };
    }

}