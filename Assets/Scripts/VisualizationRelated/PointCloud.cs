using UnityEngine;
using System.Collections;


[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PointCloud : MonoBehaviour
{

    public Mesh mesh;
    int numPoints = 30000;

    // Use this for initialization
    void Start()
    {
        mesh = new Mesh();

        GetComponent<MeshFilter>().mesh = mesh;
        CreateMesh();
    }

    void CreateMesh()
    {
        Vector3[] points = new Vector3[numPoints];
        int[] indecies = new int[numPoints];
        Color[] colors = new Color[numPoints];
        float y_coord;
        float x_coord;
        for (int i = 0; i < points.Length; ++i)
        {            
            y_coord = Random.Range(0, 1.0f);
            //x_coord = (1-y_coord) * Random.Range(-.5f, .5f);
            float u1 = Random.Range(0, 1.0f); //these are uniform(0,1) random doubles
            float u2 = Random.Range(0, 1.0f);
            float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Sin(2.0f * Mathf.PI * u2); //random normal(0,1)
            x_coord = 0.25f*(1-y_coord) * randStdNormal; //random normal(mean,stdDev^2)
            y_coord =  y_coord + Random.Range(-.2f, 0);
            points[i] = new Vector3(x_coord, y_coord-0.5f, 0);
            indecies[i] = i;
            colors[i] = new Color(1-Mathf.Abs(x_coord/.6f) + Random.Range(-.2f,.2f), 0/*1 - Mathf.Abs(y_coord/1.2f)*/, 0, .1f);
        }

        mesh.vertices = points;
        mesh.colors = colors;
        mesh.SetIndices(indecies, MeshTopology.Points, 0);

    }
}