using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Assets.Scripts.Plants.Growth;
using Assets.Scripts.Plants.Setup;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEditor;
using UnityEngine;

public interface ILoadBalancer
{
    public UpdateChunk CurrentChunk { get; }
    public List<UpdateChunk> UpdateChunks { get; }
    public void RegisterEndSimulationAction(Action action);
    public void BalanceChunks();
}

public class LoadBalancer : MonoBehaviour, ILoadBalancer
{
    public float DesiredChunkMilliseconds = 5;
    public float MsPerDay { get; private set; }

    public UpdateChunk CurrentChunk => _currentChunk.Value; 
    public List<UpdateChunk> UpdateChunks => _updateChunks.ToList(); 

    private List<Action> _environmentalSystems = new List<Action>();
    private LinkedListNode<UpdateChunk> _currentChunk;
    private LinkedList<UpdateChunk> _updateChunks = new LinkedList<UpdateChunk>();
    public Dictionary<int, double[]> ChunkProcessingTimes = new Dictionary<int, double[]>();
    private int _lastId = 1;
    private Stopwatch _dayLengthTimer = new Stopwatch();

    public void Start()
    {
        //This is the environment chunk
        AddChunk(-1, Vector3.zero);
        //This is the main entities chunk
        AddChunk(_lastId++, Camera.main.transform.position);

        _currentChunk = _updateChunks.First;
        _dayLengthTimer.Start();
    }

    public void Update()
    {
        if (_currentChunk.Value.IsEnvironmental)
        {
            foreach (var runEnvironmentalSystem in _environmentalSystems)
            {
                runEnvironmentalSystem();
            }

            MsPerDay = _dayLengthTimer.ElapsedMilliseconds;
            _dayLengthTimer.Restart();
        }
        else
        {
            ChunkProcessingTimes[_currentChunk.Value.Id][Singleton.TimeService.DayOfTheWeek] = CalculateChunkProcessingTime();
        }
        
        _currentChunk = _currentChunk.Next ?? _updateChunks.First;
    }

    public void RegisterEndSimulationAction(Action action)
    {
        _environmentalSystems.Add(action);
    }

    public void BalanceChunks()
    {
        foreach (var chunkResult in ChunkProcessingTimes.ToArray())
        {
            if (chunkResult.Value.Average() > DesiredChunkMilliseconds)
            {
                SplitChunk(_updateChunks.Single(x => x.Id == chunkResult.Key));
            }
            else if (chunkResult.Value.Average() < DesiredChunkMilliseconds / 2 && _updateChunks.Count(x => x.Id > 0) > 1)
            {
                CoalesceChunk(_updateChunks.Single(x => x.Id == chunkResult.Key));
            }
        }
    }

    private double CalculateChunkProcessingTime()
    {
        var sum = 0d;
        var enumerator = World.DefaultGameObjectInjectionWorld.Systems.GetEnumerator();
        while (enumerator.MoveNext())
        {
            var system = enumerator.Current;
            sum += enumerator.Current.Time.DeltaTime;
        }

        return sum;
    }

    private bool splitDirection;
    private void SplitChunk(UpdateChunk chunkBeingSplit)
    {
        var bounds = new Bounds(chunkBeingSplit.Position, Vector3.one);
        var query = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(UpdateChunk), typeof(LocalToWorld) },
            None = new ComponentType[] { typeof(LocalToParent) },
        });
        query.ResetFilter();
        query.AddSharedComponentFilter(chunkBeingSplit);
        var entities = query.ToComponentDataArray<LocalToWorld>(Allocator.Temp);
        foreach (var localToWorld in entities)
        {
            bounds.Encapsulate(localToWorld.Position);
        }

        var offset = bounds.extents;
        offset.Scale(splitDirection ? new Vector3(0, 0.3f, 0) : new Vector3(0.3f, 0, 0));
        splitDirection = !splitDirection;

        AddChunk(_lastId++, bounds.center + offset);
        AddChunk(_lastId++, bounds.center - offset);
        CoalesceChunk(chunkBeingSplit);
    }

    private void CoalesceChunk(UpdateChunk chunk)
    {
        _updateChunks.Remove(chunk);
        ChunkProcessingTimes.Remove(chunk.Id);
    }

    private void AddChunk(int id, float3 position)
    {
        _updateChunks.AddLast(new UpdateChunk { Id = id, Position = position });
        var val = DesiredChunkMilliseconds / 2d;
        ChunkProcessingTimes[id] = new double[]{ val, val, val, val, val, val, val };
    }

}

#if UNITY_EDITOR
[CustomEditor(typeof(LoadBalancer))]
public class LoadBalancerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var service = (LoadBalancer)target;

        DrawDefaultInspector();

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField($"Chunks Per Day: {service.UpdateChunks.Count}");
        EditorGUILayout.LabelField($"Ms Per Day: {service.MsPerDay}");
        EditorGUILayout.Space(5);

        foreach (var chunkData in service.ChunkProcessingTimes)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Chunk Id: {chunkData.Key} |  Processing Time: {chunkData.Value.Average()}");
            EditorGUILayout.EndHorizontal();
        }

    }
}
#endif