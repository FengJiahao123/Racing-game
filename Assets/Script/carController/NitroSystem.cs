using UnityEngine;

public class NitroSystem : MonoBehaviour
{
    [Header("Nitro Settings")]
    public float nitroBoost = 5000f;    // Nitro boost value
    public float nitroDuration = 5f;  // Nitro duration time
    public float nitroCooldown = 3f;  // Nitro cooldown time
    public float nitroAccumulationRate = 0.1f; // Nitro accumulation rate

    private bool isNitroActive = false; // Whether Nitro is active
    private float nitroTimeLeft = 0f;   // Remaining Nitro time
    private float cooldownTimeLeft = 0f; // Cooldown time
    private float nitroAmount = 0f; // Current accumulated Nitro amount
    private GameManager gameManager;
    void Start()
    {
        // Automatically find the RaceManager instance
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }

    }
    void Update()
    {
        // If Nitro is active, decrease the remaining time
        if (isNitroActive)
        {
            nitroTimeLeft -= Time.deltaTime;
            if (nitroTimeLeft <= 0f)
            {
                DeactivateNitro();
            }
        }
        else
        {
            // If Nitro is on cooldown, decrease the cooldown time
            if (cooldownTimeLeft > 0f)
            {
                cooldownTimeLeft -= Time.deltaTime;
            }
        }

        // Only accumulate Nitro when drifting
        if (ShouldAccumulateNitro())
        {
            AccumulateNitro();
        }

        // If Nitro is not on cooldown and enough Nitro is accumulated, press 'N' to activate Nitro
        if (Input.GetKeyDown(KeyCode.N) && cooldownTimeLeft <= 0f && nitroAmount >= 1f)
        {
            ActivateNitro();
        }
    }

    // Activate Nitro
    void ActivateNitro()
    {
        isNitroActive = true;
        nitroTimeLeft = nitroDuration;  // Reset Nitro duration
        cooldownTimeLeft = nitroCooldown; // Set cooldown time
        nitroAmount -= 1f; // Use one Nitro unit
    }

    // Deactivate Nitro
    void DeactivateNitro()
    {
        isNitroActive = false;
    }

    // Check if Nitro is active
    public bool IsNitroActive()
    {
        return isNitroActive;
    }

    // Nitro accumulation condition: Nitro is accumulated only when the speed is less than or equal to 100 km/h and the car is drifting
    bool ShouldAccumulateNitro()
    {
        CarController carController = GetComponent<CarController>();
        if (carController != null)
        {
            // Accumulate Nitro only when the car is drifting
            if (carController.isDrifting)
            {
                return true;
            }
        }

        return false;
    }

    // Accumulate Nitro
    void AccumulateNitro()
    {
        if (nitroAmount < 1f) // Max Nitro amount limit
        {
            nitroAmount += nitroAccumulationRate * Time.deltaTime;
        }
    }

    // Get the remaining Nitro amount (for use in other UI)
    public float GetNitroAmount()
    {
        return nitroAmount;
    }

    // Simple UI to display Nitro status
    void OnGUI()
    {
        if (gameManager.raceEnded) return; // If no UI is needed, exit immediately
        // Display Nitro status (whether Nitro is active)
        GUI.Label(new Rect(10, 150, 200, 20), "Nitro Status: " + (isNitroActive ? "Active" : "Inactive"));

        // Display Nitro remaining progress bar
        GUI.HorizontalScrollbar(new Rect(10, 180, 200, 20), 0f, nitroAmount, 0f, 1f);

        // Prompt the player whether Nitro is available
        if (nitroAmount >= 1f)
        {
            GUI.Label(new Rect(10, 210, 200, 20), "Press 'N' to Activate Nitro");
        }
        else
        {
            GUI.Label(new Rect(10, 210, 200, 20), "Wait for Nitro to Charge");
        }
    }
}
