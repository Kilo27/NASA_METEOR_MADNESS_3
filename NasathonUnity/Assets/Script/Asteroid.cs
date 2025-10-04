using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Asteroid : MonoBehaviour
{

    public Mesh mesh;
    public int longitudeSegments = 20;
    public int latitudeSegments = 20;

    public float radius;
    public float mass;
    public float velocity;

    private GameObject earth;

    private void Start()
    {
        earth = GameObject.Find("Earth");
    }

    public void InitializeMesh(AsteroidCreationUI.AsteroidCreationData data)
    {
        radius = data.diameter / 2;
        mass = data.mass;
        velocity = data.velocity;

        mesh = new Mesh();
        mesh.name = "Procedural Asteroid";

        int vertCount = (longitudeSegments + 1) * (latitudeSegments + 1);
        Vector3[] vertices = new Vector3[vertCount];
        Vector3[] normals = new Vector3[vertCount];
        Vector2[] uv = new Vector2[vertCount];
        int index = 0;

        for (int lat = 0; lat <= latitudeSegments; lat++)
        {
            float a1 = Mathf.PI * lat / latitudeSegments;
            float sin1 = Mathf.Sin(a1);
            float cos1 = Mathf.Cos(a1);

            for (int lon = 0; lon <= longitudeSegments; lon++)
            {
                float a2 = 2 * Mathf.PI * lon / longitudeSegments;
                float sin2 = Mathf.Sin(a2);
                float cos2 = Mathf.Cos(a2);

                float x = sin1 * cos2;
                float y = cos1;
                float z = sin1 * sin2;

                Vector3 vertex = new Vector3(x, y, z) * radius;
                vertices[index] = vertex;
                normals[index] = vertex.normalized;
                uv[index] = new Vector2((float)lon / longitudeSegments, (float)lat / latitudeSegments);
                index++;
            }
        }

        int[] triangles = new int[longitudeSegments * latitudeSegments * 6];
        int triIndex = 0;

        for (int lat = 0; lat < latitudeSegments; lat++)
        {
            for (int lon = 0; lon < longitudeSegments; lon++)
            {
                int current = lat * (longitudeSegments + 1) + lon;
                int next = current + longitudeSegments + 1;

                triangles[triIndex++] = current;
                triangles[triIndex++] = current + 1;
                triangles[triIndex++] = next;

                triangles[triIndex++] = current + 1;
                triangles[triIndex++] = next + 1;
                triangles[triIndex++] = next;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.uv = uv;

        ApplyCraters(vertices, radius, craterCount: 3);

        mesh.vertices = vertices;
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;
    }

    void ApplyCraters(Vector3[] vertices, float radius, int craterCount)
    {
        float craterRadius = radius * 0.5f;
        float craterDepth = radius * 1.0f;

        for (int i = 0; i < craterCount; i++)
        {
            Vector3 craterCenter = Random.onUnitSphere * radius;

            for (int v = 0; v < vertices.Length; v++)
            {
                float dist = Vector3.Distance(vertices[v], craterCenter);

                if (dist < craterRadius)
                {
                    float falloff = 1f - (dist / craterRadius);
                    float depth = falloff * craterDepth;
                    vertices[v] -= vertices[v].normalized * depth;
                }
            }
        }
    }

    private void Update()
    {
        Move();
    }

    private void Move()
    {
        // Find Earth by tag (make sure your Earth GameObject has the "Earth" tag)


        if (earth != null)
        {
            // Calculate direction to Earth
            Vector3 directionToEarth = (earth.transform.position - transform.position).normalized;

            // Move towards Earth
            transform.position += directionToEarth * velocity * Time.deltaTime;
        }
        else
        {
            Debug.LogWarning("Earth not found! Make sure there's a GameObject with tag 'Earth'");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log(other);
        Earth earth = other.GetComponent<Earth>();
        earth.CrumpleAtWorldPoint(transform.position, earth.crumpleRadius, earth.crumpleAmount);
        Destroy(this.gameObject);
    }
}
