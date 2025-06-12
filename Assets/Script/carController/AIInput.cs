using UnityEngine;

public class AIInput : MonoBehaviour, IDriverInput
{
    private GameManager gameManager;
    [Header("Waypoint")]
    public Transform[] waypoints;
    public float reachRadius = 5f; // How close to the target point is considered "reached"
    public float targetSpeedKPH = 180f;  // AI target speed in km/h
    public float maxSteerAngle = 30f;    // Maximum steering angle

    private int currentWP = 0;  // Current waypoint index
    private int previousWP = 0; // Previous waypoint index
    private Rigidbody rb;  // Vehicle's Rigidbody component

    private bool isAccelerating = false; // Whether it is accelerating
    private float speedIncreaseTime = 0f; // Acceleration time

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    void Start()
    {
        // Automatically find the RaceManager instance
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }
        if (gameManager == null)
        {
            Debug.LogError("RaceManager reference is missing!");
        }
        targetSpeedKPH = gameManager.speed;
    }

    public float Throttle
    {
        get
        {
            // Current speed (km/h)
            float speed = rb.velocity.magnitude * 3.6f;

            // Check if we need to accelerate to 220 km/h
            if (currentWP == 18 || currentWP == 40)
            {
                // If we've reached waypoints 18 or 40, start accelerating
                isAccelerating = true;
                speedIncreaseTime = 5f; // Accelerate over 5 seconds
                targetSpeedKPH = 220f;  // Accelerate to 220 km/h
            }

            // Linear deceleration: the closer the speed is to the target, the smaller the throttle
            return Mathf.Clamp01((targetSpeedKPH - speed) / targetSpeedKPH);
        }
    }

    public float Steer
    {
        get
        {
            // Get the current target waypoint position
            Vector3 targetPosition = waypoints[currentWP].position;

            // Calculate the direction from the vehicle's current position to the target waypoint
            Vector3 toTarget = targetPosition - transform.position;
            Vector3 dirFlat = Vector3.ProjectOnPlane(toTarget, Vector3.up).normalized;

            // Calculate the angle error between the vehicle's current forward direction and the target direction
            float angleError = Vector3.SignedAngle(transform.forward, dirFlat, Vector3.up);

            // If we've reached the target, switch to the next waypoint
            if (toTarget.magnitude < reachRadius)
            {
                previousWP = currentWP; // Update the previous waypoint
                currentWP = (currentWP + 1) % waypoints.Length;  // Cycle through the waypoints
            }

            // Return the steering angle (-1 to 1)
            return Mathf.Clamp(angleError / maxSteerAngle, -1f, 1f);
        }
    }

    public bool Brake => false;     // Simple AI, no need for additional braking
    public bool Handbrake => false; // No handbrake (drifting)

    // Collision detection: When AI hits the wall and the speed is less than 20, correct the position
    private void OnCollisionEnter(Collision collision)
    {
        // Check if it's a wall (you can customize this based on your scene)
        if (collision.gameObject.CompareTag("Wall"))
        {
            // If speed is less than 20 km/h, perform position correction
            if (rb.velocity.magnitude * 3.6f < 20f)
            {
                CorrectPosition();
                Debug.Log($"[AI] Hit the wall and speed is less than 20, returning to waypoint {previousWP}");
            }
        }
    }

    // Correct the position when speed is less than 20 and collision occurs
    private void CorrectPosition()
    {
        // Get the position of the previous waypoint
        Vector3 correctedPosition = waypoints[previousWP].position;
        // Increase the height on the Y axis to prevent clipping and falling
        correctedPosition.y += 1f;  // For example, adjust height +1

        // Adjust the AI vehicle's position to the new corrected position
        transform.position = correctedPosition;

        // Force the AI vehicle's direction to face the previous waypoint
        Vector3 directionToPreviousWP = waypoints[previousWP].position - transform.position;
        transform.rotation = Quaternion.LookRotation(directionToPreviousWP);  // Direction correction

        rb.velocity = Vector3.zero; // Reset velocity
    }

    void Update()
    {
        // Timing for acceleration
        if (isAccelerating)
        {
            speedIncreaseTime -= Time.deltaTime;
            if (speedIncreaseTime <= 0f)
            {
                targetSpeedKPH = gameManager.speed;  // Return to normal speed
                isAccelerating = false;  // End acceleration
                Debug.Log("[AI] Acceleration completed, returning to normal speed");
            }
        }
    }
}
