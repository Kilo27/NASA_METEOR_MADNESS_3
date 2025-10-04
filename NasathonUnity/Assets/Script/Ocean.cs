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
    [Range(1f, 180f)]
    public float ogWrapAngleDegrees = 30f;
    private float wrapAngleDegrees;

    [Header("Crater Settings")]
    public float ogCraterDepth = 0.2f;
    [Range(1f, 90f)]
    public float ogCraterRadius = 15f;
    public float ogCraterFalloff = 2f;
    
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
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (mantle != null)
                {
                    // mantle.DeformMantle(hit.point, 1.0f); 
                }
                StartTsunami(hit.point, 1.0f);
            }
        }
    }

    // --- ⬇️ ALTERED METHOD ⬇️ ---
    // This method now just starts the master sequence coroutine.
    public void StartTsunami(Vector3 worldImpactPoint, float mass)
    {
        StopAllCoroutines();
        StartCoroutine(RunImpactSequence(worldImpactPoint, mass));
    }

    // --- ✨ NEW MASTER COROUTINE ✨ ---
    // This coroutine controls the sequence of events: crater, then tsunami, then fill.
    private IEnumerator RunImpactSequence(Vector3 worldImpactPoint, float mass)
    {
        // 1. Set up all the impact parameters
        craterDepth = ogCraterDepth + (mass / 100);
        craterFalloff = ogCraterFalloff + (mass / 100);
        craterRadius = ogCraterRadius + (mass / 100);
        wrapAngleDegrees = ogWrapAngleDegrees + (mass / 100);

        // 2. Apply the initial "evaporation" crater
        ApplyCrater(worldImpactPoint);

        // 3. Run the tsunami animation AND WAIT FOR IT TO COMPLETE.
        // The 'yield return' is the magic here. It pauses this coroutine
        // until AnimateTsunami is finished.
        yield return StartCoroutine(AnimateTsunami(worldImpactPoint));
        
        // 4. NOW that the tsunami is over, start the crater fill animation.
        yield return StartCoroutine(AnimateCraterFill(worldImpactPoint));
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
        // This coroutine remains mostly the same, but it no longer needs to worry
        // about the fill animation happening at the same time.
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
                Vector3 baseVertex = originalVertices[i];
                float heightOffset = 0f;
                float distanceToCrest = Mathf.Abs(vertexAngles[i] - currentWaveAngle);

                if (distanceToCrest < waveWidth / 2f)
                {
                    float waveT = distanceToCrest / (waveWidth / 2f);
                    float waveShape = Mathf.Cos(waveT * Mathf.PI * 0.5f);
                    heightOffset = waveShape * currentAmplitude;
                }
                
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

        // When the tsunami is over, snap back to the base mesh, which still has the empty crater.
        mesh.vertices = originalVertices;
        mesh.RecalculateNormals();
        var finalCol = GetComponent<MeshCollider>();
        if (finalCol != null)
        {
            finalCol.sharedMesh = null;
            finalCol.sharedMesh = mesh;
        }
    }

    private IEnumerator AnimateCraterFill(Vector3 worldImpactPoint)
    {
        // This coroutine is now only responsible for filling the crater, and it
        // directly modifies the mesh vertices since the tsunami is over.
        if (mantle == null) yield break;
        
        Vector3[] mantleVertices = mantle.DeformedVertices; 
        if (mantleVertices == null || mantleVertices.Length != originalVertices.Length) yield break;
    
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
                    Vector3 targetPosition = mantleVertices[i].normalized * radius;
                    if (originalVertices[i] != targetPosition)
                    {
                        originalVertices[i] = Vector3.MoveTowards(
                            originalVertices[i],
                            targetPosition,
                            craterFillSpeed * Time.deltaTime
                        );
                        isStillFilling = true; 
                    }
                }
            }

            // Directly update the mesh vertices each frame during the fill
            mesh.vertices = originalVertices;
            mesh.RecalculateNormals();
            
            yield return null;
        }
    }
}