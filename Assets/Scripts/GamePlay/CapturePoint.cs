using System.Collections.Generic;
using UnityEngine;

public class CapturePoint : MonoBehaviour
{
    public float CaptureRadius = 20;
    public bool IsCaptured;

    public Material CapturedStem;
    public Material CapturedLeaf;

    public List<GameObject> Stems;
    public List<GameObject> Leaves;

    private GameService _gameService;

    private void Start()
    {
        _gameService = FindObjectOfType<GameService>();
    }

    private void Update()
    {
        if(!IsCaptured && DI.LandService.SampleRootDepth(transform.position + Random.insideUnitSphere * CaptureRadius) > 0)
        {
            Capture();
        }
    }

    private void Capture()
    {
        IsCaptured = true;
        _gameService.PointCapturedSubject.Publish(this);
        foreach (var stem in Stems)
        {
            stem.GetComponent<Renderer>().material = CapturedStem;
        }
        foreach (var leaf in Leaves)
        {
            leaf.GetComponent<Renderer>().material = CapturedLeaf;
        }
    }
}
