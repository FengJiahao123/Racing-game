using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public enum FollowMode
    {
        SpringPhysics,  // Spring physics simulation (with inertia)
        SmoothDamp      // Smooth interpolation (more stable)
    }

    [Header("Basic Settings")]
    public Transform target;                      // Target player vehicle
    public Vector3 offset = new Vector3(0, 3, -6); // Camera offset relative to the target
    public FollowMode mode = FollowMode.SmoothDamp; // Follow mode

    [Header("Spring Physics Parameters")]
    public float springStrength = 20f;            // Spring strength (recommended range 15~25)
    public float damping = 12f;                   // Damping coefficient (recommended range 10~15)
    private Vector3 physicsVelocity = Vector3.zero; // Physics velocity

    [Header("Smooth Interpolation Parameters")]
    public float smoothTime = 0.1f;               // Smooth time (smaller values make the follow faster)
    public float maxSpeed = Mathf.Infinity;       // Maximum follow speed
    private Vector3 smoothVelocity = Vector3.zero; // Smooth velocity

    [Header("Rotation Settings")]
    public float rotationSmooth = 5f;             // Rotation smoothness
    public bool lookAhead = true;                 // Whether to predict the vehicle's forward direction
    public float lookAheadDistance = 2f;          // Look ahead distance

    void Awake()
    {
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = 144;
    }

    void Start()
    {
        // If no target vehicle is manually set, automatically find the vehicle with the "Player" tag
        if (target == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                target = player.transform;  // Set the target to the player's selected vehicle
            }
        }

        // If the player's vehicle is not found, output a warning
        if (target == null)
        {
            Debug.LogWarning("Player vehicle not found. Make sure the vehicle has 'Player' tag.");
        }
    }

    void LateUpdate()
    {
        if (target == null) return;  // Exit update if there is no target

        // Calculate the target position (including offset)
        Vector3 desiredPosition = target.TransformPoint(offset);

        // Update camera position based on the selected mode
        switch (mode)
        {
            case FollowMode.SpringPhysics:
                // Spring physics simulation (with inertia)
                Vector3 displacement = desiredPosition - transform.position;
                Vector3 springForce = displacement * springStrength;
                Vector3 dampingForce = -physicsVelocity * damping;
                physicsVelocity += (springForce + dampingForce) * Time.deltaTime;
                transform.position += physicsVelocity * Time.deltaTime;
                break;

            case FollowMode.SmoothDamp:
                // Smooth interpolation (more stable)
                transform.position = Vector3.SmoothDamp(
                    transform.position,
                    desiredPosition,
                    ref smoothVelocity,
                    smoothTime,
                    maxSpeed,
                    Time.deltaTime
                );
                break;
        }

        // Calculate the target rotation direction
        Vector3 lookDirection = target.forward;
        if (lookAhead)
        {
            // Predict the vehicle's forward direction (reduces camera lag during sharp turns)
            lookDirection = (target.position + target.forward * lookAheadDistance) - transform.position;
        }

        Quaternion desiredRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            desiredRotation,
            rotationSmooth * Time.deltaTime
        );
    }

    void OnEnable()
    {
        // Initialize camera position and velocity
        if (target != null)
        {
            transform.position = target.TransformPoint(offset);
            transform.rotation = Quaternion.LookRotation(target.forward);
            physicsVelocity = Vector3.zero;
            smoothVelocity = Vector3.zero;
        }
    }

    // Debug helper: Display the follow offset in the Scene window
    void OnDrawGizmosSelected()
    {
        if (target != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(target.position, target.TransformPoint(offset));
            Gizmos.DrawWireSphere(target.TransformPoint(offset), 0.5f);
        }
    }
}
