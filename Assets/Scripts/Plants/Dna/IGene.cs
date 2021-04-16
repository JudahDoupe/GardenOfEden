using System;
using System.Collections.Generic;
using Unity.Entities;

namespace Assets.Scripts.Plants.Dna
{
    public interface IGene
    {
        GeneCategory GeneCategory { get; }
        GeneType GeneType { get; }
        List<NodeType> NodeDependencies { get; }
        List<GeneType> GeneDependencies { get; }
        void Apply(Dictionary<NodeType, Entity> nodes);
    }

    public enum NodeType
    {
        Bud,
        Vegetation,
        EnergyProduction,
        Reproduction,
        Embryo,
        Root,
    }

    public enum GeneCategory
    {
        Vegetation,
        Reproduction,
        EnergyProduction,
    }

    public enum GeneType
    {
        VegetationMorphology,
        EmbryoGrowthTrigger,
        ReproductionMorphology,
        ReproductionTrigger,
        ReproductionPigment,
        EnergyProductionMorphology,
        RootMorphology,
        Dormancy,
    }
}
