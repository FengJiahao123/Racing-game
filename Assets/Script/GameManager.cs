using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("车辆生成与UI引用")]
    public Transform spawnPoint;          // 玩家车辆的生成位置
    public Transform aiSpawnPoint;        // AI 车辆的生成位置
    public GameObject[] carPrefabs;      // 玩家车和AI车的预制体
    public TextMeshProUGUI countdownText;    // 倒计时文本
    public TextMeshProUGUI gameTimeText;      // 游戏时间文本
    public TextMeshProUGUI lapText;          // 显示当前圈数的文本
    public TextMeshProUGUI speedText;        // 显示速度的文本
    public float speed = 0;
    [Header("比赛结束 UI")]
    [Tooltip("跑完所需圈数后显示的结束面板")]
    public GameObject finishPanel;            // 在 Inspector 中拖入结束面板
    public TextMeshProUGUI finishTimeText;    // 显示“恭喜你 用时 xx:xx:xxx”
    public Button restartButton;              // 再来一局按钮
    public Button mainMenuButton;             // 回主菜单按钮

    private float countdownTime = 5f;         // 倒计时时长
    private bool countdownActive = false;     // 是否倒计时中
    private GameObject playerCar;             // 玩家车辆
    private bool canControlCar = false;       // 是否允许玩家控制
    private float gameTime = 0f;              // 已经过的游戏时间
    public bool raceEnded = false;           // 比赛是否已经结束
    private CarController carController;      // 引用玩家的 CarController
                                              // 手动指定路径点
    public Transform[] waypoints;  // 将路径点数组公开，手动拖入 Inspector

    private RaceManager raceManager;          // 用来监听比赛结束事件

    public GameObject PlayerCar
    {
        get { return playerCar; }
    }

    void Start()
    {
        // 1. 生成玩家车辆
        int selectedCar = PlayerPrefs.GetInt("SelectedCarIndex", 0);
        playerCar = Instantiate(carPrefabs[selectedCar], spawnPoint.position, spawnPoint.rotation);

        // 给玩家车辆打上 Tag="Player"
        playerCar.tag = "Player";
        // 将根节点 Layer 设置为 "Player"
        int playerLayerIndex = LayerMask.NameToLayer("Player");
        playerCar.layer = playerLayerIndex;

        // 2. 通过代码生成 AI 车，使用 aiSpawnPoint 位置
        if (carPrefabs.Length > 2)  // 假设 carPrefabs[2] 是 AI 车
        {
            GameObject aiCar = Instantiate(carPrefabs[2], aiSpawnPoint.position, aiSpawnPoint.rotation);
            aiCar.tag = "AI";  // 设置为 AI 车辆 Tag
            aiCar.layer = LayerMask.NameToLayer("AI");
            // 给 AI 车添加控制脚本
            AIInput aiInput = aiCar.GetComponent<AIInput>();
            if (aiInput != null)
            {
                // 动态获取路径点并赋值给 AIInput
                aiInput.waypoints = waypoints;  // 获取所有路径点
            }
            // 禁用 AI 车的 CarController，直到倒计时结束后启用
            CarController aiCarController = aiCar.GetComponent<CarController>();
            if (aiCarController != null)
            {
                aiCarController.enabled = false;  // 禁用 AI 车的控制
            }
        }
        // 2. 获取车辆的 CarController 组件
        carController = playerCar.GetComponent<CarController>();

        // 3. 让主摄像机跟随玩家车辆
        CameraFollow cameraFollow = Camera.main.GetComponent<CameraFollow>();
        if (cameraFollow != null)
        {
            cameraFollow.target = playerCar.transform;
        }

        // 4. 初始阶段不允许玩家控制
        canControlCar = false;
        raceEnded = false;

        // 5. 一开始就启动倒计时
        countdownActive = true;
        countdownText.gameObject.SetActive(true);

        // 6. 找到 RaceManager 幯订阅比赛结束事件
        raceManager = FindObjectOfType<RaceManager>();
        if (raceManager != null)
        {
            raceManager.OnRaceCompleted += HandleRaceEnd;
        }
        else
        {
            Debug.LogWarning("[GameManager] 未找到 RaceManager，比赛结束将无法触发。");
        }

        // 7. 确保结束面板初始隐藏
        if (finishPanel != null)
        {
            finishPanel.SetActive(false);
        }

        // 8. 隐藏游戏时间文本
        if (gameTimeText != null)
        {
            gameTimeText.gameObject.SetActive(false);
        }

        // 9. 给按钮绑定回调
        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartButtonClicked);
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
    }

    void Update()
    {
        // 1. 倒计时阶段
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
                // 倒计时结束后启用 AI 车的控制
                EnableAIControl();
            }
        }

        // 2. 如果未倒计时且尚未开始控制，按空格可重新启动倒计时（比赛未结束时）
        if (!countdownActive && !canControlCar && !raceEnded && Input.GetKeyDown(KeyCode.Space))
        {
            StartCountdown();
        }

        // 3. 玩家控制及游戏时钟（比赛未结束时）
        if (canControlCar && !raceEnded)
        {
            // 启动车辆控制脚本
            if (playerCar != null)
            {
                var controller = playerCar.GetComponent<CarController>();
                if (controller != null && !controller.enabled)
                {
                    controller.enabled = true;
                }
            }

            // 更新游戏时钟文本
            gameTime += Time.deltaTime;
            int minutes = Mathf.FloorToInt(gameTime / 60f);
            int seconds = Mathf.FloorToInt(gameTime % 60f);
            int milliseconds = Mathf.FloorToInt((gameTime * 1000f) % 1000f);
            gameTimeText.text = string.Format("{0:D2}:{1:D2}:{2:D3}", minutes, seconds, milliseconds);

            // 显示当前圈数
            if (lapText != null)
            {
                lapText.text = $"Lap: {raceManager.CurrentLap}/{raceManager.TotalLaps}";
            }

            // 显示当前速度
            if (speedText != null && carController != null)
            {
                speedText.text = $"Speed: {carController.KPH:F2} km/h";
            }
        }
    }

    /// <summary>
    /// 重置并重新启动倒计时
    /// </summary>
    void StartCountdown()
    {
        countdownTime = 5f;
        countdownActive = true;
        canControlCar = false;
        countdownText.gameObject.SetActive(true);
    }

    /// <summary>
    /// 启动游戏时钟
    /// </summary>
    void StartGameTimer()
    {
        gameTime = 0f;
        gameTimeText.gameObject.SetActive(true);
    }

    /// <summary>
    /// RaceManager 通知比赛结束后调用此方法
    /// </summary>
    private void HandleRaceEnd()
    {
        Debug.Log("[GameManager] 收到比赛结束通知");

        // 标记比赛已结束
        raceEnded = true;

        // 禁止玩家继续控制并关闭 CarController
        canControlCar = false;
        if (playerCar != null)
        {
            var controller = playerCar.GetComponent<CarController>();
            if (controller != null)
                controller.enabled = false;  // 停止玩家控制
        }

        // 禁止 AI 继续控制并关闭 CarController
        GameObject[] aiCars = GameObject.FindGameObjectsWithTag("AI");
        foreach (var aiCar in aiCars)
        {
            var aiController = aiCar.GetComponent<CarController>();
            var aiinput = aiCar.GetComponent<AIInput>();
            if (aiController != null)
            {
                aiController.enabled = false;  // 停止 AI 控制
                aiinput.enabled = false;
            }
        }

        // 获取 AI 完成的圈数
        AICheckpointManager aiCheckpointManager = FindObjectOfType<AICheckpointManager>();
        if (aiCheckpointManager != null)
        {
            int aiCrossCount = aiCheckpointManager.GetAiCrossCount(); // 获取 AI 完成的圈数

            // 获取玩家的已完成圈数
            int playerCompletedLaps = raceManager.CurrentLap;

            // 判断玩家和 AI 是否完成比赛，并显示相应信息
            if (playerCompletedLaps >= raceManager.TotalLaps)
            {
                // 如果 AI 完成比赛并且玩家完成比赛
                if (aiCrossCount >= raceManager.TotalLaps)
                {
                    // 玩家输了，显示失败消息
                    if (finishTimeText != null)
                    {
                        finishTimeText.text = $"You lost! Your time is {GetFormattedTime()}";
                    }
                    Debug.Log("You lost! Better luck next time.");
                }
                else
                {
                    // 玩家赢了，显示成功消息
                    if (finishTimeText != null)
                    {
                        finishTimeText.text = $"You win! Your time is {GetFormattedTime()}";
                    }
                    Debug.Log("Congratulations! You won the race.");
                }
            }
        }

        // 显示结束面板
        if (finishPanel != null)
        {
            finishPanel.SetActive(true);
        }
    }

    // 用于格式化时间的辅助函数
    private string GetFormattedTime()
    {
        int minutes = Mathf.FloorToInt(gameTime / 60f);
        int seconds = Mathf.FloorToInt(gameTime % 60f);
        int milliseconds = Mathf.FloorToInt((gameTime * 1000f) % 1000f);
        return $"{minutes:D2}:{seconds:D2}:{milliseconds:D3}";
    }
    /// <summary>
    /// “再来一局” 按钮回调：重新加载当前场景
    /// </summary>
    private void OnRestartButtonClicked()
    {
        if (!raceEnded) return;
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

    /// <summary>
    /// “回主菜单” 按钮回调：加载主菜单场景
    /// </summary>
    private void OnMainMenuButtonClicked()
    {
        if (!raceEnded) return;
        // 假设主菜单场景名为 "MainMenu"
        SceneManager.LoadScene("MainMenuScene");
    }

    private void OnDestroy()
    {
        // 反注册比赛结束事件
        if (raceManager != null)
        {
            raceManager.OnRaceCompleted -= HandleRaceEnd;
        }
    }
    // 启用 AI 控制
    void EnableAIControl()
    {
        // 找到所有的 AI 车并启用它们的控制
        GameObject[] aiCars = GameObject.FindGameObjectsWithTag("AI");
        foreach (var aiCar in aiCars)
        {
            CarController aiCarController = aiCar.GetComponent<CarController>();
            if (aiCarController != null)
            {
                aiCarController.enabled = true;  // 启用 AI 车的控制
            }
        }
    }
}