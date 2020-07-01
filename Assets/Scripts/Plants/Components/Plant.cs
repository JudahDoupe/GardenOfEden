using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Plant : MonoBehaviour
{
    public float lastUpdateDate;
    public int PlantId;
    public PlantDna Dna;

    public List<IGrowthRule> GrowthRules { get; set; }

    public Node Shoot { get; set; }
    public Root Root { get; set; }

    public bool IsAlive { get; set; } = true;
    public bool IsGrowing { get; set; } = false;

    public Volume WaterCapacity = Volume.FromCubicMeters(5);
    public Volume StoredWater { get; set; }
    public Area StoredLight { get; set; }

    void Start()
    {
        Shoot = Node.Create(PlantDna.NodeType.ApicalBud, null, this);
        Root = Root.Create(this);
        lastUpdateDate = EnvironmentApi.GetDate();
        GrowthRules = Dna.GetGrowthRules().ToList();

        DI.LightService.AddLightAbsorber(this, (absorbedLight) => StoredLight += absorbedLight);
        DI.PlantGrowthService.AddPlant(this);
    }

    public void UpdateMesh(float seconds = 0)
    {
        UpdateMeshRecursively(Shoot, seconds);
        if (seconds > 0)
        {
            StartCoroutine(StartGrowTimer(seconds));
        }
    }
    private void UpdateMeshRecursively(Node node, float seconds)
    {
        foreach(var branchNode in node.Branches)
        {
            UpdateMeshRecursively(branchNode, seconds);
        }
        if (seconds > 0)
        {
            StartCoroutine(node.SmoothUpdateMesh(seconds));
        }
        else
        {
            node.UpdateMesh();
        }
    }
    private IEnumerator StartGrowTimer(float seconds)
    {
        IsGrowing = true;
        yield return new WaitForSeconds(seconds);
        IsGrowing = false;
    }

    public void Kill()
    {
        IsAlive = false;
        Shoot.Kill();
        Destroy(gameObject);
    }

    public void Accept(IVisitor Visitor)
    {
        Visitor.VisitPlant(this);
    }
}