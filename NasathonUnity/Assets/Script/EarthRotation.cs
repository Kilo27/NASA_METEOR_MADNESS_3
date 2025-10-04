using UnityEngine;

public class EarthRotation : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private bool useRealisticRotation = true;
    [SerializeField] private float timeScale = 3600f; // 1 real second = 1 game hour
    
    [Header("Debug Info")]
    [SerializeField] private float currentRotationAngle;
    [SerializeField] private float currentEarthTime;

    // Real Earth constants
    private const float REAL_ROTATION_SPEED = 15.04107f; // Degrees per hour
    private const float AXIAL_TILT = 23.44f; // Earth's tilt in degrees
    private float accumulatedTime = 0f;

    private void Start()
    {
        // Set up Earth with realistic tilt
        transform.rotation = Quaternion.Euler(AXIAL_TILT, 0, 0);
        
        Debug.Log("Earth rotation started! Current settings:");
        Debug.Log($"- Realistic rotation: {useRealisticRotation}");
        Debug.Log($"- Time scale: {timeScale} (1 real second = {timeScale/3600f} game hours)");
        Debug.Log($"- Axial tilt: {AXIAL_TILT}°");
    }

    private void Update()
    {
        if (useRealisticRotation)
        {
            RotateRealistic();
        }
        else
        {
            // Simple rotation for testing (1 full rotation every 10 real seconds)
            transform.Rotate(0, 36f * Time.deltaTime, 0);
        }
        
        // Update debug info
        currentRotationAngle = transform.eulerAngles.y;
        currentEarthTime = accumulatedTime % 24f;
    }

    private void RotateRealistic()
    {
        // Calculate how much game time has passed (in hours)
        float gameHoursPassed = Time.deltaTime * timeScale / 3600f;
        
        // Calculate rotation based on real Earth speed (15.04107° per hour)
        float rotationAngle = REAL_ROTATION_SPEED * gameHoursPassed;
        
        // Apply the rotation
        transform.Rotate(0, rotationAngle, 0, Space.Self);
        
        // Track total time
        accumulatedTime += gameHoursPassed;
    }

    // Public methods to tweak settings at runtime
    public void SetTimeScale(float newTimeScale)
    {
        timeScale = newTimeScale;
        Debug.Log($"Time scale set to: {timeScale}");
    }
    
    public void ToggleRealisticRotation(bool realistic)
    {
        useRealisticRotation = realistic;
        Debug.Log($"Realistic rotation: {useRealisticRotation}");
    }
}