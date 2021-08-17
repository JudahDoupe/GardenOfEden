using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;

public interface ILandService
{
    float SampleHeight(Coordinate coord);
}

public class LandService : MonoBehaviour, ILandService
{
    public static float SeaLevel = 1000f;
    public static Renderer Renderer;

    /* Publicly Accessible Methods */

    private bool _showContinents;

    public float SampleHeight(Coordinate coord)
    {
        return EnvironmentDataStore.LandHeightMap.Sample(coord).r;
    }

    /* Inner Mechanations */

    void Start()
    {
        Singleton.LoadBalancer.RegisterEndSimulationAction(ProcessDay);

        Renderer = GetComponent<Renderer>();
        Renderer.material.SetTexture("HeightMap", EnvironmentDataStore.LandHeightMap);
        Renderer.gameObject.GetComponent<MeshFilter>().mesh.bounds = new Bounds(Vector3.zero, new Vector3(2000,2000,2000));
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
        EnvironmentDataStore.LandHeightMap.UpdateTextureCache();
    }
}
