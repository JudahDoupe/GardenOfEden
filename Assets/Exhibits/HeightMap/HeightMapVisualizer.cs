using Assets.Scripts.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HeightMapVisualizer : MonoBehaviour
{
    public GameObject[] Planes = new GameObject[6];
    public GameObject Shpere;

    [Header("Expanded Layers")]
    public float LayerSetback = 100;

    [Header("Cube Map")]
    public float CubeScale = 1000;
    public Vector3 Position;
    public Vector3 Rotation;

    [Header("Sphere Map")]

    [Header("Height Map")]

    [Header("Colored Height Map")]


    private Controls Controls;
    private VisualizationState[] States;
    private int StateIndex = -1;

    void Start()
    {
        Controls = new Controls();
        Controls.Exhibit.Enable();

        Controls.Exhibit.Forward.performed += Forward;
        Controls.Exhibit.Back.performed += Back;

        var startPosition = new VisualizationState {
            TextureAlphas = new float[] { 1, 1, 1, 1, 1, 1 },
            TextureRotations = new []
            {
                Quaternion.LookRotation(-Camera.main.transform.forward, Camera.main.transform.up),
                Quaternion.LookRotation(-Camera.main.transform.forward, Camera.main.transform.up),
                Quaternion.LookRotation(-Camera.main.transform.forward, Camera.main.transform.up),
                Quaternion.LookRotation(-Camera.main.transform.forward, Camera.main.transform.up),
                Quaternion.LookRotation(-Camera.main.transform.forward, Camera.main.transform.up),
                Quaternion.LookRotation(-Camera.main.transform.forward, Camera.main.transform.up),
            }
        };

        var expandedLayers = new VisualizationState(startPosition)
        {
            TexturePositions = new[]
            {
                new Vector3(3,-3,3) * LayerSetback,
                new Vector3(2,-2,2) * LayerSetback,
                new Vector3(1,-1,1) * LayerSetback,
                new Vector3(0,0,0)  * LayerSetback,
                new Vector3(-1,1,-1) * LayerSetback,
                new Vector3(-2,2,-2) * LayerSetback,
            },
            TextureAlphas = new float[] { 1, 0.9f, 0.8f, 0.7f, 0.6f, 0.5f },
        };

        var cubeMap = new VisualizationState(startPosition)
        {
            TextureAlphas = new float[] { 1, 1, 1, 1, 1, 1 },
            TextureRotations = new[]
            {
                Quaternion.LookRotation(Vector3.up, Vector3.forward),
                Quaternion.LookRotation(Vector3.down, Vector3.forward),
                Quaternion.LookRotation(Vector3.right, Vector3.up),
                Quaternion.LookRotation(Vector3.left, Vector3.up),
                Quaternion.LookRotation(Vector3.forward, Vector3.up),
                Quaternion.LookRotation(Vector3.back, Vector3.up),
            },
            TexturePositions = new[]
            {
                Vector3.up * CubeScale, 
                Vector3.down * CubeScale, 
                Vector3.right * CubeScale, 
                Vector3.left * CubeScale, 
                Vector3.forward * CubeScale, 
                Vector3.back * CubeScale, 
            },
            ParentPosition = Position,
            ParentRotation = Quaternion.Euler(Rotation),
        };

        States = new[]
        {
            startPosition,
            expandedLayers,
            cubeMap,
        };

        Forward(new InputAction.CallbackContext());
    }

    private void Forward(InputAction.CallbackContext context)
    {
        StateIndex = Mathf.Clamp(StateIndex + 1, 0, States.Length - 1);
        SetState();
    }
    private void Back(InputAction.CallbackContext context)
    {
        StateIndex = Mathf.Clamp(StateIndex - 1, 0, States.Length - 1);
        SetState();
    }
    private void SetState()
    {
        var state = States[StateIndex];

        for (var i = 0; i < Planes.Length; i++)
        {
            var plane = Planes[i].transform;
            plane.AnimatePosition(state.TransitionTime, state.TexturePositions[i]);
            plane.AnimateRotation(state.TransitionTime, state.TextureRotations[i] * Quaternion.AngleAxis(90, Vector3.right));
            plane.AnimateOpacity(state.TransitionTime, state.TextureAlphas[i]);
        }

        transform.AnimatePosition(state.TransitionTime, state.ParentPosition);
        transform.AnimateRotation(state.TransitionTime, state.ParentRotation);
        Shpere.transform.AnimateOpacity(state.TransitionTime, state.SphereAlpha);

    }

    public class VisualizationState
    {
        public VisualizationState() 
        {
            TexturePositions = new[] { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };
            TextureRotations = new[] { Quaternion.identity, Quaternion.identity, Quaternion.identity, Quaternion.identity, Quaternion.identity, Quaternion.identity };
            TextureAlphas = new float[] { 0, 0, 0, 0, 0, 0 };
            ParentPosition = Vector3.zero;
            ParentRotation = Quaternion.identity;
            SphereAlpha = 0;
            TransitionTime = 1;
        }
        public VisualizationState(VisualizationState state) 
        {
            TexturePositions = state.TexturePositions;
            TextureRotations = state.TextureRotations;
            TextureAlphas = state.TextureAlphas;
            ParentPosition = state.ParentPosition;
            ParentRotation = state.ParentRotation;
            SphereAlpha = state.SphereAlpha;
            TransitionTime = state.TransitionTime;
        }

        public Vector3[] TexturePositions; 
        public Quaternion[] TextureRotations;
        public float[] TextureAlphas;

        public Vector3 ParentPosition;
        public Quaternion ParentRotation;
        public float SphereAlpha;

        public float TransitionTime;
    }
}
