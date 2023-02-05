using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

public class PlateBakerV2 : MonoBehaviour
{
    public bool Debug = false;
    public ComputeShader BakePlatesShader;
    public int MinContinentSize = 2500;
    public int MsBudget = 5;

    private PlateTectonicsData _data;
    private EnvironmentMap _tmpContinentalIdMap;

    private void Start() => Planet.Data.Subscribe(data =>
    {
        _data = data.PlateTectonics;
        _tmpContinentalIdMap = new EnvironmentMap(_data.ContinentalIdMap.ToDbData());
    });

    
    public void BakePlates()
    {
        CancelBake();
        StartCoroutine(BakePlatesAsync());
    }

    public void CancelBake()
    {
        StopAllCoroutines();
    }


    private IEnumerator BakePlatesAsync()
    {
        var logTimer = Stopwatch.StartNew();
        var stepTimer = Stopwatch.StartNew();
        InitializeLabelingGPU();
        while (RunLabelingIterationGpu())
        {
            if (stepTimer.ElapsedMilliseconds >= MsBudget)
                continue;

            yield return new WaitForEndOfFrame();
            stepTimer.Restart();
        }

        
        Log($"Labeled Continents [time:{logTimer.ElapsedMilliseconds}ms]");

        
        logTimer.Restart();
        var refreshTask = _tmpContinentalIdMap.RefreshCacheAsync();
        yield return new WaitUntil(() => refreshTask.IsCompleted);
        var continentMaps = refreshTask.Result.Select(x => x.GetRawTextureData<float>().ToArray()).ToArray();

        var continentsTask = Task.Run(() => IdentifyContinents(continentMaps));
        yield return new WaitUntil(() => continentsTask.IsCompleted);
        var continents = continentsTask.Result;


        Log($"Analyzing Continents [time:{logTimer.ElapsedMilliseconds}ms] [plateCount:{continents.Count}]");


        logTimer.Restart();

        _data.Plates = continents.Values.Where(x => x.IsRoot).Select((c, i) => new PlateData(c.Relabel, i)).ToList();
        _data.PlateThicknessMaps.Layers = _data.Plates.Count * 6;
        var continentRelabels = continents.Values.Select(x => new RelabelGpuData { From = x.Label, To = x.Root.Relabel }).ToArray();
        RelabelContinentsGPU(continentRelabels);
        _data.ContinentalIdMap.RefreshCache();


        Log($"Relabeling Continents [time:{logTimer.ElapsedMilliseconds}ms] [plateCount:{_data.Plates.Count}]");
        
        GetComponent<BreakPlateTool>().Unlock();
    }

    private Dictionary<int, Continent> IdentifyContinents(float[][] continentMaps)
    {
        var continents = new Dictionary<int, Continent>();

        Continent GetOrCreateContinent(int label) => continents.ContainsKey(label)
            ? continents[label]
            : continents[label] = new Continent(label);

        int[] Neighbors(int x, int y, int w) => new[]
        {
            Sample(CoordinateTransforms.GetSourceXyw(new int3(x + 1, y, w))),
            Sample(CoordinateTransforms.GetSourceXyw(new int3(x - 1, y, w))),
            Sample(CoordinateTransforms.GetSourceXyw(new int3(x, y + 1, w))),
            Sample(CoordinateTransforms.GetSourceXyw(new int3(x, y - 1, w)))
        };

        int Sample(int3 xyw) => (int)math.round(continentMaps[xyw.z][xyw.x + xyw.y * Coordinate.TextureWidthInPixels]);

        for (var w = 0; w < 6; w++)
        for (var x = 0; x < Coordinate.TextureWidthInPixels; x++)
        for (var y = 0; y < Coordinate.TextureWidthInPixels; y++)
        {
            var label = Sample(new int3(x, y, w));
            var continent = GetOrCreateContinent(label);
            continent.Size++;

            foreach (var neighborLabel in Neighbors(x, y, w))
            {
                var neighbor = GetOrCreateContinent(neighborLabel);
                if (!neighbor.Equals(continent))
                    continent.Neighbors.Add(neighbor);
            }
        }

        var minLabel = 1;
        foreach (var continent in continents.Values.OrderByDescending(x => x.Size))
        {
            if (continent.Size > (MinContinentSize * MinContinentSize))
            {
                continent.Relabel = minLabel++;
            }
            else
            {
                continent.Root = continent.Neighbors
                    .Select(x => x.Root)
                    .Where(x => !x.Root.Equals(continent))
                    .OrderByDescending(x => x.Size)
                    .First();
                continent.Root.Size += continent.Size;
                
                foreach (var neighbor in continent.Neighbors.Where(x => !x.Equals(continent.Root))) 
                    continent.Root.Neighbors.Add(neighbor);
            }
        }

        return continents;
    }

