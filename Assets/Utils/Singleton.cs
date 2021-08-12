using Assets.Scripts.Utils;
using UnityEngine;

public class Singleton : MonoBehaviour
{
    public static Singleton Instance;

    public static ILandService Land;
    public static PlateTectonics PlateTectonics;
    public static WaterService Water;

    public static TimeService TimeService;
    public static ILoadBalancer LoadBalancer;
    public static EnvironmentDataStore EnvironmentalChunkService;

    public static PerspectiveController PerspectiveController;
    public static RenderMeshLibrary RenderMeshLibrary;
    public static DnaService DnaService;


    private void Awake()
    {
        Instance = this;
        Land = FindObjectOfType<LandService>();
        PlateTectonics = FindObjectOfType<PlateTectonics>();
        Water = FindObjectOfType<WaterService>();

        TimeService = FindObjectOfType<TimeService>();
        LoadBalancer = FindObjectOfType<ProximityLoadBalancer>();
        EnvironmentalChunkService = FindObjectOfType<EnvironmentDataStore>();

        PerspectiveController = FindObjectOfType<PerspectiveController>();
        RenderMeshLibrary = FindObjectOfType<RenderMeshLibrary>();
        DnaService = FindObjectOfType<DnaService>();
    }
}
