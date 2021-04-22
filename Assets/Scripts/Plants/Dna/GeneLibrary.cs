using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Plants.Cleanup;
using Assets.Scripts.Plants.Environment;
using Assets.Scripts.Plants.Growth;
using QuikGraph;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Scripts.Plants.Dna
{
    public class GeneLibrary
    {
        private readonly Dictionary<string, Gene> _genes = new Dictionary<string, Gene>();
        private readonly AdjacencyGraph<string, Edge<string>> _graph = new AdjacencyGraph<string, Edge<string>>();
        private readonly Dictionary<GeneCategory, Dictionary<GeneType, string>> _defaultGenes = new Dictionary<GeneCategory, Dictionary<GeneType, string>>();

        public void AddGene(Gene gene)
        {
            _genes[gene.Name] = gene;
            if (!_defaultGenes.ContainsKey(gene.Category))
            {
                _defaultGenes[gene.Category] = new Dictionary<GeneType, string>();
            }
            if (!_defaultGenes[gene.Category].ContainsKey(gene.Type))
            {
                _defaultGenes[gene.Category][gene.Type] = gene.Name;
            }

            foreach (var ancestor in gene.AncestorGenes)
            {
                var edges = new List<Edge<string>>
                {
                    new Edge<string>(ancestor, gene.Name),
                    new Edge<string>(gene.Name, ancestor)
                };
                _graph.AddVerticesAndEdgeRange(edges);
            }
        }
        public Gene GetGene(string name)
        {
            return _genes[name];
        }
        public Gene GetDefaultGene(GeneCategory category, GeneType type)
        {
            return GetGene(_defaultGenes[category][type]);
        }
        public List<Gene> GetEvolutions(string name)
        {
            return _graph.TryGetOutEdges(name, out var edges)
                ? edges.Select(x => GetGene(x.Target)).ToList()
                : new List<Gene>();
        }

        public GeneLibrary()
        {
            AddGene( new Gene(GeneCategory.Vegetation, GeneType.Morphology, "Straight Parallel")
                .WithDependency(GeneCategory.Vegetation, GeneType.ReproductionTrigger)
                .WithDependency(GeneCategory.Reproduction, GeneType.Morphology)
                .WithDependency(GeneCategory.EnergyProduction, GeneType.Morphology)
                .ModifiesNode(NodeType.Vegetation)
                .WithName("Stem")
                .WithComponent(new LightAbsorber())
                .WithComponent(new Photosynthesis { Efficiency = 1 })
                .WithComponent(new AssignInternodeMesh { Entity = Singleton.RenderMeshLibrary.Library["GreenStem"].Entity })
                .WithComponent(new PrimaryGrowth { DaysToMature = 3, InternodeLength = 1, InternodeRadius = 0.1f })
                .WithComponent(new Metabolism { Resting = 0.1f })
                .Gene.ModifiesNode(NodeType.Bud)
                .WithName("Bud")
                .WithComponent(new PrimaryGrowth { DaysToMature = 1, NodeSize = new float3(0.01f, 0.01f, 0.01f) })
                .WithComponent(new Metabolism { Resting = 0.1f })
                .WithComponent(new NodeDivision { RemainingDivisions = 6, MinEnergyPressure = 0.8f })
                .WithDivision(NodeType.Vegetation, DivisionOrder.PreNode, LifeStage.Vegetation)
                .WithDivision(NodeType.EnergyProduction, DivisionOrder.InPlace, LifeStage.Vegetation, Quaternion.LookRotation(Vector3.left, Vector3.forward))
                .WithDivision(NodeType.EnergyProduction, DivisionOrder.InPlace, LifeStage.Vegetation, Quaternion.LookRotation(Vector3.right, Vector3.forward))
                .WithDivision(NodeType.Reproduction, DivisionOrder.Replace, LifeStage.Reproduction)
                .Gene);

            AddGene( new Gene(GeneCategory.Vegetation, GeneType.ReproductionTrigger, "Deterministic")
                .WithDependency(GeneCategory.Reproduction, GeneType.Morphology)
                .ModifiesNode(NodeType.Bud)
                .WithComponent(new DeterministicLifeStageTrigger { CurrentStage = LifeStage.Vegetation, NextStage = LifeStage.Reproduction })
                .Gene);

            AddGene( new Gene(GeneCategory.EnergyProduction, GeneType.Morphology, "Leaf")
                .ModifiesNode(NodeType.EnergyProduction)
                .WithName("Leaf")
                .WithComponent(new LightAbsorber())
                .WithComponent(new Photosynthesis { Efficiency = 1 })
                .WithComponent(new AssignInternodeMesh { Entity = Singleton.RenderMeshLibrary.Library["GreenStem"].Entity })
                .WithComponent(new AssignNodeMesh { Entity = Singleton.RenderMeshLibrary.Library["Leaf"].Entity })
                .WithComponent(new PrimaryGrowth { DaysToMature = 4, InternodeLength = 0.1f, InternodeRadius = 0.1f, NodeSize = new float3(1, 0.1f, 1) })
                .WithComponent(new Metabolism { Resting = 0.025f })
                .Gene);

            AddGene( new Gene(GeneCategory.Reproduction, GeneType.Morphology, "Sporangia")
                .WithDependency(GeneCategory.Reproduction, GeneType.VegetationTrigger)
                .ModifiesNode(NodeType.Reproduction)
                .WithName("Sporangia")
                .WithComponent(new AssignNodeMesh { Entity = Singleton.RenderMeshLibrary.Library["Sporangia"].Entity })
                .WithComponent(new PrimaryGrowth { DaysToMature = 8, NodeSize = 0.5f })
                .WithComponent(new NodeDivision { Stage = LifeStage.Reproduction, RemainingDivisions = 5, MinEnergyPressure = 0.8f })
                .WithComponent(new Metabolism { Resting = 0.1f })
                .WithDivision(NodeType.Embryo, DivisionOrder.PostNode, LifeStage.Reproduction, Quaternion.LookRotation(Vector3.up))
                .Gene.ModifiesNode(NodeType.Embryo)
                .WithName("Spore")
                .WithComponent(new WindDispersal())
                .WithComponent(new PrimaryGrowth { DaysToMature = 3, NodeSize = 0.25f })
                .WithComponent(new NodeDivision { Stage = LifeStage.Reproduction })
                .WithComponent(new Metabolism { Resting = 0.0f })
                .WithoutComponent<LightBlocker>()
                .WithDivision(NodeType.Bud, DivisionOrder.PostNode, LifeStage.Vegetation, Quaternion.LookRotation(Vector3.forward, Vector3.right))
                .Gene);

            AddGene( new Gene(GeneCategory.Reproduction, GeneType.VegetationTrigger, "Disconnect")
                .WithDependency(GeneCategory.Vegetation, GeneType.Morphology)
                .ModifiesNode(NodeType.Embryo)
                .WithComponent(new ParentLifeStageTrigger { ParentedStage = LifeStage.Reproduction, UnparentedStage = LifeStage.Vegetation })
                .Gene);
        }
    }
}