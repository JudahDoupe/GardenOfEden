using Assets.GamePlay.Cameras;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PerspectiveController : MonoBehaviour
{
    public Camera Camera;
    public Transform Focus;
    public float Altitude => Camera.transform.position.magnitude;
    public CameraState CurrentState => new CameraState(Camera, Focus);

    private List<(CameraPerspective, CameraTransition)> _perspectives = new List<(CameraPerspective, CameraTransition)>();
    private readonly StateMachine<CameraPerspective> _stateMachine = new StateMachine<CameraPerspective>();
    
    public void SetPerspective(CameraPerspective perspective, CameraTransition transition)
    {
        if (perspective == _stateMachine.State) return;

        if (_stateMachine.State != null)
        {
            _stateMachine.State.Disable();
        }

        _perspectives = new List<(CameraPerspective, CameraTransition)> { (perspective, transition) };

        CameraUtils.TransitionState(perspective.TransitionToState(), transition, () => {
            _stateMachine.SetState(perspective);
        });
    }
    public void SetPerspectives(params (CameraPerspective, CameraTransition)[] perspectives)
    {
        _perspectives = perspectives.ToList();
        ZoomIn();
    }

    public void ZoomIn()
    {
        var available = _perspectives.Where(x => x.Item1.MinAltitude <= Altitude).ToArray();
        if (!available.Any()) return;

        var (perspective, transition) = available.Aggregate((x, y) => x.Item1.MaxAltitude > y.Item1.MaxAltitude ? x : y);
        SetPerspective(perspective, transition);
    }
    public void ZoomOut()
    {
        var available = _perspectives.Where(x => x.Item1.MaxAltitude >= Altitude).ToArray();
        if (!available.Any()) return;

        var (perspective, transition) = available.Aggregate((x, y) => x.Item1.MinAltitude < y.Item1.MinAltitude ? x : y);
        SetPerspective(perspective, transition);
    }

    private void Start()
    {
        SetPerspective(FindObjectOfType<MainMenuCamera>(), CameraTransition.Instant);
    }
}
