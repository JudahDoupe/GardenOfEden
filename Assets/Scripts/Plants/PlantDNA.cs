using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public struct PlantDna
{
    public string Name;
    public int SpeciesId;
    public int Generation;

    public List<NodeDna> NodeDna;
    public List<InternodeDna> InternodeDna;

    public NodeDna GetNodeDna(NodeType type)
    {
        return NodeDna.Any(x => x.Type == type) ?
                NodeDna.FirstOrDefault(x => x.Type == type) :
                NodeDna.FirstOrDefault();
    }
    public InternodeDna GetInternodeDna(NodeType type)
    {
        return InternodeDna.Any(x => x.Type == type) ?
                InternodeDna.FirstOrDefault(x => x.Type == type) :
                InternodeDna.FirstOrDefault();
    }
}

[Serializable]
public struct NodeDna
{
    public NodeType Type;
    public float Size;
    [Range(0.01f, 1)]
    public float GrowthRate;
    public string MeshId;
}

[Serializable]
public struct InternodeDna
{
    public NodeType Type;
    public float Length;
    public float Radius;
}
