using System.Collections.Generic;
using Assets.Scripts.Plants.Growth;
using Unity.Entities;

namespace Assets.Scripts.Plants.Dna.ReproductionGenes.ReproductionTrigger
{
    public class Deterministic : IGene
    {
        public GeneType GeneType => GeneType.ReproductionTrigger;

        public List<NodeType> NodeDependencies => new List<NodeType> { NodeType.Bud };
        public List<GeneType> GeneDependencies => new List<GeneType> { GeneType.ReproductionMorphology };

        public void Apply(Dictionary<NodeType, Entity> nodes)
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;

            var bud = nodes[NodeType.Bud];
            em.AddComponentData(bud, new DeterministicLifeStageTrigger{ CurrentStage = LifeStage.Vegetation, NextStage = LifeStage.Reproduction});
        }
    }
}
