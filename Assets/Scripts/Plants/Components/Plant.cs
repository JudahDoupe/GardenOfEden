using UnityEngine;

public class Plant : Interactable
{
    public int PlantId;
    public PlantDna Dna;
    public IGrowthState GrowthState;
    public Volume StoredWater;
    public Volume StoredSugar;
    public Structure Trunk { get; set; }

    public float PlantedDate;
    public float LastUpdatedDate;
    public float AgeInDay => LastUpdatedDate - PlantedDate;

    public bool IsAlive = true;
    public bool IsGrowing = false;
    public bool IsMature => Trunk.IsMature;

    public int TotalStructures => transform.GetComponentsInChildren<Structure>()?.Length ?? 1;
    public float RootRadius => 10 * Mathf.Sqrt(TotalStructures) / Mathf.PI;
    public Volume SustainingSugar => Volume.FromCubicMeters(0.01f * TotalStructures); //TODO: store this in the structure

    void Start()
    {
        Trunk = Structure.Create(this, 1);
        Trunk.transform.parent = transform;
        Trunk.transform.localPosition = Vector3.zero;
        Trunk.transform.localRotation = Quaternion.identity;
        PlantedDate = EnvironmentApi.GetDate();
        LastUpdatedDate = PlantedDate;
    }

    public override void Interact(FirstPersonController player)
    {
        var camera = FindObjectOfType<CameraController>();
        camera.RotateAround(transform.position + new Vector3(0,.5f,0), new Vector3(0, 1, -3));
        var ui = FindObjectOfType<UIController>();
        ui.ShowEvolutionMenu();
    }

}