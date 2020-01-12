using UnityEngine;

public class EnvironmentApi : MonoBehaviour
{
    public static float GetDate()
    {
        return Instance._date;
    }

    /* INNER MECHINATIONS */

    public static EnvironmentApi Instance;

    private float _date;

    private void Awake()
    {
        Instance = this;
        _date = 0;
    }

    private void Update()
    {
        _date += Time.deltaTime;
    }
}
