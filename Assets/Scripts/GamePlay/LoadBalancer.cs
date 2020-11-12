using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Plants.Systems;
using UnityEngine;

public class LoadBalancer : MonoBehaviour
{

    public UpdateChunk CurrentChunk => _currentChunk.Value; 
    public List<UpdateChunk> UpdateChunks => _updateChunks.ToList(); 

    private List<Action> _endSimulationActions = new List<Action>();
    private LinkedListNode<UpdateChunk> _currentChunk;
    private LinkedList<UpdateChunk> _updateChunks;
    private int lastId = 0;

    public void Start()
    {
        _updateChunks = new LinkedList<UpdateChunk>();
        _updateChunks.AddFirst(new UpdateChunk {Id = -1});
        _updateChunks.AddFirst(new UpdateChunk {Id = lastId++, Position = Camera.main.transform.position});
        _currentChunk = _updateChunks.First;
    }

    public void Update()
    {
        if (_currentChunk.Value.Id == -1)
        {
            foreach (var endSimulationAction in _endSimulationActions)
            {
                endSimulationAction();
            }
        }
        
        _currentChunk = _currentChunk.Next ?? _updateChunks.First;
    }

    public void RegisterEndSimulationAction(Action action)
    {
        _endSimulationActions.Add(action);
    }

}