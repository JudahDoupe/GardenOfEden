using System;
using UnityEngine;

public interface ILandService
{
    float SampleHeight(Coordinate coord);
}

public class LandService : MonoBehaviour, ILandService
{
    public static float SeaLevel = 1000f;

    /* Publicly Accessible Methods */

    public float SampleHeight(Coordinate coord)
    {
        return EnvironmentDataStore.LandMap.Sample(coord).r + SeaLevel;
    }

    /* Inner Mechanations */

    private Renderer LandRenderer;

    void Start()
    {
        Singleton.LoadBalancer.RegisterEndSimulationAction(ProcessDay);

        LandRenderer = GetComponent<Renderer>();
        LandRenderer.material.SetTexture("HeightMap", EnvironmentDataStore.LandMap);
        LandRenderer.gameObject.GetComponent<MeshFilter>().mesh.bounds = new Bounds(Vector3.zero, new Vector3(2000,2000,2000));
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
        EnvironmentDataStore.LandMap.UpdateTextureCache();
    }
}
