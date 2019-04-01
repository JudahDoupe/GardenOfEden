using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Plant : MonoBehaviour
{
    public bool IsManipulatable;
    public float Age;
    public Structure Trunk;

	public void Start()
	{
	    Age = IsManipulatable ? 1 : 0;
        transform.localEulerAngles = new Vector3(-90,0,0);
	    Trunk.Plant = this;
	}
    public void Update()
    {
        if (!IsManipulatable) Age += Time.smoothDeltaTime / 3f;
    }
}