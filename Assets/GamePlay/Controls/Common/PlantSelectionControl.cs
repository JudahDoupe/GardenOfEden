using Assets.Scripts.Plants.Growth;
using Unity.Entities;
using UnityEngine;

public class PlantSelectionControl : MonoBehaviour
{
    public float Range = 1;
    public bool Active { get; private set; }

    public void Enable() => Active = true;
    public void Disable() => Active = false;

    private Entity _selectedPlant;

    void Update()
    {
        if (!Active) return;

        var newPlant = GetSelectedPlant();
        if (newPlant != _selectedPlant)
        {
            CameraUtils.SetEntityOutline(_selectedPlant, false);
            _selectedPlant = newPlant;
        }

        CameraUtils.SetEntityOutline(_selectedPlant, true);

        if (Input.GetKeyDown(KeyCode.Mouse0) && _selectedPlant != Entity.Null)
        {
            Singleton.PerspectiveController.SelectPlant(_selectedPlant);
        }
    }

    private Entity GetSelectedPlant()
    {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        var cursorPos = CameraUtils.GetCursorWorldPosition();
        var entity = CameraUtils.GetClosestEntityWithComponent<Node>(cursorPos, Range);
        if (em.Exists(entity))
        {
            return CameraUtils.GetParentEntityWithComponent<Coordinate>(entity);
        }
        return Entity.Null;
    }
}
