using UnityEngine;

public class Singleton<T> : MonoBehaviour
    where T : MonoBehaviour
{
    public static T Instance;

    void Awake()
    {
        Instance = this as T;
    }
}