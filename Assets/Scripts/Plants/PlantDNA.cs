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