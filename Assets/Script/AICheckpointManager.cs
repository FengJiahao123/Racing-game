using UnityEngine;

public class AICheckpointManager : MonoBehaviour
{
    private RaceManager raceManager;

    private GameObject[] aiCars;       // 存储 AI 车辆的引用
    private int aiCrossCount = 0;   // 记录 AI 过终点的次数
                                    // 在 AICheckpointManager 中添加方法
    public int GetAiCrossCount()
    {
        return aiCrossCount;
    }

    void Start()
    {
        // 获取 AI 车辆
        aiCars = GameObject.FindGameObjectsWithTag("AI");

        // 自动查找 RaceManager 实例
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

    // 当 AI 车碰到终点区域时触发
    private void OnTriggerEnter(Collider other)
    {
        // 检查是否是 AI 车辆经过终点
        if (other.CompareTag("AI"))
        {
            aiCrossCount++;
            Debug.Log($"AI 经过终点次数: {aiCrossCount}");

            // 当 AI 达到 TotalLaps 时，停止 AI 控制
            if (aiCrossCount >= raceManager.TotalLaps)
            {
                foreach (var aiCar in aiCars)
                {
                    var aiController = aiCar.GetComponent<CarController>();
                    if (aiController != null)
                    {
                        aiController.enabled = false;  // 停止 AI 控制
                    }
                }
                Debug.Log("AI 完成比赛，停止控制！");
            }
        }
    }
}
