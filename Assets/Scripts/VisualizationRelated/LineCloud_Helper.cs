using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LineCloud_Helper : MonoBehaviour {

    public int numLines = 15;
    public float lineWidth = 3f;
    public float coneWidth = 5f;
    public Color c1 = Color.red;
    public Color c2 = Color.red;
    //private List<Material> materials; //apparently needed because of memory leaks.

    // Use this for initialization
    void Awake () {
        //materials = new List<Material>();
        for (int ii = 0; ii < numLines; ii++)
        {
            GameObject temp = new GameObject("LineMule" + ii);
            temp.transform.parent = transform;
            temp.transform.localPosition = new Vector3(0, 0, 0);
            LineRenderer lineRenderer = temp.AddComponent<LineRenderer>();
            lineRenderer.useWorldSpace = false;

            lineRenderer.material = new Material(Shader.Find("Particles/Alpha Blended"));            
            //materials.Add(lineRenderer.material);
            lineRenderer.SetColors(c1,c2);
            lineRenderer.SetWidth(lineWidth,lineWidth);
            lineRenderer.SetVertexCount(5);

            Vector3[] points = new Vector3[5];
            points[0] = Vector3.zero;
            var delta = Random.Range(-coneWidth, coneWidth);
            for(int jj = 1; jj < 5; jj++)
            {
                points[jj] = new Vector3(points[jj-1].x + (jj * delta), (-jj / 2.0f)*Random.Range(.8f,1.2f), 0);
            }            
            lineRenderer.SetPositions(points);

        }
	}

	// Update is called once per frame
	void Update () {
	
	}
}
