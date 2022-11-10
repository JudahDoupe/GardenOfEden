using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

public class PlateBakerV2 : MonoBehaviour
{
    public ComputeShader BakePlatesShader;
    public int MinContinentSize = 2500;
    public int MsBudget = 5;

    private EnvironmentMap _tmpPlateThicknessMaps;
    private EnvironmentMap _tmpContinentalIdMap;
    private PlateTectonicsData _data;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
            BakePlates();
    }

    public void Initialize(PlateTectonicsData data)
    {
        _data = data;
        _tmpContinentalIdMap = new EnvironmentMap(_data.ContinentalIdMap.ToDbData());
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

        AlignPlates();
        yield return new WaitForEndOfFrame();

        yield return LabelContinents();

        var relabels = IdentifyRelabels();
        yield return new WaitForEndOfFrame();

        RelabelContinents(relabels);
        yield return new WaitForEndOfFrame();

        SyncCpuData(relabels.Values.Distinct().ToList());
        SyncGpuData();

        UnityEngine.Debug.Log($"Bake Time: {stopwatch.ElapsedMilliseconds}ms");
    }

    private void AlignPlates()
    {
        var stopwatch = new Stopwatch();
        stopwatch.Restart();

        _tmpPlateThicknessMaps = new EnvironmentMap(_data.PlateThicknessMaps.ToDbData());
        RunTectonicKernel("AlignPlateThicknessMaps");
        foreach (var plate in _data.Plates)
        {
            plate.Rotation = Quaternion.identity;
            plate.Velocity = Quaternion.identity;
        }
        _data.PlateThicknessMaps.SetTextures(_tmpPlateThicknessMaps);

        UnityEngine.Debug.Log($"Align Plates Time: {stopwatch.ElapsedMilliseconds}ms");
    }

    private IEnumerator LabelContinents()
    {
        var totalStopwatch = new Stopwatch();
        var fameStopwatch = new Stopwatch();

        fameStopwatch.Restart();
        totalStopwatch.Restart();
        
        var iterations = 0;
        var frames = 0;
        
        RunTectonicKernel("InitializeLabeling");
        while (RunTectonicKernel("RunLabelingIteration"))
        {
            iterations++;
            if (fameStopwatch.ElapsedMilliseconds > MsBudget)
            {
                yield return new WaitForEndOfFrame();
                fameStopwatch.Restart();
                frames++;
            } 
        }
        
        UnityEngine.Debug.Log($"Labeling Time: {totalStopwatch.ElapsedMilliseconds}ms");
        UnityEngine.Debug.Log($"Labeling Iterations: {iterations}");
        UnityEngine.Debug.Log($"Labeling Frames: {frames}");
    }

    private Dictionary<float, float> IdentifyRelabels()
    {
        //TODO
        var merges = new Dictionary<float, float>();
        return merges;
    }

    private void RelabelContinents(Dictionary<float, float> merges)
    {
        //TODO

    }

    private void SyncCpuData(List<float> ids)
    {
        //TODO:
        //Remove missing GPU plates
        //Add missing cpu plates
    }
     
    private void SyncGpuData()
    {
        //_data.ContinentalIdMap.SetTextures(_tmpContinentalIdMap);
    }

    private bool RunTectonicKernel(string name)
    {
        var kernel = BakePlatesShader.FindKernel(name);
        using var plateBuffer = new ComputeBuffer(_data.Plates.Count(), Marshal.SizeOf(typeof(PlateGpuData)));
        plateBuffer.SetData(_data.Plates.Select(x => x.ToGpuData()).ToArray());
        BakePlatesShader.SetBuffer(kernel, "Plates", plateBuffer);

        using var labelBuffer = new ComputeBuffer(1, Marshal.SizeOf(typeof(LabelGpuData)));
        var labelData = new[] { new LabelGpuData { Changed = 0 } };
        labelBuffer.SetData(labelData);
        BakePlatesShader.SetBuffer(kernel, "Label", labelBuffer);

        BakePlatesShader.SetInt("NumPlates", _data.Plates.Count());
        BakePlatesShader.SetFloat("MantleHeight", _data.MantleHeight);
        BakePlatesShader.SetTexture(kernel, "TmpPlateThicknessMaps", _tmpPlateThicknessMaps.RenderTexture);
        BakePlatesShader.SetTexture(kernel, "PlateThicknessMaps", _data.PlateThicknessMaps.RenderTexture);
        BakePlatesShader.SetTexture(kernel, "TmpContinentalIdMap", _tmpContinentalIdMap.RenderTexture);
        BakePlatesShader.SetTexture(kernel, "ContinentalIdMap", _data.ContinentalIdMap.RenderTexture);
        BakePlatesShader.Dispatch(kernel, Coordinate.TextureWidthInPixels / 8, Coordinate.TextureWidthInPixels / 8, 1);

        labelBuffer.GetData(labelData);
        return labelData[0].Changed > 0;
    }

    public struct LabelGpuData
    {
        public int Changed;
    }
}
