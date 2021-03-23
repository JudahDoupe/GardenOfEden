using Assets.Scripts.Plants.Cleanup;
using Assets.Scripts.Plants.Environment;
using Assets.Scripts.Plants.Growth;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Assets.Scripts.Plants.Dna
{
    public enum NodeType
    {
        Dna,
        Bud,
        Vegetation,
        EnergyProduction,
        Reproduction,
        Embryo,
    }
    public enum GeneType
    {
        Morphology,
        Pigment,
        Dormancy,
        Growth
    }
    public struct DnaReference : IComponentData
    {
        public Entity Entity;
    }

    public class Dna
    {
        public List<IGene> Genes = new List<IGene>();
        public List<NodeType> NodeTypes => Genes.SelectMany(x => x.RequiredNodes).Distinct().ToList();

        public void SetGene(IGene gene)
        {
            Genes.RemoveAll(x => x.GeneType == gene.GeneType && x.NodeType == gene.NodeType);
            Genes.Add(gene);
        }

        public Entity Spawn(Coordinate coord)
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            var protoNodes = new Dictionary<NodeType, Entity>();

            foreach (var nodeType in NodeTypes)
            {
                protoNodes[nodeType] = em.CreateEntity(GameService.plantNodeArchetype);
            }
            foreach (var gene in Genes)
            {
                gene.Apply(protoNodes);
            }

            var plant = em.Instantiate(protoNodes[NodeType.Embryo]); 
            em.SetSharedComponentData(plant, Singleton.LoadBalancer.ActiveEntityChunk);
            em.RemoveComponent<Dormant>(plant);
            em.RemoveComponent<Parent>(plant);
            em.RemoveComponent<LocalToParent>(plant);
            em.SetComponentData(plant, new EnergyStore { Capacity = 0.5f, Quantity = 0.5f });
            em.SetComponentData(plant, new Translation { Value = coord.xyz });
            em.SetComponentData(plant, new Rotation { Value = Quaternion.LookRotation(Vector3.Normalize(coord.xyz)) });
            return protoNodes[NodeType.Dna];
        }
    }

    public interface IGene
    {
        NodeType NodeType { get; }
        GeneType GeneType { get; }
        List<NodeType> RequiredNodes { get; }
        void Apply(Dictionary<NodeType, Entity> nodes);
    }

    public class LeafGene : IGene
    {
        public NodeType NodeType => NodeType.EnergyProduction;
        public GeneType GeneType => GeneType.Morphology;

        public List<NodeType> RequiredNodes => new List<NodeType> { NodeType.EnergyProduction };

        public void Apply(Dictionary<NodeType, Entity> nodes)
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            var entity = nodes[NodeType];

            em.AddComponentData(entity, new LightAbsorber());
            em.AddComponentData(entity, new Photosynthesis { Efficiency = 1 });
            em.AddComponentData(entity, new AssignInternodeMesh { Entity = Singleton.RenderMeshLibrary.Library["GreenStem"].Entity });
            em.AddComponentData(entity, new AssignNodeMesh { Entity = Singleton.RenderMeshLibrary.Library["Leaf"].Entity });
            em.AddComponentData(entity, new PrimaryGrowth { DaysToMature = 4, InternodeLength = 0.1f, InternodeRadius = 0.1f, NodeSize = new float3(1, 0.1f, 1) });
            em.SetComponentData(entity, new DnaReference { Entity = nodes[NodeType.Dna] });
            em.SetComponentData(entity, new Metabolism { Resting = 0.01f });
            em.SetComponentData(entity, new Health { Value = 1 });
        }
    }

    public class VegGene : IGene
    {
        public NodeType NodeType => NodeType.Vegetation;
        public GeneType GeneType => GeneType.Morphology;

        public List<NodeType> RequiredNodes => new List<NodeType> { NodeType.Vegetation };

        public void Apply(Dictionary<NodeType, Entity> nodes)
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            var entity = nodes[NodeType];

            em.AddComponentData(entity, new LightAbsorber());
            em.AddComponentData(entity, new Photosynthesis { Efficiency = 1 });
            em.AddComponentData(entity, new AssignInternodeMesh { Entity = Singleton.RenderMeshLibrary.Library["GreenStem"].Entity });
            em.AddComponentData(entity, new PrimaryGrowth { DaysToMature = 3, InternodeLength = 1, InternodeRadius = 0.1f });
            em.SetComponentData(entity, new DnaReference { Entity = nodes[NodeType.Dna] });
            em.SetComponentData(entity, new Metabolism { Resting = 0.1f });
            em.SetComponentData(entity, new Health { Value = 1 });
        }
    }

    public class BudGene : IGene
    {
        public NodeType NodeType => NodeType.Bud;
        public GeneType GeneType => GeneType.Morphology;

        public List<NodeType> RequiredNodes => new List<NodeType> { NodeType.Bud };

        public void Apply(Dictionary<NodeType, Entity> nodes)
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            var entity = nodes[NodeType];

            em.SetComponentData(entity, new Node { Size = new float3(0.01f, 0.01f, 0.01f) });
            em.AddComponentData(entity, new DeterministicReproductionTrigger());
            em.AddComponentData(entity, new NodeDivision { RemainingDivisions = 6, Stage = LifeStage.Vegetation, MinEnergyPressure = 0.8f });
            em.SetComponentData(entity, new DnaReference { Entity = nodes[NodeType.Dna] });
            em.SetComponentData(entity, new Metabolism { Resting = 0.01f });
            em.SetComponentData(entity, new Health { Value = 1 });
        }
    }

    public class SporangiaGene : IGene
    {
        public NodeType NodeType => NodeType.Reproduction;
        public GeneType GeneType => GeneType.Morphology;

        public List<NodeType> RequiredNodes => new List<NodeType> { NodeType.Reproduction };

        public void Apply(Dictionary<NodeType, Entity> nodes)
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            var entity = nodes[NodeType];

            em.AddComponentData(entity, new AssignNodeMesh { Entity = Singleton.RenderMeshLibrary.Library["Sporangia"].Entity });
            em.AddComponentData(entity, new PrimaryGrowth { DaysToMature = 8, NodeSize = 0.5f });
            em.AddComponentData(entity, new NodeDivision { Stage = LifeStage.Embryo, RemainingDivisions = 5, MinEnergyPressure = 0.8f });
            em.SetComponentData(entity, new DnaReference { Entity = nodes[NodeType.Dna] });
            em.SetComponentData(entity, new Metabolism { Resting = 0.01f });
            em.SetComponentData(entity, new Health { Value = 1 });
        }
    }

    public class SporeGene : IGene
    {
        public NodeType NodeType => NodeType.Embryo;
        public GeneType GeneType => GeneType.Morphology;

        public List<NodeType> RequiredNodes => new List<NodeType> { NodeType.Embryo };

        public void Apply(Dictionary<NodeType, Entity> nodes)
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            var entity = nodes[NodeType];

            em.SetComponentData(entity, new Node { Size = new float3(0.25f, 0.25f, 0.25f) });
            em.AddComponentData(entity, new WindDispersal());
            em.AddComponentData(entity, new ParentDormancyTrigger { IsDormantWhenParented = true, IsDormantWhenUnparented = false });
            em.AddComponentData(entity, new NodeDivision { Stage = LifeStage.Seedling });
            em.SetComponentData(entity, new DnaReference { Entity = nodes[NodeType.Dna] });
            em.SetComponentData(entity, new Metabolism { Resting = 0.001f });
            em.SetComponentData(entity, new Health { Value = 1 });
            em.RemoveComponent<LightBlocker>(entity);
        }
    }

    public class StraightGrowthGene : IGene
    {
        public NodeType NodeType => NodeType.Dna;

        public GeneType GeneType => GeneType.Morphology;

        public List<NodeType> RequiredNodes => new List<NodeType> { NodeType.Dna };

        public void Apply(Dictionary<NodeType, Entity> nodes)
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            var entity = nodes[NodeType];
            var divisionInstructions = em.AddBuffer<DivisionInstruction>(entity);
            divisionInstructions.Add(new DivisionInstruction
            {
                Entity = nodes[NodeType.Vegetation],
                Stage = LifeStage.Vegetation,
                Order = DivisionOrder.PreNode
            });
            divisionInstructions.Add(new DivisionInstruction
            {
                Entity = nodes[NodeType.EnergyProduction],
                Stage = LifeStage.Vegetation,
                Order = DivisionOrder.InPlace,
                Rotation = Quaternion.LookRotation(Vector3.left, Vector3.forward)
            });
            divisionInstructions.Add(new DivisionInstruction
            {
                Entity = nodes[NodeType.EnergyProduction],
                Stage = LifeStage.Vegetation,
                Order = DivisionOrder.InPlace,
                Rotation = Quaternion.LookRotation(Vector3.right, Vector3.forward)
            });
            divisionInstructions.Add(new DivisionInstruction
            {
                Entity = nodes[NodeType.Bud],
                Stage = LifeStage.Seedling,
                Order = DivisionOrder.PostNode,
                Rotation = Quaternion.LookRotation(Vector3.forward, Vector3.right)
            });
            divisionInstructions.Add(new DivisionInstruction
            {
                Entity = nodes[NodeType.Reproduction],
                Stage = LifeStage.Reproduction,
                Order = DivisionOrder.Replace,
            });
            divisionInstructions.Add(new DivisionInstruction
            {
                Entity = nodes[NodeType.Embryo],
                Stage = LifeStage.Embryo,
                Order = DivisionOrder.PostNode,
                Rotation = Quaternion.LookRotation(Vector3.up)
            });
        }
    }
}
