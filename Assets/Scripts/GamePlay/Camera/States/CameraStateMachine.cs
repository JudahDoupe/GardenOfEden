using System.Collections.Generic;

namespace CameraState
{
    public interface ICameraState
    {
        void TransitionTo();
        void Update();
        void TransitionAway();
    }
    public enum CameraStateType
    {
        Cinematic,
        Birdseye,
        Inspection,
    }

    public class CameraStateMachine
    {
        private Dictionary<CameraStateType, ICameraState> _states;
        private ICameraState _currentState;

        public CameraStateMachine()
        {
            _states = new Dictionary<CameraStateType, ICameraState>
            {
                {CameraStateType.Cinematic, new Cinematic()},
                {CameraStateType.Birdseye, new BirdsEye()},
                {CameraStateType.Inspection, new Inspection()},
            };
            Set(CameraStateType.Cinematic);
        }

        public void Set(CameraStateType state)
        {
            if (_currentState != _states[state])
            {
                _currentState?.TransitionAway();
                _currentState = _states[state];
                _currentState.TransitionTo();
            }
        }
        public void Update()
        {
            _currentState.Update();
        }
    }
}

