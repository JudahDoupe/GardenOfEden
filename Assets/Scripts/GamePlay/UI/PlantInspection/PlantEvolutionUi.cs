using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlantEvolutionUi : MonoBehaviour, UiState
{
    public GameObject ButtonPrefab;
    public GameObject GeneContainer;
    public GameObject DecriptionContainer;
    public Text Title;
    public Text Description;
    public Button AcceptButton;

    public BasicInfoUi ExitState;

    private UiData _uiData;

    public bool Enable(UiData data)
    {
        if (data.FocusedPlant == null)
        {
            return false;
        }
        _uiData = data;
        selectedGene = null;
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
        DecriptionContainer.SetActive(false);
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
        var currentGene = genes.FirstOrDefault(g => _uiData.FocusedPlant.PlantDna.Genes.Any(x => x.Method.Name == g.Name));
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

    private PlantGene selectedGene;
    public void SelectGene(PlantGene gene)
    {
        DecriptionContainer.SetActive(true);
        Title.text = gene.Name;
        Description.text = "Gene Description";
        selectedGene = gene;
    }

    public void ReplaceGene()
    {
        if (selectedGene == null)
            return;

        if (!_uiData.FocusedPlant.PlantDna.Genes.Any(x => x.Method.Name == selectedGene.Name))
        {
            var newDna = new PlantDna { Name = "New Plant" };
            newDna.Genes = _uiData.FocusedPlant.PlantDna.Genes.Where(x => x.Category != selectedGene.Category.ToString()).ToList();
            newDna.Genes.Add(selectedGene.Dna);
            var oldPlant = _uiData.FocusedPlant;
            _uiData.FocusedPlant = new GameObject().AddComponent<Plant>();
            _uiData.FocusedPlant.transform.position = oldPlant.transform.position;
            _uiData.FocusedPlant.transform.rotation = oldPlant.transform.rotation;
            _uiData.FocusedPlant.PlantDna = newDna;
            oldPlant.Kill();
        }

        Singleton.UiController.SetState(ExitState);
    }
}
