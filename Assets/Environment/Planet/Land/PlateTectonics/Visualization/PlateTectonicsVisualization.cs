using Assets.Scripts.Utils;
using UnityEngine;

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

    public void ShowFaultLines(bool show)
    {
        StartCoroutine(AnimationUtils.AnimateFloat(1, FaultLineMaterial.GetFloat("Transparency"), show ? 0.3f : 0, x => FaultLineMaterial.SetFloat("Transparency", x)));
    }
    public void Initialize()
    {
        OutlineReplacementMaterial.SetTexture("ContinentalIdMap", EnvironmentDataStore.ContinentalIdMap);
        OutlineReplacementMaterial.SetTexture("HeightMap", EnvironmentDataStore.LandHeightMap);
    }

    private void Start()
    {
        SetLandMaterialValues();
    }
    private void Update()
    {
        if (IsActive)
        {
            Singleton.PlateTectonics.TectonicsShader.SetInt("RenderPlate", ShowIndividualPlate);
        }
        SetLandMaterialValues();
    }

    private void SetLandMaterialValues()
    {
        var landMaterial = GetComponent<Renderer>().material;
        landMaterial.SetFloat("MantleHeight", Singleton.PlateTectonics.MantleHeight);
        landMaterial.SetFloat("MaxHeight", Coordinate.PlanetRadius * 2);
        landMaterial.SetFloat("FacetDencity", FacetsDencity);
        landMaterial.SetFloat("FacetStrength", FacetStrength);
        landMaterial.SetFloat("FacetPatchSize", PatchSize);
        landMaterial.SetFloat("FacetPatchDencity", PatchDencity);
        landMaterial.SetFloat("FacetPatchFalloffSharpness", PatchFalloffSharpness);
        landMaterial.SetFloat("NormalNoiseScale", NoiseScale);
        landMaterial.SetFloat("NormalNoiseStrength", NoiseStrength);
    }
}
