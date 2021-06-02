using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

public class Slider2D : Control
{
    public Vector2 Min = new Vector2(-1,-1);
    public Vector2 Max = new Vector2(1,1);

    public float Radius { get; private set; }
    public float Height { get; private set; }

    public UnityAction<Coordinate, float, float> UpdateFunction;

    public Vector2 LocalOffset => _slider.transform.localPosition;
    public Vector2 GlobalOffset => Vector3.Scale(_slider.transform.localPosition, transform.localScale);
    public Vector3 GlobalPosition => _slider.transform.position;

    private Slider _slider;
    private LineRenderer _heightLine;

    private void Start()
    {
        _heightLine = transform.GetComponent<LineRenderer>();
        _slider = transform.GetComponentInChildren<Slider>();
        _slider.Min = new Vector3(Min.x, Min.y, 0);
        _slider.Max = new Vector3(Max.x, Max.y, 0);
    }

    private void Update()
    {
        /*
        IsInUse = _slider.IsClicked;
        if (IsActive)
        {
            Radius = math.abs(GlobalOffset.x) + (IsInUse ? 20 : 0);
            Height = new Coordinate(GlobalPosition).Altitude;
            Singleton.PerspectiveController.FocusRadius = Radius;
            Singleton.PerspectiveController.LockAltitude = IsInUse;

            if (IsInUse)
            {
                UpdateFunction(Singleton.PerspectiveController.FocusCoord, Radius, Height);
            }
        }

        SetHeightLine();
        */
    }

    private void SetHeightLine()
    {
        var offset = (transform.position - Camera.main.transform.position).normalized * 0.1f;
        var start = GlobalPosition + offset;
        var end = transform.TransformPoint(Vector3.Scale(LocalOffset, new Vector3(-1, 1, 1))) + offset;
        var numPoints = math.round(math.max(2, Radius / 20));
        _heightLine.positionCount = (int)numPoints;
        for (int i = 0; i < numPoints; i++)
        {
            var pos = Vector3.Lerp(start, end, i / (numPoints - 1));
            var coord = new Coordinate(pos);
            coord.Altitude = Height;
            _heightLine.SetPosition(i, transform.InverseTransformPoint(coord.xyz));
        }

        _heightLine.widthMultiplier = transform.localScale.x / 2;
    }
}
