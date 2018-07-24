using UnityEngine;
using System.Collections;

public class COA_Helper : MonoBehaviour {

    static int count = 0;
    private double certainty;
    public GameObject start = null;

    private GameObject[] startPoint;
    private LineRenderer lr;
	// Use this for initialization
	void Start () {
        certainty = Random.value;
        lr = GetComponent<LineRenderer>();
    }
	
	// Update is called once per frame
	void Update () {
       
        Vector3[] points = new Vector3[2];
        if (!start)
        {
            startPoint = GameObject.FindGameObjectsWithTag("Asset");
            //int ii = (int)(Random.value * startPoint.Length) % startPoint.Length;
            int ii = count % startPoint.Length;
            count++;
            start = startPoint[ii];
        }
        if(start)
        {
            points[0] = transform.position;
            points[1] = start.transform.position;
            points[0].y+=4;
            points[1].y+=4;
        }
       else
        {
            points[0] = Vector2.zero;
            points[1] = Vector2.zero;
        }
        lr.SetPositions(points);

    }
}
