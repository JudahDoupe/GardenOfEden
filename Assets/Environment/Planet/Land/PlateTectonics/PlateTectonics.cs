using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using System.Linq;
using Random = UnityEngine.Random;
using System.Runtime.InteropServices;

public class PlateTectonics : MonoBehaviour
{
    public int NumPlates = 50;
    [Range(0.0001f, 1)]
    public float FaultLineNoise = 0.25f;
    [Range(0, 0.01f)]
    public float PlateDriftSpeed = 0.001f;
    [Range(0, 0.1f)]
    public float PlateVelocityDampening = 0.08f;

    public List<Plate> Plates = new List<Plate>();
    public ComputeShader TectonicsShader;

    private void Start()
    {
        Singleton.LoadBalancer.RegisterEndSimulationAction(ProcessDay);
    }
    private void Update()
    {
        if (Plates.Count != NumPlates)
        {
            Regenerate(NumPlates, 1);
        }
    }

    public void ProcessDay()
    {
        UpdatePlateIdMap();
        UpdatePlateVelocity();
        IntegratePlateVelocity();

        EnvironmentDataStore.ContinentalHeightMap.UpdateTextureCache();
    }
    public void Regenerate(int numPlates, int nodesPerPlate)
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
                plate.Nodes.Add(node);
            }
            Plates.Add(plate);
        }

        LandService.Renderer.material.SetTexture("TectonicPlateIdMap", EnvironmentDataStore.ContinentalIdMap);
        LandService.Renderer.material.SetInt("NumTectonicPlates", numPlates);
    }

    private void UpdatePlateIdMap()
    {
        RunTectonicKernel("SetIds");
    }
    private void UpdatePlateVelocity()
    {
        var nodes = Plates.SelectMany(x => x.Nodes);
        foreach (var node in nodes)
        {
            node.Velocity = Vector3.Lerp(node.Velocity, CalculateDriftVelocity(node), PlateVelocityDampening);
        }

        RunTectonicKernel("SetVelocities");
    }
    private void IntegratePlateVelocity()
    {
        RunTectonicKernel("IntegrateVelocities");

        var nodes = Plates.SelectMany(x => x.Nodes);
        foreach (var node in nodes)
        {
            node.Coord.LocalPlanet += node.Velocity;
            node.Coord.Altitude = Singleton.Water.SeaLevel;
        }
    }
    private void RunTectonicKernel(string kernelName)
    {
        int kernel = TectonicsShader.FindKernel(kernelName);
        var nodeData = Plates.SelectMany(p => p.Nodes.Select(n => new PlateNodeData
            {
                Id = p.Id,
                Position = n.Coord.LocalPlanet,
                Velocity = (new Coordinate(n.Coord.LocalPlanet + n.Velocity).TextureUv(n.Coord.TextureW) - n.Coord.TextureUvw.xy) * Coordinate.TextureWidthInPixels
            })).ToArray();
        using var buffer = new ComputeBuffer(nodeData.Length, Marshal.SizeOf(typeof(PlateNodeData)));
        buffer.SetData(nodeData);
        TectonicsShader.SetBuffer(kernel, "Nodes", buffer);
        TectonicsShader.SetTexture(kernel, "ContinentalIdMap", EnvironmentDataStore.ContinentalIdMap);
        TectonicsShader.SetTexture(kernel, "ContinentalHeightMap", EnvironmentDataStore.ContinentalHeightMap);
        TectonicsShader.SetTexture(kernel, "ContinentalVelocityMap", EnvironmentDataStore.ContinentalVelocityMap);
        TectonicsShader.SetFloat("SeaLevel", Singleton.Water.SeaLevel);
        TectonicsShader.SetFloat("FaultLineNoise", FaultLineNoise * 100);
        TectonicsShader.Dispatch(kernel, Coordinate.TextureWidthInPixels / 8, Coordinate.TextureWidthInPixels / 8, 1);
    }
    private float3 CalculateDriftVelocity(PlateNode node)
    {
        var nodes = Plates.SelectMany(x => x.Nodes);
        Vector3 drift = Vector3.zero;
        foreach (var otherNode in nodes)
        {
            var vector = (node.Coord.LocalPlanet - otherNode.Coord.LocalPlanet).ToVector3();
            var direction = vector.normalized;
            var distance = vector.magnitude;
            var magnitude = 1 - math.pow(distance / Singleton.Water.SeaLevel, 3);
            drift += direction * magnitude;
        }
        drift /= nodes.Count();
        drift *= PlateDriftSpeed;
        var driftCoord = new Coordinate(drift.ToFloat3());
        driftCoord.Altitude = Singleton.Water.SeaLevel;
        return (driftCoord.LocalPlanet - node.Coord.LocalPlanet) * PlateDriftSpeed;
    }

    private struct PlateNodeData
    {
        public int Id;
        public float3 Position;
        public float2 Velocity;
    }

    public class Plate
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

    public class PlateNode
    {
        public Coordinate Coord;
        public float3 Velocity;
    }
}
