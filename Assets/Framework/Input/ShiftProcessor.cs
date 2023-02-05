using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
[InitializeOnLoad]
#endif
public class ShiftProcessor : InputProcessor<float>
{
#if UNITY_EDITOR
    static ShiftProcessor()
    {
        Initialize();
    }
#endif

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Initialize()
    {
        InputSystem.RegisterProcessor<ShiftProcessor>();
    }

    [Tooltip("Number to add to incoming values.")]
    public float shift = 0;

    public override float Process(float value, InputControl control)
    {
        return value + shift; 
    }
}