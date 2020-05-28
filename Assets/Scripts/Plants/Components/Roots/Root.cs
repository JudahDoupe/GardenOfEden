using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Root : MonoBehaviour
{
    public Plant Plant;
    public RootDna Dna;

    public float Depth;
    public float Radius;

    public static Root Create(Plant plant)
    {
        var dna = plant.Dna.RootDna;
        Root root;
        switch (dna.Type)
        {
            case RootDna.RootType.Relative:
                root = new GameObject(dna.Type.ToString()).AddComponent<RelativeRoot>();
                break;
            default:
                root = new GameObject(dna.Type.ToString()).AddComponent<Root>();
                break;
        }

        root.transform.parent = plant.transform;
        root.transform.localPosition = new Vector3(0, 0, 0);
        root.transform.localRotation = Quaternion.identity;

        root.Plant = plant;
        root.Dna = dna;

        return root;
    }

    public void Start()
    {
        DI.RootService.AddRoots(this, AbsorbWater);
    }
    public void AbsorbWater(Volume water)
    {
        Plant.StoredWater += water;
    }
    public virtual void Grow(float days)
    {
        Radius += days;
        Depth = Mathf.Min(Depth + days, DI.LandService.SampleSoilDepth(transform.position));
    }
}
