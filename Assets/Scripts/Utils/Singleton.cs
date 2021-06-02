using Assets.Scripts.Utils;
using UnityEngine;

public class Singleton : MonoBehaviour
{
    public static ILandService Land;
    public static WaterService Water;

    public static GameService GameService;
    public static TimeService TimeService;
    public static ILoadBalancer LoadBalancer;
    public static EnvironmentDataStore EnvironmentalChunkService;

    public static PerspectiveController PerspectiveController;
    public static RenderMeshLibrary RenderMeshLibrary;
    public static DnaService DnaService;


    private void Awake()
    {
        Land = FindObjectOfType<LandService>();
        Water = FindObjectOfType<WaterService>();

        GameService = FindObjectOfType<GameService>();
        TimeService = FindObjectOfType<TimeService>();
        LoadBalancer = FindObjectOfType<ProximityLoadBalancer>();
        EnvironmentalChunkService = FindObjectOfType<EnvironmentDataStore>();

        PerspectiveController = FindObjectOfType<PerspectiveController>();
        RenderMeshLibrary = FindObjectOfType<RenderMeshLibrary>();
        DnaService = FindObjectOfType<DnaService>();
    }
}
