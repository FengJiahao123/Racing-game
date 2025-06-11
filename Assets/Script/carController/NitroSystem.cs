using UnityEngine;

public class NitroSystem : MonoBehaviour
{
    [Header("Nitro Settings")]
    public float nitroBoost = 5000f;    // ��������ֵ
    public float nitroDuration = 5f;  // ��������ʱ��
    public float nitroCooldown = 3f;  // ������ȴʱ��
    public float nitroAccumulationRate = 0.1f; // ������������

    private bool isNitroActive = false; // �Ƿ񼤻��
    private float nitroTimeLeft = 0f;   // ʣ�൪��ʱ��
    private float cooldownTimeLeft = 0f; // ��ȴʱ��
    private float nitroAmount = 0f; // ������ǰ������
    private GameManager gameManager;
    void Start()
    {
        // �Զ����� RaceManager ʵ��
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }

    }
    void Update()
    {
        // ����������ڼ���״̬������ʣ��ʱ��
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
            // �����������ȴ�У�������ȴʱ��
            if (cooldownTimeLeft > 0f)
            {
                cooldownTimeLeft -= Time.deltaTime;
            }
        }

        // ֻ��Ư��ʱ�Ż��۵���
        if (ShouldAccumulateNitro())
        {
            AccumulateNitro();
        }

        // �������������ȴ�У����һ������㹻�ĵ��������� `N` ���������
        if (Input.GetKeyDown(KeyCode.N) && cooldownTimeLeft <= 0f && nitroAmount >= 1f)
        {
            ActivateNitro();
        }
    }

    // �����
    void ActivateNitro()
    {
        isNitroActive = true;
        nitroTimeLeft = nitroDuration;  // ���õ�������ʱ��
        cooldownTimeLeft = nitroCooldown; // ������ȴʱ��
        nitroAmount -= 1f; // ʹ��һ��������λ
    }

    // ֹͣ����
    void DeactivateNitro()
    {
        isNitroActive = false;
    }

    // �жϵ����Ƿ񼤻�
    public bool IsNitroActive()
    {
        return isNitroActive;
    }

    // ��������������ֻ�е�����С�ڵ��� 100 km/h �ҳ�������Ư��ʱ���ܵ���
    bool ShouldAccumulateNitro()
    {
        CarController carController = GetComponent<CarController>();
        if (carController != null)
        {
            // ֻ�е�����Ư��ʱ�Ż��ܵ���
            if (carController.isDrifting)
            {
                return true;
            }
        }

        return false;
    }

    // ���۵���
    void AccumulateNitro()
    {
        if (nitroAmount < 1f) // �����������
        {
            nitroAmount += nitroAccumulationRate * Time.deltaTime;
        }
    }

    // ��ȡ����ʣ�������ɹ�����UIʹ�ã�
    public float GetNitroAmount()
    {
        return nitroAmount;
    }

// �� NitroSystem ��ֱ�ӻ��Ƽ򵥵� UI
void OnGUI()
    {
        if (gameManager.raceEnded) return; // �������Ҫ��ʾ UI��ֱ���˳�
        // ��ʾ����״̬���Ƿ񼤻����
        GUI.Label(new Rect(10, 70, 200, 20), "Nitro Status: " + (isNitroActive ? "Active" : "Inactive"));

        // ��ʾ����ʣ�������
        GUI.HorizontalScrollbar(new Rect(10, 100, 200, 20), 0f, nitroAmount, 0f, 1f);

        // ��ʾ��ҵ����Ƿ����
        if (nitroAmount >= 1f)
        {
            GUI.Label(new Rect(10, 130, 200, 20), "Press 'N' to Activate Nitro");
        }
        else
        {
            GUI.Label(new Rect(10, 130, 200, 20), "Wait for Nitro to Charge");
        }
    }
}
