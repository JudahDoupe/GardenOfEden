using System;
using UnityEngine;

[Serializable]
public class PlantDna
{
    public string Name;
    public int SpeciesId;
    public int Generation;

    public float NodesPerDay;
    public StemDna StemDna;
    public LeafDna LeafDna;
    public RootDna RootDna;
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
