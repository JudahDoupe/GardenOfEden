using Assets.Scripts.Plants.ECS.Components;
using System.Linq;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class GameService : MonoBehaviour
{
    public bool IsGameInProgress { get; private set; }

    private void Start()
    {
        StartGame();
    }

    private void StartGame()
    {
        IsGameInProgress = true;

        var em = World.DefaultGameObjectInjectionWorld.EntityManager;

        for (var i = 0; i < 500; i++)
        {
            var plant = em.CreateEntity(Singleton.ArchetypeLibrary.Library["Plant"]);
            em.SetComponentData(plant, new Translation { Value = Singleton.LandService.ClampToTerrain(new Vector3(Random.Range(-100f,100f), 50, Random.Range(-200f, 0f))) });
            em.SetComponentData(plant, new Rotation { Value = Quaternion.LookRotation(Vector3.up) });

            var node = em.CreateEntity(Singleton.ArchetypeLibrary.Library["Node"]);
            em.SetComponentData(node, new Translation { Value = new Vector3(0, 0, 0.1f) });
            em.SetComponentData(node, new Rotation { Value = Quaternion.LookRotation(Vector3.forward) });
            em.SetComponentData(node, new Parent { Value = plant });
            em.AddComponent<TerminalBud>(node);
            var internode = em.CreateEntity(Singleton.ArchetypeLibrary.Library["Internode"]);
            em.SetComponentData(internode, new Rotation { Value = Quaternion.LookRotation(Vector3.forward) });
            em.SetComponentData(internode, new NonUniformScale { Value = new Vector3(0.1f, 0.1f, 1) });
            em.SetComponentData(internode, new Internode { HeadNode = node, TailNode = plant }); 
            em.SetComponentData(node, new InternodeReference { Internode = internode });
            em.AddComponentData(internode, new AssignMesh{MeshName = "GreenStem"});

            var leftLeaf = em.CreateEntity(Singleton.ArchetypeLibrary.Library["Leaf"]);
            em.SetComponentData(leftLeaf, new Translation { Value = new Vector3(0, 0, 0.01f) });
            em.SetComponentData(leftLeaf, new Rotation { Value = Quaternion.LookRotation(Vector3.left) });
            em.SetComponentData(leftLeaf, new Parent { Value = node });
            em.AddComponentData(leftLeaf, new AssignMesh { MeshName = "Leaf" });
            var leftLeafInternode = em.CreateEntity(Singleton.ArchetypeLibrary.Library["Internode"]);
            em.SetComponentData(leftLeafInternode, new Rotation { Value = Quaternion.LookRotation(Vector3.forward) });
            em.SetComponentData(leftLeafInternode, new NonUniformScale { Value = new Vector3(0.05f, 0.05f, 1) });
            em.SetComponentData(leftLeafInternode, new Internode { HeadNode = leftLeaf, TailNode = node });
            em.SetComponentData(leftLeaf, new InternodeReference { Internode = leftLeafInternode });
            em.AddComponentData(leftLeafInternode, new AssignMesh{MeshName = "GreenStem"});


            var rightLeaf = em.CreateEntity(Singleton.ArchetypeLibrary.Library["Leaf"]);
            em.SetComponentData(rightLeaf, new Translation { Value = new Vector3(0, 0, 0.01f) });
            em.SetComponentData(rightLeaf, new Rotation { Value = Quaternion.LookRotation(Vector3.right) });
            em.SetComponentData(rightLeaf, new Parent { Value = node });
            em.AddComponentData(rightLeaf, new AssignMesh{MeshName = "Leaf"});
            var rightLeafInternode = em.CreateEntity(Singleton.ArchetypeLibrary.Library["Internode"]);
            em.SetComponentData(rightLeafInternode, new Rotation { Value = Quaternion.LookRotation(Vector3.forward) });
            em.SetComponentData(rightLeafInternode, new NonUniformScale { Value = new Vector3(0.05f, 0.05f, 1) });
            em.SetComponentData(rightLeafInternode, new Internode { HeadNode = rightLeaf, TailNode = node });
            em.SetComponentData(rightLeaf, new InternodeReference { Internode = rightLeafInternode });
            em.AddComponentData(rightLeafInternode, new AssignMesh{MeshName = "GreenStem"});
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
