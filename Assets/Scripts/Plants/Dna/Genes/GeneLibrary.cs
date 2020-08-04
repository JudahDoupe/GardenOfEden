using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using static PlantDna;

public static class GeneLibrary
{
    private static Dictionary<PlantGeneCategory, List<PlantGene>> _plantGenes;


    public static List<PlantGene> GetGenesInCategory(PlantGeneCategory category)
    {
        if (_plantGenes == null)
        {
            LoadGenes();
        }
        return _plantGenes[category];
    }
    
    public static void LoadGenes()
    {
        LoadGeneCategory(PlantGeneCategory.Vegatation);
        LoadGeneCategory(PlantGeneCategory.Reproduction);
        LoadGeneCategory(PlantGeneCategory.EnergyProduction);
    }

    private static void LoadGeneCategory(PlantGeneCategory category)
    {
        var vegatationMethods = category.GetLibrary().GetMethods(BindingFlags.Static | BindingFlags.Public);
        var geneList = vegatationMethods.Select(x =>
        {
            return new GeneDna
            {
                Category = PlantGeneCategory.Vegatation.ToString(),
                Method = new Method
                {
                    Name = x.Name,
                    Parameters = x.GetParameters().Where(p => p.ParameterType != typeof(Plant)).Select(p =>
                        new Method.Parameter
                        {
                            Name = p.Name,
                            Value = p.DefaultValue.ToString(),
                        }
                    ).ToList(),
                }
            };
        });
        _plantGenes[category] = geneList.Select(x => new PlantGene(x)).ToList();
    }
}

public enum PlantGeneCategory
{
    Vegatation,
    Reproduction,
    EnergyProduction,
}

public static class PlantGeneCategoryExtentions
{
    public static Type GetLibrary(this PlantGeneCategory category)
    {
        switch (category)
        {
            case PlantGeneCategory.Vegatation:
                return typeof(VegatationGenes);
            case PlantGeneCategory.Reproduction:
                return typeof(ReproductionGenes);
            case PlantGeneCategory.EnergyProduction:
                return typeof(EnergyProductionGenes);
            default:
                return typeof(VegatationGenes);
        }
    }
}