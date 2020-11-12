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

        var dna = em.CreateEntity();

        var vegNode = em.CreateEntity();
        em.AddComponentData(vegNode, new Dormant());
        em.AddComponentData(vegNode, new Node());
        em.AddComponentData(vegNode, new Internode());
        em.AddComponentData(vegNode, new Translation());
        em.AddComponentData(vegNode, new Rotation());
        em.AddComponentData(vegNode, new Parent ());
        em.AddComponentData(vegNode, new LocalToParent());
        em.AddComponentData(vegNode, new LocalToWorld());
        em.AddComponentData(vegNode, new EnergyStore());
        em.AddComponentData(vegNode, new EnergyFlow());
        em.AddComponentData(vegNode, new LightAbsorption());
        em.AddComponentData(vegNode, new Photosynthesis { Efficiency = 1 });
        em.AddComponentData(vegNode, new AssignInternodeMesh { MeshName = "GreenStem" });
        em.AddComponentData(vegNode, new PrimaryGrowth { GrowthRate = 0.1f, InternodeLength = 1, InternodeRadius = 0.1f });
        em.AddComponentData(vegNode, new DnaReference { Entity = dna });
        em.AddSharedComponentData(vegNode, new UpdateChunk());

        var leaf = em.CreateEntity();
        em.AddComponentData(leaf, new Dormant());
        em.AddComponentData(leaf, new Node());
        em.AddComponentData(leaf, new Internode());
        em.AddComponentData(leaf, new Translation());
        em.AddComponentData(leaf, new Rotation());
        em.AddComponentData(leaf, new Parent ());
        em.AddComponentData(leaf, new LocalToParent());
        em.AddComponentData(leaf, new LocalToWorld());
        em.AddComponentData(leaf, new EnergyStore());
        em.AddComponentData(leaf, new EnergyFlow());
        em.AddComponentData(leaf, new LightAbsorption());
        em.AddComponentData(leaf, new Photosynthesis { Efficiency = 1 });
        em.AddComponentData(leaf, new AssignInternodeMesh { MeshName = "GreenStem" });
        em.AddComponentData(leaf, new AssignNodeMesh { MeshName = "Leaf" });
        em.AddComponentData(leaf, new PrimaryGrowth { GrowthRate = 0.1f, InternodeLength = 0.1f, InternodeRadius = 0.1f, NodeSize = 1 });
        em.AddComponentData(leaf, new DnaReference { Entity = dna });
        em.AddSharedComponentData(leaf, new UpdateChunk());

        var bud = em.CreateEntity();
        em.AddComponentData(bud, new Dormant());
        em.AddComponentData(bud, new Node { Size = new float3(0.01f, 0.01f, 0.01f) });
        em.AddComponentData(bud, new Translation());
        em.AddComponentData(bud, new Rotation());
        em.AddComponentData(bud, new Parent());
        em.AddComponentData(bud, new LocalToParent());
        em.AddComponentData(bud, new LocalToWorld());
        em.AddComponentData(bud, new EnergyStore());
        em.AddComponentData(bud, new EnergyFlow());
        em.AddComponentData(bud, new LightAbsorption());
        em.AddComponentData(bud, new DeterministicReproductionTrigger());
        em.AddComponentData(bud, new NodeDivision {RemainingDivisions = 6, Type = NodeType.Vegetation});
        em.AddComponentData(bud, new DnaReference { Entity = dna });
        em.AddSharedComponentData(bud, new UpdateChunk());

        var sporangia = em.CreateEntity();
        em.AddComponentData(sporangia, new Dormant());
        em.AddComponentData(sporangia, new Node());
        em.AddComponentData(sporangia, new Translation());
        em.AddComponentData(sporangia, new Rotation());
        em.AddComponentData(sporangia, new Parent());
        em.AddComponentData(sporangia, new LocalToParent());
        em.AddComponentData(sporangia, new LocalToWorld());
        em.AddComponentData(sporangia, new EnergyStore());
        em.AddComponentData(sporangia, new EnergyFlow());
        em.AddComponentData(sporangia, new LightAbsorption());
        em.AddComponentData(sporangia, new AssignNodeMesh { MeshName = "Sporangia" });
        em.AddComponentData(sporangia, new PrimaryGrowth { GrowthRate = 0.1f,NodeSize = 1 });
        em.AddComponentData(sporangia, new NodeDivision { Type = NodeType.Embryo, RemainingDivisions = 15 });
        em.AddComponentData(sporangia, new DnaReference { Entity = dna });
        em.AddSharedComponentData(sporangia, new UpdateChunk());

        var spore = em.CreateEntity();
        em.AddComponentData(spore, new Dormant());
        em.AddComponentData(spore, new Node{Size = new float3(0.5f,0.5f,0.5f)});
        em.AddComponentData(spore, new Translation());
        em.AddComponentData(spore, new Rotation());
        em.AddComponentData(spore, new Parent());
        em.AddComponentData(spore, new LocalToParent());
        em.AddComponentData(spore, new LocalToWorld());
        em.AddComponentData(spore, new EnergyStore());
        em.AddComponentData(spore, new EnergyFlow());
        em.AddComponentData(spore, new LightAbsorption ());
        em.AddComponentData(spore, new WindDispersal ());
        em.AddComponentData(spore, new NodeDivision { Type = NodeType.Seedling });
        em.AddComponentData(spore, new DnaReference { Entity = dna });
        em.AddSharedComponentData(spore, new UpdateChunk());

        var embryoBuffer = em.AddBuffer<EmbryoNode>(dna);
        embryoBuffer.Add(new EmbryoNode
        {
            Entity = vegNode,
            Type = NodeType.Vegetation,
            Order = DivisionOrder.PreNode,
            Rotation = Quaternion.LookRotation(Vector3.forward, Vector3.right)
        });
        embryoBuffer.Add(new EmbryoNode
        {
            Entity = leaf,
            Type = NodeType.Vegetation,
            Order = DivisionOrder.InPlace,
            Rotation = Quaternion.LookRotation(Vector3.left, Vector3.forward)
        }); 
        embryoBuffer.Add(new EmbryoNode
        {
            Entity = leaf,
            Type = NodeType.Vegetation,
            Order = DivisionOrder.InPlace,
            Rotation = Quaternion.LookRotation(Vector3.right, Vector3.forward)
        });
        embryoBuffer.Add(new EmbryoNode
        {
            Entity = bud,
            Type = NodeType.Seedling,
            Order = DivisionOrder.PostNode,
            Rotation = Quaternion.LookRotation(Vector3.forward, Vector3.right)
        });
        embryoBuffer.Add(new EmbryoNode
        {
            Entity = sporangia,
            Type = NodeType.Reproduction,
            Order = DivisionOrder.Replace,
            Rotation = Quaternion.LookRotation(Vector3.forward, Vector3.right)
        });
        embryoBuffer.Add(new EmbryoNode
        {
            Entity = spore,
            Type = NodeType.Embryo,
            Order = DivisionOrder.PostNode,
            Rotation = Quaternion.LookRotation(Vector3.up)
        });

        for (var i = 0; i < 500; i++)
        {
            var plant = em.Instantiate(spore);
            em.RemoveComponent<Dormant>(plant);
            em.RemoveComponent<Parent>(plant);
            em.RemoveComponent<LocalToParent>(plant);
            em.SetComponentData(plant, new EnergyStore {Capacity = 1, Quantity = 1});
            em.SetComponentData(plant, new Translation { Value = Singleton.LandService.ClampToTerrain(new Vector3(Random.Range(-100f, 100f), 50, Random.Range(-200f, 0f))) });
            em.SetComponentData(plant, new Rotation { Value = Quaternion.LookRotation(Vector3.up) });
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
