using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class ConeCast : MonoBehaviour
{
    Mesh coneMesh;
    MeshFilter coneMeshFilter;

    private int rayCount = 100;
    [SerializeField] private Material coneMaterial;
    [SerializeField] [Range(0, 100)] private float coneLength;
    [SerializeField] [Range(0, 10)] private float coneWidth;

    void Start()
    {
        coneMeshFilter = transform.AddComponent<MeshFilter>();
        coneMesh = new Mesh();
        coneWidth *= Mathf.Deg2Rad;
    }

    void Update()
    {
        DrawCone();
    }

    void DrawCone()
    {
        Vector3[] Vertices = new Vector3[rayCount + 1];
        Vertices[0] = Vector3.zero;

        int[] Triangles = new int[rayCount * 3];

        float currentAngle = coneWidth / 2;
        float angleIncrements = coneWidth / rayCount;
        float Sine;
        float Cosine;

        for (int i = 0; i <= rayCount; i++)
        {
            Sine = Mathf.Sin(currentAngle);
            Cosine = Mathf.Cos(currentAngle);

            float x = coneWidth * Sine;
            float z = coneWidth * Cosine;

            Vertices[i] = new Vector3(x, 0f, z);

            currentAngle += angleIncrements;
        }

        for (int i = 0; i < rayCount; i++)
        {
            Triangles[i * 3] = i;
            Triangles[i * 3 + 1] = (i + 1) % rayCount;
            Triangles[i * 3 + 2] = rayCount;
        }

        coneMesh.vertices = Vertices;
        coneMesh.triangles = Triangles;
        coneMeshFilter.mesh = coneMesh;
    }
}
