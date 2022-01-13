using Assets.Scripts.Utils;
using UnityEngine;

public class Singleton : MonoBehaviour
{
    public static Singleton Instance;

    public static PlateTectonicsSimulation PlateTectonics;
    public static WaterSimulation Water;

    public static TimeService TimeService;
    public static ILoadBalancer LoadBalancer;

    public static PerspectiveController PerspectiveController;
    public static RenderMeshLibrary RenderMeshLibrary;


    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        PlateTectonics = FindObjectOfType<PlateTectonicsSimulation>();
        Water = FindObjectOfType<WaterSimulation>();

        TimeService = FindObjectOfType<TimeService>();
        LoadBalancer = FindObjectOfType<ProximityLoadBalancer>();

        PerspectiveController = FindObjectOfType<PerspectiveController>();
        RenderMeshLibrary = FindObjectOfType<RenderMeshLibrary>();
    }
}
