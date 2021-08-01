using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using System.Linq;
using Random = UnityEngine.Random;
using System.Runtime.InteropServices;

public class PlateTectonics
{
    public static List<Plate> Plates = new List<Plate>();
    public static float FaultLineNoise = 0;

    public static void Regenerate(int numPlates, int nodesPerPlate)
    {
        Plates.Clear();
        for (int p = 0; p < numPlates; p++)
        {
            var plate = new Plate
            {
                Id = p,
                Nodes = new List<PlateNode>()
            };
            for(int n = 0; n < nodesPerPlate; n++)
            {
                var node = new PlateNode();
                var coord = new Coordinate(new float3(Singleton.Water.SeaLevel, 0, 0));
                coord.TextureUvw = new float3(Random.value, Random.value, Random.Range(0, 6));
                node.Coord = coord;
                node.Velocity = new Vector2(Random.value, Random.value).normalized;
                plate.Nodes.Add(node);
            }
            Plates.Add(plate);
        }

        LandService.Renderer.material.SetTexture("TectonicPlateIdMap", EnvironmentDataStore.TectonicPlateIdMap);
        LandService.Renderer.material.SetInt("NumTectonicPlates", numPlates);
    }

    public static void UpdatePlates()
    {
        var tectonicsShader = Resources.Load<ComputeShader>("Shaders/Tectonics");
        int idsKernel = tectonicsShader.FindKernel("SetIds");
        var nodes = Plates.SelectMany(p => p.Nodes.Select(n => new PlateNodeData 
        {
            Id = p.Id,
            Position = n.Coord.LocalPlanet,
            Velocity = n.Velocity 
        })).ToArray();
        using var buffer = new ComputeBuffer(nodes.Length, Marshal.SizeOf(typeof(PlateNodeData)));
        buffer.SetData(nodes);
        tectonicsShader.SetBuffer(idsKernel, "Nodes", buffer);
        tectonicsShader.SetTexture(idsKernel, "TectonicsPlateIdMap", EnvironmentDataStore.TectonicPlateIdMap);
        tectonicsShader.SetFloat("SeaLevel", Singleton.Water.SeaLevel);
        tectonicsShader.SetFloat("Noise", FaultLineNoise);
        tectonicsShader.Dispatch(idsKernel, Coordinate.TextureWidthInPixels / 8, Coordinate.TextureWidthInPixels / 8, 1);
    }

    private struct PlateNodeData
    {
        public int Id;
        public float3 Position;
        public float2 Velocity;
    }
}

public struct Plate
{
    public int Id;
    public List<PlateNode> Nodes;
    public float3 Center() 
    {
        float3 sum = new float3(0,0,0);
        foreach(var node in Nodes)
        {
            sum += node.Coord.LocalPlanet;
        }
        return sum / Nodes.Count();
    }
}

public struct PlateNode
{
    public Coordinate Coord;
    public Vector2 Velocity;
}
