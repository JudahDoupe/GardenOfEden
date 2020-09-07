using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
            Singleton.LightService,
            Singleton.WaterService,
            Singleton.LandService,
            Singleton.GrowthService,
        };
        StartNextDay();
        _processQueue.Peek().ProcessDay();
    }

    public void Update()
    {
        if (_processQueue.Peek().HasDayBeenProccessed())
        {
            StartNextProcess();
        }
    }

    private void StartNextProcess()
    {
        _processQueue.Dequeue();
        if (!_processQueue.Any())
        {
            StartNextDay();
        }
        _processQueue.Peek().ProcessDay();
    }
    private void StartNextDay()
    {
        Day++;
        DayLength = _dayLengthTimer.Elapsed.Seconds;
        _dayLengthTimer.Restart();
        _processQueue = new Queue<IDailyProcess>(_dailyProcesses);
    }
}
