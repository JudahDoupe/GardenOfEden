using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;

public interface ILandService
{
    float SampleHeight(Coordinate coord);
}

public class LandService : MonoBehaviour, ILandService
{
    public int NumPlates = 5;
    [Range(0, 1)]
    public float FaultLineNoise = 0;
    [Range(0, 1)]
    public float PlateDriftSpeed = 1;
    [Range(0, 1)]
    public float PlateVelocityDampening = 0.5f;
    public static float SeaLevel = 1000f;
    public static Renderer Renderer;

    /* Publicly Accessible Methods */

    private bool _showContinents;

    public float SampleHeight(Coordinate coord)
    {
        return EnvironmentDataStore.ContinentalHeightMap.Sample(coord).r;
    }

    /* Inner Mechanations */

    void Start()
    {
        Singleton.LoadBalancer.RegisterEndSimulationAction(ProcessDay);

        Renderer = GetComponent<Renderer>();
        Renderer.material.SetTexture("HeightMap", EnvironmentDataStore.ContinentalHeightMap);
        Renderer.gameObject.GetComponent<MeshFilter>().mesh.bounds = new Bounds(Vector3.zero, new Vector3(2000,2000,2000));

        PlateTectonics.Regenerate(NumPlates, 1);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            _showContinents = !_showContinents;
            Renderer.material.SetFloat("ShowContinents", _showContinents ? 1 : 0);
        }
    }

    public void ProcessDay()
    {
        if(NumPlates != PlateTectonics.Plates.Count)
        {
            PlateTectonics.Regenerate(NumPlates, 1);
        }

        PlateTectonics.FaultLineNoise = FaultLineNoise;
        PlateTectonics.DriftSpeed = PlateDriftSpeed / 100;
        PlateTectonics.Dampening = PlateVelocityDampening / 10;

        PlateTectonics.UpdatePlateIdMap();
        PlateTectonics.UpdatePlateVelocity();
        PlateTectonics.IntegratePlateVelocity();

        EnvironmentDataStore.ContinentalHeightMap.UpdateTextureCache();
    }
}
