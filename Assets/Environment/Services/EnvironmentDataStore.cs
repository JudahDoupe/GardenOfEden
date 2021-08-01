using UnityEngine;

public class EnvironmentDataStore : MonoBehaviour
{
    public static RenderTexture WaterSourceMap { get; set; }
    public static RenderTexture WaterMap { get; set; }
    public static RenderTexture LandMap { get; set; }
    public static RenderTexture ContinentalIdMap { get; set; }
    public static RenderTexture ConntinentalHeightMap { get; set; }
    public static RenderTexture ContinentalVelocityMap { get; set; }

    void Awake()
    {
        LandMap = NewTexture(4);
        WaterMap = NewTexture(4);
        WaterSourceMap = NewTexture(4);
        ContinentalIdMap = NewTexture(1);
        ConntinentalHeightMap = NewTexture(1);
        ContinentalVelocityMap = NewTexture(2);
    }

    private RenderTexture NewTexture(int channels)
    {
        var format = channels switch
        {
            1 => RenderTextureFormat.RFloat,
            2 => RenderTextureFormat.RGFloat,
            _ => RenderTextureFormat.ARGBFloat
        };
        return new RenderTexture(512, 512, 4, format, 0).ResetTexture().Initialize();
    }
}