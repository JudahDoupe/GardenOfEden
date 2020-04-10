using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeTracker : MonoBehaviour
{
    public float CreationDate;
    public float LastUpdateDate;
    public float Age => EnvironmentApi.GetDate() - CreationDate;
}
