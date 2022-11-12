using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;

public class PlateBakerV2 : MonoBehaviour
{
    public ComputeShader BakePlatesShader;
    public int MinContinentSize = 2500;
    public int MsBudget = 5;

    private EnvironmentMap _tmpPlateThicknessMaps;
    private EnvironmentMap _tmpContinentalIdMap;
    private PlateTectonicsData _data;
    private Dictionary<float, float> _continentMerges;

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

        yield return IdentifyRelabels();

        yield return RelabelContinents();

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

        RunTectonicKernel("AlignPlateThicknessMaps");
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
        
        UnityEngine.Debug.Log($"Labeling Time: {totalStopwatch.ElapsedMilliseconds}ms | Frames: {frames} | Iterations: {iterations}");
        yield return new WaitForEndOfFrame();
    }

    private IEnumerator IdentifyRelabels()
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

        //vote for neighbors
        float[] Neighbors(int x, int y, int w) => new float[] {
            Sample(x + 1, y, w),
            Sample(x - 1, y, w),
            Sample(x, y + 1, w),
            Sample(x, y - 1, w),
        };
        float Sample(int x, int y, int w) => continentMaps[w][x + y * Coordinate.TextureWidthInPixels];

        //TODO: Taskify this
        var voters = new Dictionary<float, Dictionary<float, int>>();

        for (var w = 0; w < 6; w++)
        {
            for (var x = 1; x < Coordinate.TextureWidthInPixels - 1; x++)
            {
                for (var y = 1; y < Coordinate.TextureWidthInPixels - 1; y++)
                {
                    var center = Sample(x, y, w);
                    if (!voters.TryGetValue(center, out Dictionary<float, int> voter))
                        voter = voters[center] = new Dictionary<float, int>();

                    foreach (var candidate in Neighbors(x, y, w).Concat(new[] { center }))
                    {
                        if (!voter.TryGetValue(candidate, out int votes))
                            votes = voter[candidate] = 0;

                        voter[candidate]++;
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

        // tally votes
        _continentMerges = new Dictionary<float, float>();
        var totalVotes = voters.SelectMany(x => x.Value.Keys).Distinct().ToDictionary(voter => voter, _ => 0);
        foreach(var (voter, candidates) in voters)
        foreach(var (candidate, votes) in candidates)
        {
            totalVotes[candidate] += votes;

            if (fameStopwatch.ElapsedMilliseconds > MsBudget)
            {
                yield return new WaitForEndOfFrame();
                fameStopwatch.Restart();
                frames++;
            }
        }


        var x_0 = totalVotes.Keys.Count();
        var x_1 = totalVotes.Where(x => x.Value > 10).Count();
        var x_2 = totalVotes.Where(x => x.Value > 100).Count();
        var x_3 = totalVotes.Where(x => x.Value > 1000).Count();
        var x_4 = totalVotes.Where(x => x.Value > 10000).Count();
        var x_5 = totalVotes.Where(x => x.Value > 100000).Count();

        // relabel winners
        var winningCandidates = totalVotes.Where(x => x.Value > (MinContinentSize * 5)).Select(x => x.Key).ToList();
        if (!winningCandidates.Any())
        {
            winningCandidates.Add(totalVotes.Aggregate((x, y) => x.Value > y.Value ? x : y).Key);
        }
        foreach(var candidate in winningCandidates)
        {
            _continentMerges[candidate] = winningCandidates.IndexOf(candidate);

            if (fameStopwatch.ElapsedMilliseconds > MsBudget)
            {
                yield return new WaitForEndOfFrame();
                fameStopwatch.Restart();
                frames++;
            }
        }

        // relabel losers
        var losingCandidates = totalVotes.Select((candidate, votes) => candidate.Key).Where(x => !winningCandidates.Contains(x)).ToList();
        foreach (var candidate in losingCandidates)
        {
            var largestCandidate = voters[candidate].Any(x => winningCandidates.Contains(x.Key)) 
                ? voters[candidate].First(x => winningCandidates.Contains(x.Key)).Key
                : voters[candidate].First().Key;
            var largestCandidateLabel = _continentMerges.ContainsKey(largestCandidate) ? _continentMerges[largestCandidate] : largestCandidate;
            _continentMerges[candidate] = largestCandidateLabel;

            foreach (var (key, value) in _continentMerges.ToList())
            {
                if (value == candidate) _continentMerges[key] = largestCandidateLabel;
            }

            if (fameStopwatch.ElapsedMilliseconds > MsBudget)
            {
                yield return new WaitForEndOfFrame();
                fameStopwatch.Restart();
                frames++;
            }
        }


        //update gpu data
        //TODO


        UnityEngine.Debug.Log($"Identify Relabels Time: {totalStopwatch.ElapsedMilliseconds}ms | Frames: {frames} | Labels: {winningCandidates.Count}");
        UnityEngine.Debug.Log($"{x_0} | > 10: {x_1} | > 100 {x_2} | > 1000 {x_3} | > 10000 {x_4} | > 100000 {x_5}");
        yield return new WaitForEndOfFrame();
    }

    private IEnumerator RelabelContinents()
    {

        yield return new WaitForEndOfFrame();
    }
     
    private IEnumerator SyncData()
    {
        var continentIds = _continentMerges.Values.Distinct().ToList();
        //TODO:
        //Remove missing GPU plates
        //Add missing cpu plates
        yield return new WaitForEndOfFrame();

        //_data.ContinentalIdMap.SetTextures(_tmpContinentalIdMap);
        yield return new WaitForEndOfFrame();
    }

    private class Continent
    {
        public float Label;
        public int Size;
        public List<Continent> Neighbors;
        public Continent Parent;
    }

    #endregion

    #region GPU IO

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

    #endregion
}
