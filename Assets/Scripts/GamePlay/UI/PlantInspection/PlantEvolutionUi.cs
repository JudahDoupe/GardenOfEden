using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlantEvolutionUi : MonoBehaviour, UiState
{
    public GameObject ButtonPrefab;
    public GameObject GeneContainer;
    public Text Title;
    public Text Description;
    public Button AcceptButton;

    public UiState ExitState;

    private UiData _uiData;

    public bool Enable(UiData data)
    {
        if (data.FocusedPlant == null)
        {
            return false;
        }
        _uiData = data;
        Title.text = data.FocusedPlant.PlantDna.Name;
        HideGeneList();
        GetComponent<Canvas>().enabled = true;
        return true;
    }

    public bool Disable(UiData data)
    {
        GetComponent<Canvas>().enabled = false;
        return true;
    }

    public void HideGeneList()
    {
        foreach (Transform child in GeneContainer.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }

    public void ShowVegatationGenes()
    {
        ShowGeneList(PlantGeneCategory.Vegatation);
    }
    public void ShowReproductionGenes()
    {
        ShowGeneList(PlantGeneCategory.Reproduction);
    }
    public void ShowEnergyProductionGenes()
    {
        ShowGeneList(PlantGeneCategory.EnergyProduction);
    }
    private void ShowGeneList(PlantGeneCategory category)
    {
        HideGeneList();
        var genes = GeneLibrary.GetGenesInCategory(category);
        var currentGene = genes.Where(g => _uiData.FocusedPlant.PlantDna.Genes.Any(x => x.Method.Name == g.Name));
        var newGenes = genes.Where(g => g != currentGene).ToList();
        var r = 10;
        foreach(var gene in genes)
        {
            var button = GameObject.Instantiate(ButtonPrefab, GeneContainer.transform);
            button.GetComponentInChildren<Text>().text = gene.Name;
            button.GetComponent<Button>().onClick.AddListener(() => SelectGene(gene));
            if (gene != currentGene)
            {
                float n = newGenes.Count();
                float i = newGenes.IndexOf(gene);
                var theta = (i / n) * 2f * Mathf.PI;
                var position = new Vector2(200 * Mathf.Sin(theta), 100 * Mathf.Cos(theta));
                button.GetComponent<RectTransform>().anchoredPosition = position;
            } 
            else
            {
                button.GetComponent<Button>().Select();
            }
        }
    }

    public void SelectGene(PlantGene gene)
    {
        Title.text = gene.Name;
        Description.text = "Gene Description";
    }

    public void ReplaceGene(PlantGene gene)
    {
        if (!_uiData.FocusedPlant.PlantDna.Genes.Any(x => x.Method.Name == gene.Name))
        {
            var newDna = new PlantDna();
            newDna.Genes = _uiData.FocusedPlant.PlantDna.Genes.Where(x => x.Category != gene.Category.ToString()).ToList();
            newDna.Genes.Add(gene.Dna);
            var oldPlant = _uiData.FocusedPlant;
            _uiData.FocusedPlant = DI.ReproductionService.PlantSeed(newDna, oldPlant.transform.position);
            oldPlant.Kill();
        }

        DI.UiController.SetState(ExitState);
    }
}
