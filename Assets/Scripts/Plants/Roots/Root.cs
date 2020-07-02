using UnityEngine;

public class Root : MonoBehaviour
{
    public Plant Plant;

    public float Depth;
    public float Radius;

    public static Root Create(Plant plant)
    {
        var root = new GameObject("Roots").AddComponent<Root>();

        root.transform.parent = plant.transform;
        root.transform.localPosition = new Vector3(0, 0, 0);
        root.transform.localRotation = Quaternion.identity;

        root.Plant = plant;

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
        var bounds = Plant.transform.GetBounds();
        Radius = (bounds.extents.x + bounds.extents.y) / 2f;
        Depth = Mathf.Min(bounds.extents.z / 2, DI.LandService.SampleSoilDepth(transform.position));
    }
}
