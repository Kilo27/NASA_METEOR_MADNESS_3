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

<<<<<<< HEAD
    [Tooltip("How far the wave travels around the sphere in degrees before fading out.")]
    [Range(1f, 180f)]
    public float ogWrapAngleDegrees = 30f;
    private float wrapAngleDegrees;

=======
>>>>>>> 8b479549d3a99ba58c73ae1b8f5ed5e050de840d
    [Header("Crater Settings")]
    public float ogCraterDepth = 0.2f;
    [Range(1f, 90f)]
    public float ogCraterRadius = 15f;
    public float ogCraterFalloff = 2f;
    
<<<<<<< HEAD
    // --- ✨ NEW PARAMETER ✨ ---
=======
>>>>>>> 8b479549d3a99ba58c73ae1b8f5ed5e050de840d
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
<<<<<<< HEAD
        // Attempt to find the Mantle object in the scene.
=======
>>>>>>> 8b479549d3a99ba58c73ae1b8f5ed5e050de840d
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
<<<<<<< HEAD
                // This assumes your Mantle script has a public method to deform itself.
                // You would call this from your main asteroid impact script.
                if (mantle != null)
                {
                    // Example: mantle.DeformMantle(hit.point, 1.0f);
=======
                if (mantle != null)
                {
                    // mantle.DeformMantle(hit.point, 1.0f); 
>>>>>>> 8b479549d3a99ba58c73ae1b8f5ed5e050de840d
                }
                StartTsunami(hit.point, 1.0f);
            }
        }
    }

<<<<<<< HEAD
    public void StartTsunami(Vector3 worldImpactPoint, float mass)
    {
        StopAllCoroutines();
=======
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
>>>>>>> 8b479549d3a99ba58c73ae1b8f5ed5e050de840d
        craterDepth = ogCraterDepth + (mass / 100);
        craterFalloff = ogCraterFalloff + (mass / 100);
        craterRadius = ogCraterRadius + (mass / 100);
        wrapAngleDegrees = ogWrapAngleDegrees + (mass / 100);

<<<<<<< HEAD
        ApplyCrater(worldImpactPoint);
        
        // --- ✨ LOGIC UPDATE: Start both coroutines ✨ ---
        StartCoroutine(AnimateTsunami(worldImpactPoint));
        StartCoroutine(AnimateCraterFill(worldImpactPoint)); // The new coroutine
=======
        // 2. Apply the initial "evaporation" crater
        ApplyCrater(worldImpactPoint);

        // 3. Run the tsunami animation AND WAIT FOR IT TO COMPLETE.
        // The 'yield return' is the magic here. It pauses this coroutine
        // until AnimateTsunami is finished.
        yield return StartCoroutine(AnimateTsunami(worldImpactPoint));
        
        // 4. NOW that the tsunami is over, start the crater fill animation.
        yield return StartCoroutine(AnimateCraterFill(worldImpactPoint));
>>>>>>> 8b479549d3a99ba58c73ae1b8f5ed5e050de840d
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
<<<<<<< HEAD
=======
        // This coroutine remains mostly the same, but it no longer needs to worry
        // about the fill animation happening at the same time.
>>>>>>> 8b479549d3a99ba58c73ae1b8f5ed5e050de840d
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
<<<<<<< HEAD
                // Read from the (potentially changing) base mesh each frame
=======
>>>>>>> 8b479549d3a99ba58c73ae1b8f5ed5e050de840d
                Vector3 baseVertex = originalVertices[i];
                float heightOffset = 0f;
                float distanceToCrest = Mathf.Abs(vertexAngles[i] - currentWaveAngle);

                if (distanceToCrest < waveWidth / 2f)
                {
                    float waveT = distanceToCrest / (waveWidth / 2f);
                    float waveShape = Mathf.Cos(waveT * Mathf.PI * 0.5f);
                    heightOffset = waveShape * currentAmplitude;
                }
                
<<<<<<< HEAD
                // Apply the wave height on top of the base vertex position
=======
>>>>>>> 8b479549d3a99ba58c73ae1b8f5ed5e050de840d
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

<<<<<<< HEAD
        // When the tsunami is over, snap back to the base mesh, which is still being filled.
=======
        // When the tsunami is over, snap back to the base mesh, which still has the empty crater.
>>>>>>> 8b479549d3a99ba58c73ae1b8f5ed5e050de840d
        mesh.vertices = originalVertices;
        mesh.RecalculateNormals();
        var finalCol = GetComponent<MeshCollider>();
        if (finalCol != null)
        {
            finalCol.sharedMesh = null;
            finalCol.sharedMesh = mesh;
        }
    }

<<<<<<< HEAD
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
=======
    private IEnumerator AnimateCraterFill(Vector3 worldImpactPoint)
    {
        // This coroutine is now only responsible for filling the crater, and it
        // directly modifies the mesh vertices since the tsunami is over.
        if (mantle == null) yield break;
        
        Vector3[] mantleVertices = mantle.DeformedVertices; 
        if (mantleVertices == null || mantleVertices.Length != originalVertices.Length) yield break;
>>>>>>> 8b479549d3a99ba58c73ae1b8f5ed5e050de840d
    
        Vector3 localImpactPoint = transform.InverseTransformPoint(worldImpactPoint).normalized;
        bool isStillFilling = true;

        while (isStillFilling)
        {
            isStillFilling = false;
        
            for (int i = 0; i < originalVertices.Length; i++)
            {
                float angle = Vector3.Angle(originalVertices[i].normalized, localImpactPoint);
<<<<<<< HEAD

                if (angle < craterRadius)
                {
                    // The target for this water vertex is on the mantle's surface, at sea level
                    Vector3 targetPosition = mantleVertices[i].normalized * radius;

                    if (originalVertices[i] != targetPosition)
                    {
                        // Slowly move the base vertex towards its final resting position
=======
                if (angle < craterRadius)
                {
                    Vector3 targetPosition = mantleVertices[i].normalized * radius;
                    if (originalVertices[i] != targetPosition)
                    {
>>>>>>> 8b479549d3a99ba58c73ae1b8f5ed5e050de840d
                        originalVertices[i] = Vector3.MoveTowards(
                            originalVertices[i],
                            targetPosition,
                            craterFillSpeed * Time.deltaTime
                        );
<<<<<<< HEAD
                    
=======
>>>>>>> 8b479549d3a99ba58c73ae1b8f5ed5e050de840d
                        isStillFilling = true; 
                    }
                }
            }
<<<<<<< HEAD
            yield return null; // Wait for the next frame
=======

            // Directly update the mesh vertices each frame during the fill
            mesh.vertices = originalVertices;
            mesh.RecalculateNormals();
            
            yield return null;
>>>>>>> 8b479549d3a99ba58c73ae1b8f5ed5e050de840d
        }
    }
}