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
    public ComputeShader BakePlatesShader;
    public int MinContinentSize = 2500;
    public int MsBudget = 5;

    private EnvironmentMap _tmpPlateThicknessMaps;
    private EnvironmentMap _tmpContinentalIdMap;
    private PlateTectonicsData _data;
    private RelabelGpuData[] _continentRelabels;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
            BakePlates();
    }

    public void Initialize(PlateTectonicsData data)
    {
        _data = data;
        _tmpContinentalIdMap = new EnvironmentMap(_data.ContinentalIdMap.ToDbData());
        _tmpPlateThicknessMaps = new EnvironmentMap(_data.PlateThicknessMaps.ToDbData());
    }

    public void BakePlates()
    {
        StopAllCoroutines();
        StartCoroutine(BakePlatesAsync());
    }

    private IEnumerator BakePlatesAsync()
    {
        var stopwatch = new Stopwatch();
        stopwatch.Restart();

        yield return AlignPlates();

        yield return LabelContinents();

        yield return RelabelsContinents();

        yield return SyncData();

        UnityEngine.Debug.Log($"Bake Time: {stopwatch.ElapsedMilliseconds}ms");
    }

    #region Steps

    private IEnumerator AlignPlates()
    {
        var stopwatch = new Stopwatch();
        stopwatch.Restart();

        _tmpPlateThicknessMaps.Layers = _data.PlateThicknessMaps.Layers;
        yield return new WaitForEndOfFrame();

        AlignPlatesGPU();
        foreach (var plate in _data.Plates)
        {
            plate.Rotation = Quaternion.identity;
            plate.Velocity = Quaternion.identity;
        }
        _data.PlateThicknessMaps.SetTextures(_tmpPlateThicknessMaps);

        UnityEngine.Debug.Log($"Align Plates Time: {stopwatch.ElapsedMilliseconds}ms | Frames: {2}");
        yield return new WaitForEndOfFrame();
    }

    private IEnumerator LabelContinents()
    {
        var totalStopwatch = new Stopwatch();
        var fameStopwatch = new Stopwatch();

        fameStopwatch.Restart();
        totalStopwatch.Restart();
        
        var iterations = 0;
        var frames = 0;

        InitializeLabelingGPU();
        while (RunLabelingIterationGpu())
        {
            iterations++;
            if (fameStopwatch.ElapsedMilliseconds > MsBudget)
            {
                yield return new WaitForEndOfFrame();
                fameStopwatch.Restart();
                frames++;
            } 
        }
        
        UnityEngine.Debug.Log($"Labeling Time: {totalStopwatch.ElapsedMilliseconds}ms | Frames: {frames} | Iterations: {iterations}");
        yield return new WaitForEndOfFrame();
    }

    private IEnumerator RelabelsContinents()
    {
        var frames = 0;
        var totalStopwatch = new Stopwatch();
        var fameStopwatch = new Stopwatch();

        fameStopwatch.Restart();
        totalStopwatch.Restart();

        yield return new WaitForEndOfFrame();

        //get labels from gpu
        var refreshTask = _tmpPlateThicknessMaps.RefreshCacheAsync();
        yield return new WaitUntil(() => refreshTask.IsCompleted);
        var continentMaps = _tmpPlateThicknessMaps.CachedTextures.Select(x => x.GetRawTextureData<float>().ToArray()).ToArray();

        //build continent graph
        float[] Neighbors(int x, int y, int w) => new float[] {
            Sample(CoordinateTransforms.GetSourceXyw(new int3(x + 1, y, w))),
            Sample(CoordinateTransforms.GetSourceXyw(new int3(x - 1, y, w))),
            Sample(CoordinateTransforms.GetSourceXyw(new int3(x, y + 1, w))),
            Sample(CoordinateTransforms.GetSourceXyw(new int3(x, y - 1, w))),
        };
        float Sample(int3 xyw) => math.round(continentMaps[xyw.z][xyw.x + xyw.y * Coordinate.TextureWidthInPixels]);

        //TODO: Taskify this
        var continents = new Dictionary<float, Continent>();
        Continent GetOrCreateContinent(float label) => continents.ContainsKey(label)
            ? continents[label]
            : continents[label] = new Continent(label);

        for (var w = 0; w < 6; w++)
        {
            for (var x = 0; x < Coordinate.TextureWidthInPixels; x++)
            {
                for (var y = 0; y < Coordinate.TextureWidthInPixels; y++)
                {
                    var label = Sample(new int3(x,y,w));
                    var continent = GetOrCreateContinent(label);
                    continent.Size++;

                    foreach (var neighborLabel in Neighbors(x, y, w))
                    {
                        var neighbor = GetOrCreateContinent(neighborLabel);
                        if (!neighbor.Equals(continent))
                            continent.Neighbors.Add(neighbor);
                    }

                    if (fameStopwatch.ElapsedMilliseconds > MsBudget)
                    {
                        yield return new WaitForEndOfFrame();
                        fameStopwatch.Restart();
                        frames++;
                    }
                }
            }
        }

        UnityEngine.Debug.Log($"Identify Continents Time: {totalStopwatch.ElapsedMilliseconds}ms | Frames: {frames} | Labels: {continents.Count()}");
        yield return new WaitForEndOfFrame();
        totalStopwatch.Restart();
        fameStopwatch.Restart();
        frames = 0;

        var continentList = continents.Values.ToList();
        var minIdx = 0;
        var minLabel = 1.0001f;
        foreach (var continent in continents.Values)
        {
            if (continent.Size < MinContinentSize)
            {
                var neighbors = continent.Neighbors.Select(x => x.Root).Where(x => !x.Root.Equals(continent));
                var newRoot = neighbors.Aggregate((x, y) => x.Size > y.Size ? x : y);
                continent.Root = newRoot;
                newRoot.Size += continent.Size;
                foreach(var neighbor in neighbors.Where(x => !x.Equals(newRoot)))
                {
                    newRoot.Neighbors.Add(neighbor);
                }
            }
            else
            {
                continent.Relabel = minLabel;
                continent.Idx = minIdx;
                minLabel += 1;
                minIdx += 1;
            }

            if (fameStopwatch.ElapsedMilliseconds > MsBudget)
            {
                yield return new WaitForEndOfFrame();
                fameStopwatch.Restart();
                frames++;
            }
        }


        UnityEngine.Debug.Log($"Identify Relabels Time: {totalStopwatch.ElapsedMilliseconds}ms | Frames: {frames} | Labels: {math.floor(minLabel)}");
        yield return new WaitForEndOfFrame();
        totalStopwatch.Restart();


        _continentRelabels = continents.Values.Select(x => new RelabelGpuData{ From = x.Label, To = x.Root.Relabel, ToIdx = x.Idx }).ToArray();
        _tmpPlateThicknessMaps.Layers = _continentRelabels.Count();
        RelableContinentsGPU();


        UnityEngine.Debug.Log($"Relabel Time: {totalStopwatch.ElapsedMilliseconds}ms");
        yield return new WaitForEndOfFrame();
    }
     
    private IEnumerator SyncData()
    {
        var stopwatch = new Stopwatch();
        stopwatch.Restart();

        _data.Plates = _continentRelabels.Select((c, i) => new PlateData(c.To, i)).ToList();
        _data.ContinentalIdMap.SetTextures(_tmpContinentalIdMap);
        _data.PlateThicknessMaps.SetTextures(_tmpPlateThicknessMaps);

        UnityEngine.Debug.Log($"Sync Data Time: {stopwatch.ElapsedMilliseconds}ms");
        yield return new WaitForEndOfFrame();
    }

    private class Continent : IEquatable<Continent>
    {
        public float Label;
        public float Relabel;
        public int Idx;
        public int Size;
        public HashSet<Continent> Neighbors;

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
            Idx = 0;
            Size = 0;
            Neighbors = new HashSet<Continent>();
            _parent = null;
        }

        public bool Equals(Continent other) => other.Label == this.Label; 
    }

    #endregion

    #region GPU IO

    private void AlignPlatesGPU()
    {
        var kernel = BakePlatesShader.FindKernel("AlignPlateThicknessMaps");
        using var plateBuffer = new ComputeBuffer(_data.Plates.Count(), Marshal.SizeOf(typeof(PlateGpuData)));
        plateBuffer.SetData(_data.Plates.Select(x => x.ToGpuData()).ToArray());
        BakePlatesShader.SetBuffer(kernel, "Plates", plateBuffer);

        BakePlatesShader.SetInt("NumPlates", _data.Plates.Count());
        BakePlatesShader.SetFloat("MantleHeight", _data.MantleHeight);
        BakePlatesShader.SetTexture(kernel, "TmpPlateThicknessMaps", _tmpPlateThicknessMaps.RenderTexture);
        BakePlatesShader.SetTexture(kernel, "PlateThicknessMaps", _data.PlateThicknessMaps.RenderTexture);
        BakePlatesShader.Dispatch(kernel, Coordinate.TextureWidthInPixels / 8, Coordinate.TextureWidthInPixels / 8, 1);
    }
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
    private void RelableContinentsGPU()
    {
        var kernel = BakePlatesShader.FindKernel("RelabelContinents");

        using var relabelBuffer = new ComputeBuffer(_continentRelabels.Count(), Marshal.SizeOf(typeof(RelabelGpuData)));
        relabelBuffer.SetData(_continentRelabels);
        BakePlatesShader.SetBuffer(kernel, "Relabels", relabelBuffer);
        BakePlatesShader.SetInt("NumRelabels", _continentRelabels.Count());

        using var plateBuffer = new ComputeBuffer(_data.Plates.Count(), Marshal.SizeOf(typeof(PlateGpuData)));
        plateBuffer.SetData(_data.Plates.Select(x => x.ToGpuData()).ToArray());
        BakePlatesShader.SetBuffer(kernel, "Plates", plateBuffer);
        BakePlatesShader.SetInt("NumPlates", _data.Plates.Count());

        BakePlatesShader.SetFloat("MantleHeight", _data.MantleHeight);
        BakePlatesShader.SetTexture(kernel, "TmpContinentalIdMap", _tmpContinentalIdMap.RenderTexture);
        BakePlatesShader.SetTexture(kernel, "ContinentalIdMap", _data.ContinentalIdMap.RenderTexture);
        BakePlatesShader.SetTexture(kernel, "TmpPlateThicknessMaps", _tmpPlateThicknessMaps.RenderTexture);
        BakePlatesShader.SetTexture(kernel, "PlateThicknessMaps", _data.PlateThicknessMaps.RenderTexture);

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
        public int ToIdx;
    }

    #endregion
}
