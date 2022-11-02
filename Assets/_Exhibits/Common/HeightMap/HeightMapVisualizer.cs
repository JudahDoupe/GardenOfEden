using Assets.Scripts.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

public class HeightMapVisualizer : MonoBehaviour
{
    public GameObject[] Planes = new GameObject[6];
    public GameObject Cube;
    public string Planet = "Earth";

    [Header("Expanded Layers")]
    public float LayerSetback = 100;

    [Header("Cube Map")]
    public float CubeScale = 1000;
    public Vector3 Position;
    public Vector3 Rotation;

    [Header("Sphere Map")]

    [Header("Height Map")]

    [Header("Colored Height Map")]
    public Vector3 ClosePosition;


    private Controls Controls;
    private VisualizationState[] States;
    private int StateIndex = -1;

    void Start()
    {
        Controls = new Controls();
        Controls.Exhibit.Enable();

        Controls.Exhibit.Forward.performed += Forward;
        Controls.Exhibit.Back.performed += Back;


        var map = EnvironmentMapDataStore.GetOrCreate(new EnvironmentMapDbData(Planet, "LandHeightMap")).Result;
        for (int i = 0; i < 6; i++)
        {
            Planes[i].GetComponent<Renderer>().material.SetTexture("HeightMap", map.CachedTextures[i]);
        }
        Cube.GetComponent<Renderer>().material.SetTexture("HeightMap", map.RenderTexture);

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
            },
            TexturePositions = new[]
            {
                Vector3.back * 1,
                Vector3.back * 2,
                Vector3.back * 3,
                Vector3.back * 4,
                Vector3.back * 5,
                Vector3.back * 6,
            },
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
                Quaternion.LookRotation(Vector3.right, Vector3.back),
                Quaternion.LookRotation(Vector3.left, Vector3.back),
                Quaternion.LookRotation(Vector3.up, Vector3.forward),
                Quaternion.LookRotation(Vector3.down, Vector3.forward),
                Quaternion.LookRotation(Vector3.forward, Vector3.up),
                Quaternion.LookRotation(Vector3.back, Vector3.up),
            },
            TexturePositions = new[]
            {
                Vector3.right * CubeScale, 
                Vector3.left * CubeScale, 
                Vector3.up * CubeScale, 
                Vector3.down * CubeScale, 
                Vector3.forward * CubeScale, 
                Vector3.back * CubeScale, 
            },
            ParentPosition = Position,
            ParentRotation = Quaternion.Euler(Rotation),
        };

        var cubeMapFromSphere = new VisualizationState(cubeMap)
        {
            TextureAlphas = new float[] { 0,0,0,0,0,0 },
            SphereAlpha = 1,
            TransitionTime = 0
        };

        var sphereMap = new VisualizationState(cubeMapFromSphere)
        {
            TransitionTime = 1,
            CubeToSphereLerp = 1,
        };

        var heightMap = new VisualizationState(sphereMap)
        {
            TransitionTime = 1,
            SphereToHeightLerp = 1,
        };

        var rotateCloser = new VisualizationState(sphereMap)
        {
            TransitionTime = 3,
            ParentPosition = ClosePosition,
            ParentRotation = Quaternion.LookRotation(Vector3.back, Vector3.up),
        };

        States = new[]
        {
            startPosition,
            expandedLayers,
            cubeMap,
            cubeMapFromSphere,
            sphereMap,
            heightMap,
            rotateCloser,
        };

        Forward(new InputAction.CallbackContext());
    }

    private void Forward(InputAction.CallbackContext context)
    {
        StateIndex = Mathf.Clamp(StateIndex + 1, 0, States.Length - 1);
        SetState(States[StateIndex].TransitionTime);
    }
    private void Back(InputAction.CallbackContext context)
    {
        StateIndex = Mathf.Clamp(StateIndex - 1, 0, States.Length - 1);
        SetState(States[StateIndex + 1].TransitionTime);
    }
    private void SetState(float speed)
    {
        var state = States[StateIndex];

        for (var i = 0; i < Planes.Length; i++)
        {
            var plane = Planes[i].transform;
            plane.AnimatePosition(speed, state.TexturePositions[i], ease: EaseType.InOut);
            plane.AnimateRotation(speed, state.TextureRotations[i] * Quaternion.AngleAxis(90, Vector3.right), ease: EaseType.InOut);
            plane.AnimateOpacity(speed, state.TextureAlphas[i], ease: EaseType.InOut);
        }

        transform.AnimatePosition(speed, state.ParentPosition, ease: EaseType.InOut);
        transform.AnimateRotation(speed, state.ParentRotation, ease: EaseType.InOut);
        Cube.transform.AnimateOpacity(speed, state.SphereAlpha, ease: EaseType.InOut);
        var material = Cube.transform.GetComponent<Renderer>().material;
        CameraController.Instance.StartCoroutine(
            AnimationUtils.AnimateFloat(speed, 
                                        material.GetFloat("CubeToSphere"), 
                                        state.CubeToSphereLerp, 
                                        x => material.SetFloat("CubeToSphere", x),
                                        ease: EaseType.InOut));
        CameraController.Instance.StartCoroutine(
            AnimationUtils.AnimateFloat(speed,
                                        material.GetFloat("SphereToHeight"),
                                        state.SphereToHeightLerp,
                                        x => material.SetFloat("SphereToHeight", x),
                                        ease: EaseType.InOut));

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
            CubeToSphereLerp = 0;
            SphereToHeightLerp = 0;
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
            CubeToSphereLerp = state.CubeToSphereLerp;
            SphereToHeightLerp = state.SphereToHeightLerp;
        }

        public Vector3[] TexturePositions; 
        public Quaternion[] TextureRotations;
        public float[] TextureAlphas;

        public Vector3 ParentPosition;
        public Quaternion ParentRotation;
        public float SphereAlpha;
        public float CubeToSphereLerp;
        public float SphereToHeightLerp;

        public float TransitionTime;
    }
}
