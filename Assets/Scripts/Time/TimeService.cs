using Assets.Scripts.Plants.Systems;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Unity.Entities;
using UnityEditor;
using UnityEngine;

public class TimeService : MonoBehaviour, IDailyProcess
{
    public int Day = 0;
    public float DayLength = 0;
    public Dictionary<string, float> ProcessTimes = new Dictionary<string, float>();

    private LinkedList<IDailyProcess> _dailyProcesses = new LinkedList<IDailyProcess>();
    private LinkedListNode<IDailyProcess> _currentProcess;
    private Stopwatch _dayLengthTimer = new Stopwatch();
    private Stopwatch _updateTimer = new Stopwatch();
    private Stopwatch _processTimer = new Stopwatch();

    public void Start()
    {
        _dailyProcesses = new LinkedList<IDailyProcess>();
        _currentProcess = _dailyProcesses.AddFirst(Singleton.WaterService);
        _currentProcess = _dailyProcesses.AddAfter(_currentProcess, Singleton.LandService);
        PopulateDailySystems();
        _currentProcess = _dailyProcesses.AddLast(this);
    }

    public void Update()
    {
        if (Singleton.GameService.IsGameInProgress)
        {
           _updateTimer.Restart();
            while(_updateTimer.ElapsedMilliseconds < 4)
            {
                if (_currentProcess.Value.HasDayBeenProccessed())
                {
                    _currentProcess = _currentProcess.Next ?? _dailyProcesses.First;
                    _currentProcess.Value.ProcessDay();
                    _processTimer.Restart();
                    ProcessTimes[_currentProcess.Value.GetType().Name] = _processTimer.ElapsedMilliseconds;
                }
            }
        }
    }

    public void ProcessDay()
    {
        Day++;
        DayLength = _dayLengthTimer.Elapsed.Milliseconds;
        _dayLengthTimer.Restart();
    }

    public bool HasDayBeenProccessed() => true;

    private void PopulateDailySystems()
    {
        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => typeof(IDailyProcess).IsAssignableFrom(p) && typeof(SystemBase).IsAssignableFrom(p));

        foreach(var type in types)
        {
            _dailyProcesses.AddAfter(_currentProcess, World.DefaultGameObjectInjectionWorld.GetOrCreateSystem(type) as IDailyProcess);
        }
    }
}

[CustomEditor(typeof(TimeService))]
public class TimeServiceEditor : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        foreach (var keyValue in ((TimeService)target).ProcessTimes)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(keyValue.Key);
            EditorGUILayout.LabelField(keyValue.Value.ToString()); 
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5);
        }
    }
}