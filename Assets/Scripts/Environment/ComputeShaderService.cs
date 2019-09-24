using UnityEngine;

public class ComputeShaderService : MonoBehaviour
{
    public Camera TerrainCamera;

    public RenderTexture HeightMap;
    public RenderTexture NormalMap;
    public RenderTexture WaterMap;
    public RenderTexture RootMap;
    private RenderTexture WaterResult;
    private RenderTexture RootResult;

    public ComputeShader WaterShedShader;
    public ComputeShader RainShader;
    public ComputeShader RootShader;

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

    public static Texture2D GetWaterMap()
    {
        return RenderTextureToTexture2D(Instance.WaterMap);
    }

    public static Texture2D SpreadRoots(Texture2D currentRoots, Vector3 location, float radius, float depth)
    {
        if (currentRoots == null)
        {
            Instance.RootResult.Release();
            Instance.RootResult.enableRandomWrite = true;
            Instance.RootResult.Create();
        }
        else
        {
            Graphics.Blit(currentRoots, Instance.RootResult);
        }
        var uv = LocationToUV(location);
        int kernelId = Instance.RootShader.FindKernel("CSMain");
        Instance.RootShader.SetVector("RootData", new Vector4(uv.x, uv.y, radius, depth));
        Instance.RootShader.SetTexture(kernelId, "RootMap", Instance.RootMap);
        Instance.RootShader.SetTexture(kernelId, "Result", Instance.RootResult);
        Instance.RootShader.Dispatch(kernelId, 512 / 8, 512 / 8, 1);
        return RenderTextureToTexture2D(Instance.RootResult);
    }

    void Start()
    {
        Instance = this;
        WaterResult = new RenderTexture(512, 512, 24);
        WaterResult.enableRandomWrite = true;
        WaterResult.Create();

        WaterMap.Release();

        int kernelId = WaterShedShader.FindKernel("CSMain");
        WaterShedShader.SetTexture(kernelId, "HeightMap", HeightMap);
        WaterShedShader.SetTexture(kernelId, "NormalMap", NormalMap);
        WaterShedShader.SetTexture(kernelId, "WaterMap", WaterMap);
        WaterShedShader.SetTexture(kernelId, "Result", WaterResult);

        kernelId = RainShader.FindKernel("CSMain");
        RainShader.SetTexture(kernelId, "NormalMap", NormalMap);
        RainShader.SetTexture(kernelId, "Result", WaterResult);

        RootResult = new RenderTexture(512, 512, 24);
        RootResult.enableRandomWrite = true;
        RootResult.Create();

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
        Graphics.CopyTexture(WaterResult, WaterMap);
    }

    public void Rain()
    {
        int kernelId = RainShader.FindKernel("CSMain");
        RainShader.Dispatch(kernelId, 512 / 8, 512 / 8, 1);
        Graphics.CopyTexture(WaterResult, WaterMap);
    }
}
