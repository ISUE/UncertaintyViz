using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapBoundsControl : MonoBehaviour {

    public float minLatitiude = -180;
    public float maxLatitiude = 180 ;
    public float minLongitiude = -90;
    public float maxLongitude = 90;

    bool init = false;

	// Update is called once per frame
	void Update () {
        if (!init)
        {
            OnlineMaps.instance.positionRange = new OnlineMapsPositionRange(minLatitiude, minLongitiude, maxLatitiude, maxLongitude);
            init = !init;
        }
    }
}
