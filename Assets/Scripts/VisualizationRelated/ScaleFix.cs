using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleFix : MonoBehaviour {
    public int defaultZoom;
    public float defaultScale = -1;

    private bool init = false;
    private OnlineMapsMarker3D[] markers;

    static int overallDefaultZoom = -1;
    static float overallCurrentScale = -1;
    // Use this for initialization
    void Start()
    {
        if (overallDefaultZoom < 0)
            overallDefaultZoom = OnlineMaps.instance.zoom;
        OnlineMaps.instance.OnChangeZoom += OnChangeZoom;
    }


    void OnChangeZoom()
    {
        float originalScale = defaultScale;
        float zoomRatio = (float)(1 << OnlineMaps.instance.zoom) / (float)(1 << overallDefaultZoom);
        float newScale = originalScale * zoomRatio;
        if (overallCurrentScale != newScale)
            overallCurrentScale = newScale;
        //Debug.Log(transform.localScale);

        markers = OnlineMapsTileSetControl.instance.markers3D;
        for (int ii = 0; ii < markers.Length; ii++)
        {
            if (markers[ii].transform == transform)
            {
                markers[ii].scale = newScale;
            }
        }
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
                    defaultScale = markers[ii].scale;
                }
            }
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
