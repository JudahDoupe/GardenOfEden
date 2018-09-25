using UnityEngine;
using System.Collections;
using JetBrains.Annotations;

public class DetectClicksAndTouches : MonoBehaviour
{	
	private Camera _camera;
    private Hit _hit;


    void Start()
	{
	    _camera = Camera.main;
	}
	
	void Update ()
	{
        if (Input.GetMouseButtonDown(0))
		{
		    RaycastHit hit;
            var ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 100))
		    {
                _hit = new Hit(hit);
                Click();
		    }			
		}
	    if (Input.GetMouseButton(0))
	    {
            if(_hit != null)Drag();
	    }
	    if (Input.GetMouseButtonUp(0))
	    {
	        _hit = null;
	    }
    }

    public void Click()
    {
        _hit.Object.SendMessage("Click", _hit.Position, SendMessageOptions.DontRequireReceiver);
    }

    public void Drag()
    {
        var newPos = _camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, _hit.ScreenPoint.z));
        _hit.Object.SendMessage("Drag", newPos + _hit.PositionObjectOffset, SendMessageOptions.DontRequireReceiver);
    }

    private class Hit
    {
        public readonly GameObject Object;
        public readonly Vector3 Position;
        public readonly Vector3 PositionObjectOffset;
        public readonly Vector3 ScreenPoint;

        public Hit(RaycastHit hit)
        {
            Object = hit.transform.gameObject;
            Position = hit.point;
            PositionObjectOffset = Object.transform.position - Position;
            ScreenPoint = Camera.main.WorldToScreenPoint(Position);
        }
    }
}
