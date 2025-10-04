using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]


public class MeshGenerator : MonoBehaviour
{
    // You can keep your old public variables, but the new method will use these:
    public Mesh mesh;
    public float radius = 1f;
    [Range(0, 6)]
    public int subdivisions = 4; // 3-4 is a good starting point

    // --- REPLACE YOUR OLD GenerateSphere() METHOD WITH THIS ---
    public void GenerateIcosphere()
{
    mesh = new Mesh();
    mesh.name = "Procedural Icosphere";

    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    Dictionary<long, int> midpointCache = new Dictionary<long, int>();

    // Create the 12 vertices of an Icosahedron
    float t = (1f + Mathf.Sqrt(5f)) / 2f;
    vertices.Add(new Vector3(-1, t, 0).normalized * radius);
    vertices.Add(new Vector3(1, t, 0).normalized * radius);
    vertices.Add(new Vector3(-1, -t, 0).normalized * radius);
    vertices.Add(new Vector3(1, -t, 0).normalized * radius);
    vertices.Add(new Vector3(0, -1, t).normalized * radius);
    vertices.Add(new Vector3(0, 1, t).normalized * radius);
    vertices.Add(new Vector3(0, -1, -t).normalized * radius);
    vertices.Add(new Vector3(0, 1, -t).normalized * radius);
    vertices.Add(new Vector3(t, 0, -1).normalized * radius);
    vertices.Add(new Vector3(t, 0, 1).normalized * radius);
    vertices.Add(new Vector3(-t, 0, -1).normalized * radius);
    vertices.Add(new Vector3(-t, 0, 1).normalized * radius);

    // Create the 20 base triangles
    triangles.AddRange(new int[] { 0, 11, 5, 0, 5, 1, 0, 1, 7, 0, 7, 10, 0, 10, 11 });
    triangles.AddRange(new int[] { 1, 5, 9, 5, 11, 4, 11, 10, 2, 10, 7, 6, 7, 1, 8 });
    triangles.AddRange(new int[] { 3, 9, 4, 3, 4, 2, 3, 2, 6, 3, 6, 8, 3, 8, 9 });
    triangles.AddRange(new int[] { 4, 9, 5, 2, 4, 11, 6, 2, 10, 8, 6, 7, 9, 8, 1 });

    // Subdivide the triangles
    for (int i = 0; i < subdivisions; i++)
    {
        List<int> newTriangles = new List<int>();
        for (int j = 0; j < triangles.Count; j += 3)
        {
            int v1 = triangles[j];
            int v2 = triangles[j + 1];
            int v3 = triangles[j + 2];
            int a = GetMidpointIndex(midpointCache, v1, v2, vertices);
            int b = GetMidpointIndex(midpointCache, v2, v3, vertices);
            int c = GetMidpointIndex(midpointCache, v3, v1, vertices);
            newTriangles.AddRange(new int[] { v1, a, c, v2, b, a, v3, c, b, a, b, c });
        }
        triangles = newTriangles;
    }

    // --- THIS IS THE CRUCIAL PART THAT WAS LIKELY MISSING ---
    // First, assign the vertices and triangles BEFORE calculating UVs and normals
    mesh.vertices = vertices.ToArray();
    mesh.triangles = triangles.ToArray();

    // Calculate UV coordinates for texturing
    Vector2[] uvs = new Vector2[vertices.Count];
    for (int i = 0; i < vertices.Count; i++)
    {
        Vector3 unitVertex = vertices[i].normalized;
        float u = 0.5f + (Mathf.Atan2(unitVertex.z, unitVertex.x) / (2 * Mathf.PI));
        float v = 0.5f - (Mathf.Asin(unitVertex.y) / Mathf.PI);
        uvs[i] = new Vector2(u, v);
    }
    // Assign the calculated UVs to the mesh
    mesh.uv = uvs;

    mesh.RecalculateNormals();

    GetComponent<MeshFilter>().sharedMesh = mesh;
    var meshCollider = GetComponent<MeshCollider>();
    if (meshCollider != null)
    {
        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = mesh;
    }
}

    private int GetMidpointIndex(Dictionary<long, int> cache, int i1, int i2, List<Vector3> vertices)
    {
        long smallerIndex = Mathf.Min(i1, i2);
        long greaterIndex = Mathf.Max(i1, i2);
        long key = (smallerIndex << 32) + greaterIndex;

        if (cache.TryGetValue(key, out int ret))
        {
            return ret;
        }

        Vector3 p1 = vertices[i1];
        Vector3 p2 = vertices[i2];
        Vector3 middle = (p1 + p2) / 2f;
        
        int newIndex = vertices.Count;
        vertices.Add(middle.normalized * radius);
        cache.Add(key, newIndex);
        return newIndex;
    }
}



