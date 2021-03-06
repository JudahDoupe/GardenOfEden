﻿using System;
using UnityEngine;

public interface ILandService
{
    float SampleHeight(Coordinate coord);
    void SetBedrockHeight(Coordinate location, float radius, float height);
}

public class LandService : MonoBehaviour, ILandService
{
    public static float SeaLevel = 1000f;

    /* Publicly Accessible Methods */

    public float SampleHeight(Coordinate coord)
    {
        return EnvironmentDataStore.LandMap.Sample(coord).r + SeaLevel;
    }

    public void SetBedrockHeight(Coordinate location, float radius, float height)
    {
        var shader = Resources.Load<ComputeShader>("Shaders/TerrainManipulation");
        var kernelId = shader.FindKernel("SmoothLerp");
        shader.SetInt("Channel", 0);
        shader.SetFloat("Value", height - SeaLevel);
        shader.SetFloat("Speed", Time.deltaTime * 1f);
        shader.SetFloat("Radius", radius);
        shader.SetFloats("AdditionCenter", location.x, location.y, location.z);
        shader.SetTexture(kernelId, "Map", EnvironmentDataStore.LandMap);
        shader.Dispatch(kernelId, EnvironmentDataStore.TextureSize / 8, EnvironmentDataStore.TextureSize / 8, 1);

        UpdateLand();
    }

    /* Inner Mechanations */

    private Renderer LandRenderer;

    void Start()
    {
        Singleton.LoadBalancer.RegisterEndSimulationAction(ProcessDay);

        LandRenderer = GetComponent<Renderer>();
        LandRenderer.material.SetTexture("_HeightMap", EnvironmentDataStore.LandMap);
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
    private void UpdateLand()
    {
        var updateShader = Resources.Load<ComputeShader>("Shaders/Land");
        int updateKernel = updateShader.FindKernel("Update");
        updateShader.SetTexture(updateKernel, "LandMap", EnvironmentDataStore.LandMap);
        updateShader.Dispatch(updateKernel, EnvironmentDataStore.TextureSize / 8, EnvironmentDataStore.TextureSize / 8, 1);
    }
    public void ProcessDay()
    {
        EnvironmentDataStore.LandMap.UpdateTextureCache();
    }
}
