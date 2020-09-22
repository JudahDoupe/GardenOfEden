using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class GameService : MonoBehaviour
{
    public bool IsGameInProgress { get; private set; }

    public RenderMesh Stem;

    private void Start()
    {
        StartGame();
    }

    private void StartGame()
    {
        IsGameInProgress = true;

        var em = World.DefaultGameObjectInjectionWorld.EntityManager;

        var plantArch = em.CreateArchetype(
            typeof(Translation),
            typeof(Rotation),
            typeof(LocalToWorld));
        var nodeArch = em.CreateArchetype(
            typeof(Translation),
            typeof(Rotation),
            typeof(Parent),
            typeof(LocalToWorld),
            typeof(LocalToParent));
        var internodeMeshArch = em.CreateArchetype(
            typeof(Translation),
            typeof(Rotation),
            typeof(NonUniformScale),
            typeof(Parent),
            typeof(LocalToWorld),
            typeof(LocalToParent),
            typeof(RenderMesh),
            typeof(RenderBounds));


        var plant = em.CreateEntity(plantArch);
        em.SetName(plant, "plant");
        em.SetComponentData(plant, new Translation { Value = Singleton.LandService.ClampToTerrain(new Vector3(-11, 50, -116)) });
        em.SetComponentData(plant, new Rotation { Value = Quaternion.LookRotation(Vector3.up) });

        var lastNode = plant;
        for (var i = 0; i < 5; i++)
        {

            var angle = Random.Range(-0.1f, 0.1f);
            var offset = new Vector3(angle, angle, angle);

            var node = em.CreateEntity(nodeArch);
            em.SetName(node, "node");
            em.SetComponentData(node, new Translation { Value = new Vector3(0,0,1) });
            em.SetComponentData(node, new Rotation { Value = Quaternion.LookRotation(Vector3.forward + offset) });
            em.SetComponentData(node, new Parent { Value = lastNode });
            lastNode = node;

            var internode = em.CreateEntity(internodeMeshArch);
            em.SetName(internode, "internodeMesh");
            em.SetComponentData(internode, new Parent { Value = node });
            em.SetComponentData(internode, new Rotation { Value = Quaternion.LookRotation(Vector3.forward) });
            em.SetComponentData(internode, new NonUniformScale { Value = new Vector3(0.1f,0.1f,1) });
            em.SetSharedComponentData(internode, Stem);
        }

    }

    private void EndGame()
    {
        IsGameInProgress = false;
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
