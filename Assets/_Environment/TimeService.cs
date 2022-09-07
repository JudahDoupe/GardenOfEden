using UnityEditor;
using UnityEngine;

public class TimeService : Singleton<TimeService>
{
    public static int DayOfTheWeek { get; private set; } = 0;
    public static int DayOfTheMonth { get; private set; } = 0;
    public static int DayOfTheYear { get; private set; } = 0;
    public static int MonthOfTheYear { get; private set; } = 0;
    public static int Year { get; private set; } = 0;

    public void Start()
    {
        LoadBalancer.RegisterEndSimulationAction(IncrementDay);
    }

    private static void IncrementDay()
    {
        DayOfTheWeek = (DayOfTheWeek + 1) % 7;
        DayOfTheMonth  = (DayOfTheMonth + 1) % 30;
        DayOfTheYear  = (DayOfTheYear + 1) % 360;
        MonthOfTheYear = (MonthOfTheYear + ((DayOfTheMonth == 0) ? 1 : 0)) % 12;
        Year += (DayOfTheYear == 0) ? 1 : 0;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(TimeService))]
public class TimeServiceEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField($"Day {TimeService.DayOfTheYear}   |   Month {TimeService.MonthOfTheYear}   |   Year {TimeService.Year}");
        EditorGUILayout.Space(5);
    }
}
#endif