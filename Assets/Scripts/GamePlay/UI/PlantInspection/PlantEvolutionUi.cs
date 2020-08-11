using System;
using System.Linq;
using UnityEngine;

public class PlantEvolutionUi : MonoBehaviour, UiState
{
    private UiData _uiData;

    public bool Enable(UiData data)
    {
        _uiData = data;
        return true;
    }

    public bool Disable(UiData data)
    {
        return true;
    }

    public void HideGeneList()
    {
    }

    public void ShowGeneList(PlantGeneCategory category)
    {
        var genes = GeneLibrary.GetGenesInCategory(category);
        foreach(var gene in genes)
        {
        }
                
    }

    public void SelectGene(PlantGene gene)
    {

    }

    public void ReplaceGene(PlantGene gene)
    {
        var newDna = new PlantDna();
        newDna.Genes = _uiData.FocusedPlant.PlantDna.Genes.Where(x => x.Category != gene.Category.ToString()).ToList();
        newDna.Genes.Add(gene.Dna);
        var oldPlant = _uiData.FocusedPlant;
        _uiData.FocusedPlant = DI.ReproductionService.PlantSeed(newDna, oldPlant.transform.position);
        oldPlant.Kill();
    }
}
