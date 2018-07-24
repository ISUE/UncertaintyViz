using System;
using UnityEngine;

public class CheckMapCompleteLoaded : MonoBehaviour
{
    public static Action OnMapCompleteLoaded;

    private void Start()
    {
        OnMapCompleteLoaded += () => Debug.Log("Complete Loaded");
        OnlineMaps.instance.OnMapUpdated += OnMapUpdated;
        OnlineMaps.instance.OnChangeZoom += OnChangeZoom;
    }

    private void OnMapUpdated()
    {
        lock (OnlineMapsTile.tiles)
        {
            if (OnlineMapsTile.tiles.Count == 0) return;
            foreach (OnlineMapsTile t in OnlineMapsTile.tiles)
            {
                // Check tile status
                // If status == loading or status == none (not started), then wait more
                if (t.status == OnlineMapsTileStatus.loading || t.status == OnlineMapsTileStatus.none) return;
            }
        }

        OnlineMaps.instance.OnMapUpdated -= OnMapUpdated;
        if (OnMapCompleteLoaded != null) OnMapCompleteLoaded();
        //Destroy(this);
    }

    private void OnChangeZoom()
    {
        OnlineMaps.instance.OnMapUpdated += OnMapUpdated;
    }
}