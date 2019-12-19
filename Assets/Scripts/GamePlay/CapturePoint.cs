using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CapturePoint : MonoBehaviour
{
    public bool IsCaptured;

    public Material CapturedStem;
    public Material CapturedLeaf;

    public List<GameObject> Stems;
    public List<GameObject> Leaves;

    private void Update()
    {
        if(!IsCaptured && PlantApi.SampleRootDepth(transform.position + Random.insideUnitSphere * 20) > 0)
        {
            Capture();
        }
    }

    private void Capture()
    {
        IsCaptured = true;
        foreach(var stem in Stems)
        {
            stem.GetComponent<Renderer>().material = CapturedStem;
        }
        foreach (var leaf in Leaves)
        {
            leaf.GetComponent<Renderer>().material = CapturedLeaf;
        }
    }
}
