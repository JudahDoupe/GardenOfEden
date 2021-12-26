using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Debug = UnityEngine.Debug;

public class PlateBaker : MonoBehaviour
{
    public ComputeShader BakePlatesShader;
    public float MiliseconsPerFrame = 10;
    public int MinContinentSize = 200;

    private IEnumerator _handle = null;
    private bool _needsBaking = false;
    private bool _isBaking = false;

    private void Update()
    {
        var plates = Singleton.PlateTectonics.GetAllPlates();
        if (plates.Any(x => !x.IsAligned))
        {
            _needsBaking = true;
        }
        if (_needsBaking && !_isBaking && plates.All(x => x.IsStopped))
        {
            _handle = BakePlates();
            StartCoroutine(_handle);
        }
        if (_isBaking && !plates.All(x => x.IsStopped))
        {
            StopCoroutine(_handle);
            _isBaking = false;
            Debug.Log("Canceled Bake");
        }
    }

    private IEnumerator BakePlates()
    {
        Debug.Log("Starting Bake");
        _isBaking = true;
        
        AlignPlates();
        
        var continents = new List<Continent>();
        yield return PopulateContinents(continents);
        yield return UpdateContinentIds(continents);
        RunTectonicKernel("BakePlates");

        foreach (var plateId in Singleton.PlateTectonics.GetAllPlates().Where(x=> !continents.Select(x => x.CurrentId).Contains(x.Id)).Select(x => x.Id))
        {
            Singleton.PlateTectonics.RemovePlate(plateId);
        }

        Debug.Log("Finished Bake");
        _isBaking = false;
        _needsBaking = false;
    }
    private void AlignPlates()
    {
        RunTectonicKernel("StartAligningPlateThicknessMaps");
        foreach (var plate in Singleton.PlateTectonics.GetAllPlates())
        {
            plate.Rotation = Quaternion.identity;
            plate.Velocity = Quaternion.identity;
        }
        RunTectonicKernel("FinishAligningPlateThicknessMaps");
    }
    private void RunTectonicKernel(string name)
    {
        var plates = Singleton.PlateTectonics.GetAllPlates();
        int kernel = BakePlatesShader.FindKernel(name);
        using var buffer = new ComputeBuffer(plates.Count(), Marshal.SizeOf(typeof(Plate.GpuData)));
        buffer.SetData(plates.Select(x => x.ToGpuData()).ToArray());
        BakePlatesShader.SetBuffer(kernel, "Plates", buffer);
        BakePlatesShader.SetInt("NumPlates", plates.Count());
        BakePlatesShader.SetFloat("MantleHeight", Singleton.PlateTectonics.MantleHeight);
        BakePlatesShader.SetTexture(kernel, "TmpPlateThicknessMaps", EnvironmentDataStore.TmpPlateThicknessMaps);
        BakePlatesShader.SetTexture(kernel, "PlateThicknessMaps", EnvironmentDataStore.PlateThicknessMaps);
        BakePlatesShader.SetTexture(kernel, "TmpContinentalIdMap", EnvironmentDataStore.TmpContinentalIdMap);
        BakePlatesShader.SetTexture(kernel, "ContinentalIdMap", EnvironmentDataStore.ContinentalIdMap);
        BakePlatesShader.Dispatch(kernel, Coordinate.TextureWidthInPixels / 8, Coordinate.TextureWidthInPixels / 8, 1);
    }

    private IEnumerator PopulateContinents(List<Continent> continents)
    {
        var stopwatch = new Stopwatch();
        var open = new HashSet<int3>();
        var neighbors = new Queue<TexCoord>();
        var textureArray = EnvironmentDataStore.ContinentalIdMap.CachedTextures().Select(x => x.GetRawTextureData<float>()).ToArray();

        stopwatch.Start();
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

                    if (stopwatch.ElapsedMilliseconds > MiliseconsPerFrame)
                    {
                        yield return new WaitForEndOfFrame();
                        stopwatch.Restart();
                    }
                }
            }
        }

        while (open.Any())
        {
            var current = new TexCoord(open.First());
            var continent = new Continent
            {
                CurrentId = textureArray[current.ArrayW][current.ArrayXY],
            };
            continents.Add(continent);
            neighbors.Enqueue(current);
            open.Remove(current.Xyw);

            while (neighbors.Any())
            {
                current = neighbors.Dequeue();
                continent.TexCoords.Add(current);
                continent.Size++;

                foreach (var neighbor in current.Neighbors.Select(x => new TexCoord(x)))
                {
                    var neighborId = textureArray[neighbor.ArrayW][neighbor.ArrayXY];
                    if (neighborId == continent.CurrentId && open.Contains(neighbor.Xyw))
                    {
                        neighbors.Enqueue(neighbor);
                        open.Remove(neighbor.Xyw);
                    }
                    else if(neighborId != continent.CurrentId && neighborId != 0)
                    {
                        if (continent.Neighbors.ContainsKey(neighborId))
                        {
                            continent.Neighbors[neighborId]++;
                        }
                        else
                        {
                            continent.Neighbors[neighborId] = 1;
                        }
                    }
                }

                if (stopwatch.ElapsedMilliseconds > MiliseconsPerFrame)
                {
                    yield return new WaitForEndOfFrame();
                    stopwatch.Restart();
                }
            }
        }

        foreach(var array in textureArray)
        {
            array.Dispose();
        }
    }
    private IEnumerator UpdateContinentIds(List<Continent> continents)
    {
        var stopwatch = new Stopwatch();
        var c = Coordinate.TextureWidthInPixels * Coordinate.TextureWidthInPixels;
        var textureArrays = new float[][]{
            new float[c],
            new float[c],
            new float[c],
            new float[c],
            new float[c],
            new float[c],
        };

        stopwatch.Start();
        foreach (var continent in continents)
        {
            var tmp = continent;
            var continentsWithId = continents.Where(x => x.CurrentId == continent.CurrentId);
            var isLargest = continentsWithId.Aggregate((x, y) => x.Size > y.Size ? x : y) == continent;

            if (continent.Size < MinContinentSize)
            {
                continent.CurrentId = continent.Neighbors.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
            }
            if (continent.Size >= MinContinentSize && continentsWithId.Count() > 1 && !isLargest)
            {
                var plate = Singleton.PlateTectonics.AddPlate();
                continent.CurrentId = plate.Id;
            }

            foreach (var texCoord in continent.TexCoords)
            {
                textureArrays[texCoord.ArrayW][texCoord.ArrayXY] = continent.CurrentId;
                    
                if (stopwatch.ElapsedMilliseconds > MiliseconsPerFrame)
                {
                    yield return new WaitForEndOfFrame();
                    stopwatch.Restart();
                }
            }
        }

        var texture2dArray = new Texture2DArray(EnvironmentDataStore.TmpContinentalIdMap.width, EnvironmentDataStore.TmpContinentalIdMap.height, 6, EnvironmentDataStore.TmpContinentalIdMap.graphicsFormat, TextureCreationFlags.None);
        for (int i = 0; i < textureArrays.Length; i++)
        {
            texture2dArray.SetPixelData(textureArrays[i], 0, i);
        }
        texture2dArray.Apply();
        EnvironmentDataStore.TmpContinentalIdMap.UpdateTexture(texture2dArray);
    }

    private class Continent
    {
        public float CurrentId = 0;
        public int Size = 0;
        public Dictionary<float, int> Neighbors = new Dictionary<float, int>();
        public List<TexCoord> TexCoords = new List<TexCoord>();
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
}