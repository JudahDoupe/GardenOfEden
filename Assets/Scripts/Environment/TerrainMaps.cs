using UnityEngine;

public class TerrainMaps : MonoBehaviour
{
    public RenderTexture HeightMap;
    public RenderTexture NormalMap;
    public RenderTexture RiverMap;
    public RenderTexture WaterMap;
    private RenderTexture Result;

    public ComputeShader WaterShedShader;


    void Start()
    {
        InitializeWaterShed();
    }

    public int speed = 30;
    private int countdown = 0;
    void FixedUpdate()
    {
        if (countdown-- < 0)
        {
            UpdateWaterShed();
            countdown = speed;
        }
    }

    public void InitializeWaterShed()
    {
        Result = new RenderTexture(512,512,24);
        Result.enableRandomWrite = true;
        Result.Create();

        WaterMap.Release();

        int kernelId = WaterShedShader.FindKernel("CSMain");
        WaterShedShader.SetTexture(kernelId, "HeightMap", HeightMap);
        WaterShedShader.SetTexture(kernelId, "NormalMap", NormalMap);
        WaterShedShader.SetTexture(kernelId, "RiverMap", RiverMap);
        WaterShedShader.SetTexture(kernelId, "WaterMap", WaterMap);
        WaterShedShader.SetTexture(kernelId, "Result", Result);
    }
    public void UpdateWaterShed()
    {
        int kernelId = WaterShedShader.FindKernel("CSMain");
        WaterShedShader.Dispatch(kernelId, 512 / 8, 512 / 8, 1);
        Graphics.CopyTexture(Result, WaterMap);
    }
}
