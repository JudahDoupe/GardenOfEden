using UnityEngine;

public class HoverAndClickControl : MonoBehaviour
{
    public bool Active { get; private set; }

    public void Enable() => Active = true;
    public void Disable() => Active = false;

    void Update()
    {
        if (!Active) return;

        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit))
        {
            hit.transform.gameObject.SendMessage("Hover", SendMessageOptions.DontRequireReceiver);
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                hit.transform.gameObject.SendMessage("Click");
            }
        }
    }
}
