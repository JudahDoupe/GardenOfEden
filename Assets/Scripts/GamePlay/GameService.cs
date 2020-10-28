using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Assets.Scripts.Plants.Systems;
using Unity.Mathematics;
using Random = UnityEngine.Random;

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
            var vegEmbryo = em.CreateEntity();
            em.AddComponentData(vegEmbryo, new Dormant());
            em.AddComponentData(vegEmbryo, new Node());
            em.AddComponentData(vegEmbryo, new Internode());
            em.AddComponentData(vegEmbryo, new Translation());
            em.AddComponentData(vegEmbryo, new Rotation { Value = Quaternion.LookRotation(Vector3.forward) });
            em.AddComponentData(vegEmbryo, new Parent ());
            em.AddComponentData(vegEmbryo, new LocalToParent());
            em.AddComponentData(vegEmbryo, new LocalToWorld());
            em.AddComponentData(vegEmbryo, new EnergyStore());
            em.AddComponentData(vegEmbryo, new EnergyFlow());
            em.AddComponentData(vegEmbryo, new LightAbsorption());
            em.AddComponentData(vegEmbryo, new Photosynthesis { Efficiency = 1 });
            em.AddComponentData(vegEmbryo, new AssignInternodeMesh { MeshName = "GreenStem" });
            em.AddComponentData(vegEmbryo, new PrimaryGrowth { GrowthRate = 0.1f, InternodeLength = 1, InternodeRadius = 0.1f });

            var leafEmbryo = em.CreateEntity();
            em.AddComponentData(leafEmbryo, new Dormant());
            em.AddComponentData(leafEmbryo, new Node());
            em.AddComponentData(leafEmbryo, new Internode());
            em.AddComponentData(leafEmbryo, new Translation());
            em.AddComponentData(leafEmbryo, new Rotation());
            em.AddComponentData(leafEmbryo, new Parent ());
            em.AddComponentData(leafEmbryo, new LocalToParent());
            em.AddComponentData(leafEmbryo, new LocalToWorld());
            em.AddComponentData(leafEmbryo, new EnergyStore());
            em.AddComponentData(leafEmbryo, new EnergyFlow());
            em.AddComponentData(leafEmbryo, new LightAbsorption());
            em.AddComponentData(leafEmbryo, new Photosynthesis { Efficiency = 1 });
            em.AddComponentData(leafEmbryo, new AssignInternodeMesh { MeshName = "GreenStem" });
            em.AddComponentData(leafEmbryo, new AssignNodeMesh { MeshName = "Leaf" });
            em.AddComponentData(leafEmbryo, new PrimaryGrowth { GrowthRate = 0.1f, InternodeLength = 0.1f, InternodeRadius = 0.1f, NodeSize = 1 });

            var budEmbryo = em.CreateEntity();
            em.AddComponentData(budEmbryo, new Dormant());
            em.AddComponentData(budEmbryo, new Node { Size = new float3(0.01f, 0.01f, 0.01f) });
            em.AddComponentData(budEmbryo, new Translation());
            em.AddComponentData(budEmbryo, new Rotation());
            em.AddComponentData(budEmbryo, new Parent());
            em.AddComponentData(budEmbryo, new LocalToParent());
            em.AddComponentData(budEmbryo, new LocalToWorld());
            em.AddComponentData(budEmbryo, new EnergyStore());
            em.AddComponentData(budEmbryo, new EnergyFlow());
            em.AddComponentData(budEmbryo, new LightAbsorption());
            var embryoBuffer = em.AddBuffer<NodeDivision>(budEmbryo);
            embryoBuffer.Add(new NodeDivision { Entity = vegEmbryo, Rotation = Quaternion.LookRotation(Vector3.forward, Vector3.right), Order = DivisionOrder.PreNode, RemainingDivisions = 10} );
            embryoBuffer.Add(new NodeDivision { Entity = leafEmbryo, Rotation = Quaternion.LookRotation(Vector3.left, Vector3.forward), Order = DivisionOrder.InPlace, RemainingDivisions = 10 });
            embryoBuffer.Add(new NodeDivision { Entity = leafEmbryo, Rotation = Quaternion.LookRotation(Vector3.right, Vector3.forward), Order = DivisionOrder.InPlace, RemainingDivisions = 10 });

            var baseNode = em.CreateEntity();
            em.AddComponentData(baseNode, new Node{Size = new float3(0.5f,0.5f,0.5f)});
            em.AddComponentData(baseNode, new Translation { Value = Singleton.LandService.ClampToTerrain(new Vector3(Random.Range(-100f, 100f), 50, Random.Range(-200f, 0f))) });
            em.AddComponentData(baseNode, new Rotation { Value = Quaternion.LookRotation(Vector3.up) });
            em.AddComponentData(baseNode, new LocalToWorld());
            em.AddComponentData(baseNode, new EnergyStore {Capacity = 1, Quantity = 1});
            em.AddComponentData(baseNode, new EnergyFlow());
            em.AddComponentData(baseNode, new LightAbsorption ());
            embryoBuffer = em.AddBuffer<NodeDivision>(baseNode);
            embryoBuffer.Add(new NodeDivision { Entity = budEmbryo, Rotation = Quaternion.LookRotation(Vector3.forward, Vector3.right), Order = DivisionOrder.PostNode});
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
