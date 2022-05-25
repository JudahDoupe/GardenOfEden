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

    public static void Save() => PlanetDataStore.UpdatePlanet(Data);
    public void ResetPlanet()
    {
        Data.PlateTectonics.NeedsRegeneration = true;
        Data.Water.NeedsRegeneration = true;
        Initialize(Data);
    }

    public void Initialize(PlanetData data)
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
        Data = data;
        
        FindObjectOfType<WaterSimulation>().Initialize(data.Water);
        FindObjectOfType<PlateTectonicsSimulation>().Initialize(data.PlateTectonics);
    }

    void Start()
    {
        Initialize(PlanetDataStore.GetOrCreate(Name));
    }

    void Update()
    {
        transform.Rotate(new Vector3(0, RotationSpeed * Time.deltaTime, 0));
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        em.SetComponentData(Entity, new Rotation{ Value = transform.rotation });
    }
}
