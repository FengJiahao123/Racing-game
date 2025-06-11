using UnityEngine;

public class AICheckpointManager : MonoBehaviour
{
    private RaceManager raceManager;

    private GameObject[] aiCars;       // �洢 AI ����������
    private int aiCrossCount = 0;   // ��¼ AI ���յ�Ĵ���
                                    // �� AICheckpointManager ����ӷ���
    public int GetAiCrossCount()
    {
        return aiCrossCount;
    }

    void Start()
    {
        // ��ȡ AI ����
        aiCars = GameObject.FindGameObjectsWithTag("AI");

        // �Զ����� RaceManager ʵ��
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

    // �� AI �������յ�����ʱ����
    private void OnTriggerEnter(Collider other)
    {
        // ����Ƿ��� AI ���������յ�
        if (other.CompareTag("AI"))
        {
            aiCrossCount++;
            Debug.Log($"AI �����յ����: {aiCrossCount}");

            // �� AI �ﵽ TotalLaps ʱ��ֹͣ AI ����
            if (aiCrossCount >= raceManager.TotalLaps)
            {
                foreach (var aiCar in aiCars)
                {
                    var aiController = aiCar.GetComponent<CarController>();
                    if (aiController != null)
                    {
                        aiController.enabled = false;  // ֹͣ AI ����
                    }
                }
                Debug.Log("AI ��ɱ�����ֹͣ���ƣ�");
            }
        }
    }
}
