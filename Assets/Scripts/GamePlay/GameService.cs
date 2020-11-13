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

        SpawnSpagooter();
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

    public void SpawnSpagooter()
    {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;

        var plantNodeArchetype = em.CreateArchetype(
            typeof(Node),
            typeof(Translation),
            typeof(Rotation),
            typeof(Parent),
            typeof(LocalToParent),
            typeof(LocalToWorld),
            typeof(EnergyStore),
            typeof(EnergyFlow),
            typeof(LightAbsorption),
            typeof(Dormant),
            typeof(UpdateChunk),
            typeof(DnaReference)
        );


        var dna = em.CreateEntity();

        var vegNode = em.CreateEntity(plantNodeArchetype);
        em.AddComponentData(vegNode, new Internode());
        em.AddComponentData(vegNode, new Photosynthesis { Efficiency = 1 });
        em.AddComponentData(vegNode, new AssignInternodeMesh { MeshName = "GreenStem" });
        em.AddComponentData(vegNode, new PrimaryGrowth { GrowthRate = 0.1f, InternodeLength = 1, InternodeRadius = 0.1f });
        em.SetComponentData(vegNode, new DnaReference { Entity = dna });

        var leaf = em.CreateEntity(plantNodeArchetype);
        em.AddComponentData(leaf, new Internode());
        em.AddComponentData(leaf, new Photosynthesis { Efficiency = 1 });
        em.AddComponentData(leaf, new AssignInternodeMesh { MeshName = "GreenStem" });
        em.AddComponentData(leaf, new AssignNodeMesh { MeshName = "Leaf" });
        em.AddComponentData(leaf, new PrimaryGrowth { GrowthRate = 0.1f, InternodeLength = 0.1f, InternodeRadius = 0.1f, NodeSize = 1 });
        em.SetComponentData(leaf, new DnaReference { Entity = dna });

        var bud = em.CreateEntity(plantNodeArchetype);
        em.SetComponentData(bud, new Node { Size = new float3(0.01f, 0.01f, 0.01f) });
        em.AddComponentData(bud, new DeterministicReproductionTrigger());
        em.AddComponentData(bud, new NodeDivision { RemainingDivisions = 6, Type = NodeType.Vegetation });
        em.SetComponentData(bud, new DnaReference { Entity = dna });

        var sporangia = em.CreateEntity(plantNodeArchetype);
        em.AddComponentData(sporangia, new AssignNodeMesh { MeshName = "Sporangia" });
        em.AddComponentData(sporangia, new PrimaryGrowth { GrowthRate = 0.1f, NodeSize = 1 });
        em.AddComponentData(sporangia, new NodeDivision { Type = NodeType.Embryo, RemainingDivisions = 15 });
        em.SetComponentData(sporangia, new DnaReference { Entity = dna });

        var spore = em.CreateEntity(plantNodeArchetype);
        em.SetComponentData(spore, new Node { Size = new float3(0.25f, 0.25f, 0.25f) });
        em.AddComponentData(spore, new WindDispersal());
        em.AddComponentData(spore, new NodeDivision { Type = NodeType.Seedling });
        em.SetComponentData(spore, new DnaReference { Entity = dna });

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
            em.SetComponentData(plant, new EnergyStore { Capacity = 0.5f, Quantity = 0.5f });
            em.SetComponentData(plant, new Translation { Value = Singleton.LandService.ClampToTerrain(new Vector3(Random.Range(-100f, 100f), 50, Random.Range(-200f, 0f))) });
            em.SetComponentData(plant, new Rotation { Value = Quaternion.LookRotation(Vector3.up) });
        }
    }
}
