using Assets.Scripts.Utils;
using UnityEngine;

public class PlateTectonicsVisualization : MonoBehaviour
{
    [Header("Visualization")]
    public Material OutlineReplacementMaterial;
    public Material FaultLineMaterial;
    [Range(0, 10)]
    public int ShowIndividualPlate = 0;
    
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


    private void Update()
    {
        if (IsActive)
        {
            Singleton.PlateTectonics.TectonicsShader.SetInt("RenderPlate", ShowIndividualPlate);
        }
    }
}
