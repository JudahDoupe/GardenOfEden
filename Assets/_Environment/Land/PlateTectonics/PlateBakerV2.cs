using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

public class PlateBakerV2 : MonoBehaviour
{
    public ComputeShader BakePlatesShader;
    public int MinContinentSize = 2500;
    public int MsBudget = 5;

    private EnvironmentMap _tmpContinentalIdMap;
    private PlateTectonicsData _data;

    public void Initialize(PlateTectonicsData data)
    {
        _data = data;
        _tmpContinentalIdMap = new EnvironmentMap(_data.ContinentalIdMap.ToDbData());
    }

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
        Debug.Log("Labeling Continents");


        var stepTimer = DebugUtils.StartTimer();
        InitializeLabelingGPU();
        while (RunLabelingIterationGpu())
        {
            if (stepTimer.ElapsedMilliseconds < MsBudget)
            {
                yield return new WaitForEndOfFrame();
                stepTimer.Restart();
            }
        }


        Debug.Log("Analyzing Continents");


        var refreshTask = _tmpContinentalIdMap.RefreshCacheAsync();
        yield return new WaitUntil(() => refreshTask.IsCompleted);
        var continentMaps = refreshTask.Result.Select(x => x.GetRawTextureData<float>().ToArray()).ToArray();

        var continentsTask = Task.Run(() => IdentifyContinents(continentMaps));
        yield return new WaitUntil(() => continentsTask.IsCompleted);
        var continents = continentsTask.Result;


        Debug.Log("Relabeling Continents");


        _data.Plates = continents.Values.Where(x => x.IsRoot).Select((c, i) => new PlateData(c.Relabel, i)).ToList();

        _data.PlateThicknessMaps.Layers = _data.Plates.Count() * 6;
        var _continentRelabels = continents.Values.Select(x => new RelabelGpuData{ From = x.Label, To = x.Root.Relabel}).ToArray();
        RelableContinentsGPU(_continentRelabels);
        _data.ContinentalIdMap.RefreshCache();


        Debug.Log("Plates Baked");
    }

    private Dictionary<float, Continent> IdentifyContinents(float[][] continentMaps)
    {
        var continents = new Dictionary<float, Continent>();
        Continent GetOrCreateContinent(float label) => continents.ContainsKey(label)
            ? continents[label]
            : continents[label] = new Continent(label);

        float[] Neighbors(int x, int y, int w) => new float[] {
            Sample(CoordinateTransforms.GetSourceXyw(new int3(x + 1, y, w))),
            Sample(CoordinateTransforms.GetSourceXyw(new int3(x - 1, y, w))),
            Sample(CoordinateTransforms.GetSourceXyw(new int3(x, y + 1, w))),
            Sample(CoordinateTransforms.GetSourceXyw(new int3(x, y - 1, w))),
        };
        float Sample(int3 xyw) => math.round(continentMaps[xyw.z][xyw.x + xyw.y * Coordinate.TextureWidthInPixels]);

        for (var w = 0; w < 6; w++)
        {
            for (var x = 0; x < Coordinate.TextureWidthInPixels; x++)
            {
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
            }
        }

        var continentList = continents.Values.ToList();
        var minLabel = 1.0001f;
        foreach (var continent in continents.Values)
        {
            if (continent.Size < MinContinentSize)
            {
                var neighbors = continent.Neighbors.Select(x => x.Root).Where(x => !x.Root.Equals(continent));
                var newRoot = neighbors.Aggregate((x, y) => x.Size > y.Size ? x : y);
                continent.Root = newRoot;
                newRoot.Size += continent.Size;
                foreach (var neighbor in neighbors.Where(x => !x.Equals(newRoot)))
                {
                    newRoot.Neighbors.Add(neighbor);
                }
            }
            else
            {
                continent.Relabel = minLabel;
                minLabel += 1;
            }
        }

        return continents;
    }

    private class Continent : IEquatable<Continent>
    {
        public float Label;
        public float Relabel;
        public int Size;
        public HashSet<Continent> Neighbors;
        public bool IsRoot => _parent == null;

        private Continent _parent;
        public Continent Root
        {
            get => _parent == null ? this : _parent.Root;
            set => _parent = value.Root;
        }

        public Continent(float label)
        {
            Label = label;
            Relabel = 0;
            Size = 0;
            Neighbors = new HashSet<Continent>();
            _parent = null;
        }


        public bool Equals(Continent other) => other.Label == this.Label; 
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
    private void RelableContinentsGPU(RelabelGpuData[] relabels)
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

    #endregion
}
