using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class Planet : MonoBehaviour
{
    public float RotationSpeed;
    public string Name = "New Planet";

    public static Entity Entity;
    public static Transform Transform;
    public static LocalToWorld LocalToWorld => World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<LocalToWorld>(Entity);
    public static PlanetData Data;

    [ContextMenu("Save")]
    public static void Save() => PlanetDataStore.Update(Data);
    [ContextMenu("Reset")]
    public void ResetPlanet()
    {
        Initialize(PlanetDataStore.Create(Name));
    }

    public void Initialize(PlanetData data)
    {
        Data = data;
        
        FindObjectOfType<WaterSimulation>().Initialize(data.Water);
        FindObjectOfType<PlateTectonicsSimulation>().Initialize(data.PlateTectonics);
    }

    void Start()
    {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        Entity = em.CreateEntity();
        em.AddComponent<Translation>(Entity);
        em.AddComponent<Rotation>(Entity);
        em.AddComponent<LocalToWorld>(Entity);
#if UNITY_EDITOR
        em.SetName(Entity, "Planet");
#endif

        Transform = transform;

        Initialize(PlanetDataStore.GetOrCreate(Name));
    }

    void Update()
    {
        transform.Rotate(new Vector3(0, RotationSpeed * Time.deltaTime, 0));
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        em.SetComponentData(Entity, new Rotation{ Value = transform.rotation });
    }
}
