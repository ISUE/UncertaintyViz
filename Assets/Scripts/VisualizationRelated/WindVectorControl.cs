using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindVectorControl : MonoBehaviour {

    //Wind Vectors default to point to the north
    float rotation = 0;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public float getRotation()
    {
        return rotation;
    }

    public void setRotation(float angle)
    {
        this.gameObject.transform.eulerAngles = new Vector3(0, 0, angle);
    }
}
