﻿using System.Collections.Generic;
using Unity.Entities;

namespace Assets.Scripts.Plants.Dna
{
    public interface IGene
    {
        GeneType GeneType { get; }
        List<NodeType> NodeDependencies { get; }
        List<GeneType> GeneDependencies { get; }
        void Apply(Dictionary<NodeType, Entity> nodes);
    }

    public enum NodeType
    {
        Dna,
        Bud,
        Vegetation,
        EnergyProduction,
        Reproduction,
        Embryo,
        Root,
    }

    public enum GeneType
    {
        VegetationMorphology,
        ReproductionMorphology,
        ReproductionTrigger,
        ReproductionPigment,
        EnergyProductionMorphology,
        RootMorphology,
        Dormancy,
    }
}
