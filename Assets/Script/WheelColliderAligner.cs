using UnityEngine;

public class WheelColliderAligner : MonoBehaviour
{
    [Header("Drag References")]
    public GameObject wheelModel;  // 3D model of the wheel (must have MeshRenderer)
    public WheelCollider wheelCollider; // Corresponding WheelCollider

    [Header("Debug Options")]
    public bool showDebugGizmos = true; // Show debug sphere
    public Color modelCenterColor = Color.green; // Color for the wheel model center
    public Color colliderPosColor = Color.red;   // Color for the WheelCollider position

    private bool hasBeenAligned = false; // Flag to indicate if alignment has been done

    // Perform alignment calculation on start
    void Start()
    {
        AlignWheelCollider(); // Call alignment function once on start
    }

    // Only calculate once on initialization to avoid updating every frame
    void AlignWheelCollider()
    {
        if (wheelModel == null || wheelCollider == null) return;

        // Get the exact geometric center of the wheel model (world coordinates)
        Renderer renderer = wheelModel.GetComponent<Renderer>();
        if (renderer == null)
        {
            Debug.LogError("Wheel model is missing MeshRenderer!");
            return;
        }

        Bounds bounds = renderer.bounds;
        Vector3 modelWorldCenter = bounds.center;

        // Calculate the local center point of the WheelCollider (considering suspension distance)
        Vector3 colliderCenter = wheelCollider.transform.InverseTransformPoint(modelWorldCenter);

        // Use positive value to fix Y-axis offset so that the WheelCollider's position aligns with the model's geometric center
        colliderCenter.y = colliderCenter.y + wheelCollider.suspensionDistance / 2f; // Fix Y-axis value

        // Fix: Ensure x and z coordinates are aligned to avoid offset
        colliderCenter.x = Mathf.Round(colliderCenter.x * 100f) / 100f; // Round to two decimal places
        colliderCenter.z = Mathf.Round(colliderCenter.z * 100f) / 100f; // Round to two decimal places

        // Apply the corrected center
        wheelCollider.center = colliderCenter;

        // Automatically calculate perfect radius (use model height as radius)
        wheelCollider.radius = Mathf.Round(bounds.extents.y * 100f) / 100f; // Round to two decimal places

        hasBeenAligned = true; // Mark as aligned after completion
    }

    // If necessary, the alignment can be recalculated through a button or other conditions
    void OnValidate()
    {
        // Re-align if necessary (for example, if properties were manually modified)
        if (!hasBeenAligned)
        {
            AlignWheelCollider(); // Align only if not yet aligned
        }
    }

    // Draw debug information
    void OnDrawGizmos()
    {
        if (!showDebugGizmos || wheelModel == null || wheelCollider == null) return;

        // Draw the wheel model center (green)
        Renderer renderer = wheelModel.GetComponent<Renderer>();
        if (renderer != null)
        {
            Gizmos.color = modelCenterColor;
            Gizmos.DrawSphere(renderer.bounds.center, 0.03f);
        }

        // Draw the WheelCollider's actual position (red)
        wheelCollider.GetWorldPose(out Vector3 colliderPos, out Quaternion _);
        Gizmos.color = colliderPosColor;
        Gizmos.DrawSphere(colliderPos, 0.03f);

        // Draw the connecting line (for easy offset viewing)
        Gizmos.color = Color.white;
        Gizmos.DrawLine(renderer.bounds.center, colliderPos);
    }
}
