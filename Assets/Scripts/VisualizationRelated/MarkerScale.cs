using UnityEngine;
using System.Collections;

public class MarkerScale : MonoBehaviour
{

    public int defaultZoom;
    public float defaultScale = -1;

    private bool init = false;
    public double certainty;
    private double cameraDistance;
    private OnlineMapsMarker3D[] markers;
    // Use this for initialization
    void Start()
    {
        defaultZoom = OnlineMaps.instance.zoom;
        cameraDistance = OnlineMapsTileSetControl.instance.cameraDistance;
        OnlineMaps.instance.OnChangeZoom += OnChangeZoom;
        //certainty = GetComponent<COA_Helper>().certainty;
        certainty = Random.value;
        //OnChangeZoom();
    }

    void OnChangeZoom()
    {
        /*
        if (defaultScale < 0)
        {
            markers = OnlineMapsTileSetControl.instance.markers3D;
            for(int ii = 0; ii < markers.Length; ii++)
            {
                if (markers[ii].scale > 10)
                {
                    Vector3 v = markers[ii].transform.localPosition;
                    v.y = (float)(0.25f * cameraDistance * certainty);
                    markers[ii].transform.localPosition = v;
                    Debug.Log(v.y);
                }
                
            }
            defaultScale = transform.localScale.x;
        }*/
        float originalScale = defaultScale;
        float zoomRatio = (float)(1<<OnlineMaps.instance.zoom) / (float)(1<<defaultZoom);
        float newScale = originalScale * zoomRatio;
        //Debug.Log(transform.localScale);

        transform.localScale = new Vector3(newScale, newScale, newScale);
    }

    void Update()
    {
        if (!init)
        {
            init = true;

            markers = OnlineMapsTileSetControl.instance.markers3D;            
            for (int ii = 0; ii < markers.Length; ii++)
            {
                if (markers[ii].transform == transform)
                {
                    Vector3 v = markers[ii].transform.localPosition;
                    v.y = (float)(0.2f * cameraDistance * certainty);
                    markers[ii].transform.localPosition = v;
                    //Debug.Log(ii + " " + v.y);
                }                

            }
            defaultScale = transform.localScale.x;
        }
    }
}
