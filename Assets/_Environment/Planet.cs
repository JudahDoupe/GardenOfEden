using System;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class Planet : Singleton<Planet>
{
    public static Entity Entity;
    public static Transform Transform;
    public static Signal<PlanetData> Data;
    public float RotationSpeed;

    public static LocalToWorld LocalToWorld => World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<LocalToWorld>(Entity);

    private void Awake()
    {
        Instance = this;
        Data = new Signal<PlanetData>(null);
        Transform = transform;
    }

    private void Update()
    {
        transform.Rotate(new Vector3(0, RotationSpeed * Time.deltaTime, 0));
    }

    [ContextMenu("Save")]
    public void Save(Action callback = null) => Instance.RunTaskInCoroutine(PlanetDataStore.Update(Data.Value), callback);

    [ContextMenu("Load")]
    public void Load(string planetName) => Instance.RunTaskInCoroutine(PlanetDataStore.GetOrCreate(planetName), data => Data.Publish(data));
}