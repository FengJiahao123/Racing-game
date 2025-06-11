using UnityEngine;

public class NitroSystem : MonoBehaviour
{
    [Header("Nitro Settings")]
    public float nitroBoost = 5000f;    // 氮气加速值
    public float nitroDuration = 5f;  // 氮气持续时间
    public float nitroCooldown = 3f;  // 氮气冷却时间
    public float nitroAccumulationRate = 0.1f; // 氮气积累速率

    private bool isNitroActive = false; // 是否激活氮气
    private float nitroTimeLeft = 0f;   // 剩余氮气时间
    private float cooldownTimeLeft = 0f; // 冷却时间
    private float nitroAmount = 0f; // 氮气当前积累量
    private GameManager gameManager;
    void Start()
    {
        // 自动查找 RaceManager 实例
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }

    }
    void Update()
    {
        // 如果氮气处于激活状态，减少剩余时间
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
            // 如果氮气在冷却中，减少冷却时间
            if (cooldownTimeLeft > 0f)
            {
                cooldownTimeLeft -= Time.deltaTime;
            }
        }

        // 只在漂移时才积累氮气
        if (ShouldAccumulateNitro())
        {
            AccumulateNitro();
        }

        // 如果氮气不在冷却中，并且积累有足够的氮气，按下 `N` 键来激活氮气
        if (Input.GetKeyDown(KeyCode.N) && cooldownTimeLeft <= 0f && nitroAmount >= 1f)
        {
            ActivateNitro();
        }
    }

    // 激活氮气
    void ActivateNitro()
    {
        isNitroActive = true;
        nitroTimeLeft = nitroDuration;  // 重置氮气持续时间
        cooldownTimeLeft = nitroCooldown; // 设置冷却时间
        nitroAmount -= 1f; // 使用一个氮气单位
    }

    // 停止氮气
    void DeactivateNitro()
    {
        isNitroActive = false;
    }

    // 判断氮气是否激活
    public bool IsNitroActive()
    {
        return isNitroActive;
    }

    // 氮气积累条件：只有当车速小于等于 100 km/h 且车辆正在漂移时积攒氮气
    bool ShouldAccumulateNitro()
    {
        CarController carController = GetComponent<CarController>();
        if (carController != null)
        {
            // 只有当车辆漂移时才积攒氮气
            if (carController.isDrifting)
            {
                return true;
            }
        }

        return false;
    }

    // 积累氮气
    void AccumulateNitro()
    {
        if (nitroAmount < 1f) // 最大氮气量限制
        {
            nitroAmount += nitroAccumulationRate * Time.deltaTime;
        }
    }

    // 获取氮气剩余量（可供其他UI使用）
    public float GetNitroAmount()
    {
        return nitroAmount;
    }

// 在 NitroSystem 中直接绘制简单的 UI
void OnGUI()
    {
        if (gameManager.raceEnded) return; // 如果不需要显示 UI，直接退出
        // 显示氮气状态（是否激活氮气）
        GUI.Label(new Rect(10, 70, 200, 20), "Nitro Status: " + (isNitroActive ? "Active" : "Inactive"));

        // 显示氮气剩余进度条
        GUI.HorizontalScrollbar(new Rect(10, 100, 200, 20), 0f, nitroAmount, 0f, 1f);

        // 提示玩家氮气是否可用
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
