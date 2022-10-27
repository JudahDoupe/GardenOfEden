using System.Collections;
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
    public static void Save() => PlanetDataStore.Update(Data).ConfigureAwait(false);
    
    [ContextMenu("Load")]
    public void Load() => StartCoroutine(LoadAsync());
    private IEnumerator LoadAsync()
    {
        var dataTask = PlanetDataStore.GetOrCreate(Name);
        yield return new WaitUntil(() => dataTask.IsCompleted);
        Initialize(dataTask.Result);
    }

    [ContextMenu("Reset")]
    public void ResetPlanet() => StartCoroutine(ResetPlanetAsync());
    private IEnumerator ResetPlanetAsync()
    {
        var dataTask = PlanetDataStore.Create(Name);
        yield return new WaitUntil(() => dataTask.IsCompleted);
        Initialize(dataTask.Result);
    }

    public void Initialize(PlanetData data)
    {
        Data = data;
        FindObjectOfType<SystemsController>().InitializeAllSystems(data);
        FindObjectOfType<SystemsController>().EnableGlobe();
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
