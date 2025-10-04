using System.Collections;
using UnityEngine;

[RequireComponent(typeof(MeshCollider))]
public class Ocean : MeshGenerator
{
    [Header("Tsunami Animation Settings")]
    public float waveAmplitude = 0.1f;
    [Range(1f, 90f)]
    public float waveWidth = 25f;
    public float waveSpeed = 60f;

    [Tooltip("How far the wave travels around the sphere in degrees before fading out.")]
    [Range(1f, 180f)]
    public float ogWrapAngleDegrees = 30f;
    private float wrapAngleDegrees;

    [Header("Crater Settings")]
    public float ogCraterDepth = 0.2f;
    [Range(1f, 90f)]
    public float ogCraterRadius = 15f;
    public float ogCraterFalloff = 2f;
    
    // --- ✨ NEW PARAMETER ✨ ---
    [Tooltip("The speed at which water flows back into the crater.")]
    public float craterFillSpeed = 0.05f; 

    private float craterDepth;
    private float craterRadius;
    private float craterFalloff;

    private Vector3[] originalVertices;
    private Vector3[] currentVertices;

    private Mantle mantle;

    void Awake()
    {
        // Attempt to find the Mantle object in the scene.
        GameObject mantleObject = GameObject.Find("Mantle");
        if (mantleObject != null)
        {
            mantle = mantleObject.GetComponent<Mantle>();
        }
        else
        {
            Debug.LogError("Could not find an object named 'Mantle' in the scene.");
        }
        
        GenerateIcosphere();
        originalVertices = mesh.vertices;
    }

    void Update()
    {
        // Example for testing with a mouse click
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // This assumes your Mantle script has a public method to deform itself.
                // You would call this from your main asteroid impact script.
                if (mantle != null)
                {
                    // Example: mantle.DeformMantle(hit.point, 1.0f);
                }
                StartTsunami(hit.point, 1.0f);
            }
        }
    }

    public void StartTsunami(Vector3 worldImpactPoint, float mass)
    {
        StopAllCoroutines();
        craterDepth = ogCraterDepth + (mass / 100);
        craterFalloff = ogCraterFalloff + (mass / 100);
        craterRadius = ogCraterRadius + (mass / 100);
        wrapAngleDegrees = ogWrapAngleDegrees + (mass / 100);

        ApplyCrater(worldImpactPoint);
        
        // --- ✨ LOGIC UPDATE: Start both coroutines ✨ ---
        StartCoroutine(AnimateTsunami(worldImpactPoint));
        //StartCoroutine(AnimateCraterFill(worldImpactPoint)); // The new coroutine
    }

    private void ApplyCrater(Vector3 worldImpactPoint)
    {
        Vector3 localImpactPoint = transform.InverseTransformPoint(worldImpactPoint).normalized;

        for (int i = 0; i < originalVertices.Length; i++)
        {
            Vector3 vertexDir = originalVertices[i].normalized;
            float angle = Vector3.Angle(vertexDir, localImpactPoint);

            if (angle < craterRadius)
            {
                float normalizedDist = angle / craterRadius;
                float craterInfluence = Mathf.Pow(1 - normalizedDist, craterFalloff);
                float depthOffset = craterInfluence * craterDepth;
                originalVertices[i] = vertexDir * (radius - depthOffset);
            }
        }
        // Immediately update the mesh to show the new crater
        mesh.vertices = originalVertices;
        mesh.RecalculateNormals();
    }

    private IEnumerator AnimateTsunami(Vector3 worldImpactPoint)
    {
        currentVertices = new Vector3[originalVertices.Length];
        Vector3 localImpactPoint = transform.InverseTransformPoint(worldImpactPoint).normalized;

        float[] vertexAngles = new float[originalVertices.Length];
        for (int i = 0; i < originalVertices.Length; i++)
        {
            vertexAngles[i] = Vector3.Angle(originalVertices[i].normalized, localImpactPoint);
        }

        float duration = wrapAngleDegrees / waveSpeed;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float currentWaveAngle = elapsedTime * waveSpeed;
            float dissipation = 1f - Mathf.Clamp01(currentWaveAngle / wrapAngleDegrees);
            float currentAmplitude = waveAmplitude * dissipation;

            for (int i = 0; i < currentVertices.Length; i++)
            {
                // Read from the (potentially changing) base mesh each frame
                Vector3 baseVertex = originalVertices[i];
                float heightOffset = 0f;
                float distanceToCrest = Mathf.Abs(vertexAngles[i] - currentWaveAngle);

                if (distanceToCrest < waveWidth / 2f)
                {
                    float waveT = distanceToCrest / (waveWidth / 2f);
                    float waveShape = Mathf.Cos(waveT * Mathf.PI * 0.5f);
                    heightOffset = waveShape * currentAmplitude;
                }
                
                // Apply the wave height on top of the base vertex position
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

        // When the tsunami is over, snap back to the base mesh, which is still being filled.
        mesh.vertices = originalVertices;
        mesh.RecalculateNormals();
        var finalCol = GetComponent<MeshCollider>();
        if (finalCol != null)
        {
            finalCol.sharedMesh = null;
            finalCol.sharedMesh = mesh;
        }
    }

    // --- ✨ NEW COROUTINE ✨ ---
    private IEnumerator AnimateCraterFill(Vector3 worldImpactPoint)
    {
        if (mantle == null)
        {
            Debug.LogError("Mantle reference is missing. Cannot fill crater.");
            yield break;
        }
        
        // This requires you to add a public property to your Mantle.cs script.
        // See the note below the code block for an example.
        Vector3[] mantleVertices = mantle.DeformedVertices; 
    
        if (mantleVertices == null || mantleVertices.Length != originalVertices.Length)
        {
            Debug.LogError("Mantle vertices not available or mismatched length!");
            yield break;
        }
    
        Vector3 localImpactPoint = transform.InverseTransformPoint(worldImpactPoint).normalized;
        bool isStillFilling = true;

        while (isStillFilling)
        {
            isStillFilling = false;
        
            for (int i = 0; i < originalVertices.Length; i++)
            {
                float angle = Vector3.Angle(originalVertices[i].normalized, localImpactPoint);

                if (angle < craterRadius)
                {
                    // The target for this water vertex is on the mantle's surface, at sea level
                    Vector3 targetPosition = mantleVertices[i].normalized * radius;

                    if (originalVertices[i] != targetPosition)
                    {
                        // Slowly move the base vertex towards its final resting position
                        originalVertices[i] = Vector3.MoveTowards(
                            originalVertices[i],
                            targetPosition,
                            craterFillSpeed * Time.deltaTime
                        );
                    
                        isStillFilling = true; 
                    }
                }
            }
            yield return null; // Wait for the next frame
        }
    }
}