using UnityEngine;

public class ComputeShaderService : MonoBehaviour
{
    public Camera TerrainCamera;

    [Header("Render Textures")]
    public RenderTexture HeightMap;
    public RenderTexture NormalMap;
    public RenderTexture WaterMap;
    public RenderTexture RootMap;
    public RenderTexture Output;
    public RenderTexture Input;

    [Header("Compute Shaders")]
    public ComputeShader WaterShader;
    public ComputeShader WaterShedShader;
    public ComputeShader RainShader;
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

        RootMap.Release();
        RootMap.enableRandomWrite = true;
        RootMap.Create();

        int kernelId = WaterShedShader.FindKernel("CSMain");
        WaterShedShader.SetTexture(kernelId, "HeightMap", HeightMap);
        WaterShedShader.SetTexture(kernelId, "NormalMap", NormalMap);
        WaterShedShader.SetTexture(kernelId, "WaterMap", WaterMap);
        WaterShedShader.SetTexture(kernelId, "Result", Output);

        kernelId = RainShader.FindKernel("CSMain");
        RainShader.SetTexture(kernelId, "NormalMap", NormalMap);
        RainShader.SetTexture(kernelId, "Result", WaterMap);

        var updateKernel = WaterShader.FindKernel("Update");
        WaterShader.SetTexture(updateKernel, "TerrainHeightMap", HeightMap);
        WaterShader.SetTexture(updateKernel, "WaterMap", WaterMap);
        WaterShader.SetTexture(updateKernel, "Result", Output);
        WaterShader.SetTexture(updateKernel, "Test", Input);
        var rainKernel = WaterShader.FindKernel("Rain");
        WaterShader.SetTexture(rainKernel, "WaterMap", WaterMap);
    }

    private int iiii = 0;
    void FixedUpdate()
    {
        if(iiii++ == 1)
        {
            iiii = 0;
            UpdateWaterTable();
        }
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
        int kernelId = WaterShader.FindKernel("Update");
        WaterShader.Dispatch(kernelId, 512 / 8, 512 / 8, 1);
        Graphics.CopyTexture(Output, WaterMap);
    }

    public void Rain()
    {
        int kernelId = WaterShader.FindKernel("Rain");
        WaterShader.Dispatch(kernelId, 512 / 8, 512 / 8, 1);
    }
}
