// CelestialBody.cs
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CelestialBody : MonoBehaviour
{
    [Tooltip("The mass of this celestial body. Larger masses have stronger gravity.")]
    public float mass;

    [Tooltip("The initial velocity of this body. Crucial for establishing orbits.")]
    public Vector3 initialVelocity;

    // The universal gravitational constant, scaled for game purposes.
    public const float G = 8f; 

    // A static list to hold all celestial bodies in the scene.
    private static List<CelestialBody> allBodies;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        
        // Important: Disable Unity's built-in gravity.
        rb.useGravity = false;
    }

    void OnEnable()
    {
        // Add this body to the static list when it's created.
        if (allBodies == null)
        {
            allBodies = new List<CelestialBody>();
        }
        allBodies.Add(this);
    }

    void OnDisable()
    {
        // Remove this body from the list when it's destroyed.
        allBodies.Remove(this);
    }

    void Start()
    {
        // Apply the initial velocity once the simulation starts.
        rb.linearVelocity = initialVelocity;
    }
    
    // FixedUpdate is used for physics calculations.
    void FixedUpdate()
    {
        // Loop through every other body in the simulation to calculate gravitational forces.
        foreach (CelestialBody otherBody in allBodies)
        {
            // Skip calculating gravity with ourself.
            if (otherBody == this)
            {
                continue;
            }

            // --- Gravitational Force Calculation ---

            // 1. Find the distance between the two bodies.
            float distance = Vector3.Distance(transform.position, otherBody.transform.position);

            // 2. Calculate the magnitude of the gravitational force using Newton's law.
            // F = G * (m1 * m2) / r^2
            float forceMagnitude = (G * mass * otherBody.mass) / (distance * distance);

            // 3. Get the direction of the force.
            Vector3 forceDirection = (otherBody.transform.position - transform.position).normalized;

            // 4. Combine direction and magnitude to create the final force vector.
            Vector3 forceVector = forceDirection * forceMagnitude;

            Debug.DrawRay(transform.position, forceVector.normalized * 1000f, Color.red);

            // 5. Apply the force to our Rigidbody.
            rb.AddForce(forceVector);
        }
    }
}