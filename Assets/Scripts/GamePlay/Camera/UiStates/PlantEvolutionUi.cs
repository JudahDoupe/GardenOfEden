using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlantEvolutionUi : MonoBehaviour, IUiState
{
    public CameraController Controller;
    public GameObject ButtonPrefab;
    public GameObject GeneContainer;
    public GameObject DecriptionContainer;
    public GameObject ChomosomeContainer;
    public GameObject NameContainer;
    public Text Title;
    public Text Description;
    public Text Name;

    private ICameraState _nextCameraState;

    public void UpdateUi()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Controller.UiState.SetState(FindObjectOfType<BasicInfoUi>());
        }
    }

    public void Enable()
    {
        if (Controller.FocusedPlant == null)
        {
            Controller.UiState.SetState(FindObjectOfType<CinematicUi>());
        }

        _nextCameraState = Controller.CameraState.State;
        Controller.CameraState.SetState(FindObjectOfType<StaticCamera>());

        selectedGene = null;
        HideGeneList();
        GetComponent<Canvas>().enabled = true;
        ChomosomeContainer.SetActive(true);
        NameContainer.SetActive(false);
    }

    public void Disable()
    {
        GetComponent<Canvas>().enabled = false;
        Controller.CameraState.SetState(_nextCameraState);
        HideGeneList();
        ChomosomeContainer.SetActive(false);
        NameContainer.SetActive(false);
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
    public void NameSpecies()
    {
        HideGeneList();
        ChomosomeContainer.SetActive(false);
        NameContainer.SetActive(true);
    }

    private void ShowGeneList(PlantGeneCategory category)
    {
        HideGeneList();
        var genes = GeneCache.GetGenesInCategory(category);
        var currentGene = genes.FirstOrDefault(g => Controller.FocusedPlant.PlantDna.Genes.Any(x => x.Name == g.Name));
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

        if (!Controller.FocusedPlant.PlantDna.Genes.Any(x => x.Name == selectedGene.Name))
        {
            var newDna = new PlantDna { Name = Name.text };
            newDna.Genes = Controller.FocusedPlant.PlantDna.Genes.Where(x => x.Category != selectedGene.Category).ToList();
            newDna.Genes.Add(selectedGene);
            var oldPlant = Controller.FocusedPlant;
            Controller.FocusedPlant = new GameObject().AddComponent<Plant>();
            Controller.FocusedPlant.transform.position = oldPlant.transform.position;
            Controller.FocusedPlant.transform.rotation = oldPlant.transform.rotation;
            Controller.FocusedPlant.PlantDna = newDna;
            Controller.FocusedPlant.StoredEnergy = 5;
            oldPlant.Kill();
            PlantDnaDataStore.SaveDna(newDna.ToDto());
        }

        _nextCameraState = FindObjectOfType<ObservationCamera>();
        Controller.UiState.SetState(FindObjectOfType<BasicInfoUi>());
    }
}
