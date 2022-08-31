using Assets.Scripts.Utils;

public class Singletons : Singleton<Singletons>
{
    public static TimeService TimeService;
    public static ILoadBalancer LoadBalancer;

    public static CameraController PerspectiveController;
    public static RenderMeshLibrary RenderMeshLibrary;

    private void OnEnable()
    {

        //TODO: Make every singleton inherite from Singleton

        TimeService = FindObjectOfType<TimeService>();
        LoadBalancer = FindObjectOfType<ProximityLoadBalancer>();

        PerspectiveController = FindObjectOfType<CameraController>();
        RenderMeshLibrary = FindObjectOfType<RenderMeshLibrary>();
    }
}
