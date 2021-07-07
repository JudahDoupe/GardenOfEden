using Assets.Scripts.Plants.Growth;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class PlantSelectionControl : MonoBehaviour
{
    public float Range;
    public bool Active { get; private set; }

    public bool Enable() => Active = true;
    public bool Disable() => Active = false;

    private Entity _selectedPlant;

    void Update()
    {
        if (!Active) return;

        var newPlant = GetSelectedPlant();
        if (newPlant != _selectedPlant)
        {
            SetChildrenLayer(_selectedPlant);
            SetChildrenLayer(newPlant);
            _selectedPlant = newPlant;
        }

        if (Input.GetKeyDown(KeyCode.Mouse0) && _selectedPlant != Entity.Null)
        {
            Singleton.PerspectiveController.SelectPlant(_selectedPlant);
        }
    }

    private Entity GetSelectedPlant()
    {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        var cursorPos = CameraUtils.GetCursorWorldPosition();
        var entity = CameraUtils.GetClosestEntityWithComponent<Node>(cursorPos);
        if (Vector3.Distance(em.GetComponentData<LocalToWorld>(entity).Position, cursorPos) < Range)
        {
            return CameraUtils.GetParentEntityWithComponent<Coordinate>(entity);
        }
        else
        {
            return Entity.Null;
        }
    }

    private void SetChildrenLayer(Entity e)
    {

    }
}
