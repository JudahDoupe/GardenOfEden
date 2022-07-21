using UnityEditor;
using UnityEngine;

public class TimeService : MonoBehaviour
{
    public int DayOfTheWeek { get; private set; } = 0;
    public int DayOfTheMonth { get; private set; } = 0;
    public int DayOfTheYear { get; private set; } = 0;
    public int MonthOfTheYear { get; private set; } = 0;
    public int Year { get; private set; } = 0;

    public void Start()
    {
        Singleton.LoadBalancer.RegisterEndSimulationAction(IncrementDay);
    }

    private void IncrementDay()
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
        var service = (TimeService)target;

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField($"Day {service.DayOfTheYear}   |   Month {service.MonthOfTheYear}   |   Year {service.Year}");
        EditorGUILayout.Space(5);
    }
}
#endif