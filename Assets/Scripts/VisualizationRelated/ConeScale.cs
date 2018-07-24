using UnityEngine;
using System.Linq;
using System;

public class ConeScale : MonoBehaviour
{

    public int defaultZoom;
    public float defaultScale = -1;

    public bool init = false;
    static int overallDefaultZoom = -1;
    static float overallCurrentScale = -1;
    // Use this for initialization
    void Start()
    {
        if (overallDefaultZoom < 0)
            overallDefaultZoom = OnlineMaps.instance.zoom;
        if (overallCurrentScale > 0 && overallCurrentScale != transform.localScale.x)
            transform.localScale = new Vector3(overallCurrentScale,overallCurrentScale,overallCurrentScale);
        defaultZoom = overallDefaultZoom;
        //OnlineMaps.instance.OnChangeZoom += OnChangeZoom; //THIS DISABLES THE ORIGINAL CONE SCALING OPERATION IN FAVOR OF THE METHOD FROM SCALEFIX
     
    }

    void OnChangeZoom()
    {

        float originalScale = defaultScale;
        float zoomRatio = (float)(1 << OnlineMaps.instance.zoom) / (float)(1 << overallDefaultZoom);
        float newScale = originalScale * zoomRatio;
        if (overallCurrentScale != newScale)
            overallCurrentScale = newScale;
        //Debug.Log(transform.localScale);

        transform.localScale = new Vector3(newScale, newScale, newScale);
    }

    void Update()
    {
        if (!init)
        {
            init = true;
            defaultScale = transform.localScale.x;
        }
    }

    void OnDestroy()
    {
        if (OnlineMaps.instance != null)
          OnlineMaps.instance.OnChangeZoom -= OnChangeZoom;
    }

    public static void SetOverallDefaultZoom(int newzoom)
    {
        overallDefaultZoom = newzoom;
    }

    public static float GetOverallDefaultZoom()
    {
        return overallDefaultZoom;
    }
}
