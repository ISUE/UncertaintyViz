using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;

public class ConeHelper : MonoBehaviour
{
    static List<int> angles = new List<int> { 180};
    static int index = 0;
    /*
    public GameObject conePrefab;
    public GameObject discretePrefab;
    public GameObject greenPrefab;
    public GameObject fanPrefab;
    public GameObject pointCloudPrefab;
    public GameObject lineCloudPrefab;
    public GameObject greenDynamicLeftPrefab;
    public GameObject greenDynamicRightPrefab;
    public GameObject greenDynamicStraightPrefab;
    public GameObject greenEasyPrefab;
    */

    public GameObject fanPrefab;
    public GameObject fanLowComplexityLowUncertainty;
    public GameObject fanLowComplexityMidUncertainty;
    public GameObject fanLowComplexityHighUncertainty;
    public GameObject fanHighComplexityLowUncertainty;
    public GameObject fanHighComplexityMidUncertainty;
    public GameObject fanHighComplexityHighUncertainty;

    public GameObject greenPrefab;
    public GameObject greenDynamicLeftPrefab;
    public GameObject greenDynamicRightPrefab;
    public GameObject greenDynamicStraightPrefab;
    public GameObject greenEasyPrefab;

    int mode = 0;    
    public float rotation;

    GameObject plan;

    // Use this for initialization
    void Start()
    {

        //rotation = Random.value * 3.0f % 3 * 120;
        rotation = angles[index];
        index = (index + 1) % angles.Count;

        plan = Instantiate(fanPrefab);
        plan.transform.parent = transform;
        plan.transform.localPosition = new Vector3(0, .2f, 0);
        plan.transform.Rotate(new Vector3(90, rotation, 0));
        plan.transform.localScale = new Vector3(20, 20, 20);

    }

    // Update is called once per frame
    void Update()
    {
        if (SceneManager.GetActiveScene().name.Contains("StudyFlow"))
            return;
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButtonDown(0))
        {
            rotation = plan.transform.rotation.eulerAngles.y;
            RaycastHit[] hits;
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            hits = Physics.RaycastAll(ray);
            if (hits.Length > 0)
            {
                //print("Hit " + hit.collider.gameObject.transform.parent.transform.parent.gameObject + " "  + this.gameObject + " " + (hit.collider.gameObject.transform.parent.transform.parent == gameObject));                

                if (CheckHit(hits, this.gameObject))
                {
                    int zoom = plan.GetComponent<ConeScale>().defaultZoom;
                    float scale = plan.GetComponent<ConeScale>().defaultScale;

                    float curScale = plan.transform.localScale.x;
                    mode = (mode + 1) % 7;

                    if (mode == 0)
                    {
                        GameObject.Destroy(plan);
                        plan = Instantiate(fanPrefab);
                        plan.transform.parent = transform;
                        plan.transform.localPosition = new Vector3(0, .2f, 0);
                        plan.transform.Rotate(new Vector3(90, rotation, 0));
                        plan.transform.localScale = new Vector3(curScale, curScale, curScale);
                    }
                    else if (mode == 1)
                    {
                        GameObject.Destroy(plan);
                        plan = Instantiate(fanLowComplexityLowUncertainty);
                        plan.transform.parent = transform;
                        plan.transform.localPosition = new Vector3(0, .2f, 0);
                        plan.transform.Rotate(new Vector3(90, rotation, 0));
                        plan.transform.localScale = new Vector3(curScale, curScale, curScale);
                    }
                    else if (mode == 2)
                    {
                        GameObject.Destroy(plan);
                        plan = Instantiate(fanLowComplexityMidUncertainty);
                        plan.transform.parent = transform;
                        plan.transform.localPosition = new Vector3(0, .2f, 0);
                        plan.transform.Rotate(new Vector3(90, rotation, 0));
                        plan.transform.localScale = new Vector3(curScale, curScale, curScale);
                    }
                    else if (mode == 3)
                    {
                        GameObject.Destroy(plan);
                        plan = Instantiate(fanLowComplexityHighUncertainty);
                        plan.transform.parent = transform;
                        plan.transform.localPosition = new Vector3(0, .2f, 0);
                        plan.transform.Rotate(new Vector3(90, rotation, 0));
                        plan.transform.localScale = new Vector3(curScale, curScale, curScale);
                    }
                    else if (mode == 4)
                    {
                        GameObject.Destroy(plan);
                        plan = Instantiate(fanHighComplexityLowUncertainty);
                        plan.transform.parent = transform;
                        plan.transform.localPosition = new Vector3(0, .2f, 0);
                        plan.transform.Rotate(new Vector3(90, rotation, 0));
                        plan.transform.localScale = new Vector3(curScale, curScale, curScale);
                    }
                    else if (mode == 5)
                    {
                        GameObject.Destroy(plan);
                        plan = Instantiate(fanHighComplexityMidUncertainty);
                        plan.transform.parent = transform;
                        plan.transform.localPosition = new Vector3(0, .2f, 0);
                        plan.transform.Rotate(new Vector3(90, rotation, 0));
                        plan.transform.localScale = new Vector3(curScale, curScale, curScale);
                    }
                    else if (mode == 6)
                    {
                        GameObject.Destroy(plan);
                        plan = Instantiate(fanHighComplexityHighUncertainty);
                        plan.transform.parent = transform;
                        plan.transform.localPosition = new Vector3(0, .2f, 0);
                        plan.transform.Rotate(new Vector3(90, rotation, 0));
                        plan.transform.localScale = new Vector3(curScale, curScale, curScale);
                    }
                    else if (mode == 7)
                    {
                        GameObject.Destroy(plan);
                        plan = Instantiate(greenDynamicLeftPrefab);
                        plan.transform.parent = transform;
                        plan.transform.localPosition = new Vector3(0, .2f, 0);
                        plan.transform.Rotate(new Vector3(90, rotation, 0));
                        plan.transform.localScale = new Vector3(curScale, curScale, curScale);
                    }
                    else if (mode == 8)
                    {
                        GameObject.Destroy(plan);
                        plan = Instantiate(greenDynamicRightPrefab);
                        plan.transform.parent = transform;
                        plan.transform.localPosition = new Vector3(0, .2f, 0);
                        plan.transform.Rotate(new Vector3(90, rotation, 0));
                        plan.transform.localScale = new Vector3(curScale, curScale, curScale);
                    }
                    else if (mode == 9)
                    {
                        GameObject.Destroy(plan);
                        plan = Instantiate(greenEasyPrefab);
                        plan.transform.parent = transform;
                        plan.transform.localPosition = new Vector3(0, .2f, 0);
                        plan.transform.Rotate(new Vector3(90, rotation, 0));
                        plan.transform.localScale = new Vector3(curScale, curScale, curScale);
                    }
                    plan.GetComponent<ConeScale>().init = true;
                    plan.GetComponent<ConeScale>().defaultZoom = zoom;
                    plan.GetComponent<ConeScale>().defaultScale = scale;
                }
            }
        }

