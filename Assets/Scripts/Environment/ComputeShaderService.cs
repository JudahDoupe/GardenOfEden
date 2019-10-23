using UnityEngine;

public class ComputeShaderService : MonoBehaviour
{
    public Camera TerrainCamera;

    [Header("Render Textures")]
    public RenderTexture HeightMap;
    public RenderTexture WaterMap;
    public RenderTexture WaterHeightMap;
    public RenderTexture RootMap;
    public RenderTexture Output;
    public RenderTexture Input;

    [Header("Compute Shaders")]
    public ComputeShader WaterShader;
    public ComputeShader RootShader;
    public ComputeShader SubtractShader;

    public static int TextureSize = 512;

    /*  Utils */

    public Vector2 LocationToUv(Vector3 location)
    {
        var size = TerrainCamera.orthographicSize;
        var relativePosition = location - TerrainCamera.transform.position;
        var uvPos = relativePosition / size;
        var uv = new Vector2(uvPos.x, uvPos.z);
        return (uv + new Vector2(1, 1)) / 2;
    }
    public Vector2 LocationToXy(Vector3 location)
    {
        var uv = LocationToUv(location);
        return new Vector2(Mathf.FloorToInt(uv.x * 512),Mathf.FloorToInt(uv.y * 512));
    }

    /* Shader Methods */

    public Texture2D SpreadRoots(Texture2D currentRoots, Vector3 location, float radius, float depth)
    {
        if (currentRoots == null)
        {
            Output.Release();
            Output.enableRandomWrite = true;
            Output.Create();
        }
        else
        {
            Graphics.Blit(currentRoots, Output);
        }
        var uv = LocationToUv(location);
        int kernelId = RootShader.FindKernel("CSMain");
        RootShader.SetVector("RootData", new Vector4(uv.x, uv.y, radius, depth));
        RootShader.SetTexture(kernelId, "RootMap", RootMap);
        RootShader.SetTexture(kernelId, "Result", Output);
        RootShader.Dispatch(kernelId, 512 / 8, 512 / 8, 1);
        return Output.ToTexture2D();
    }

    public Texture2D AbsorbWater(Texture2D rootMap, float multiplier)
    {
        if (rootMap == null)
        {
            Input.Release();
            Input.enableRandomWrite = true;
            Input.Create();
        }
        else
        {
            Graphics.Blit(rootMap, Input);
        }

        int kernelId = SubtractShader.FindKernel("CSMain");
        SubtractShader.SetTexture(kernelId, "Base", WaterMap);
        SubtractShader.SetTexture(kernelId, "Mask", Input);
        SubtractShader.SetTexture(kernelId, "Result", Output);
        SubtractShader.SetFloat("Multiplier", multiplier);
        SubtractShader.Dispatch(kernelId, 512 / 8, 512 / 8, 1);
        return WaterMap.ToTexture2D();
    }

    /* Inner Mechanations */

    void Start()
    {
        Input.Release();
        Input.enableRandomWrite = true;
        Input.Create();

        Output.Release();
        Output.enableRandomWrite = true;
        Output.Create();

        WaterMap.Release();
        WaterMap.enableRandomWrite = true;
        WaterMap.Create();

        WaterHeightMap.Release();
        WaterHeightMap.enableRandomWrite = true;
        WaterHeightMap.Create();

        RootMap.Release();
        RootMap.enableRandomWrite = true;
        RootMap.Create();

        var updateKernel = WaterShader.FindKernel("Update");
        WaterShader.SetTexture(updateKernel, "TerrainHeightMap", HeightMap);
        WaterShader.SetTexture(updateKernel, "WaterMap", WaterMap);
        WaterShader.SetTexture(updateKernel, "WaterHeightMap", WaterHeightMap);
        WaterShader.SetTexture(updateKernel, "Result", Output);
        WaterShader.SetTexture(updateKernel, "Test", Input);
        var rainKernel = WaterShader.FindKernel("Rain");
        WaterShader.SetTexture(rainKernel, "WaterMap", WaterMap);
        var hfsKernel = WaterShader.FindKernel("SuppressHighFrequencies");
        WaterShader.SetTexture(hfsKernel, "WaterMap", WaterMap);
        WaterShader.SetTexture(hfsKernel, "WaterHeightMap", WaterHeightMap);
        WaterShader.SetTexture(hfsKernel, "TerrainHeightMap", HeightMap);
    }

    void FixedUpdate()
    {
        UpdateWaterTable();
    }

    void Update()
    {
        if (UnityEngine.Input.GetKeyDown(KeyCode.R))
        {
            Rain();
        }

        if (UnityEngine.Input.GetKeyDown(KeyCode.U))
        {
        }
    }

    public void UpdateWaterTable()
    {
        int updateKernel = WaterShader.FindKernel("Update");
        WaterShader.Dispatch(updateKernel, 512 / 8, 512 / 8, 1);
        Graphics.CopyTexture(Output, WaterMap);

        var hfsKernel = WaterShader.FindKernel("SuppressHighFrequencies");
        WaterShader.Dispatch(hfsKernel, 512 / 8, 512 / 8, 1);
    }

    public void Rain()
    {
        int kernelId = WaterShader.FindKernel("Rain");
        WaterShader.Dispatch(kernelId, 512 / 8, 512 / 8, 1);
    }
}
