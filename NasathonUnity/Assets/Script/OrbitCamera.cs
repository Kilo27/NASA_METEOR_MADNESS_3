using UnityEngine;

public class OrbitCamera : MonoBehaviour
{
    public Transform target;        // The object to orbit around
    public float distance = 5.0f;   // Distance from the target
    public float zoomSpeed = 2.0f;  // Zoom sensitivity
    public float minDistance = 1.0f;
    public float maxDistance = 2000.0f;

    public float sensitivity = 5.0f;
    public float yMinLimit = -180f;
    public float yMaxLimit = 180f;

    private float x = 0.0f;
    private float y = 0.0f;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;

        if (target == null)
        {
            // Default to world origin if no target is assigned
            GameObject tempTarget = new GameObject("Camera Target");
            tempTarget.transform.position = Vector3.zero;
            target = tempTarget.transform;
        }
    }

    void LateUpdate()
    {
        // Handle rotation
        if (Input.GetMouseButton(1)) // Right mouse button
        {
            x += Input.GetAxis("Mouse X") * sensitivity;
            y -= Input.GetAxis("Mouse Y") * sensitivity;

            // Optionally clamp Y rotation
            // y = Mathf.Clamp(y, yMinLimit, yMaxLimit);
        }

        // Handle zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0.0f)
        {
            distance -= scroll * zoomSpeed;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }

        // Calculate new position and rotation
        Quaternion rotation = Quaternion.Euler(y, x, 0);
        Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
        Vector3 position = rotation * negDistance + target.position;

        transform.rotation = rotation;
        transform.position = position;
    }
}
