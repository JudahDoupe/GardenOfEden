using System;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

[RequireComponent(typeof(PlateTectonicsSimulation))]
public class PlateTectonicsVisualization : MonoBehaviour
{
    public ComputeShader ComputeShader;

    [Header("Materials")]
    public Material OutlineReplacementMaterial;
    public Material FaultLineMaterial;
    [Range(0, 10)]
    public int ShowIndividualPlate;

    [Header("Facets")]
    [Range(0, 0.1f)]
    public float FacetsDencity = 0.005f;
    [Range(0, 1)]
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

    private PlateTectonicsData _data;

    public bool IsActive { get; private set; }


    private void Start()
    {
        Planet.Data.Subscribe(data =>
        {
            _data = data.PlateTectonics;
            OutlineReplacementMaterial.SetTexture("ContinentalIdMap", _data.VisualizedContinentalIdMap.RenderTexture);
            OutlineReplacementMaterial.SetTexture("HeightMap", _data.LandHeightMap.RenderTexture);
            SetLandMaterialValues();
            Disable();
        });
    }

    private void OnValidate()
    {
        if (_data != null)
        {
            SetLandMaterialValues();
        }
    }

    public void Enable()
    {
        IsActive = true;
    }

    public void Disable()
    {
        HideOutlines();
        IsActive = false;
    }

    public void OutlinePlates(params float[] outlinedPlateIds)
    {
        if (!IsActive) return;
        UpdateVisualizationMap(outlinedPlateIds);
        FaultLineMaterial.SetFloat("Transparency", outlinedPlateIds.Any() ? 0.6f : 0.3f);
    }

    public void HideOutlines()
    {
        FaultLineMaterial.SetFloat("Transparency", 0);
    }


    private void UpdateVisualizationMap(float[] outlinedPlates)
    {
        ComputeShader.SetInt("NumPlates", outlinedPlates.Length);
        var kernel = ComputeShader.FindKernel(outlinedPlates.Any() ? "OutlinePlates" : "OutlineAllPlates");
        using var buffer = new ComputeBuffer(outlinedPlates.Length + 1, Marshal.SizeOf(typeof(OutlinedPlate)));
        buffer.SetData(outlinedPlates.Append(0).Select(x => new OutlinedPlate { PlateId = x }).ToArray());
        ComputeShader.SetBuffer(kernel, "OutlinedPlates", buffer);
        ComputeShader.SetTexture(kernel, "ContinentalIdMap", _data.ContinentalIdMap.RenderTexture);
        ComputeShader.SetTexture(kernel, "VisualizedContinentalIdMap", _data.VisualizedContinentalIdMap.RenderTexture);
        ComputeShader.Dispatch(kernel, Coordinate.TextureWidthInPixels / 8, Coordinate.TextureWidthInPixels / 8, 1);
    }

    private void SetLandMaterialValues()
    {
        GetComponent<MeshFilter>().sharedMesh.bounds = new Bounds(Vector3.zero, new Vector3(1, 1, 1) * Coordinate.PlanetRadius * 2);
        var landMaterial = GetComponent<Renderer>().sharedMaterial;
        landMaterial.SetTexture("HeightMap", _data.LandHeightMap.RenderTexture);
        landMaterial.SetTexture("ContinentalIdMap", _data.ContinentalIdMap.RenderTexture);
        landMaterial.SetFloat("MantleHeight", _data.MantleHeight);
        landMaterial.SetFloat("MaxHeight", _data.MantleHeight + _data.MantleHeight / 3);
        landMaterial.SetFloat("FacetDencity", FacetsDencity);
        landMaterial.SetFloat("FacetStrength", FacetStrength);
        landMaterial.SetFloat("FacetPatchSize", PatchSize);
        landMaterial.SetFloat("FacetPatchDencity", PatchDencity);
        landMaterial.SetFloat("FacetPatchFalloffSharpness", PatchFalloffSharpness);
        landMaterial.SetFloat("NormalNoiseScale", NoiseScale);
        landMaterial.SetFloat("NormalNoiseStrength", NoiseStrength);
        landMaterial.SetInt("RenderPlate", ShowIndividualPlate);
    }

    private struct OutlinedPlate
    {
        public float PlateId;
    }
}