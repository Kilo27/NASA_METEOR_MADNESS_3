using System.Collections;
using UnityEngine;

[RequireComponent(typeof(MeshCollider))]
public class Ocean : MeshGenerator
{
    [Header("Tsunami Animation Settings")]
    [Tooltip("The initial height (amplitude) of the wave crest.")]
    public float waveAmplitude = 0.1f;
    [Tooltip("The width of the wave crest in degrees. A larger value creates a wider wave.")]
    [Range(1f, 90f)]
    public float waveWidth = 25f;
    [Tooltip("The speed the wave travels across the surface in degrees per second.")]
    public float waveSpeed = 60f;

    // --- ✨ NEW CRATER PARAMETERS ✨ ---
    [Header("Crater Settings")]
    [Tooltip("The maximum depth of the crater at the impact point.")]
    public float craterDepth = 0.2f;
    [Tooltip("The radius of the crater in degrees.")]
    [Range(1f, 90f)]
    public float craterRadius = 15f;
    [Tooltip("Controls the sharpness of the crater's edge. >1 is sharper, <1 is softer.")]
    public float craterFalloff = 2f;

    // Private variables for the animation state
    private Vector3[] originalVertices;
    private Vector3[] currentVertices;

    void Awake()
    {
        GenerateIcosphere();
        // Initialize originalVertices here to prevent null reference on first impact
        originalVertices = mesh.vertices; 
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                StartTsunami(hit.point);
            }
        }
    }

    public void StartTsunami(Vector3 worldImpactPoint)
    {
        StopAllCoroutines();
        
        // --- ✨ LOGIC UPDATE ✨ ---
        // First, apply the permanent crater to the mesh's base data.
        ApplyCrater(worldImpactPoint);
        // Then, start the wave animation on top of the newly cratered surface.
        StartCoroutine(AnimateTsunami(worldImpactPoint));
    }

    // --- ✨ NEW METHOD TO CREATE THE CRATER ✨ ---
    private void ApplyCrater(Vector3 worldImpactPoint)
    {
        Vector3 localImpactPoint = transform.InverseTransformPoint(worldImpactPoint).normalized;

        for (int i = 0; i < originalVertices.Length; i++)
        {
            Vector3 vertexDir = originalVertices[i].normalized;
            float angle = Vector3.Angle(vertexDir, localImpactPoint);

            // Only affect vertices within the crater's radius
            if (angle < craterRadius)
            {
                // Calculate how deep the crater should be at this vertex
                float normalizedDist = angle / craterRadius; // 0 at center, 1 at edge
                float craterInfluence = Mathf.Pow(1 - normalizedDist, craterFalloff);
                float depthOffset = craterInfluence * craterDepth;

                // Apply the new, deeper position to our base vertex data
                originalVertices[i] = vertexDir * (radius - depthOffset);
            }
        }
    }
    
    private IEnumerator AnimateTsunami(Vector3 worldImpactPoint)
    {
        // The 'originalVertices' array now contains the crater deformation
        currentVertices = new Vector3[originalVertices.Length];
        
        Vector3 localImpactPoint = transform.InverseTransformPoint(worldImpactPoint).normalized;

        float[] vertexAngles = new float[originalVertices.Length];
        for (int i = 0; i < originalVertices.Length; i++)
        {
            vertexAngles[i] = Vector3.Angle(originalVertices[i].normalized, localImpactPoint);
        }

        float duration = 180f / waveSpeed;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            float currentWaveAngle = elapsedTime * waveSpeed;
            float dissipation = 1f - Mathf.Clamp01(currentWaveAngle / 180f);
            float currentAmplitude = waveAmplitude * dissipation;

            for (int i = 0; i < currentVertices.Length; i++)
            {
                // The base position is now the cratered originalVertex
                Vector3 baseVertex = originalVertices[i];
                float heightOffset = 0f;
                float distanceToCrest = Mathf.Abs(vertexAngles[i] - currentWaveAngle);

                if (distanceToCrest < waveWidth / 2f)
                {
                    float waveT = distanceToCrest / (waveWidth / 2f);
                    float waveShape = Mathf.Cos(waveT * Mathf.PI * 0.5f);
                    heightOffset = waveShape * currentAmplitude;
                }

                // We add the wave height to the base radius of the cratered vertex
                currentVertices[i] = baseVertex.normalized * (baseVertex.magnitude + heightOffset);
            }

            mesh.vertices = currentVertices;
            mesh.RecalculateNormals();
            
            var col = GetComponent<MeshCollider>();
            if (col != null)
            {
                col.sharedMesh = null;
                col.sharedMesh = mesh;
            }

            yield return null;
        }

        // This now correctly resets the mesh to the cratered state
        mesh.vertices = originalVertices;
        mesh.RecalculateNormals();
        var finalCol = GetComponent<MeshCollider>();
        if (finalCol != null)
        {
            finalCol.sharedMesh = null;
            finalCol.sharedMesh = mesh;
        }
    }
}