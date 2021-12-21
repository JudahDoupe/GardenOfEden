using Assets.Scripts.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlateTectonicsSimulation : MonoBehaviour, ISimulation
{
    [Header("Generation")]
    [Range(1, 30)]
    public int NumPlates = 16;
    public float MantleHeight = 900;
    [Range(0,100)]
    public float FaultLineNoise = 0.25f;
    public void Regenerate() => Regenerate(Plates.Count);
    public void Regenerate(int numPlates)
    {
        Plates.Clear();
        FindObjectOfType<PlateTectonicsVisualization>().Initialize();

        for (int p = 1; p <= numPlates; p++)
        {
            var plate = AddPlate(p + 0.0001f);
            plate.Rotation = Random.rotation;
        }

        RunTectonicKernel("ResetPlateThicknessMaps");
        RunTectonicKernel("ResetContinentalIdMap");
        BakeMaps();
        UpdateHeightMap();
        Singleton.Water.Regenerate();
    }

    [Header("Simulation")]
    public ComputeShader TectonicsShader;
    public float OceanicCrustThickness = 25;
    [Range(0, 0.1f)]
    public float SubductionRate = 0.001f;
    [Range(0, 0.1f)]
    public float InflationRate = 0.001f;
    [Range(0.00001f, 10f)]
    public float Gravity = 1f;
    [Range(1, 2)]
    public float PlateCohesion = 1.5f;
    [Range(0, 1)]
    public float PlateInertia = 0.3f;
    public float PlateSpeed = 500;


    private bool _isActive;
    public bool IsActive { 
        get => _isActive; 
        set
        {
            _isActive = value;
            FindObjectOfType<PlateTectonicsAudio>().IsActive = value;
            FindObjectOfType<PlateTectonicsVisualization>().IsActive = value;
        }
    }


    private List<Plate> Plates = new List<Plate>();
    public List<Plate> GetAllPlates() => Plates;
    public Plate GetPlate(float id) => Plates.First(x => x.Id == id);
    public Plate AddPlate(float id)
    {
        var plate = new Plate
        {
            Id = id,
            Idx = Plates.Count,
            Rotation = Quaternion.identity,
            Velocity = Quaternion.identity,
            TargetVelocity = Quaternion.identity,
        };
        var currentLayerCount = Plates.Count * 6;
        var newLayerCount = (Plates.Count + 1) * 6;

        if (Plates.Count == 0)
        {
            EnvironmentDataStore.PlateThicknessMaps.ResetTexture(newLayerCount);
            EnvironmentDataStore.TmpPlateThicknessMaps.ResetTexture(newLayerCount);
        }
        else
        {
            Graphics.CopyTexture(EnvironmentDataStore.PlateThicknessMaps, EnvironmentDataStore.TmpPlateThicknessMaps);
            EnvironmentDataStore.PlateThicknessMaps.ResetTexture(newLayerCount);
            for (var i = 0; i < currentLayerCount; i++)
            {
                Graphics.CopyTexture(EnvironmentDataStore.TmpPlateThicknessMaps, i, EnvironmentDataStore.PlateThicknessMaps, i);
            }
            EnvironmentDataStore.TmpPlateThicknessMaps.ResetTexture(newLayerCount);
        }

        Plates.Add(plate);
        NumPlates = Plates.Count;
        return plate;
    }
    public void RemovePlate(float id)
    {
        var plate = Plates.FirstOrDefault(x => x.Id == id);
        if (plate == null) return;
        
        Plates.Remove(plate);
        var newLayerCount = Plates.Count * 6;

        Graphics.CopyTexture(EnvironmentDataStore.PlateThicknessMaps, EnvironmentDataStore.TmpPlateThicknessMaps);
        EnvironmentDataStore.PlateThicknessMaps.ResetTexture(newLayerCount);
        foreach (var p in Plates)
        {
            var newIdx = Plates.IndexOf(p);
            for (var i = 0; i < 6; i++)
            {
                Graphics.CopyTexture(EnvironmentDataStore.TmpPlateThicknessMaps, (p.Idx * 6) + i, EnvironmentDataStore.PlateThicknessMaps, (newIdx * 6) + i);
            }
            p.Idx = newIdx;
        }
        EnvironmentDataStore.TmpPlateThicknessMaps.ResetTexture(newLayerCount);
        NumPlates = Plates.Count;
    }

    public void UpdateSystem()
    {
        UpdateVelocity();
        UpdateContinentalIdMap();
        UpdatePlateThicknessMaps();
        UpdateHeightMap();

        if (Plates.Any(x => !x.IsAligned) && Plates.All(x => x.IsStopped))
        {
            BakeMaps();
        }
    }
    public void UpdateVelocity()
    {
        foreach (var plate in Plates)
        {
            var velocity = Quaternion.Slerp(plate.Velocity, plate.TargetVelocity, (1 - PlateInertia));
            plate.Velocity = Quaternion.Slerp(Quaternion.identity, velocity, PlateSpeed * Time.deltaTime);
            plate.Rotation *= plate.Velocity;
        }
    }
    public void UpdateContinentalIdMap()
    {
        RunTectonicKernel("UpdateContinentalIdMap");
        EnvironmentDataStore.ContinentalIdMap.UpdateTextureCache();
    }
    public void UpdatePlateThicknessMaps()
    {
        RunTectonicKernel("UpdatePlateThicknessMaps");
    }
    public void UpdateHeightMap()
    {
        RunTectonicKernel("UpdateHeightMap");
        RunTectonicKernel("SmoothPlates");
        EnvironmentDataStore.LandHeightMap.UpdateTextureCache();
    }
    public void BakeMaps()
    {
        RunTectonicKernel("StartAligningPlateThicknessMaps");
        foreach(var plate in Plates)
        {
            plate.Rotation = Quaternion.identity;
            plate.Velocity = Quaternion.identity;
        }
        RunTectonicKernel("FinishAligningPlateThicknessMaps");
        var timer = new Stopwatch();
        timer.Start();
        //DetectContinents();
        UnityEngine.Debug.Log(timer.ElapsedMilliseconds);
        timer.Stop();
    }
  
    private void RunTectonicKernel(string kernelName)
    {
        int kernel = TectonicsShader.FindKernel(kernelName);
        using var buffer = new ComputeBuffer(NumPlates, Marshal.SizeOf(typeof(Plate.GpuData)));
        buffer.SetData(Plates.Select(x => x.ToGpuData()).ToArray());
        TectonicsShader.SetBuffer(kernel, "Plates", buffer);
        TectonicsShader.SetTexture(kernel, "LandHeightMap", EnvironmentDataStore.LandHeightMap);
        TectonicsShader.SetTexture(kernel, "PlateThicknessMaps", EnvironmentDataStore.PlateThicknessMaps);
        TectonicsShader.SetTexture(kernel, "TmpPlateThicknessMaps", EnvironmentDataStore.TmpPlateThicknessMaps);
        TectonicsShader.SetTexture(kernel, "ContinentalIdMap", EnvironmentDataStore.ContinentalIdMap);
        TectonicsShader.SetInt("NumPlates", NumPlates);
        TectonicsShader.SetFloat("OceanicCrustThickness", OceanicCrustThickness);
        TectonicsShader.SetFloat("MantleHeight", MantleHeight);
        TectonicsShader.SetFloat("SubductionRate", SubductionRate);
        TectonicsShader.SetFloat("InflationRate", InflationRate);
        TectonicsShader.SetFloat("Gravity", Gravity);
        TectonicsShader.SetFloat("PlateCohesion", PlateCohesion);
        TectonicsShader.SetFloat("FaultLineNoise", FaultLineNoise);
        TectonicsShader.Dispatch(kernel, Coordinate.TextureWidthInPixels / 8, Coordinate.TextureWidthInPixels / 8, 1);
    }

    private void Start()
    {
        IsActive = false;
    }
    private void Update()
    {
        if (Plates.Count != NumPlates)
        {
            Regenerate(NumPlates);
        }
        if (IsActive)
        {
            UpdateSystem();
        }
    }

    private class Continent
    {
        public float CurrentId = 0;
        public int Size = 0;
        public Dictionary<float, int> Neighbors = new Dictionary<float, int>();
        public List<int3> TexCoords = new List<int3>();
    }
    private class TexCoord : IEquatable<TexCoord>
    {
        public readonly List<int3> Neighbors;
        public readonly int3 Xyw;

        public TexCoord(int3 coord)
        {
            Xyw = coord;
            Neighbors = new List<int3>
            {
                CoordinateTransforms.GetSourceXyw(new int3(coord.x-1, coord.y, coord.z)),
                CoordinateTransforms.GetSourceXyw(new int3(coord.x+1, coord.y, coord.z)),
                CoordinateTransforms.GetSourceXyw(new int3(coord.x, coord.y+1, coord.z)),
                CoordinateTransforms.GetSourceXyw(new int3(coord.x, coord.y-1, coord.z)),
            };
        }
        public bool Equals(TexCoord other) => Xyw.Equals(other.Xyw);
        public int ArrayW => Xyw.z;
        public int ArrayXY => Xyw.y * Coordinate.TextureWidthInPixels + Xyw.x;
    }
    private List<Continent> DetectContinents()
    {
        var continents = new List<Continent>();
        var searched = new Dictionary<int3, bool>();
        var neighbors = new Queue<TexCoord>();
        var textureArray = EnvironmentDataStore.ContinentalIdMap.CachedTextures().Select(x => x.GetRawTextureData<float>().ToArray()).ToArray();

        for (var w = 0; w < 6; w++)
        {
            for (var x = 0; x < Coordinate.TextureWidthInPixels; x++)
            {
                for (var y = 0; y < Coordinate.TextureWidthInPixels; y++)
                {
                    var xyw = new int3(x, y, w);
                    if (!CoordinateTransforms.IsBoundryPixel(xyw))
                    {
                        searched[xyw] = false;
                    }
                }
            }
        }

        while (searched.Any(x => x.Value == false))
        {
            var current = new TexCoord(searched.First(x => x.Value == false).Key);
            var continent = new Continent
            {
                CurrentId = textureArray[current.ArrayW][current.ArrayXY],
            };
            continents.Add(continent);
            neighbors.Enqueue(current);
            searched[current.Xyw] = true;

            while (neighbors.Any())
            {
                current = neighbors.Dequeue();
                foreach (var n in current.Neighbors.Where(x => !searched[x]))
                {
                    var nId = textureArray[current.ArrayW][current.ArrayXY];
                    if (nId == continent.CurrentId)
                    {
                        neighbors.Enqueue(new TexCoord(n));
                        searched[n] = true;
                        continent.TexCoords.Add(n);
                        continent.Size++;
                    }
                    else
                    {
                        if (continent.Neighbors.ContainsKey(nId))
                        {
                            continent.Neighbors[nId]++;
                        }
                        else
                        {
                            continent.Neighbors[nId] = 1;
                        }
                    }
                }
            }
        }

        return continents;
    }

    public class Plate
    {
        public float Id;
        public int Idx;
        public Quaternion Rotation;
        public Quaternion Velocity;
        public Quaternion TargetVelocity;
        public Vector3 Center => Rotation * Vector3.forward * (Singleton.Water.SeaLevel + 100);
        public bool IsStopped => Quaternion.Angle(Velocity, Quaternion.identity) < 0.001f;
        public bool IsAligned => Quaternion.Angle(Rotation, Quaternion.identity) < 0.001f;
        public GpuData ToGpuData() => new GpuData { Id = Id, Idx = Idx, Rotation = new float4(Rotation[0], Rotation[1], Rotation[2], Rotation[3]) };

        public struct GpuData
        {
            public float Id;
            public int Idx;
            public float4 Rotation;
        }
    }
}
