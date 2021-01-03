using System;
using UnityEngine;

public interface ILandService
{
    float SampleHeight(Coordinate coord);
    void AddBedrockHeight(Coordinate location, float radius, float height);
}

public class LandService : MonoBehaviour, ILandService
{
    public float SeaLevel = 1000f;

    /* Publicly Accessible Methods */

    public float SampleHeight(Coordinate coord)
    {
        return EnvironmentDataStore.LandMap.Sample(coord).r + SeaLevel;
    }

    public void AddBedrockHeight(Coordinate location, float radius, float height)
    {
        var shader = Resources.Load<ComputeShader>("Shaders/SmoothAdd");
        var kernelId = shader.FindKernel("SmoothAdd");
        shader.SetFloat("Radius", radius);
        shader.SetFloats("Channels", 1, 0, 0, 0);
        shader.SetFloats("AdditionCenter", location.x, location.y, location.z);
        shader.SetTexture(kernelId, "Map", EnvironmentDataStore.LandMap);
        shader.SetFloat("Strength", height);
        shader.Dispatch(kernelId, EnvironmentDataStore.TextureSize / 8, EnvironmentDataStore.TextureSize / 8, 1);
    }

    /* Inner Mechanations */

    private Renderer LandRenderer;

    void Start()
    {
        Singleton.LoadBalancer.RegisterEndSimulationAction(ProcessDay);

        LandRenderer = GetComponent<Renderer>();
        LandRenderer.material.SetTexture("_LandMap", EnvironmentDataStore.LandMap);
        LandRenderer.gameObject.GetComponent<MeshFilter>().mesh.bounds = new Bounds(Vector3.zero, new Vector3(2000,2000,2000));
    }

    void FixedUpdate()
    {
        SetMaterialShaderVariables();
    }
    private void SetMaterialShaderVariables()
    {
        var focusPos = Singleton.CameraController.FocusPos;
        LandRenderer.sharedMaterial.SetVector("_FocusPosition", new Vector4(focusPos.x, focusPos.y, focusPos.z, 0));
        LandRenderer.material.SetFloat("_SeaLevel", SeaLevel);
        LandRenderer.material.SetFloat("_FocusRadius", Singleton.CameraController.FocusRadius);
    }
    public void ProcessDay()
    {
        EnvironmentDataStore.LandMap.UpdateTextureCache();
    }
}
