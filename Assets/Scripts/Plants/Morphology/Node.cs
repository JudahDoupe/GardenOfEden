using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Node : MonoBehaviour, IDataBaseObject<NodeDto>
{
    public Plant Plant { get; set; }
    public Node Base { get; set; }
    public List<Node> Branches { get; set; } = new List<Node>();
    public RenderingInstanceData NodeMesh { get; set; }
    public RenderingInstanceData InternodeMesh { get; set; }
    public NodeDna Dna => Plant.PlantDna.Nodes.FirstOrDefault(x => x.Type == Type) ?? new NodeDna();

    public int CreationDate { get; set; }
    public int Age => Singleton.TimeService.Day - CreationDate;

    public string Type;
    public float Size;
    public float InternodeLength;
    public float InternodeRadius;
    public float SurfaceArea;

    public float AbsorbedLight;
    public float GrowthHormone;

    public NodeDto ToDto()
    {
        return new NodeDto
        {
            Transform = transform.ToDto(),
            Branches = Branches.Select(x => x.ToDto()).ToArray(),
            CreationDate = CreationDate,
            Type = Type,
            Size = Size,
            InternodeLength = InternodeLength,
            InternodeRadius = InternodeRadius,
            SurfaceArea = SurfaceArea,
            AbsorbedLight = AbsorbedLight,
            GrowthHormone = GrowthHormone,
        };
    }
}

public class NodeDto
{
    public TransformDto Transform { get; set; }
    public NodeDto[] Branches { get; set; }
    public int CreationDate { get; set; }
    public string Type { get; set; }
    public float Size { get; set; }
    public float InternodeLength { get; set; }
    public float InternodeRadius { get; set; }
    public float SurfaceArea { get; set; }
    public float AbsorbedLight { get; set; }
    public float GrowthHormone { get; set; }
}