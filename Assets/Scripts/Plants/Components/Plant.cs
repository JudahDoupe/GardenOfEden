using UnityEngine;

public class Plant : MonoBehaviour
{
    public int PlantId;
    public PlantDna Dna;

    public Node Shoot { get; set; }
    public Root Root { get; set; }

    public bool IsAlive { get; set; } = true;

    public Volume WaterCapacity = Volume.FromCubicMeters(5);
    public Volume StoredWater { get; set; }
    public Area StoredLight { get; set; }

    void Start()
    {
        Shoot = Node.Create(NodeType.Bud, null, this);
        Root = Root.Create(this);

        DI.LightService.AddLightAbsorber(this, (absorbedLight) => StoredLight += absorbedLight);
    }

    public void UpdateMesh()
    {
        UpdateMeshRecursively(Shoot);
    }
    private void UpdateMeshRecursively(Node node)
    {
        foreach(var branchNode in node.Branches)
        {
            UpdateMeshRecursively(branchNode);
        }
        node.UpdateMesh();
    }

    public void Kill()
    {
        IsAlive = false;
        Shoot.Kill();
        DI.GrowthService.StopPlantGrowth(this);
        Destroy(gameObject);
    }

    public void Accept(IFairy fairy)
    {
        fairy.VisitPlant(this);
    }
}