using Assets.Scripts.Plants.Growth;
using System.Collections.Generic;
using Unity.Entities;

namespace Assets.Scripts.Plants.Dna.ReproductionGenes
{
    public class DeterministicTriggerGene : IGene
    {
        public GeneType GeneType => GeneType.ReproductionTrigger;

        public List<NodeType> NodeDependencies => new List<NodeType> { NodeType.Bud };
        public List<GeneType> GeneDependencies => new List<GeneType> { GeneType.ReproductionMorphology };

        public void Apply(Dictionary<NodeType, Entity> nodes)
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;

            var bud = nodes[NodeType.Bud];
            em.AddComponentData(bud, new DeterministicReproductionTrigger());
            var divisionInstructions = em.HasComponent<DivisionInstruction>(bud)
                ? em.GetBuffer<DivisionInstruction>(bud)
                : em.AddBuffer<DivisionInstruction>(bud);
            divisionInstructions.Add(new DivisionInstruction
            {
                Entity = nodes[NodeType.Reproduction],
                Stage = LifeStage.Reproduction,
                Order = DivisionOrder.Replace,
            });
        }
    }
}
