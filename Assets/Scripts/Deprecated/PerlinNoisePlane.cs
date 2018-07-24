using UnityEngine;
using System.Collections;

public class PerlinNoisePlane : MonoBehaviour
{
    public bool isAnimating = false;
    public float perlinScale = 4.56f;
    public float waveSpeed = 1f;
    public float waveHeight = 2f;

    private Mesh mesh;

    void Start()
    {
        AnimateMesh(0);
    }

    void Update()
    {
        if(isAnimating)
            AnimateMesh(Time.timeSinceLevelLoad);
    }

    public void AnimateMesh(float time)
    {
        if (!mesh)
            mesh = GetComponent<MeshFilter>().mesh;

        Vector3[] vertices = mesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
            float pX = (vertices[i].x * perlinScale) + (time * waveSpeed);
            float pZ = (vertices[i].z * perlinScale) + (time * waveSpeed);

            vertices[i].y = (Mathf.PerlinNoise(pX, pZ) - 0.5f) * waveHeight;
        }

        mesh.vertices = vertices;
    }
}