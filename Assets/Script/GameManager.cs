using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Vehicle Spawning and UI References")]
    public Transform spawnPoint;          // Player vehicle spawn position
    public Transform aiSpawnPoint;        // AI vehicle spawn position
    public GameObject[] carPrefabs;      // Player and AI vehicle prefabs
    public TextMeshProUGUI countdownText;    // Countdown text
    public TextMeshProUGUI gameTimeText;      // Game time text
    public TextMeshProUGUI lapText;          // Current lap display text
    public TextMeshProUGUI speedText;        // Speed display text
    public float speed = 0;
    [Header("End Race UI")]
    [Tooltip("End panel displayed after completing required laps")]
    public GameObject finishPanel;            // Drag the end panel into the Inspector
    public TextMeshProUGUI finishTimeText;    // Display "Congratulations! Time: xx:xx:xxx"
    public Button restartButton;              // Restart button
    public Button mainMenuButton;             // Main menu button

    private float countdownTime = 5f;         // Countdown duration
    private bool countdownActive = false;     // Is countdown active
    private GameObject playerCar;             // Player vehicle
    private bool canControlCar = false;       // Whether the player can control the car
    private float gameTime = 0f;              // Elapsed game time
    public bool raceEnded = false;           // Has the race ended
    private CarController carController;      // Reference to the player's CarController
                                              // Manually specify the waypoints
    public Transform[] waypoints;  // Expose waypoints array and manually drag into the Inspector

    private RaceManager raceManager;          // Used to listen to the race end event

    public GameObject PlayerCar
    {
        get { return playerCar; }
    }

    void Start()
    {
        // 1. Spawn the player vehicle
        int selectedCar = PlayerPrefs.GetInt("SelectedCarIndex", 0);
        playerCar = Instantiate(carPrefabs[selectedCar], spawnPoint.position, spawnPoint.rotation);

        // Tag the player vehicle as "Player"
        playerCar.tag = "Player";
        // Set the root node Layer to "Player"
        int playerLayerIndex = LayerMask.NameToLayer("Player");
        playerCar.layer = playerLayerIndex;

        // 2. Spawn AI vehicles through code, using aiSpawnPoint position
        if (carPrefabs.Length > 2)  // Assume carPrefabs[2] is the AI vehicle
        {
            GameObject aiCar = Instantiate(carPrefabs[2], aiSpawnPoint.position, aiSpawnPoint.rotation);
            aiCar.tag = "AI";  // Set AI vehicle tag
            aiCar.layer = LayerMask.NameToLayer("AI");
            // Add control script to the AI vehicle
            AIInput aiInput = aiCar.GetComponent<AIInput>();
            if (aiInput != null)
            {
                // Dynamically get waypoints and assign to AIInput
                aiInput.waypoints = waypoints;  // Get all waypoints
            }
            // Disable AI vehicle's CarController until the countdown is over
            CarController aiCarController = aiCar.GetComponent<CarController>();
            if (aiCarController != null)
            {
                aiCarController.enabled = false;  // Disable AI vehicle control
            }
        }
        // 2. Get the CarController component of the vehicle
        carController = playerCar.GetComponent<CarController>();

        // 3. Make the main camera follow the player vehicle
        CameraFollow cameraFollow = Camera.main.GetComponent<CameraFollow>();
        if (cameraFollow != null)
        {
            cameraFollow.target = playerCar.transform;
        }

        // 4. Initially disallow player control
        canControlCar = false;
        raceEnded = false;

        // 5. Start the countdown right away
        countdownActive = true;
        countdownText.gameObject.SetActive(true);

        // 6. Find RaceManager and subscribe to the race end event
        raceManager = FindObjectOfType<RaceManager>();
        if (raceManager != null)
        {
            raceManager.OnRaceCompleted += HandleRaceEnd;
        }
        else
        {
            Debug.LogWarning("[GameManager] RaceManager not found, race end will not be triggered.");
        }

        // 7. Ensure the finish panel is initially hidden
        if (finishPanel != null)
        {
            finishPanel.SetActive(false);
        }

        // 8. Hide the game time text
        if (gameTimeText != null)
        {
            gameTimeText.gameObject.SetActive(false);
        }

        // 9. Bind button callbacks
        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartButtonClicked);
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
    }

    void Update()
    {
        // 1. Countdown phase
        if (countdownActive && !raceEnded)
        {
            countdownTime -= Time.deltaTime;
            countdownText.text = Mathf.Ceil(countdownTime).ToString();

            if (countdownTime <= 0f)
            {
                countdownActive = false;
                countdownText.text = "Go!";
                countdownText.gameObject.SetActive(false);

                canControlCar = true;
                StartGameTimer();
                // After the countdown ends, enable AI vehicle control
                EnableAIControl();
            }
        }

        // 2. If no countdown is active and control hasn't started, press space to restart the countdown (if the race isn't over)
        if (!countdownActive && !canControlCar && !raceEnded && Input.GetKeyDown(KeyCode.Space))
        {
            StartCountdown();
        }

        // 3. Player control and game clock (if the race hasn't ended)
        if (canControlCar && !raceEnded)
        {
            // Enable vehicle control script
            if (playerCar != null)
            {
                var controller = playerCar.GetComponent<CarController>();
                if (controller != null && !controller.enabled)
                {
                    controller.enabled = true;
                }
            }

            // Update game clock text
            gameTime += Time.deltaTime;
            int minutes = Mathf.FloorToInt(gameTime / 60f);
            int seconds = Mathf.FloorToInt(gameTime % 60f);
            int milliseconds = Mathf.FloorToInt((gameTime * 1000f) % 1000f);
            gameTimeText.text = string.Format("{0:D2}:{1:D2}:{2:D3}", minutes, seconds, milliseconds);

            // Display current lap count
            if (lapText != null)
            {
                lapText.text = $"Lap: {raceManager.CurrentLap}/{raceManager.TotalLaps}";
            }

            // Display current speed
            if (speedText != null && carController != null)
            {
                speedText.text = $"Speed: {carController.KPH:F2} km/h";
            }
        }
    }

    /// <summary>
    /// Reset and restart the countdown
    /// </summary>
    void StartCountdown()
    {
        countdownTime = 5f;
        countdownActive = true;
        canControlCar = false;
        countdownText.gameObject.SetActive(true);
    }

    /// <summary>
    /// Start the game timer
    /// </summary>
    void StartGameTimer()
    {
        gameTime = 0f;
        gameTimeText.gameObject.SetActive(true);
    }

    /// <summary>
    /// Called when RaceManager notifies that the race is over
    /// </summary>
    private void HandleRaceEnd()
    {
        Debug.Log("[GameManager] Race end notification received");

        // Mark the race as ended
        raceEnded = true;

        // Disable player control and close the CarController
        canControlCar = false;
        if (playerCar != null)
        {
            var controller = playerCar.GetComponent<CarController>();
            if (controller != null)
                controller.enabled = false;  // Stop player control
        }

        // Disable AI control and close the CarController
        GameObject[] aiCars = GameObject.FindGameObjectsWithTag("AI");
        foreach (var aiCar in aiCars)
        {
            var aiController = aiCar.GetComponent<CarController>();
            var aiinput = aiCar.GetComponent<AIInput>();
            if (aiController != null)
            {
                aiController.enabled = false;  // Stop AI control
                aiinput.enabled = false;
            }
        }

        // Get the number of laps completed by AI
        AICheckpointManager aiCheckpointManager = FindObjectOfType<AICheckpointManager>();
        if (aiCheckpointManager != null)
        {
            int aiCrossCount = aiCheckpointManager.GetAiCrossCount(); // Get the number of laps completed by AI

            // Get the number of laps completed by the player
            int playerCompletedLaps = raceManager.CurrentLap;

            // Determine if the player and AI have completed the race and display appropriate information
            if (playerCompletedLaps >= raceManager.TotalLaps)
            {
                // If both AI and the player completed the race
                if (aiCrossCount >= raceManager.TotalLaps)
                {
                    // Player lost, show failure message
                    if (finishTimeText != null)
                    {
                        finishTimeText.text = $"You lost! Your time is {GetFormattedTime()}";
                    }
                    Debug.Log("You lost! Better luck next time.");
                }
                else
                {
                    // Get the player's progress
                    string selectedMap = PlayerPrefs.GetString("SelectedMap", "Map1");
                    SaveManager.SaveProgress(selectedMap);  // Save current level progress
                    // Player won, show success message
                    if (finishTimeText != null)
                    {
                        finishTimeText.text = $"You win! Your time is {GetFormattedTime()}";
                    }
                    Debug.Log("Congratulations! You won the race.");
                }
            }
        }

        // Show the finish panel
        if (finishPanel != null)
        {
            finishPanel.SetActive(true);
        }
    }

    // Helper function to format time
    private string GetFormattedTime()
    {
        int minutes = Mathf.FloorToInt(gameTime / 60f);
        int seconds = Mathf.FloorToInt(gameTime % 60f);
        int milliseconds = Mathf.FloorToInt((gameTime * 1000f) % 1000f);
        return $"{minutes:D2}:{seconds:D2}:{milliseconds:D3}";
    }
    /// <summary>
    /// "Play Again" button callback: reload the current scene
    /// </summary>
    private void OnRestartButtonClicked()
    {
        if (!raceEnded) return;
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

    /// <summary>
    /// "Main Menu" button callback: load the main menu scene
    /// </summary>
    private void OnMainMenuButtonClicked()
    {
        if (!raceEnded) return;
        // Assuming the main menu scene is called "MainMenu"
        SceneManager.LoadScene("MainMenuScene");
    }

    private void OnDestroy()
    {
        // Unsubscribe from the race end event
        if (raceManager != null)
        {
            raceManager.OnRaceCompleted -= HandleRaceEnd;
        }
    }
    // Enable AI control
    void EnableAIControl()
    {
        // Find all AI vehicles and enable their control
        GameObject[] aiCars = GameObject.FindGameObjectsWithTag("AI");
        foreach (var aiCar in aiCars)
        {
            CarController aiCarController = aiCar.GetComponent<CarController>();
            if (aiCarController != null)
            {
                aiCarController.enabled = true;  // Enable AI vehicle control
            }
        }
    }
}