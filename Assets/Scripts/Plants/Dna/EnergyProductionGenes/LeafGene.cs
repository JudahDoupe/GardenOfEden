using Assets.Scripts.Plants.Cleanup;
using Assets.Scripts.Plants.Environment;
using Assets.Scripts.Plants.Growth;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;

namespace Assets.Scripts.Plants.Dna.EnergyProductionGenes
{
    public class LeafGene : IGene
    {
        public GeneType GeneType => GeneType.EnergyProductionMorphology;

        public List<NodeType> NodeDependencies => new List<NodeType> { NodeType.EnergyProduction };
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
            em.SetComponentData(entity, new Metabolism { Resting = 0.01f });
            em.SetComponentData(entity, new Health { Value = 1 });
        }
    }
}
