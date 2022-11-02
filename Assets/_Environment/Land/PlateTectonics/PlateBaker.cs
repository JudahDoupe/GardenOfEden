﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class PlateBaker : MonoBehaviour
{
    public ComputeShader BakePlatesShader;
    public int MinContinentSize = 2500;
    public bool Debug = false;
    public bool IsInitialized => _data != null;
    public bool IsActive { get; private set; }

    private CancellationTokenSource _cancelation;
    private bool _isBaking = false;
    private int _lastPlateCount = 0;

    private PlateTectonicsData _data;

    private EnvironmentMap _tmpPlateThicknessMaps;
    private EnvironmentMap _tmpContinentalIdMap;

    public void Initialize(PlateTectonicsData data)
    {
        _data = data;
    }

    public void Enable()
    {
        if (Debug) UnityEngine.Debug.Log("Plate Baker Enabled");
        IsActive = true;
    }

    public void Disable()
    {
        if (Debug) UnityEngine.Debug.Log("Plate Baker Disabled");
        if (_cancelation != null) _cancelation.Cancel();
        IsActive = false;
    }

    private void Update()
    {
        if (!IsInitialized || !IsActive)
            return;

        if (NeedsBaking())
        {
            BakePlates();
        }

        if (_isBaking && (!_data.Plates.All(x => x.IsStopped) || _data.Plates.Count() != _lastPlateCount))
        {
            _cancelation.Cancel();
        }

        _lastPlateCount = _data.Plates.Count;

        bool NeedsBaking() => !_isBaking
                              && _data.Plates.Any(x => !x.IsAligned) 
                              && _data.Plates.All(x => x.IsStopped);
    }

    public void BakePlates()
    {
        _cancelation = new CancellationTokenSource();
        _data.ContinentalIdMap.RefreshCache(BakePlatesAsync);
    }
    private async void BakePlatesAsync()
    {
        if (Debug) UnityEngine.Debug.Log("Starting Bake");
        var timer = new Stopwatch();
        timer.Start();
        _isBaking = true;

        _tmpPlateThicknessMaps = new EnvironmentMap(_data.PlateThicknessMaps.ToDbData());
        _tmpContinentalIdMap = new EnvironmentMap(_data.ContinentalIdMap.ToDbData());

        AlignPlates();

        if (_cancelation.IsCancellationRequested)
        {
            _isBaking = false;
            if (Debug) UnityEngine.Debug.Log("Canceled Bake");
            return;
        }

        var continentIdMaps = _data.ContinentalIdMap.CachedTextures.Select(x => x.GetRawTextureData<float>().ToArray())
            .ToArray();
        var continents =
            await Task.Run(() => CoalesceContinents(DetectContinents(continentIdMaps)), _cancelation.Token);

        if (_cancelation.IsCancellationRequested)
        {
            _isBaking = false;
            if (Debug) UnityEngine.Debug.Log("Canceled Bake");
            return;
        }

        UpdateContinentIds(continents);

        foreach (var plateId in _data.Plates
                     .Where(x => !continents.Select(c => c.CurrentId).Contains(x.Id))
                     .Select(x => x.Id)
                     .ToArray())
        {
            _data.RemovePlate(plateId);
        }

        if (Debug) UnityEngine.Debug.Log($"Finished Bake in {timer.ElapsedMilliseconds} ms");

        Planet.Instance.Save();

        _isBaking = false;
    }

    private void AlignPlates()
    {
        RunTectonicKernel("StartAligningPlateThicknessMaps");
        foreach (var plate in _data.Plates)
        {
            plate.Rotation = Quaternion.identity;
            plate.Velocity = Quaternion.identity;
        }

        RunTectonicKernel("FinishAligningPlateThicknessMaps");
    }

    private void RunTectonicKernel(string name)
    {
        var kernel = BakePlatesShader.FindKernel(name);
        using var buffer = new ComputeBuffer(_data.Plates.Count(), Marshal.SizeOf(typeof(PlateGpuData)));
        buffer.SetData(_data.Plates.Select(x => x.ToGpuData()).ToArray());
        BakePlatesShader.SetBuffer(kernel, "Plates", buffer);
        BakePlatesShader.SetInt("NumPlates", _data.Plates.Count());
        BakePlatesShader.SetFloat("MantleHeight", _data.MantleHeight);
        BakePlatesShader.SetTexture(kernel, "TmpPlateThicknessMaps", _tmpPlateThicknessMaps.RenderTexture);
        BakePlatesShader.SetTexture(kernel, "PlateThicknessMaps", _data.PlateThicknessMaps.RenderTexture);
        BakePlatesShader.SetTexture(kernel, "TmpContinentalIdMap", _tmpContinentalIdMap.RenderTexture);
        BakePlatesShader.SetTexture(kernel, "ContinentalIdMap", _data.ContinentalIdMap.RenderTexture);
        BakePlatesShader.Dispatch(kernel, Coordinate.TextureWidthInPixels / 8, Coordinate.TextureWidthInPixels / 8, 1);
    }

    private List<Continent> DetectContinents(float[][] continentIdMap)
    {
        var continents = new List<Continent>();
        var open = new HashSet<int3>();
        var neighbors = new Queue<TexCoord>();

        for (var w = 0; w < 6; w++)
        {
            for (var x = 0; x < Coordinate.TextureWidthInPixels; x++)
            {
                for (var y = 0; y < Coordinate.TextureWidthInPixels; y++)
                {
                    var xyw = new int3(x, y, w);
                    if (!CoordinateTransforms.IsBoundryPixel(xyw))
                    {
                        open.Add(xyw);
                    }
                }
            }
        }

        while (open.Any())
        {
            var current = new TexCoord(open.First());
            var continent = new Continent
            {
                CurrentId = continentIdMap[current.ArrayW][current.ArrayXY],
            };
            continents.Add(continent);
            neighbors.Enqueue(current);
            open.Remove(current.Xyw);

            while (neighbors.Any())
            {
                current = neighbors.Dequeue();
                continent.TexCoords.Add(current.Xyw);
                continent.Size++;

                foreach (var neighbor in current.Neighbors)
                {
                    var neighborId = continentIdMap[neighbor.ArrayW][neighbor.ArrayXY];
                    if (neighborId == continent.CurrentId && open.Contains(neighbor.Xyw))
                    {
                        neighbors.Enqueue(neighbor);
                        open.Remove(neighbor.Xyw);
                    }
                    else if (neighborId != continent.CurrentId && neighborId != 0)
                    {
                        continent.Neighbors.Add(neighbor.Xyw);
                    }
                }
            }
        }

        return continents;
    }

    private List<Continent> CoalesceContinents(List<Continent> continents)
    {
        var lastCount = 0;
        var smallContinents = continents.Where(x => x.Size < MinContinentSize).OrderBy(x => x.Size).ToList();
        var largeContinents = continents.Where(x => x.Size >= MinContinentSize).OrderBy(x => x.Size).ToList();

        while (smallContinents.Any() && smallContinents.Count() != lastCount)
        {
            foreach (var continent in smallContinents.ToArray())
            {
                var potentialParents = new Dictionary<Continent, float>();
                foreach (var c in largeContinents)
                {
                    potentialParents[c] = 0;
                }

                foreach (var n in continent.Neighbors)
                {
                    var c = largeContinents.FirstOrDefault(x => x.TexCoords.Contains(n));
                    if (c != null)
                    {
                        potentialParents[c]++;
                    }
                }

                if (potentialParents.Values.Sum() > 0)
                {
                    var parentContinent = potentialParents.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
                    foreach (var t in continent.TexCoords)
                    {
                        parentContinent.TexCoords.Add(t);
                    }

                    smallContinents.Remove(continent);
                }
            }

            lastCount = smallContinents.Count();
        }

        return largeContinents.Concat(smallContinents).ToList();
    }

    private void UpdateContinentIds(List<Continent> continents)
    {
        var c = Coordinate.TextureWidthInPixels * Coordinate.TextureWidthInPixels;
        var textureArrays = new[]
        {
            new float[c],
            new float[c],
            new float[c],
            new float[c],
            new float[c],
            new float[c],
        };

        foreach (var continent in continents)
        {
            foreach (var texCoord in continent.TexCoords.Select(x => new TexCoord(x)))
            {
                textureArrays[texCoord.ArrayW][texCoord.ArrayXY] = continent.CurrentId;
            }
        }

        var texture2dArray = new Texture2DArray(Coordinate.TextureWidthInPixels, Coordinate.TextureWidthInPixels, 6,
            _tmpContinentalIdMap.GraphicsFormat, TextureCreationFlags.None);
        for (var i = 0; i < textureArrays.Length; i++)
        {
            texture2dArray.SetPixelData(textureArrays[i], 0, i);
        }

        texture2dArray.Apply();
        _tmpContinentalIdMap.SetTextures(texture2dArray);

        RunTectonicKernel("BakePlates");
    }

    private class Continent
    {
        public float CurrentId = 0;
        public int Size = 0;
        public HashSet<int3> Neighbors = new HashSet<int3>();
        public HashSet<int3> TexCoords = new HashSet<int3>();
    }

    private class TexCoord : IEquatable<TexCoord>
    {
        public List<TexCoord> Neighbors => new List<TexCoord>
        {
            new TexCoord(CoordinateTransforms.GetSourceXyw(new int3(Xyw.x - 1, Xyw.y, Xyw.z))),
            new TexCoord(CoordinateTransforms.GetSourceXyw(new int3(Xyw.x + 1, Xyw.y, Xyw.z))),
            new TexCoord(CoordinateTransforms.GetSourceXyw(new int3(Xyw.x, Xyw.y + 1, Xyw.z))),
            new TexCoord(CoordinateTransforms.GetSourceXyw(new int3(Xyw.x, Xyw.y - 1, Xyw.z))),
        };

        public readonly int3 Xyw;

        public TexCoord(int3 coord)
        {
            Xyw = coord;
        }

        public bool Equals(TexCoord other) => Xyw.Equals(other.Xyw);
        public int ArrayW => Xyw.z;
        public int ArrayXY => Xyw.y * Coordinate.TextureWidthInPixels + Xyw.x;
    }
}