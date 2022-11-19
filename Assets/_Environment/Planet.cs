using System.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class Planet : Singleton<Planet>
{
    public float RotationSpeed;
    public string Name = "New Planet";

    public static Entity Entity;
    public static Transform Transform;
    public static LocalToWorld LocalToWorld => World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<LocalToWorld>(Entity);
    public static PlanetData Data;

    [ContextMenu("Save")]
    public void Save() => Instance.RunTaskInCoroutine(PlanetDataStore.Update(Data));

    [ContextMenu("Load")]
    public void Load() => Instance.RunTaskInCoroutine(PlanetDataStore.GetOrCreate(Name), data => Initialize(data));

    [ContextMenu("Reset")]
    public void ResetPlanet() => Instance.RunTaskInCoroutine(PlanetDataStore.Create(Name), data => Initialize(data));

    public void Initialize(PlanetData data)
    {
        Data = data;
        FindObjectOfType<SystemsController>().InitializeAllSystems(data);
        FindObjectOfType<SystemsController>().EnableGlobe();
        AtmosphereVisualization.AttachToPlate(this);
    }

    void Start()
    {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        Transform = transform;
        Entity = em.CreateEntity();
        em.AddComponent<Translation>(Entity);
        em.AddComponent<Rotation>(Entity);
        em.AddComponent<LocalToWorld>(Entity);
#if UNITY_EDITOR
        em.SetName(Entity, "Planet");
#endif

        Load();
    }

    void Update()
    {
        transform.Rotate(new Vector3(0, RotationSpeed * Time.deltaTime, 0));
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        em.SetComponentData(Entity, new Rotation{ Value = transform.rotation });
    }

}
