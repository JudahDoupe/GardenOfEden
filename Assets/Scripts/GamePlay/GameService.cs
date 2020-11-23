using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Assets.Scripts.Plants.Systems;
using Unity.Mathematics;
using Unity.Rendering;
using Random = UnityEngine.Random;

public class GameService : MonoBehaviour
{
    public bool IsGameInProgress { get; private set; }

    private EntityManager em;
    private EntityArchetype plantNodeArchetype;
    private EntityArchetype meshArchetype;

    private void Start()
    {
        em = World.DefaultGameObjectInjectionWorld.EntityManager;

        plantNodeArchetype = em.CreateArchetype(
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
            typeof(DnaReference),
            typeof(Health)
        );

        meshArchetype = em.CreateArchetype(
            typeof(RenderMesh),
            typeof(RenderBounds),
            typeof(Translation),
            typeof(Rotation),
            typeof(NonUniformScale),
            typeof(LocalToWorld)
        );

        StartGame();
    }

    private void StartGame()
    {
        IsGameInProgress = true;

        SpawnSpagooter (50);
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

    public void SpawnSpagooter(int amount)
    {
        var dna = em.CreateEntity();

        var stemMesh = em.CreateEntity(meshArchetype);
        var meshData = Singleton.RenderMeshLibrary.Library["GreenStem"];
        em.SetSharedComponentData(stemMesh, meshData.Mesh);
        em.SetComponentData(stemMesh, meshData.Bounds);
        em.SetName(stemMesh, "StemMesh");

        var vegNode = em.CreateEntity(plantNodeArchetype);
        em.AddComponentData(vegNode, new Internode());
        em.AddComponentData(vegNode, new Photosynthesis { Efficiency = 1 });
        em.AddComponentData(vegNode, new AssignInternodeMesh { Entity = stemMesh });
        em.AddComponentData(vegNode, new PrimaryGrowth { GrowthRate = 0.1f, InternodeLength = 1, InternodeRadius = 0.1f });
        em.SetComponentData(vegNode, new DnaReference { Entity = dna });
        em.SetComponentData(vegNode, new Health { Value = 1 });
        em.SetName(vegNode, "Node");

        var leafMesh = em.CreateEntity(meshArchetype);
        meshData = Singleton.RenderMeshLibrary.Library["Leaf"];
        em.SetSharedComponentData(leafMesh, meshData.Mesh);
        em.SetComponentData(leafMesh, meshData.Bounds);
        em.SetName(leafMesh, "LeafMesh");

        var leaf = em.CreateEntity(plantNodeArchetype);
        em.AddComponentData(leaf, new Internode());
        em.AddComponentData(leaf, new Photosynthesis { Efficiency = 1 });
        em.AddComponentData(leaf, new AssignInternodeMesh { Entity = stemMesh });
        em.AddComponentData(leaf, new AssignNodeMesh { Entity = leafMesh });
        em.AddComponentData(leaf, new PrimaryGrowth { GrowthRate = 0.1f, InternodeLength = 0.1f, InternodeRadius = 0.1f, NodeSize = 1 });
        em.SetComponentData(leaf, new DnaReference { Entity = dna });
        em.SetComponentData(leaf, new Health { Value = 1 });
        em.SetName(leaf, "Leaf");

        var bud = em.CreateEntity(plantNodeArchetype);
        em.SetComponentData(bud, new Node { Size = new float3(0.01f, 0.01f, 0.01f) });
        em.AddComponentData(bud, new DeterministicReproductionTrigger());
        em.AddComponentData(bud, new NodeDivision { RemainingDivisions = 6, Type = NodeType.Vegetation, MinEnergyPressure = 0.5f});
        em.SetComponentData(bud, new DnaReference { Entity = dna });
        em.SetComponentData(bud, new Health { Value = 1 });
        em.SetName(bud, "Bud");

        var sporangiaMesh = em.CreateEntity(meshArchetype);
        meshData = Singleton.RenderMeshLibrary.Library["Sporangia"];
        em.SetSharedComponentData(sporangiaMesh, meshData.Mesh);
        em.SetComponentData(sporangiaMesh, meshData.Bounds);
        em.SetName(sporangiaMesh, "SporangiaMesh");

        var sporangia = em.CreateEntity(plantNodeArchetype);
        em.AddComponentData(sporangia, new AssignNodeMesh { Entity = sporangiaMesh });
        em.AddComponentData(sporangia, new PrimaryGrowth { GrowthRate = 0.1f, NodeSize = 1 });
        em.AddComponentData(sporangia, new NodeDivision { Type = NodeType.Embryo, RemainingDivisions = 5, MinEnergyPressure = 0.5f});
        em.SetComponentData(sporangia, new DnaReference { Entity = dna });
        em.SetComponentData(sporangia, new Health { Value = 1 });
        em.SetName(sporangia, "Sporangia");

        var spore = em.CreateEntity(plantNodeArchetype);
        em.SetComponentData(spore, new Node { Size = new float3(0.25f, 0.25f, 0.25f) });
        em.AddComponentData(spore, new WindDispersal());
        em.AddComponentData(spore, new UnparentDormancyTrigger());
        em.AddComponentData(spore, new NodeDivision { Type = NodeType.Seedling });
        em.SetComponentData(spore, new DnaReference { Entity = dna });
        em.SetComponentData(spore, new Health { Value = 1 });
        em.SetName(spore, "Spore");

        var embryoBuffer = em.AddBuffer<EmbryoNode>(dna);
        embryoBuffer.Add(new EmbryoNode
        {
            Entity = vegNode,
            Type = NodeType.Vegetation,
            Order = DivisionOrder.PreNode
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
        });
        embryoBuffer.Add(new EmbryoNode
        {
            Entity = spore,
            Type = NodeType.Embryo,
            Order = DivisionOrder.PostNode,
            Rotation = Quaternion.LookRotation(Vector3.up),
            RemainDormantAfterDivision = true
        });

        for (var i = 0; i < amount; i++)
        {
            var plant = em.Instantiate(spore);
            em.RemoveComponent<Dormant>(plant);
            em.RemoveComponent<Parent>(plant);
            em.RemoveComponent<LocalToParent>(plant);
            em.SetComponentData(plant, new EnergyStore { Capacity = 0.5f, Quantity = 0.5f });
            em.SetComponentData(plant, new Translation { Value = Singleton.LandService.ClampToTerrain(new Vector3(Random.Range(50f, 350f), 50, Random.Range(50f, 350f))) });
            em.SetComponentData(plant, new Rotation { Value = Quaternion.LookRotation(Vector3.up) });
        }
    }

    public void SpawnCooksporangia(int amount)
    {
        var dna = em.CreateEntity();

        var stemMesh = em.CreateEntity(meshArchetype);
        var meshData = Singleton.RenderMeshLibrary.Library["GreenStem"];
        em.SetSharedComponentData(stemMesh, meshData.Mesh);
        em.SetComponentData(stemMesh, meshData.Bounds);
        em.SetName(stemMesh, "StemMesh");

        var vegNode = em.CreateEntity(plantNodeArchetype);
        em.AddComponentData(vegNode, new Internode());
        em.AddComponentData(vegNode, new Photosynthesis { Efficiency = 1 });
        em.AddComponentData(vegNode, new AssignInternodeMesh { Entity = stemMesh });
        em.AddComponentData(vegNode, new PrimaryGrowth { GrowthRate = 0.1f, InternodeLength = 0.5f, InternodeRadius = 0.1f });
        em.SetComponentData(vegNode, new DnaReference { Entity = dna });
        em.SetComponentData(vegNode, new Health { Value = 1 });
        em.SetName(vegNode, "Node");

        var bud = em.CreateEntity(plantNodeArchetype);
        em.SetComponentData(bud, new Node { Size = new float3(0.1f, 0.1f, 0.1f) });
        em.AddComponentData(bud, new AnnualReproductionTrigger { Month = 6 });
        em.AddComponentData(bud, new NodeDivision { Type = NodeType.Vegetation, MinEnergyPressure = 0.9f});
        em.SetComponentData(bud, new DnaReference { Entity = dna });
        em.SetComponentData(bud, new Health { Value = 1 });
        em.SetName(bud, "Bud");

        var sporangiaMesh = em.CreateEntity(meshArchetype);
        meshData = Singleton.RenderMeshLibrary.Library["Sporangia"];
        em.SetSharedComponentData(sporangiaMesh, meshData.Mesh);
        em.SetComponentData(sporangiaMesh, meshData.Bounds);
        em.SetName(sporangiaMesh, "SporangiaMesh");

        var sporangia = em.CreateEntity(plantNodeArchetype);
        em.AddComponentData(sporangia, new AssignNodeMesh { Entity = sporangiaMesh });
        em.AddComponentData(sporangia, new PrimaryGrowth { GrowthRate = 0.1f, NodeSize = 0.25f });
        em.AddComponentData(sporangia, new NodeDivision { Type = NodeType.Embryo, RemainingDivisions = 5, MinEnergyPressure = 0.1f });
        em.SetComponentData(sporangia, new DnaReference { Entity = dna });
        em.SetComponentData(sporangia, new Health { Value = 1 });
        em.SetName(sporangia, "Sporangia");

        var spore = em.CreateEntity(plantNodeArchetype);
        em.SetComponentData(spore, new Node { Size = new float3(0.25f, 0.25f, 0.25f) });
        em.AddComponentData(spore, new WindDispersal());
        em.AddComponentData(spore, new UnparentDormancyTrigger());
        em.AddComponentData(spore, new NodeDivision { Type = NodeType.Seedling });
        em.SetComponentData(spore, new DnaReference { Entity = dna });
        em.SetComponentData(spore, new Health { Value = 1 });
        em.SetName(spore, "Spore");

        var embryoBuffer = em.AddBuffer<EmbryoNode>(dna);
        embryoBuffer.Add(new EmbryoNode
        {
            Entity = bud,
            Type = NodeType.Seedling,
            Order = DivisionOrder.PostNode,
            Rotation = Quaternion.LookRotation(Vector3.forward, Vector3.right)
        });
        embryoBuffer.Add(new EmbryoNode
        {
            Entity = vegNode,
            Type = NodeType.Vegetation,
            Order = DivisionOrder.Replace,
        });
        embryoBuffer.Add(new EmbryoNode
        {
            Entity = bud,
            Type = NodeType.Vegetation,
            Order = DivisionOrder.PostNode,
            Rotation = Quaternion.LookRotation(Vector3.right + Vector3.forward * 2, Vector3.forward)
        });
        embryoBuffer.Add(new EmbryoNode
        {
            Entity = bud,
            Type = NodeType.Vegetation,
            Order = DivisionOrder.PostNode,
            Rotation = Quaternion.LookRotation(Vector3.left + Vector3.forward * 2, Vector3.forward)
        });
        embryoBuffer.Add(new EmbryoNode
        {
            Entity = sporangia,
            Type = NodeType.Reproduction,
            Order = DivisionOrder.Replace,
        });
        embryoBuffer.Add(new EmbryoNode
        {
            Entity = spore,
            Type = NodeType.Embryo,
            Order = DivisionOrder.PostNode,
            Rotation = Quaternion.LookRotation(Vector3.up),
            RemainDormantAfterDivision = true
        });

        for (var i = 0; i < amount; i++)
        {
            var plant = em.Instantiate(spore);
            em.RemoveComponent<Dormant>(plant);
            em.RemoveComponent<Parent>(plant);
            em.RemoveComponent<LocalToParent>(plant);
            em.SetComponentData(plant, new EnergyStore { Capacity = 0.25f, Quantity = 0.25f });
            em.SetComponentData(plant, new Translation { Value = Singleton.LandService.ClampToTerrain(new Vector3(Random.Range(50f, 350f), 50, Random.Range(50f, 350f))) });
            em.SetComponentData(plant, new Rotation { Value = Quaternion.LookRotation(Vector3.up) });
        }
    }
}
