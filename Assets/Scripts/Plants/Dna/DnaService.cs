using Assets.Scripts.Plants.Cleanup;
using Assets.Scripts.Plants.Dna;
using Assets.Scripts.Plants.Dna.EnergyProductionGenes;
using Assets.Scripts.Plants.Dna.ReproductionGenes.EmbryoGrowthTrigger;
using Assets.Scripts.Plants.Dna.ReproductionGenes.Morphology;
using Assets.Scripts.Plants.Dna.ReproductionGenes.ReproductionTrigger;
using Assets.Scripts.Plants.Dna.VegetationGenes.Morphology;
using Assets.Scripts.Plants.Environment;
using Assets.Scripts.Plants.Growth;
using Assets.Scripts.Plants.Setup;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class DnaService : MonoBehaviour
{
    public static EntityArchetype PlantNodeArchetype;

    void Start()
    {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        PlantNodeArchetype = em.CreateArchetype(
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
            typeof(Metabolism),
            typeof(Health)
        );
    }

    public static IGene GetDefaultGene(GeneType t)
    {
        return t switch
        {
            GeneType.VegetationMorphology => new StraightParallel(),
            GeneType.ReproductionMorphology => new Sporangia(),
            GeneType.ReproductionTrigger => new Deterministic(),
            GeneType.EmbryoGrowthTrigger => new Unparent(),
            GeneType.EnergyProductionMorphology => new Leaf(),
            _ => throw new System.NotImplementedException($"Default GeneType {t} not supported"),
        };
    }
}
