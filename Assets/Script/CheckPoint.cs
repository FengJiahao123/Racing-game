using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Checkpoint : MonoBehaviour
{
    [Tooltip("Ψһ ID���� 0 ��ʼ")]
    public int id;

    [Tooltip("�����˽ڵ����һ���ɴ����Ľڵ� ID �б�")]
    public int[] nextIds;

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // �����δ��Ǳ��ν���
        if (!triggered)
        {
            // ���ԺϷ�ͨ��
            bool passed = RaceManager.Instance.TryPassCheckpoint(id);
            if (passed)
            {
                // �Ϸ�ͨ������Ǳ��Ȿ���ظ�
                triggered = true;
            }
            else if (RaceManager.Instance.IsVisitedInCurrentLap(id))
            {
                // ���� nextAllowed ���ѷ��� => ����
                RaceManager.Instance.HandleIllegalReverse(id, other.gameObject);
            }
            // �Ȳ��Ϸ�Ҳδ���ʣ����ǿ����Ҵ�����ֱ�Ӻ��ԣ���������
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            triggered = false;
    }
}