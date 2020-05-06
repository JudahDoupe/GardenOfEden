using System;
using UnityEngine;

[Serializable]
public struct PlantDna
{
    public string Name;
    public int SpeciesId;
    public int Generation;

    public NodeDna NodeDna;
    public StemDna StemDna;
    public LeafDna LeafDna;
    public RootDna RootDna;
    public FlowerDna FlowerDna;
}

[Serializable]
public struct NodeDna
{
    public int MaxDepth;
    public float NodesPerDay;
}

[Serializable]
public struct StemDna
{
    public enum StemType
    {
        Green,
        Woody,
    }

    public StemType Type;
    public float PrimaryLength;
    public float PrimaryRadius;
    public float DaysToMaturity;
    public Material Material;
}

[Serializable]
public struct LeafDna
{
    public enum LeafType
    {
        Standard,
    }

    public LeafType Type;
    public float Size;
    public float DaysToMaturity;
    public Material Material;
}

[Serializable]
public struct RootDna
{
    public enum RootType
    {
        Relative,
    }

    public RootType Type;
}

[Serializable]
public struct FlowerDna
{
    public float Size;
    public float DaysToMaturity;
    public float DaysForPolination;
    public float DaysToSeed;

    public int NumberOfSeeds;
    public Material Material;
}