        else if(Input.anyKey)
        {
            rotation = plan.transform.rotation.eulerAngles.y;

            int zoom = plan.GetComponent<ConeScale>().defaultZoom;
            float scale = plan.GetComponent<ConeScale>().defaultScale;

            float curScale = plan.transform.localScale.x;            
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                GameObject.Destroy(plan);
                plan = Instantiate(fanPrefab);
                plan.transform.parent = transform;
                plan.transform.localPosition = new Vector3(0, .2f, 0);
                plan.transform.Rotate(new Vector3(90, rotation, 0));
                plan.transform.localScale = new Vector3(curScale, curScale, curScale);
                mode = 0;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                GameObject.Destroy(plan);
                plan = Instantiate(fanLowComplexityLowUncertainty);
                plan.transform.parent = transform;
                plan.transform.localPosition = new Vector3(0, .2f, 0);
                plan.transform.Rotate(new Vector3(90, rotation, 0));
                plan.transform.localScale = new Vector3(curScale, curScale, curScale);
                mode = 1;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                GameObject.Destroy(plan);
                plan = Instantiate(fanLowComplexityMidUncertainty);
                plan.transform.parent = transform;
                plan.transform.localPosition = new Vector3(0, .2f, 0);
                plan.transform.Rotate(new Vector3(90, rotation, 0));
                plan.transform.localScale = new Vector3(curScale, curScale, curScale);
                mode = 2;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                GameObject.Destroy(plan);
                plan = Instantiate(fanLowComplexityHighUncertainty);
                plan.transform.parent = transform;
                plan.transform.localPosition = new Vector3(0, .2f, 0);
                plan.transform.Rotate(new Vector3(90, rotation, 0));
                plan.transform.localScale = new Vector3(curScale, curScale, curScale);
                mode = 3;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                GameObject.Destroy(plan);
                plan = Instantiate(fanHighComplexityLowUncertainty);
                plan.transform.parent = transform;
                plan.transform.localPosition = new Vector3(0, .2f, 0);
                plan.transform.Rotate(new Vector3(90, rotation, 0));
                plan.transform.localScale = new Vector3(curScale, curScale, curScale);
                mode = 4;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                GameObject.Destroy(plan);
                plan = Instantiate(fanHighComplexityMidUncertainty);
                plan.transform.parent = transform;
                plan.transform.localPosition = new Vector3(0, .2f, 0);
                plan.transform.Rotate(new Vector3(90, rotation, 0));
                plan.transform.localScale = new Vector3(curScale, curScale, curScale);
                mode = 5;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                GameObject.Destroy(plan);
                plan = Instantiate(fanHighComplexityHighUncertainty);
                plan.transform.parent = transform;
                plan.transform.localPosition = new Vector3(0, .2f, 0);
                plan.transform.Rotate(new Vector3(90, rotation, 0));
                plan.transform.localScale = new Vector3(curScale, curScale, curScale);
                mode = 6;
            }
            /*
            else if (Input.GetKeyDown(KeyCode.Alpha8))
            {
                GameObject.Destroy(plan);
                plan = Instantiate(greenDynamicLeftPrefab);
                plan.transform.parent = transform;
                plan.transform.localPosition = new Vector3(0, .2f, 0);
                plan.transform.Rotate(new Vector3(90, rotation, 0));
                plan.transform.localScale = new Vector3(curScale, curScale, curScale);
                mode = 7;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                GameObject.Destroy(plan);
                plan = Instantiate(greenDynamicRightPrefab);
                plan.transform.parent = transform;
                plan.transform.localPosition = new Vector3(0, .2f, 0);
                plan.transform.Rotate(new Vector3(90, rotation, 0));
                plan.transform.localScale = new Vector3(curScale, curScale, curScale);
                mode = 8;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                GameObject.Destroy(plan);
                plan = Instantiate(greenEasyPrefab);
                plan.transform.parent = transform;
                plan.transform.localPosition = new Vector3(0, .2f, 0);
                plan.transform.Rotate(new Vector3(90, rotation, 0));
                plan.transform.localScale = new Vector3(curScale, curScale, curScale);
                mode = 9;
            }*/
            plan.GetComponent<ConeScale>().init = true;
            plan.GetComponent<ConeScale>().defaultZoom = zoom;
            plan.GetComponent<ConeScale>().defaultScale = scale;
        }

    }

    bool CheckHit(RaycastHit[] check, GameObject target)
    {
        foreach (RaycastHit rh in check)
        {
            var temp = rh.collider.gameObject;
            while (temp != null)
            {
                if (temp == target)
                    return true;
                if (temp.transform.parent)
                    temp = temp.transform.parent.gameObject;
                else temp = null;
            }
        }


        return false;
    }

    public GameObject ChangePrefabToType(string type, float scale, int zoom)
    {
        var prefabList = new List<GameObject>();
        prefabList.Add(fanPrefab);
        prefabList.Add(fanLowComplexityLowUncertainty);
        prefabList.Add(fanLowComplexityMidUncertainty);
        prefabList.Add(fanLowComplexityHighUncertainty);
        prefabList.Add(fanHighComplexityLowUncertainty);
        prefabList.Add(fanHighComplexityMidUncertainty);
        prefabList.Add(fanHighComplexityHighUncertainty);
        prefabList.Add(greenDynamicRightPrefab);
        prefabList.Add(greenDynamicStraightPrefab);
        prefabList.Add(greenEasyPrefab);


        float curScale = plan.transform.localScale.x;

        for (int ii = 0; ii < prefabList.Count; ii++)        
        {
            //print(prefabList[ii].name + " " + type);
            if (prefabList[ii].name == type)
            {
                GameObject.Destroy(plan);
                plan = Instantiate(prefabList[ii]);
                plan.transform.parent = transform;
                plan.transform.localPosition = new Vector3(0, .2f, 0);
                plan.transform.Rotate(new Vector3(90, rotation, 0));
                plan.transform.localScale = new Vector3(curScale, curScale, curScale);
                mode = ii;
                plan.GetComponent<ConeScale>().init = true;
                plan.GetComponent<ConeScale>().defaultZoom = zoom;
                plan.GetComponent<ConeScale>().defaultScale = scale;
                return plan;
            }
        }

        return plan;
    }

    public GameObject getCone()
    {
        return plan;
    }

    public void setTransparent(bool val)
    {
        if (val == true)
        {
            if (plan.name.Contains("Cone")  || plan.name.Contains("Fan"))
            {
                var temp = plan.GetComponentsInChildren<MeshRenderer>();
                for (int ii = 0; ii < temp.Length; ii++)
                {
                    var col = temp[ii].material.GetColor("_TintColor");
                    col.a = .03f;
                    temp[ii].material.SetColor("_TintColor", col);
                }
            }
            else if (plan.name.Contains("Discrete"))
            {
                var temp = plan.GetComponentsInChildren<MeshRenderer>();
                for (int ii = 0; ii < temp.Length; ii++)
                {
                    var col = temp[ii].material.GetColor("_Color");
                    col.a = .2f;
                    temp[ii].material.SetColor("_Color", col);
                }
            }
            else if (plan.name.Contains("Green"))
            {
                var temp = plan.GetComponentsInChildren<MeshRenderer>();
                for (int ii = 0; ii < temp.Length; ii++)
                {
                    var col = temp[ii].material.GetColor("_Color");
                    col.a = .2f;
                    temp[ii].material.SetColor("_Color", col);
                    if (!temp[ii].material.name.Contains("temporal"))
                        continue;
                    col = temp[ii].material.GetColor("_EmissionColor");
                    col.g = .2f;
                    temp[ii].material.SetColor("_EmissionColor", col);
                }
            }
            else if (plan.name.Contains("Line"))
            {
                var LineRenderers = plan.transform.GetComponentsInChildren<LineRenderer>();
                foreach (var lr in LineRenderers)
                {
                    lr.startColor = new Color(.25f, .25f, .25f, .2f);
                    lr.endColor = new Color(1.0f, 0, 0, .2f);
                }
            }
            else if (plan.name.Contains("Point"))
            {
                //TODO: Fix this
                var pc = plan.transform.GetComponentInChildren<PointCloud>();
                var colorArray = pc.mesh.colors.Clone() as Color[];
                for (int ii = 0; ii < colorArray.Length; ii++)
                {
                    colorArray[ii].a *= .1f;
                }
                pc.mesh.SetColors(new List<Color>(colorArray));
            }
        }
        else
        {
            if (plan.name.Contains("Cone") || plan.name.Contains("Fan"))
            {
                var temp = plan.GetComponentsInChildren<MeshRenderer>();
                for (int ii = 0; ii < temp.Length; ii++)
                {
                    var col = temp[ii].material.GetColor("_TintColor");
                    col.a = .17f;
                    temp[ii].material.SetColor("_TintColor", col);
                }
            }
            else if (plan.name.Contains("Discrete"))
            {
                var temp = plan.GetComponentsInChildren<MeshRenderer>();
                for (int ii = 0; ii < temp.Length; ii++)
                {
                    var col = temp[ii].material.GetColor("_Color");
                    col.a = 1f;
                    temp[ii].material.SetColor("_Color", col);
                }
            }
            else if (plan.name.Contains("Green") )
            {
                var temp = plan.GetComponentsInChildren<MeshRenderer>();
                for (int ii = 0; ii < temp.Length; ii++)
                {
                    var col = temp[ii].material.GetColor("_Color");
                    col.a = 1f;
                    temp[ii].material.SetColor("_Color", col);
                    if (!temp[ii].material.name.Contains("temporal"))
                        continue;
                    col = temp[ii].material.GetColor("_EmissionColor");
                    col.g = .688f;
                    temp[ii].material.SetColor("_EmissionColor", col);
                }
            }
            else if (plan.name.Contains("Line"))
            {
                var LineRenderers = plan.transform.GetComponentsInChildren<LineRenderer>();
                foreach(var lr in LineRenderers)
                {
                    lr.startColor = new Color(.25f, .25f, .25f, 1.0f);
                    lr.endColor = Color.red;
                }
            }
            else if (plan.name.Contains("Point"))
            {
                //TODO: Fix this
                var pc = plan.transform.GetComponentInChildren<PointCloud>();
                var colorArray = pc.mesh.colors.Clone() as Color[];
                for (int ii = 0; ii < colorArray.Length; ii++)
                {
                    colorArray[ii].a *= 10f;
                }
                pc.mesh.SetColors(new List<Color>(colorArray));

            }
        }   
    }
}
