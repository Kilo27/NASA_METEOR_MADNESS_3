using UnityEngine;

[RequireComponent(typeof(MeshCollider))]
public class SuperEarth : MeshGenerator
{
    public float crumpleAmount = 15.0f; // How far vertices get pulled in
    public float crumpleRadius = 15.0f; // How big the area of crumpling is
    public Vector3 crumpleCenter = Vector3.zero; // Center of the crumple (world or local depending)

    Vector3 lastHitPoint;

    void Start()
    {
        CrumpleAtWorldPoint(Vector3.up, 1.0f, 1.0f);
    }

    public void CrumpleAtWorldPoint(Vector3 worldImpactPoint, float radius, float depth)
    {
        lastHitPoint = worldImpactPoint;

        MeshFilter mf = GetComponent<MeshFilter>();
        Mesh mesh = mf.sharedMesh;
        Vector3[] vertices = mesh.vertices;

        Debug.Log(vertices);

        // Convert world point into *local space of the mesh*
        Vector3 localImpactPoint = transform.InverseTransformPoint(worldImpactPoint);

        for (int i = 0; i < vertices.Length; i++)
        {
            // vertices[i] is already in local space
            float dist = Vector3.Distance(vertices[i], localImpactPoint);

            if (dist < radius)
            {
                float falloff = 1f - (dist / radius);
                float push = falloff * depth;

                // Push vertex inward toward center of sphere (or toward impact point if desired)
                vertices[i] -= vertices[i].normalized * push * 2;
            }
        }

        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        // ALSO update the collider if using MeshCollider
        var collider = GetComponent<MeshCollider>();
        if (collider != null)
        {
            collider.sharedMesh = null;
            collider.sharedMesh = mesh;
            collider.convex = true;
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Debug.Log("Raycast Hit Point (world): " + hit.point);
                Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.red, 2f);

                lastHitPoint = hit.point;  // For OnDrawGizmos
                CrumpleAtWorldPoint(hit.point, crumpleRadius, crumpleAmount);
            }
            else
            {
                Debug.LogWarning("Raycast did not hit anything.");
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(lastHitPoint, 0.05f);
    }
}
