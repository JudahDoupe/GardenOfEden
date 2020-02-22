using UIState;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public StateMachine State;

    public void Start()
    {
        State = new StateMachine();
        State.AddState(StateType.None, null);
        State.AddState(StateType.PlantDetails, FindObjectOfType<PlantDetails>());
        State.SetState(StateType.None);
    }
}
