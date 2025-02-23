using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public class LineRendererCollider : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private MeshCollider meshCollider;


    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        meshCollider = GetComponent<MeshCollider>();
    }

    private void Update()
    {
        UpdateMeshCollider();
    }

    private void UpdateMeshCollider()
    {
        Mesh mesh = new Mesh();
        lineRenderer.BakeMesh(mesh, true);
        meshCollider.sharedMesh = mesh;
    }
}