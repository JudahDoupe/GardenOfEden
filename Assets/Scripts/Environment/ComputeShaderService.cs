using UnityEngine;

public class ComputeShaderService : MonoBehaviour
{
    public Camera TerrainCamera;

    public RenderTexture HeightMap;
    public RenderTexture NormalMap;
    public RenderTexture WaterMap;
    public RenderTexture RootMap;
    public RenderTexture Output;
    private RenderTexture _result;
    private RenderTexture _input;

    public ComputeShader WaterShedShader;
    public ComputeShader RainShader;
    public ComputeShader RootShader;
    public ComputeShader SubtractShader;

    public static ComputeShaderService Instance;
    public const int TextureSize = 512;

    public static Texture2D RenderTextureToTexture2D(RenderTexture rt)
    {
        RenderTexture currentRt = RenderTexture.active;
        Texture2D rtnTex = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false);
        RenderTexture.active = rt;

        rtnTex.ReadPixels(new Rect(0, 0, TextureSize, TextureSize), 0, 0);
        rtnTex.Apply();

        RenderTexture.active = currentRt;

        return rtnTex;
    }

    public static Vector2 LocationToUV(Vector3 location)
    {
        var size = Instance.TerrainCamera.orthographicSize;
        var relativePosition = location - Instance.TerrainCamera.transform.position;
        var uvPos = relativePosition / size;
        var normalizedUv = (uvPos + new Vector3(1, 1, 1)) / 2;
        return new Vector2(normalizedUv.x, normalizedUv.z);
    }

    public static Texture2D SpreadRoots(Texture2D currentRoots, Vector3 location, float radius, float depth)
    {
        if (currentRoots == null)
        {
            Instance._result.Release();
            Instance._result.enableRandomWrite = true;
            Instance._result.Create();
        }
        else
        {
            Graphics.Blit(currentRoots, Instance._result);
        }
        var uv = LocationToUV(location);
        int kernelId = Instance.RootShader.FindKernel("CSMain");
        Instance.RootShader.SetVector("RootData", new Vector4(uv.x, uv.y, radius, depth));
        Instance.RootShader.SetTexture(kernelId, "RootMap", Instance.RootMap);
        Instance.RootShader.SetTexture(kernelId, "Result", Instance._result);
        Instance.RootShader.Dispatch(kernelId, 512 / 8, 512 / 8, 1);
        return RenderTextureToTexture2D(Instance._result);
    }

    public static Texture2D AbsorbWater(Texture2D rootmap, float multiplier)
    {
        if (rootmap == null)
        {
            Instance._input.Release();
            Instance._input.enableRandomWrite = true;
            Instance._input.Create();
        }
        else
        {
            Graphics.Blit(rootmap, Instance._input);
        }

        int kernelId = Instance.SubtractShader.FindKernel("CSMain");
        Instance.SubtractShader.SetTexture(kernelId, "Base", Instance.WaterMap);
        Instance.SubtractShader.SetTexture(kernelId, "Mask", Instance._input);
        Instance.SubtractShader.SetTexture(kernelId, "Result", Instance._result);
        Instance.SubtractShader.SetFloat("Multiplier", multiplier);
        Instance.SubtractShader.Dispatch(kernelId, 512 / 8, 512 / 8, 1);
        Graphics.Blit(Instance._result, Instance.Output);
        return RenderTextureToTexture2D(Instance.WaterMap);
    }

    void Start()
    {
        Instance = this;
        _result = new RenderTexture(512, 512, 24);
        _result.enableRandomWrite = true;
        _result.Create();

        _input = new RenderTexture(512, 512, 24);
        _input.enableRandomWrite = true;
        _input.Create();

        WaterMap.Release();
        WaterMap.enableRandomWrite = true;
        WaterMap.Create();

        int kernelId = WaterShedShader.FindKernel("CSMain");
        WaterShedShader.SetTexture(kernelId, "HeightMap", HeightMap);
        WaterShedShader.SetTexture(kernelId, "NormalMap", NormalMap);
        WaterShedShader.SetTexture(kernelId, "WaterMap", WaterMap);
        WaterShedShader.SetTexture(kernelId, "Result", _result);

        kernelId = RainShader.FindKernel("CSMain");
        RainShader.SetTexture(kernelId, "NormalMap", NormalMap);
        RainShader.SetTexture(kernelId, "Result", WaterMap);

        RootMap.Release();
        RootMap.enableRandomWrite = true;
        RootMap.Create();
    }

    void FixedUpdate()
    {
        UpdateWaterShed();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Rain();
        }
    }

    public void UpdateWaterShed()
    {
        int kernelId = WaterShedShader.FindKernel("CSMain");
        WaterShedShader.Dispatch(kernelId, 512 / 8, 512 / 8, 1);
        Graphics.CopyTexture(_result, WaterMap);
    }

    public void Rain()
    {
        int kernelId = RainShader.FindKernel("CSMain");
        RainShader.Dispatch(kernelId, 512 / 8, 512 / 8, 1);
    }
}
