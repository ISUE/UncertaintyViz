using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Detail_Helper : MonoBehaviour {

    Ray ray;
    RaycastHit hit;
    GameObject[] COAs;
    GameObject[] Assets;
    GameObject[] Targets;

    Text textbox;

    GameObject curr = null;
    bool init = false;
    
	void Start()
    {
        textbox = GetComponent<Text>();
    }

	// Update is called once per frame
	void Update () {
        if (!init)
        {
            COAs = GameObject.FindGameObjectsWithTag("COA");
            Assets = GameObject.FindGameObjectsWithTag("Asset");
            Targets = GameObject.FindGameObjectsWithTag("Target");
            init = true;
        }

        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.gameObject.transform.parent)
            {
                //print(hit.collider.gameObject.transform.parent.gameObject.name);
                curr = hit.collider.gameObject.transform.parent.gameObject;
                WriteNewInformation();
            }
            else
            {
                curr = null;
                textbox.text = "";
            }
        }
    }

    void WriteNewInformation()
    {
        for(int ii = 0; ii < COAs.Length; ii++)
        {
            if(COAs[ii].transform == curr.transform)
            {
                double certainty = System.Math.Round(curr.GetComponent<MarkerScale>().certainty,3);
                switch (ii)
                {
                    case 0:
                        textbox.text = "Type:\tCOA\nAsset:\tYacht\nTarget:\tFood\nLocation:\t(18.35919,-81.30981)\nCertainty:\t" + certainty;
                        break;
                    case 1:
                        textbox.text = "Type:\tCOA\nAsset:\tYacht\nTarget:\tUnknown\nLocation:\t(17.33804,-82.65372)\nCertainty:\t" + certainty;
                        break;
                    case 2:
                        textbox.text = "Type:\tCOA\nAsset:\tSmall Boat\nTarget:\tFood\nLocation:\t(18.77949,-81.32957)\nCertainty:\t" + certainty;
                        break;
                    case 3:
                        textbox.text = "Type:\tCOA\nAsset:\tYacht\nTarget:\tAlcohol\nLocation:\t(16.72828,-82.34663)\nCertainty:\t" + certainty;
                        break;
                    default:
                        textbox.text = "";
                        break;
                }
            }
        }
        for (int ii = 0; ii < Assets.Length; ii++)
        {
            if (Assets[ii].transform == curr.transform)
            {
                switch (ii)
                {
                    case 0:
                        textbox.text = "Type:\tAsset\nName:\tYacht\nLocation:\t(19.4133,-81.2546)\nMax Load:\t200kg\nMax Speed:\t50 knots";
                        break;
                    case 1:
                        textbox.text = "Type:\tAsset\nName:\tYacht\nLocation:\t(19.1133,-80.2546)\nMax Load:\t200kg\nMax Speed:\t50 knots";
                        break;
                    case 2:
                        textbox.text = "Type:\tAsset\nName:\tSmall Boat\nLocation:\t(19.2133,-82.2546)\nMax Load:\t70kg\nMax Speed:\t60 knots";
                        break;
                    case 3:
                        textbox.text = "";
                        break;
                    default:
                        textbox.text = "";
                        break;
                }
            }
        }
        for (int ii = 0; ii < Targets.Length; ii++)
        {
            if (Targets[ii].transform == curr.transform || Targets[ii].transform == curr.transform.parent.transform)
            {

                switch (ii)
                {
                    case 0:
                        textbox.text = "Type:\tTarget\nName:\tAlcohol\nLocation:\t(17.000,-81.2546)\nWeight:\t50kg\nValue:\t2000";
                        break;
                    case 1:
                        textbox.text = "Type:\tTarget\nName:\tUnknown\nLocation:\t(18.3133,-82.2546)\nWeight:\t?\nValue:\t?";
                        break;
                    case 2:
                        textbox.text = "Type:\tTarget\nName:\tFood\nLocation:\t(18.1330,-80.2546)\nWeight:\t40kg\nValue:\t1500";
                        break;
                    case 3:
                        textbox.text = "";
                        break;
                    default:
                        textbox.text = "";
                        break;
                }
            }
        }
    }
}
