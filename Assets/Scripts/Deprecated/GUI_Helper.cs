using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI.Extensions;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

public class GUI_Helper : MonoBehaviour {

    public GameObject guiBackground;
    public GameObject treeRoot;
    public Button mapCOAPrefab;
    public Button resetButton;

    private List<GameObject> COAs;
    private List<Button> mapCOAs;

    private OnlineMaps api;
    private OnlineMapsTileSetControl api3D;
    private double ratio;

    private double windowHeight;
    void Start()
    {
        resetButton.onClick.AddListener(onResetClick);


        COAs = new List<GameObject>(GameObject.FindGameObjectsWithTag("COA"));
        mapCOAs = new List<Button>();
        ratio = 1.0 / (COAs.Count-1);
        windowHeight = 240;
        double count = 0; 
        foreach(GameObject COA in COAs)
        {
            Button temp = Instantiate(mapCOAPrefab);
            temp.transform.SetParent(guiBackground.transform,false);

            RectTransform rt = temp.GetComponent<RectTransform>();
            rt.localScale.Set(1, 1, 1);
            rt.anchorMin = new Vector2(0.5f, 1.0f);
            rt.anchorMax = new Vector2(0.5f, 1.0f);
            rt.rect.size.Set(160, 14);
            //rt.anchoredPosition.Set(0, 0);
            float offset = (float)-(ratio * (count) * windowHeight) - 20;
            rt.anchoredPosition = new Vector3(0,offset,0);

            UILineRenderer t2 = temp.GetComponentInChildren<UILineRenderer>();
            t2.Points[0].Set(-30, -offset-150+7);            
            t2.Points[1].Set(2, 7);

            temp.onClick.AddListener(onClick);
            mapCOAs.Add(temp);
            count++;
        }
    }

    private void onClick()
    {

        GameObject temp = EventSystem.current.currentSelectedGameObject;
        if (!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl))
        {
            int selected = -1;
            for (int ii = 0; ii < mapCOAs.Count; ii++)
            {
                if (temp.transform == mapCOAs[ii].transform)
                {
                    selected = ii;
                }
            }
            for (int ii = 0; ii < mapCOAs.Count; ii++)
            {
                if (selected != ii)
                {
                    mapCOAs[ii].GetComponent<Image>().color = Color.red;
                    COAs[ii].SetActive(false);
                }
                else
                {
                    mapCOAs[ii].GetComponent<Image>().color = Color.green;
                    COAs[ii].SetActive(true);
                }
            }
        }
        else
        {
            Debug.Log("DEBUG");
            for (int ii = 0; ii < mapCOAs.Count; ii++)
            {
                if (temp.transform == mapCOAs[ii].transform)
                {
                    if(COAs[ii].activeSelf)
                    {
                        mapCOAs[ii].GetComponent<Image>().color = Color.red;
                        COAs[ii].SetActive(false);
                    }
                    else
                    {
                        mapCOAs[ii].GetComponent<Image>().color = Color.green;
                        COAs[ii].SetActive(true);
                    }
                }
            }
        }

    }

    private void onResetClick()
    {
        for (int ii = 0; ii < mapCOAs.Count; ii++)
        {
            mapCOAs[ii].GetComponent<Image>().color = Color.green;
            COAs[ii].SetActive(true);
        }
    }

    void Update()
    {
        if (api)
            api.position = api.position + new Vector2(1, 0);

    }
}