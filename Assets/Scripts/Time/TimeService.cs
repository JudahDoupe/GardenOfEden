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
    public int Day { get; private set; } = 0; 
    public float DayLength { get; private set; } = 0;

    public Dictionary<string, float> ProcessTimes = new Dictionary<string, float>();
    public readonly Stopwatch UpdateTimer = new Stopwatch();
    public int UpdateMilliseconds = 5;

    private LinkedList<IDailyProcess> _dailyProcesses = new LinkedList<IDailyProcess>();
    private LinkedListNode<IDailyProcess> _currentProcess;
    private bool _isCurrentProcessComplete = true;
    private readonly Stopwatch _dayLengthTimer = new Stopwatch();
    private readonly Stopwatch _processTimer = new Stopwatch();

    public void Start()
    {
        PopulateDailySystems();
        UpdateTimer.Restart();
        StartCoroutine(Run());
    }

    public void Update()
    {
        UpdateTimer.Restart();
    }

    private IEnumerator Run()
    {
        while (Singleton.GameService.IsGameInProgress)
        {
            if (!_isCurrentProcessComplete)
            {
                yield return new WaitUntil(() => _isCurrentProcessComplete);
            }
            if (UpdateTimer.ElapsedMilliseconds > UpdateMilliseconds)
            {
                yield return new WaitForEndOfFrame();
            }

            _isCurrentProcessComplete = false;
            _processTimer.Restart();
            _currentProcess.Value.ProcessDay(CompleteProcess);
        }
    }

    public void ProcessDay(Action callback)
    {
        Day++;
        DayLength = _dayLengthTimer.Elapsed.Milliseconds;
        _dayLengthTimer.Restart();
        callback();
    }

    private void CompleteProcess()
    {
        ProcessTimes[_currentProcess.Value.GetType().Name] = _processTimer.ElapsedMilliseconds;
        _isCurrentProcessComplete = true;
        _currentProcess = _currentProcess.Next ?? _dailyProcesses.First;
    }

    private void PopulateDailySystems()
    {
        _dailyProcesses = new LinkedList<IDailyProcess>();
        _currentProcess = _dailyProcesses.AddFirst(Singleton.WaterService);
        _currentProcess = _dailyProcesses.AddAfter(_currentProcess, Singleton.LandService);

        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => typeof(IDailyProcess).IsAssignableFrom(p) && typeof(SystemBase).IsAssignableFrom(p));
        foreach(var type in types)
        {
            _dailyProcesses.AddAfter(_currentProcess, World.DefaultGameObjectInjectionWorld.GetOrCreateSystem(type) as IDailyProcess);
        }
        _currentProcess = _dailyProcesses.AddLast(this);
    }
}

[CustomEditor(typeof(TimeService))]
public class TimeServiceEditor : Editor
{

    public override void OnInspectorGUI()
    {
        var service = (TimeService)target;
        DrawDefaultInspector();

        EditorGUILayout.Space(5);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"Day {service.Day}");
        EditorGUILayout.LabelField($"{service.DayLength} ms");
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(5);

        foreach (var keyValue in service.ProcessTimes)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(keyValue.Key);
            EditorGUILayout.LabelField($"{keyValue.Value} ms"); 
            EditorGUILayout.EndHorizontal();
        }
    }
}