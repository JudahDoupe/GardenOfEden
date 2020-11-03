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
            var dna = em.CreateEntity();

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
            em.AddComponentData(vegEmbryo, new DnaReference { Entity = dna });

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
            em.AddComponentData(leafEmbryo, new DnaReference { Entity = dna });

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
            em.AddComponentData(budEmbryo, new DeterministicReproductionTrigger());
            em.AddComponentData(budEmbryo, new NodeDivision {RemainingDivisions = 6, Type = EmbryoNodeType.Vegetation});
            em.AddComponentData(budEmbryo, new DnaReference { Entity = dna });

            var sporangiaEmbryo = em.CreateEntity();
            em.AddComponentData(sporangiaEmbryo, new Dormant());
            em.AddComponentData(sporangiaEmbryo, new Node());
            em.AddComponentData(sporangiaEmbryo, new Internode());
            em.AddComponentData(sporangiaEmbryo, new Translation());
            em.AddComponentData(sporangiaEmbryo, new Rotation());
            em.AddComponentData(sporangiaEmbryo, new Parent());
            em.AddComponentData(sporangiaEmbryo, new LocalToParent());
            em.AddComponentData(sporangiaEmbryo, new LocalToWorld());
            em.AddComponentData(sporangiaEmbryo, new EnergyStore());
            em.AddComponentData(sporangiaEmbryo, new EnergyFlow());
            em.AddComponentData(sporangiaEmbryo, new LightAbsorption());
            em.AddComponentData(sporangiaEmbryo, new AssignInternodeMesh { MeshName = "GreenStem" });
            em.AddComponentData(sporangiaEmbryo, new AssignNodeMesh { MeshName = "Sporangia" });
            em.AddComponentData(sporangiaEmbryo, new PrimaryGrowth { GrowthRate = 0.1f, InternodeLength = 0.1f, InternodeRadius = 0.1f, NodeSize = 1 });
            em.AddComponentData(sporangiaEmbryo, new DnaReference { Entity = dna });

            var spore = em.CreateEntity();
            em.AddComponentData(spore, new Node{Size = new float3(0.5f,0.5f,0.5f)});
            em.AddComponentData(spore, new Translation { Value = Singleton.LandService.ClampToTerrain(new Vector3(Random.Range(-100f, 100f), 50, Random.Range(-200f, 0f))) });
            em.AddComponentData(spore, new Rotation { Value = Quaternion.LookRotation(Vector3.up) });
            em.AddComponentData(spore, new LocalToWorld());
            em.AddComponentData(spore, new EnergyStore {Capacity = 1, Quantity = 1});
            em.AddComponentData(spore, new EnergyFlow());
            em.AddComponentData(spore, new LightAbsorption ());
            em.AddComponentData(spore, new NodeDivision { Type = EmbryoNodeType.Seedling });
            em.AddComponentData(spore, new DnaReference { Entity = dna });

            var embryoBuffer = em.AddBuffer<EmbryoNode>(dna);
            embryoBuffer.Add(new EmbryoNode
            {
                Entity = vegEmbryo,
                Type = EmbryoNodeType.Vegetation,
                Order = DivisionOrder.PreNode,
                Rotation = Quaternion.LookRotation(Vector3.forward, Vector3.right)
            });
            embryoBuffer.Add(new EmbryoNode
            {
                Entity = leafEmbryo,
                Type = EmbryoNodeType.Vegetation,
                Order = DivisionOrder.InPlace,
                Rotation = Quaternion.LookRotation(Vector3.left, Vector3.forward)
            }); 
            embryoBuffer.Add(new EmbryoNode
            {
                Entity = leafEmbryo,
                Type = EmbryoNodeType.Vegetation,
                Order = DivisionOrder.InPlace,
                Rotation = Quaternion.LookRotation(Vector3.right, Vector3.forward)
            });
            embryoBuffer.Add(new EmbryoNode
            {
                Entity = budEmbryo,
                Type = EmbryoNodeType.Seedling,
                Order = DivisionOrder.PostNode,
                Rotation = Quaternion.LookRotation(Vector3.forward, Vector3.right)
            });
            embryoBuffer.Add(new EmbryoNode
            {
                Entity = sporangiaEmbryo,
                Type = EmbryoNodeType.Reproduction,
                Order = DivisionOrder.Replace,
                Rotation = Quaternion.LookRotation(Vector3.forward, Vector3.right)
            });
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
