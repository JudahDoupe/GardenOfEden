using Assets.Scripts.Plants.Cleanup;
using Assets.Scripts.Plants.Dna;
using Assets.Scripts.Plants.Dna.EnergyProductionGenes;
using Assets.Scripts.Plants.Dna.ReproductionGenes;
using Assets.Scripts.Plants.Dna.VegetationGenes;
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
            GeneType.VegetationMorphology => new StraightGrowthGene(),
            GeneType.ReproductionMorphology => new SporangiaGene(),
            GeneType.EnergyProductionMorphology => new LeafGene(),
            GeneType.ReproductionTrigger => new DeterministicTriggerGene(),
            _ => throw new System.NotImplementedException($"Default GeneType {t} not supported"),
        };
    }
}
