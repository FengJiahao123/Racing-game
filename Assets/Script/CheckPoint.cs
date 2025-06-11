using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Checkpoint : MonoBehaviour
{
    [Tooltip("唯一 ID，从 0 开始")]
    public int id;

    [Tooltip("触发此节点后，下一步可触发的节点 ID 列表")]
    public int[] nextIds;

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // 如果尚未标记本次进入
        if (!triggered)
        {
            // 尝试合法通过
            bool passed = RaceManager.Instance.TryPassCheckpoint(id);
            if (passed)
            {
                // 合法通过，标记避免本次重复
                triggered = true;
            }
            else if (RaceManager.Instance.IsVisitedInCurrentLap(id))
            {
                // 不在 nextAllowed 且已访问 => 逆行
                RaceManager.Instance.HandleIllegalReverse(id, other.gameObject);
            }
            // 既不合法也未访问，则是跨点或乱触发，直接忽略，不算逆行
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            triggered = false;
    }
}