using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Plants.Systems;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEditor;
using UnityEngine;

public class LoadBalancer : MonoBehaviour
{
    public float DesiredChunkMilliseconds = 5;

    public UpdateChunk CurrentChunk => _currentChunk.Value; 
    public List<UpdateChunk> UpdateChunks => _updateChunks.ToList(); 

    private List<Action> _environmentalSystems = new List<Action>();
    private LinkedListNode<UpdateChunk> _currentChunk;
    private LinkedList<UpdateChunk> _updateChunks = new LinkedList<UpdateChunk>();
    public Dictionary<int, double[]> ChunkProcessingTimes = new Dictionary<int, double[]>();
    private int lastId = 1;

    public void Start()
    {
        //This is the environment chunk
        AddChunk(-1, Vector3.zero);
        //This is the main entities chunk
        AddChunk(lastId++, Camera.main.transform.position);

        _currentChunk = _updateChunks.First;
    }

    public void Update()
    {
        if (_currentChunk.Value.IsEnvironmental)
        {
            foreach (var runEnvironmentalSystem in _environmentalSystems)
            {
                runEnvironmentalSystem();
            } 

            BalanceChunks();
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

    private void BalanceChunks()
    {
        var chunkIdToSplit = -1;
        var chunkIdToCoalesce = -1;
        var maxProcessingTime = 0;
        foreach (var chunkResult in ChunkProcessingTimes)
        {
            if (chunkResult.Value.Average() > DesiredChunkMilliseconds && chunkResult.Value.Average() > maxProcessingTime)
            {
                chunkIdToSplit = chunkResult.Key;
            }
            if (chunkResult.Value.Average() < DesiredChunkMilliseconds / 10)
            {
                chunkIdToCoalesce = chunkResult.Key;
            }
        }

        if (chunkIdToSplit >= 0 && Singleton.TimeService.DayOfTheYear > 30)
        {
            SplitChunk(_updateChunks.Single(x => x.Id == chunkIdToSplit));
        }
        if (chunkIdToCoalesce >= 0 && _updateChunks.Count > 2)
        {
            CoalesceChunk(_updateChunks.Single(x => x.Id == chunkIdToCoalesce));
        }
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

        AddChunk(lastId++, bounds.center + offset);
        AddChunk(lastId++, bounds.center - offset);
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
        ChunkProcessingTimes[id] = new double[7];
    }

}

[CustomEditor(typeof(LoadBalancer))]
public class LoadBalancerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var service = (LoadBalancer)target;

        foreach (var chunkData in service.ChunkProcessingTimes)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Chunk Id: {chunkData.Key} |  Processing Time: {chunkData.Value.Average()}");
            EditorGUILayout.EndHorizontal();
        }

    }
}