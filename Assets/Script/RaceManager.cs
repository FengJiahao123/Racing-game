using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceManager : MonoBehaviour
{
    public static RaceManager Instance { get; private set; }

    [Header("Race Settings")]
    public int TotalLaps = 3;
    [Header("Delay Seconds for Show/Hide")]
    public float checkpointDelay = 1.5f;
    [Header("Visible Checkpoint Object")]
    public GameObject postLapCheckpoint;
    [Header("All Checkpoint Scripts")]
    public Checkpoint[] allCheckpoints;
    [Header("Start/Finish ID")]
    public int startFinishId = 0;

    public int CurrentLap => Mathf.Max(0, crossCount - 1);
    public System.Action OnRaceCompleted;

    private int crossCount = 0;
    private HashSet<int> nextAllowed = new HashSet<int>();
    private HashSet<int> visitedThisLap = new HashSet<int>();

    // ¡ª¡ª New: For UI prompts
    private string warningMessage = "";
    private float warningTimer = 0f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (postLapCheckpoint != null)
            postLapCheckpoint.SetActive(true);

        nextAllowed.Clear();
        nextAllowed.Add(startFinishId);
        visitedThisLap.Clear();
    }

    private void Update()
    {
        // Warning countdown
        if (warningTimer > 0f)
            warningTimer -= Time.deltaTime;
    }

    private void OnGUI()
    {
        if (warningTimer > 0f && !string.IsNullOrEmpty(warningMessage))
        {
            // Create a new GUIStyle to center the text and set the font size
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 32
            };
            // Set the rectangle width to fill the screen, height 50, vertically centered
            Rect rect = new Rect(0, Screen.height / 2 - 25, Screen.width, 50);
            GUI.Label(rect, warningMessage, style);
        }
    }

    /// <summary>
    /// External call: Displays a warning message in the center of the screen, which disappears automatically after 2 seconds.
    /// </summary>
    public void ShowWarning(string msg)
    {
        warningMessage = msg;
        warningTimer = 2f;
    }

    public bool TryPassCheckpoint(int id)
    {
        if (!nextAllowed.Contains(id))
            return false;

        if (id == startFinishId)
        {
            crossCount++;
            Debug.Log($"[RaceManager] Crossing count: {crossCount}, Completed laps: {CurrentLap}/{TotalLaps}");

            if (crossCount == 1 && postLapCheckpoint != null)
                StartCoroutine(DelayedSetActive(postLapCheckpoint, false, checkpointDelay));
            else if (crossCount == TotalLaps && postLapCheckpoint != null)
                StartCoroutine(DelayedSetActive(postLapCheckpoint, true, checkpointDelay));
            else if (CurrentLap >= TotalLaps)
                OnRaceCompleted?.Invoke();

            // New lap starts, clear the visited set
            visitedThisLap.Clear();
        }

        visitedThisLap.Add(id);

        // Update the next allowed set
        var cp = System.Array.Find(allCheckpoints, c => c.id == id);
        nextAllowed = new HashSet<int>(cp.nextIds);

        return true;
    }

    public bool IsVisitedInCurrentLap(int id) => visitedThisLap.Contains(id);

    public void HandleIllegalReverse(int id, GameObject car)
    {
        Debug.LogWarning("[RaceManager] Reverse movement detected, forced correction!");

        // Stop the car
        var rb = car.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Trigger UI warning
        ShowWarning("Reverse movement is prohibited!");

        // Teleport and face the correct direction
        var cp = System.Array.Find(allCheckpoints, c => c.id == id);
        if (cp != null)
        {
            Vector3 baseDir = Vector3.ProjectOnPlane(cp.transform.forward, Vector3.up).normalized;
            Vector3 turnRight90 = Quaternion.AngleAxis(-90f, Vector3.up) * baseDir;
            car.transform.position = cp.transform.position + turnRight90 * 4f;
            car.transform.rotation = Quaternion.LookRotation(turnRight90, Vector3.up);
        }
    }

    private IEnumerator DelayedSetActive(GameObject target, bool activeFlag, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (target != null)
            target.SetActive(activeFlag);
    }
}