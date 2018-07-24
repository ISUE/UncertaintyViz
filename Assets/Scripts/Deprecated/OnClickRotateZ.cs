using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnClickRotateZ : MonoBehaviour {

		
	void OnMouseOver () {
        if (Input.GetMouseButtonDown(1))
        {
            transform.Rotate(0, 10, 0);      
        }
    }
}
