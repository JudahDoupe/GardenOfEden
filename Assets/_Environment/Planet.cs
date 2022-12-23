using System;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class Planet : Singleton<Planet>
{
    public float RotationSpeed;

    public static Entity Entity;
    public static Transform Transform;
    public static LocalToWorld LocalToWorld => World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<LocalToWorld>(Entity);
    public static PlanetData Data;

    [ContextMenu("Save")]
    public void Save(Action callback = null) => Instance.RunTaskInCoroutine(PlanetDataStore.Update(Data), callback);

    [ContextMenu("Load")]
    public void Load(string name, Action callback = null) => Instance.RunTaskInCoroutine(PlanetDataStore.GetOrCreate(name), data =>
    {
        Initialize(data);
        callback?.Invoke();
    });

    public void Initialize(PlanetData data)
    {
        Data = data;
        
        FindObjectOfType<PlateTectonicsSimulation>().Initialize(data.PlateTectonics);
        FindObjectOfType<PlateTectonicsVisualization>().Initialize(data.PlateTectonics);
        FindObjectOfType<PlateTectonicsAudio>().Initialize(data.PlateTectonics);
        FindObjectOfType<PlateBakerV2>().Initialize(data.PlateTectonics);
        FindObjectOfType<MovePlateTool>().Initialize(data.PlateTectonics);
        FindObjectOfType<BreakPlateTool>().Initialize(data.PlateTectonics);
        FindObjectOfType<MergePlateTool>().Initialize(data.PlateTectonics);
        FindObjectOfType<LandscapeCameraTool>().Initialize(data.PlateTectonics);

        FindObjectOfType<WaterSimulation>().Initialize(data.Water);
        
        AtmosphereVisualization.AttachToPlanet(this);
    }

    void Awake()
    {
        Instance = this;
        Transform = transform;
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        Entity = em.CreateEntity();
        em.AddComponent<Translation>(Entity);
        em.AddComponent<Rotation>(Entity);
        em.AddComponent<LocalToWorld>(Entity);
#if UNITY_EDITOR
        em.SetName(Entity, "Planet");
#endif
    }

    void Update()
    {
        transform.Rotate(new Vector3(0, RotationSpeed * Time.deltaTime, 0));
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        em.SetComponentData(Entity, new Rotation{ Value = transform.rotation });
    }

}
