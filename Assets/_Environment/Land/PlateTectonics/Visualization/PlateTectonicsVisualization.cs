using Assets.Scripts.Utils;
using UnityEngine;

[RequireComponent(typeof(PlateTectonicsSimulation))]
public class PlateTectonicsVisualization : MonoBehaviour
{
    [Header("Materials")]
    public Material OutlineReplacementMaterial;
    public Material FaultLineMaterial;
    [Range(0, 10)]
    public int ShowIndividualPlate = 0;

    [Header("Facets")]
    [Range(0,0.1f)]
    public float FacetsDencity = 0.005f;
    [Range(0,1)]
    public float FacetStrength = 0.3f;
    [Range(0, 1)]
    public float PatchSize = 0.5f;
    [Range(0, 0.05f)]
    public float PatchDencity = 0.01f;
    [Range(0, 20)]
    public float PatchFalloffSharpness = 10;

    [Header("Normal Noise")]
    [Range(0, 1)]
    public float NoiseStrength = 0.3f;
    [Range(0, 0.1f)]
    public float NoiseScale = 0.5f;

    public bool IsActive { get; set; }

    private PlateTectonicsSimulation _simulation;

    public void Initialize()
    {
        OutlineReplacementMaterial.SetTexture("ContinentalIdMap", _simulation.Data.ContinentalIdMap.RenderTexture);
        OutlineReplacementMaterial.SetTexture("HeightMap", _simulation.Data.LandHeightMap.RenderTexture);
        SetLandMaterialValues();
        ShowFaultLines(false);
    }
    public void ShowFaultLines(bool show)
    {
        OutlineReplacementMaterial.SetFloat("PlateId", 0);
        FaultLineMaterial.SetFloat("Transparency", show ? 0.3f : 0);
    }
    public void HighlightPlate(float plateId)
    {
        OutlineReplacementMaterial.SetFloat("PlateId", plateId);
        FaultLineMaterial.SetFloat("Transparency", 0.6f);
    }
    
    private void Start()
    {
        _simulation = GetComponent<PlateTectonicsSimulation>();
    }
    private void OnValidate()
    {
        if (_simulation != null && _simulation.IsActive)
            SetLandMaterialValues();
    }

    private void SetLandMaterialValues()
    {
        GetComponent<MeshFilter>().sharedMesh.bounds = new Bounds(Vector3.zero, new Vector3(1,1,1) * Coordinate.PlanetRadius * 2);
        var landMaterial = GetComponent<Renderer>().sharedMaterial;
        landMaterial.SetTexture("HeightMap", _simulation.Data.LandHeightMap.RenderTexture);
        landMaterial.SetTexture("ContinentalIdMap", _simulation.Data.ContinentalIdMap.RenderTexture);
        landMaterial.SetFloat("MantleHeight", _simulation.MantleHeight);
        landMaterial.SetFloat("MaxHeight", _simulation.MantleHeight + (_simulation.MantleHeight / 3));
        landMaterial.SetFloat("FacetDencity", FacetsDencity);
        landMaterial.SetFloat("FacetStrength", FacetStrength);
        landMaterial.SetFloat("FacetPatchSize", PatchSize);
        landMaterial.SetFloat("FacetPatchDencity", PatchDencity);
        landMaterial.SetFloat("FacetPatchFalloffSharpness", PatchFalloffSharpness);
        landMaterial.SetFloat("NormalNoiseScale", NoiseScale);
        landMaterial.SetFloat("NormalNoiseStrength", NoiseStrength);
        landMaterial.SetInt("RenderPlate", ShowIndividualPlate);
    }
}
