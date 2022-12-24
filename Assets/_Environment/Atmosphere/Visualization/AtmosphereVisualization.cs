using UnityEngine;

public class AtmosphereVisualization : Singleton<AtmosphereVisualization>
{
    public Material Atmospheere;
    public RenderTexture OpticalDepth;
    public ComputeShader OpticalDepthBaker;
    public GameObject Sun;

    [Header("Settings")]
    public int BakedDetail = 100;
    [Range(1,20)]
    public int RealTimeDetail = 10;
    public int SeaLevel = 900;
    [Range(1,3)]
    public float AtmosphereScale = 2;
    [Range(1,10)]
    public float DencityFalloff = 5.5f;
    public Vector3 WaveLengths = new Vector3(460,610,700);
    [Range(1,10)]
    public float ScatteringStrength = 4;

    private void OnValidate()
    {
        UpdateVisualization();
    }
    private void Start()
    {
        UpdateVisualization();
        Atmospheere.SetFloat("_AtmoshpereScale", 0);
        Planet.Data.Subscribe(_ =>
        {
            Atmospheere.SetFloat("_AtmoshpereScale", AtmosphereScale);
        });
    }
    private void Update()
    {
        Atmospheere.SetVector("_SunDirection", (Sun.transform.position - Planet.Transform.position).normalized);
        Atmospheere.SetVector("_PlanetCenter", Planet.Transform.position);
    }

    private void UpdateVisualization()
    {
        BakeOpticalDepth();
        Atmospheere.SetTexture("_OpticalDepth", OpticalDepth);
        Atmospheere.SetFloat("_Detail", RealTimeDetail);
        Atmospheere.SetFloat("_AtmoshpereScale", AtmosphereScale);
        Atmospheere.SetFloat("_DensityFalloff", DencityFalloff);
        Atmospheere.SetFloat("_SeaLevel", SeaLevel);
        Atmospheere.SetFloat("_ScatteringStrength", ScatteringStrength);
        Atmospheere.SetVector("_WaveLengths", WaveLengths);
    }

    private void BakeOpticalDepth()
    {
        OpticalDepth.Release();
        OpticalDepth.enableRandomWrite = true;
        OpticalDepth.isPowerOfTwo = true;
        OpticalDepth.Create();

        int kernel = OpticalDepthBaker.FindKernel("BakeOpticalDepth");
        OpticalDepthBaker.SetInt("textureSize", 512);
        OpticalDepthBaker.SetInt("numOutScatteringSteps", BakedDetail);
        OpticalDepthBaker.SetFloat("seaLevel", SeaLevel);
        OpticalDepthBaker.SetFloat("atmosphereRadius", SeaLevel * AtmosphereScale);
        OpticalDepthBaker.SetFloat("densityFalloff", DencityFalloff);
        OpticalDepthBaker.SetTexture(kernel, "Result", OpticalDepth);
        OpticalDepthBaker.Dispatch(kernel, Coordinate.TextureWidthInPixels / 8, Coordinate.TextureWidthInPixels / 8, 1);
    }
}
