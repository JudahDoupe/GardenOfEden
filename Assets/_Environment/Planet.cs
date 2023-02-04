using System;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class Planet : Singleton<Planet>
{
    public static Transform Transform;
    public static Signal<PlanetData> Data;
    public float RotationSpeed;

    public static Entity Entity;
    public static LocalToWorld LocalToWorld => World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<LocalToWorld>(Entity);

    private void Awake()
    {
        Instance = this;
        Data = new Signal<PlanetData>(null);
        Transform = transform;
    }

    private void Start()
    {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        Entity = em.CreateEntity();
        em.AddComponent<LocalTransform>(Entity);
        em.AddComponent<LocalToWorld>(Entity);
#if UNITY_EDITOR
        em.SetName(Entity, "Planet");
#endif
    }

    private void Update()
    {
        transform.Rotate(new Vector3(0, RotationSpeed * Time.deltaTime, 0));
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        var t = em.GetComponentData<LocalTransform>(Entity);
        t.Rotation = transform.localRotation;
        em.SetComponentData(Entity, t);
    }

    [ContextMenu("Save")]
    public void Save(Action callback = null) => Instance.RunTaskInCoroutine(PlanetDataStore.Update(Data.Value), callback);

    [ContextMenu("Load")]
    public void Load(string planetName) => Instance.RunTaskInCoroutine(PlanetDataStore.GetOrCreate(planetName), data => Data.Publish(data));
}