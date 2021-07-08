using Assets.Scripts.Plants.Growth;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class PlantSelectionControl : MonoBehaviour
{
    public float Range = 1;
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
            SetChildrenLayer(_selectedPlant, LayerMask.NameToLayer("Default"));
            SetChildrenLayer(newPlant, LayerMask.NameToLayer("OutlinedGroup"));
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
        var entity = CameraUtils.GetClosestEntityWithComponent<Node>(cursorPos, Range);
        if (em.Exists(entity))
        {
            return CameraUtils.GetParentEntityWithComponent<Coordinate>(entity);
        }
        return Entity.Null;
    }

    private void SetChildrenLayer(Entity entity, int layer)
    {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;

        if (em.HasComponent<RenderMesh>(entity))
        {
            var mesh = em.GetSharedComponentData<RenderMesh>(entity);
            mesh.layer = layer;
            em.SetSharedComponentData(entity, mesh);
        }

        if (em.HasComponent<Child>(entity))
        {
            var children = em.GetBuffer<Child>(entity);
            for (int i = 0; i < children.Length; i++)
            {
                SetChildrenLayer(entity, layer);
            }
        }
    }
}
