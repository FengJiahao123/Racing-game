using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Checkpoint : MonoBehaviour
{
    [Tooltip("Unique ID, starting from 0")]
    public int id;

    [Tooltip("List of the next checkpoint IDs that can be triggered after this one")]
    public int[] nextIds;

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // If this entry has not been marked yet
        if (!triggered)
        {
            // Try to pass the checkpoint legally
            bool passed = RaceManager.Instance.TryPassCheckpoint(id);
            if (passed)
            {
                // Passed legally, mark to prevent repeat entry
                triggered = true;
            }
            else if (RaceManager.Instance.IsVisitedInCurrentLap(id))
            {
                // Not allowed in nextAllowed and already visited => reverse direction
                RaceManager.Instance.HandleIllegalReverse(id, other.gameObject);
            }
            // If neither legal nor visited, it's a checkpoint skip or mis-trigger, simply ignore it, not considered reverse
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            triggered = false;
    }
}