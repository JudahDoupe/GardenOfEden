using System.Collections.Generic;
using Assets.Scripts.Plants.Growth;
using Unity.Entities;

namespace Assets.Scripts.Plants.Dna.ReproductionGenes.EmbryoGrowthTrigger
{
    public class Unparent : IGene
    {
        public GeneCategory GeneCategory => GeneCategory.Reproduction;
        public GeneType GeneType => GeneType.EmbryoGrowthTrigger;

        public List<NodeType> NodeDependencies => new List<NodeType> { NodeType.Embryo };
        public List<GeneType> GeneDependencies => new List<GeneType> { GeneType.VegetationMorphology };

        public void Apply(Dictionary<NodeType, Entity> nodes)
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;

            var embryo = nodes[NodeType.Embryo];
            em.AddComponentData(embryo, new ParentLifeStageTrigger{ParentedStage = LifeStage.Reproduction, UnparentedStage = LifeStage.Vegetation});
        }
    }
}
