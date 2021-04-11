using UnityEngine;
using UnityEngine.Events;

public class EventOnClick : MonoBehaviour
{
    public UnityEvent ClickEvent;

    void OnMouseDown()
    {
        ClickEvent.Invoke();
    }
}