    private class Continent : IEquatable<Continent>
    {
        public readonly int Label;
        public readonly HashSet<Continent> Neighbors;
        public int Relabel;
        public int Size;
        private Continent _parent;

        public Continent(int label)
        {
            Label = label;
            Relabel = 0;
            Size = 0;
            Neighbors = new HashSet<Continent>();
            _parent = null;
        }

        public bool IsRoot => _parent == null;

        public Continent Root
        {
            get => _parent == null ? this : _parent.Root;
            set => _parent = value.Root;
        }


        public bool Equals(Continent other) => other.Label == Label;
    }

    #region GPU IO

    private void InitializeLabelingGPU()
    {
        var kernel = BakePlatesShader.FindKernel("InitializeLabeling");

        BakePlatesShader.SetTexture(kernel, "TmpContinentalIdMap", _tmpContinentalIdMap.RenderTexture);
        BakePlatesShader.Dispatch(kernel, Coordinate.TextureWidthInPixels / 8, Coordinate.TextureWidthInPixels / 8, 1);
    }

    private bool RunLabelingIterationGpu()
    {
        var kernel = BakePlatesShader.FindKernel("RunLabelingIteration");

        using var labelBuffer = new ComputeBuffer(1, Marshal.SizeOf(typeof(LabelGpuData)));
        var labelData = new[] { new LabelGpuData { Changed = 0 } };
        labelBuffer.SetData(labelData);
        BakePlatesShader.SetBuffer(kernel, "Label", labelBuffer);

        BakePlatesShader.SetTexture(kernel, "TmpContinentalIdMap", _tmpContinentalIdMap.RenderTexture);
        BakePlatesShader.SetTexture(kernel, "ContinentalIdMap", _data.ContinentalIdMap.RenderTexture);
        BakePlatesShader.Dispatch(kernel, Coordinate.TextureWidthInPixels / 8, Coordinate.TextureWidthInPixels / 8, 1);

        labelBuffer.GetData(labelData);
        return labelData[0].Changed > 0;
    }

    private void RelabelContinentsGPU(RelabelGpuData[] relabels)
    {
        var kernel = BakePlatesShader.FindKernel("RelabelContinents");

        using var relabelBuffer = new ComputeBuffer(relabels.Count(), Marshal.SizeOf(typeof(RelabelGpuData)));
        relabelBuffer.SetData(relabels);
        BakePlatesShader.SetBuffer(kernel, "Relabels", relabelBuffer);
        BakePlatesShader.SetInt("NumRelabels", relabels.Count());

        using var plateBuffer = new ComputeBuffer(_data.Plates.Count(), Marshal.SizeOf(typeof(PlateGpuData)));
        plateBuffer.SetData(_data.Plates.Select(x => x.ToGpuData()).ToArray());
        BakePlatesShader.SetBuffer(kernel, "Plates", plateBuffer);
        BakePlatesShader.SetInt("NumPlates", _data.Plates.Count());

        BakePlatesShader.SetFloat("MantleHeight", _data.MantleHeight);
        BakePlatesShader.SetTexture(kernel, "TmpContinentalIdMap", _tmpContinentalIdMap.RenderTexture);
        BakePlatesShader.SetTexture(kernel, "ContinentalIdMap", _data.ContinentalIdMap.RenderTexture);
        BakePlatesShader.SetTexture(kernel, "PlateThicknessMaps", _data.PlateThicknessMaps.RenderTexture);
        BakePlatesShader.SetTexture(kernel, "LandHeightMap", _data.LandHeightMap.RenderTexture);

        BakePlatesShader.Dispatch(kernel, Coordinate.TextureWidthInPixels / 8, Coordinate.TextureWidthInPixels / 8, 1);
    }

    public struct LabelGpuData
    {
        public int Changed;
    }

    public struct RelabelGpuData
    {
        public float From;
        public float To;
    }


    private void Log(string message)
    {
        if (Debug)
        {
            UnityEngine.Debug.Log(message);
        }
    }
    #endregion
}