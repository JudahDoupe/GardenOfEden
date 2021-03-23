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
    public struct DnaReference : IComponentData
    {
        public Entity Entity;
    }

    public class Dna
    {
        private List<NodeType> NodeTypes => Genes.SelectMany(x => x.NodeDependencies).Distinct().ToList();
        private List<IGene> Genes = new List<IGene>();

        public Dna(params IGene[] genes)
        {
            foreach (var gene in genes)
            {
                SetGene(gene);
            }
            ResolveDependencies();
        }

        public void SetGene(IGene gene)
        {
            Genes.RemoveAll(x => x.GeneType == gene.GeneType);
            Genes.Add(gene);
        }

        public Entity Spawn(Coordinate coord)
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            var protoNodes = new Dictionary<NodeType, Entity>();

            foreach (var nodeType in NodeTypes)
            {
                protoNodes[nodeType] = em.CreateEntity(DnaService.PlantNodeArchetype);
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

        private void ResolveDependencies()
        {
            var dependencies = new List<GeneType> { GeneType.ReproductionMorphology };

            do
            {
                foreach (var geneType in dependencies)
                {
                    if (!Genes.Any(x => x.GeneType == geneType))
                        Genes.Add(DnaService.GetDefaultGene(geneType));
                }

                foreach (var dependency in Genes.SelectMany(x => x.GeneDependencies))
                {
                    if (!dependencies.Contains(dependency)) 
                        dependencies.Add(dependency);
                }
            } while (!dependencies.All(geneType => Genes.Any(g => g.GeneType == geneType)));
        }
    }

    public class LeafGene : IGene
    {
        public GeneType GeneType => GeneType.EnergyProductionMorphology;

        public List<NodeType> NodeDependencies => new List<NodeType> { NodeType.Dna, NodeType.EnergyProduction };
        public List<GeneType> GeneDependencies => new List<GeneType> { };

        public void Apply(Dictionary<NodeType, Entity> nodes)
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            var entity = nodes[NodeType.EnergyProduction];

            em.AddComponentData(entity, new LightAbsorber());
            em.AddComponentData(entity, new Photosynthesis { Efficiency = 1 });
            em.AddComponentData(entity, new AssignInternodeMesh { Entity = Singleton.RenderMeshLibrary.Library["GreenStem"].Entity });
            em.AddComponentData(entity, new AssignNodeMesh { Entity = Singleton.RenderMeshLibrary.Library["Leaf"].Entity });
            em.AddComponentData(entity, new PrimaryGrowth { DaysToMature = 4, InternodeLength = 0.1f, InternodeRadius = 0.1f, NodeSize = new float3(1, 0.1f, 1) });
            em.SetComponentData(entity, new DnaReference { Entity = nodes[NodeType.Dna] });
            em.SetComponentData(entity, new Parent { Value = nodes[NodeType.Dna] });
            em.SetComponentData(entity, new Metabolism { Resting = 0.01f });
            em.SetComponentData(entity, new Health { Value = 1 });
        }
    }

    public class SporangiaGene : IGene
    {
        public GeneType GeneType => GeneType.ReproductionMorphology;

        public List<NodeType> NodeDependencies => new List<NodeType> { NodeType.Reproduction, NodeType.Embryo, NodeType.Dna };
        public List<GeneType> GeneDependencies => new List<GeneType> { GeneType.VegetationMorphology };

        public void Apply(Dictionary<NodeType, Entity> nodes)
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            var sporangia = nodes[NodeType.Reproduction];
            var spore = nodes[NodeType.Embryo];
            var dna = nodes[NodeType.Dna];

            em.AddComponentData(sporangia, new AssignNodeMesh { Entity = Singleton.RenderMeshLibrary.Library["Sporangia"].Entity });
            em.AddComponentData(sporangia, new PrimaryGrowth { DaysToMature = 8, NodeSize = 0.5f });
            em.AddComponentData(sporangia, new NodeDivision { Stage = LifeStage.Embryo, RemainingDivisions = 5, MinEnergyPressure = 0.8f });
            em.SetComponentData(sporangia, new DnaReference { Entity = nodes[NodeType.Dna] });
            em.SetComponentData(sporangia, new Parent { Value = nodes[NodeType.Dna] });
            em.SetComponentData(sporangia, new Metabolism { Resting = 0.01f });
            em.SetComponentData(sporangia, new Health { Value = 1 });

            em.SetComponentData(spore, new Node { Size = new float3(0.25f, 0.25f, 0.25f) });
            em.AddComponentData(spore, new WindDispersal());
            em.AddComponentData(spore, new ParentDormancyTrigger { IsDormantWhenParented = true, IsDormantWhenUnparented = false });
            em.AddComponentData(spore, new NodeDivision { Stage = LifeStage.Seedling });
            em.SetComponentData(spore, new DnaReference { Entity = nodes[NodeType.Dna] });
            em.SetComponentData(spore, new Parent { Value = nodes[NodeType.Dna] });
            em.SetComponentData(spore, new Metabolism { Resting = 0.001f });
            em.SetComponentData(spore, new Health { Value = 1 });
            em.RemoveComponent<LightBlocker>(spore);

            var divisionInstructions = em.HasComponent<DivisionInstruction>(dna)
                ? em.GetBuffer<DivisionInstruction>(dna)
                : em.AddBuffer<DivisionInstruction>(dna);
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

    public class StraightGrowthGene : IGene
    {
        public GeneType GeneType => GeneType.VegetationMorphology;
        public List<NodeType> NodeDependencies => new List<NodeType> { NodeType.Dna, NodeType.Bud, NodeType.Vegetation, NodeType.EnergyProduction };
        public List<GeneType> GeneDependencies => new List<GeneType> { GeneType.EnergyProductionMorphology, GeneType.ReproductionMorphology };

        public void Apply(Dictionary<NodeType, Entity> nodes)
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            var vegetation = nodes[NodeType.Vegetation];
            var bud = nodes[NodeType.Bud];
            var dna = nodes[NodeType.Dna];

            em.AddComponentData(vegetation, new LightAbsorber());
            em.AddComponentData(vegetation, new Photosynthesis { Efficiency = 1 });
            em.AddComponentData(vegetation, new AssignInternodeMesh { Entity = Singleton.RenderMeshLibrary.Library["GreenStem"].Entity });
            em.AddComponentData(vegetation, new PrimaryGrowth { DaysToMature = 3, InternodeLength = 1, InternodeRadius = 0.1f });
            em.SetComponentData(vegetation, new DnaReference { Entity = nodes[NodeType.Dna] });
            em.SetComponentData(vegetation, new Parent { Value = nodes[NodeType.Dna] });
            em.SetComponentData(vegetation, new Metabolism { Resting = 0.1f });
            em.SetComponentData(vegetation, new Health { Value = 1 });

            em.SetComponentData(bud, new Node { Size = new float3(0.01f, 0.01f, 0.01f) });
            em.AddComponentData(bud, new DeterministicReproductionTrigger());
            em.AddComponentData(bud, new NodeDivision { RemainingDivisions = 6, Stage = LifeStage.Vegetation, MinEnergyPressure = 0.8f });
            em.SetComponentData(bud, new DnaReference { Entity = nodes[NodeType.Dna] });
            em.SetComponentData(bud, new Parent { Value = nodes[NodeType.Dna] });
            em.SetComponentData(bud, new Metabolism { Resting = 0.01f });
            em.SetComponentData(bud, new Health { Value = 1 });

            var divisionInstructions = em.HasComponent<DivisionInstruction>(dna) 
                ? em.GetBuffer<DivisionInstruction>(dna)
                : em.AddBuffer<DivisionInstruction>(dna);
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
        }
    }
}
