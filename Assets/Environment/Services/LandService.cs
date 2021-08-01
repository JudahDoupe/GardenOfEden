using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;

public interface ILandService
{
    float SampleHeight(Coordinate coord);
}

public class LandService : MonoBehaviour, ILandService
{
    public int NumPlates = 5;
    public float FaultLineNoise = 0;
    public float PlateDriftSpeed = 1;
    [Range(0, 1)]
    public float PlateVelocityDampening = 0.5f;
    public static float SeaLevel = 1000f;
    public static Renderer Renderer;

    /* Publicly Accessible Methods */

    public float SampleHeight(Coordinate coord)
    {
        return EnvironmentDataStore.LandMap.Sample(coord).r + SeaLevel;
    }

    /* Inner Mechanations */

    void Start()
    {
        Singleton.LoadBalancer.RegisterEndSimulationAction(ProcessDay);

        Renderer = GetComponent<Renderer>();
        Renderer.material.SetTexture("HeightMap", EnvironmentDataStore.LandMap);
        Renderer.gameObject.GetComponent<MeshFilter>().mesh.bounds = new Bounds(Vector3.zero, new Vector3(2000,2000,2000));

        PlateTectonics.Regenerate(NumPlates, 1);
    }

    private void UpdateLand()
    {
        var updateShader = Resources.Load<ComputeShader>("Shaders/Land");
        int updateKernel = updateShader.FindKernel("Update");
        updateShader.SetTexture(updateKernel, "LandMap", EnvironmentDataStore.LandMap);
        updateShader.Dispatch(updateKernel, Coordinate.TextureWidthInPixels / 8, Coordinate.TextureWidthInPixels / 8, 1);
    }

    public void ProcessDay()
    {
        if(NumPlates != PlateTectonics.Plates.Count)
        {
            PlateTectonics.Regenerate(NumPlates, 1);
        }

        PlateTectonics.FaultLineNoise = FaultLineNoise;
        PlateTectonics.DriftSpeed = PlateDriftSpeed;
        PlateTectonics.Dampening = PlateVelocityDampening;

        PlateTectonics.UpdatePlateIdMap();
        PlateTectonics.UpdatePlateVelocity();
        PlateTectonics.IntegratePlateVelocity();

        EnvironmentDataStore.TectonicPlateIdMap.UpdateTextureCache();
        EnvironmentDataStore.LandMap.UpdateTextureCache();
    }
}
