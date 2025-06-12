using UnityEngine;

public class AICheckpointManager : MonoBehaviour
{
    private RaceManager raceManager;

    private GameObject[] aiCars;       // Store references to AI cars
    private int aiCrossCount = 0;   // Track the number of times AI crosses the finish line
                                    // Add method in AICheckpointManager
    public int GetAiCrossCount()
    {
        return aiCrossCount;
    }

    void Start()
    {
        // Get AI cars
        aiCars = GameObject.FindGameObjectsWithTag("AI");

        // Automatically find the RaceManager instance
        if (raceManager == null)
        {
            raceManager = FindObjectOfType<RaceManager>();
        }
        if (raceManager == null)
        {
            Debug.LogError("RaceManager reference is missing!");
        }

    }

    void Update()
    {

    }

    // Triggered when AI car crosses the finish line area
    private void OnTriggerEnter(Collider other)
    {
        // Check if it's an AI car crossing the finish line
        if (other.CompareTag("AI"))
        {
            aiCrossCount++;
            Debug.Log($"AI crossed the finish line {aiCrossCount} times");

            // Stop AI control when AI reaches TotalLaps
            if (aiCrossCount >= raceManager.TotalLaps)
            {
                foreach (var aiCar in aiCars)
                {
                    var aiController = aiCar.GetComponent<CarController>();
                    if (aiController != null)
                    {
                        aiController.enabled = false;  // Stop AI control
                    }
                }
                Debug.Log("AI completed the race, stopping control!");
            }
        }
    }
}