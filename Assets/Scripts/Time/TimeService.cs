using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Unity.Collections;
using UnityEngine;

public class TimeService : MonoBehaviour
{
    public int Day = 0;
    public float DayLength = 0;

    private List<IDailyProcess> _dailyProcesses = new List<IDailyProcess>();
    private Queue<IDailyProcess> _processQueue;
    private Stopwatch _dayLengthTimer = new Stopwatch();

    public void Start()
    {
        _dailyProcesses = new List<IDailyProcess>
        {
            Singleton.GrowthService,
            Singleton.LightService,
        };
        StartNextDay();
    }

    public void Update()
    {
        if (!_processQueue.Any())
        {
            StartNextDay();
        }
        if (_processQueue.Peek().HasDayBeenProccessed())
        {
            StartNextProcess();
        }
    }

    private void StartNextDay()
    {
        DayLength = _dayLengthTimer.Elapsed.Seconds;
        _dayLengthTimer.Restart();
        _processQueue = new Queue<IDailyProcess>(_dailyProcesses);
        _processQueue.Peek().ProcessDay();
        Day++;
    }

    private void StartNextProcess()
    {
        _processQueue.Dequeue();
        if (_processQueue.Any())
        {
            _processQueue.Peek().ProcessDay();
        }
    }
}
