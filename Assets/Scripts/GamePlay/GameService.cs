using Assets.Scripts.Plants.Cleanup;
using Assets.Scripts.Plants.Environment;
using Assets.Scripts.Plants.Growth;
using Assets.Scripts.Plants.Setup;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine.Rendering;
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
            typeof(LightBlocker),
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

        //SpawnSpagooter (50);
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

    public void SpawnSpagooter(Coordinate coord)
    {
        var dna = em.CreateEntity();

        var stemMesh = em.CreateEntity(meshArchetype);
        var meshData = Singleton.RenderMeshLibrary.Library["GreenStem"];
        RenderMeshUtility.AddComponents(stemMesh, em, meshData.Desc);

        var vegNode = em.CreateEntity(plantNodeArchetype);
        em.AddComponentData(vegNode, new Internode());
        em.AddComponentData(vegNode, new LightAbsorber());
        em.AddComponentData(vegNode, new Photosynthesis { Efficiency = 1 });
        em.AddComponentData(vegNode, new AssignInternodeMesh { Entity = stemMesh });
        em.AddComponentData(vegNode, new PrimaryGrowth { GrowthRate = 0.1f, InternodeLength = 1, InternodeRadius = 0.1f });
        em.SetComponentData(vegNode, new DnaReference { Entity = dna });
        em.SetComponentData(vegNode, new Health { Value = 1 });

        var leafMesh = em.CreateEntity(meshArchetype);
        meshData = Singleton.RenderMeshLibrary.Library["Leaf"];
        RenderMeshUtility.AddComponents(leafMesh, em, meshData.Desc);

        var leaf = em.CreateEntity(plantNodeArchetype);
        em.AddComponentData(leaf, new Internode());
        em.AddComponentData(leaf, new LightAbsorber());
        em.AddComponentData(leaf, new Photosynthesis { Efficiency = 1 });
        em.AddComponentData(leaf, new AssignInternodeMesh { Entity = stemMesh });
        em.AddComponentData(leaf, new AssignNodeMesh { Entity = leafMesh });
        em.AddComponentData(leaf, new PrimaryGrowth { GrowthRate = 0.1f, InternodeLength = 0.1f, InternodeRadius = 0.1f, NodeSize = 1 });
        em.SetComponentData(leaf, new DnaReference { Entity = dna });
        em.SetComponentData(leaf, new Health { Value = 1 });

        var bud = em.CreateEntity(plantNodeArchetype);
        em.SetComponentData(bud, new Node { Size = new float3(0.01f, 0.01f, 0.01f) });
        em.AddComponentData(bud, new DeterministicReproductionTrigger());
        em.AddComponentData(bud, new NodeDivision { RemainingDivisions = 6, Type = NodeType.Vegetation, MinEnergyPressure = 0.5f});
        em.SetComponentData(bud, new DnaReference { Entity = dna });
        em.SetComponentData(bud, new Health { Value = 1 });

        var sporangiaMesh = em.CreateEntity(meshArchetype);
        meshData = Singleton.RenderMeshLibrary.Library["Sporangia"];
        RenderMeshUtility.AddComponents(sporangiaMesh, em, meshData.Desc);

        var sporangia = em.CreateEntity(plantNodeArchetype);
        em.AddComponentData(sporangia, new AssignNodeMesh { Entity = sporangiaMesh });
        em.AddComponentData(sporangia, new PrimaryGrowth { GrowthRate = 0.1f, NodeSize = 1 });
        em.AddComponentData(sporangia, new NodeDivision { Type = NodeType.Embryo, RemainingDivisions = 5, MinEnergyPressure = 0.5f});
        em.SetComponentData(sporangia, new DnaReference { Entity = dna });
        em.SetComponentData(sporangia, new Health { Value = 1 });

        var spore = em.CreateEntity(plantNodeArchetype);
        em.SetComponentData(spore, new Node { Size = new float3(0.25f, 0.25f, 0.25f) });
        em.AddComponentData(spore, new WindDispersal());
        em.AddComponentData(spore, new UnparentDormancyTrigger());
        em.AddComponentData(spore, new NodeDivision { Type = NodeType.Seedling });
        em.SetComponentData(spore, new DnaReference { Entity = dna });
        em.SetComponentData(spore, new Health { Value = 1 });

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

        var plant = em.Instantiate(spore);
        em.RemoveComponent<Dormant>(plant);
        em.RemoveComponent<Parent>(plant);
        em.RemoveComponent<LocalToParent>(plant);
        em.SetComponentData(plant, new EnergyStore { Capacity = 0.5f, Quantity = 0.5f });
        em.SetComponentData(plant, new Translation { Value = coord.xyz });
        em.SetComponentData(plant, new Rotation { Value = Quaternion.LookRotation(Vector3.Normalize(coord.xyz)) });
    }

    public void SpawnCooksporangia(Coordinate coord)
    {
        var dna = em.CreateEntity();

        var stemMesh = em.CreateEntity(meshArchetype);
        var meshData = Singleton.RenderMeshLibrary.Library["GreenStem"];
        RenderMeshUtility.AddComponents(stemMesh, em, meshData.Desc);

        var vegNode = em.CreateEntity(plantNodeArchetype);
        em.AddComponentData(vegNode, new Internode());
        em.AddComponentData(vegNode, new LightAbsorber());
        em.AddComponentData(vegNode, new Photosynthesis { Efficiency = 1 });
        em.AddComponentData(vegNode, new AssignInternodeMesh { Entity = stemMesh });
        em.AddComponentData(vegNode, new PrimaryGrowth { GrowthRate = 0.1f, InternodeLength = 0.5f, InternodeRadius = 0.1f });
        em.SetComponentData(vegNode, new DnaReference { Entity = dna });
        em.SetComponentData(vegNode, new Health { Value = 1 });

        var bud = em.CreateEntity(plantNodeArchetype);
        em.SetComponentData(bud, new Node { Size = new float3(0.1f, 0.1f, 0.1f) });
        em.AddComponentData(bud, new AnnualReproductionTrigger { Month = 6 });
        em.AddComponentData(bud, new NodeDivision { Type = NodeType.Vegetation, MinEnergyPressure = 0.9f});
        em.SetComponentData(bud, new DnaReference { Entity = dna });
        em.SetComponentData(bud, new Health { Value = 1 });

        var sporangiaMesh = em.CreateEntity(meshArchetype);
        meshData = Singleton.RenderMeshLibrary.Library["Sporangia"];
        RenderMeshUtility.AddComponents(sporangiaMesh, em, meshData.Desc);

        var sporangia = em.CreateEntity(plantNodeArchetype);
        em.AddComponentData(sporangia, new AssignNodeMesh { Entity = sporangiaMesh });
        em.AddComponentData(sporangia, new PrimaryGrowth { GrowthRate = 0.1f, NodeSize = 0.25f });
        em.AddComponentData(sporangia, new NodeDivision { Type = NodeType.Embryo, RemainingDivisions = 5, MinEnergyPressure = 0.1f });
        em.SetComponentData(sporangia, new DnaReference { Entity = dna });
        em.SetComponentData(sporangia, new Health { Value = 1 });

        var spore = em.CreateEntity(plantNodeArchetype);
        em.SetComponentData(spore, new Node { Size = new float3(0.25f, 0.25f, 0.25f) });
        em.AddComponentData(spore, new WindDispersal());
        em.AddComponentData(spore, new UnparentDormancyTrigger());
        em.AddComponentData(spore, new NodeDivision { Type = NodeType.Seedling });
        em.SetComponentData(spore, new DnaReference { Entity = dna });
        em.SetComponentData(spore, new Health { Value = 1 });

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

        coord.Altitude = Singleton.Land.SampleHeight(coord);
        var plant = em.Instantiate(spore);
        em.RemoveComponent<Dormant>(plant);
        em.RemoveComponent<Parent>(plant);
        em.RemoveComponent<LocalToParent>(plant);
        em.SetComponentData(plant, new EnergyStore { Capacity = 0.25f, Quantity = 0.25f });
        em.SetComponentData(plant, new Translation { Value = coord.xyz });
        em.SetComponentData(plant, new Rotation { Value = Quaternion.LookRotation(Vector3.Normalize(coord.xyz)) });
    }
}
