using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("����������UI����")]
    public Transform spawnPoint;          // ��ҳ���������λ��
    public Transform aiSpawnPoint;        // AI ����������λ��
    public GameObject[] carPrefabs;      // ��ҳ���AI����Ԥ����
    public TextMeshProUGUI countdownText;    // ����ʱ�ı�
    public TextMeshProUGUI gameTimeText;      // ��Ϸʱ���ı�
    public TextMeshProUGUI lapText;          // ��ʾ��ǰȦ�����ı�
    public TextMeshProUGUI speedText;        // ��ʾ�ٶȵ��ı�
    public float speed = 0;
    [Header("�������� UI")]
    [Tooltip("��������Ȧ������ʾ�Ľ������")]
    public GameObject finishPanel;            // �� Inspector ������������
    public TextMeshProUGUI finishTimeText;    // ��ʾ����ϲ�� ��ʱ xx:xx:xxx��
    public Button restartButton;              // ����һ�ְ�ť
    public Button mainMenuButton;             // �����˵���ť

    private float countdownTime = 5f;         // ����ʱʱ��
    private bool countdownActive = false;     // �Ƿ񵹼�ʱ��
    private GameObject playerCar;             // ��ҳ���
    private bool canControlCar = false;       // �Ƿ�������ҿ���
    private float gameTime = 0f;              // �Ѿ�������Ϸʱ��
    public bool raceEnded = false;           // �����Ƿ��Ѿ�����
    private CarController carController;      // ������ҵ� CarController
                                              // �ֶ�ָ��·����
    public Transform[] waypoints;  // ��·�������鹫�����ֶ����� Inspector

    private RaceManager raceManager;          // �����������������¼�

    public GameObject PlayerCar
    {
        get { return playerCar; }
    }

    void Start()
    {
        // 1. ������ҳ���
        int selectedCar = PlayerPrefs.GetInt("SelectedCarIndex", 0);
        playerCar = Instantiate(carPrefabs[selectedCar], spawnPoint.position, spawnPoint.rotation);

        // ����ҳ������� Tag="Player"
        playerCar.tag = "Player";
        // �����ڵ� Layer ����Ϊ "Player"
        int playerLayerIndex = LayerMask.NameToLayer("Player");
        playerCar.layer = playerLayerIndex;

        // 2. ͨ���������� AI ����ʹ�� aiSpawnPoint λ��
        if (carPrefabs.Length > 2)  // ���� carPrefabs[2] �� AI ��
        {
            GameObject aiCar = Instantiate(carPrefabs[2], aiSpawnPoint.position, aiSpawnPoint.rotation);
            aiCar.tag = "AI";  // ����Ϊ AI ���� Tag
            aiCar.layer = LayerMask.NameToLayer("AI");
            // �� AI ����ӿ��ƽű�
            AIInput aiInput = aiCar.GetComponent<AIInput>();
            if (aiInput != null)
            {
                // ��̬��ȡ·���㲢��ֵ�� AIInput
                aiInput.waypoints = waypoints;  // ��ȡ����·����
            }
            // ���� AI ���� CarController��ֱ������ʱ����������
            CarController aiCarController = aiCar.GetComponent<CarController>();
            if (aiCarController != null)
            {
                aiCarController.enabled = false;  // ���� AI ���Ŀ���
            }
        }
        // 2. ��ȡ������ CarController ���
        carController = playerCar.GetComponent<CarController>();

        // 3. ���������������ҳ���
        CameraFollow cameraFollow = Camera.main.GetComponent<CameraFollow>();
        if (cameraFollow != null)
        {
            cameraFollow.target = playerCar.transform;
        }

        // 4. ��ʼ�׶β�������ҿ���
        canControlCar = false;
        raceEnded = false;

        // 5. һ��ʼ����������ʱ
        countdownActive = true;
        countdownText.gameObject.SetActive(true);

        // 6. �ҵ� RaceManager �Ѷ��ı��������¼�
        raceManager = FindObjectOfType<RaceManager>();
        if (raceManager != null)
        {
            raceManager.OnRaceCompleted += HandleRaceEnd;
        }
        else
        {
            Debug.LogWarning("[GameManager] δ�ҵ� RaceManager�������������޷�������");
        }

        // 7. ȷ����������ʼ����
        if (finishPanel != null)
        {
            finishPanel.SetActive(false);
        }

        // 8. ������Ϸʱ���ı�
        if (gameTimeText != null)
        {
            gameTimeText.gameObject.SetActive(false);
        }

        // 9. ����ť�󶨻ص�
        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartButtonClicked);
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
    }

    void Update()
    {
        // 1. ����ʱ�׶�
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
                // ����ʱ���������� AI ���Ŀ���
                EnableAIControl();
            }
        }

        // 2. ���δ����ʱ����δ��ʼ���ƣ����ո��������������ʱ������δ����ʱ��
        if (!countdownActive && !canControlCar && !raceEnded && Input.GetKeyDown(KeyCode.Space))
        {
            StartCountdown();
        }

        // 3. ��ҿ��Ƽ���Ϸʱ�ӣ�����δ����ʱ��
        if (canControlCar && !raceEnded)
        {
            // �����������ƽű�
            if (playerCar != null)
            {
                var controller = playerCar.GetComponent<CarController>();
                if (controller != null && !controller.enabled)
                {
                    controller.enabled = true;
                }
            }

            // ������Ϸʱ���ı�
            gameTime += Time.deltaTime;
            int minutes = Mathf.FloorToInt(gameTime / 60f);
            int seconds = Mathf.FloorToInt(gameTime % 60f);
            int milliseconds = Mathf.FloorToInt((gameTime * 1000f) % 1000f);
            gameTimeText.text = string.Format("{0:D2}:{1:D2}:{2:D3}", minutes, seconds, milliseconds);

            // ��ʾ��ǰȦ��
            if (lapText != null)
            {
                lapText.text = $"Lap: {raceManager.CurrentLap}/{raceManager.TotalLaps}";
            }

            // ��ʾ��ǰ�ٶ�
            if (speedText != null && carController != null)
            {
                speedText.text = $"Speed: {carController.KPH:F2} km/h";
            }
        }
    }

    /// <summary>
    /// ���ò�������������ʱ
    /// </summary>
    void StartCountdown()
    {
        countdownTime = 5f;
        countdownActive = true;
        canControlCar = false;
        countdownText.gameObject.SetActive(true);
    }

    /// <summary>
    /// ������Ϸʱ��
    /// </summary>
    void StartGameTimer()
    {
        gameTime = 0f;
        gameTimeText.gameObject.SetActive(true);
    }

    /// <summary>
    /// RaceManager ֪ͨ������������ô˷���
    /// </summary>
    private void HandleRaceEnd()
    {
        Debug.Log("[GameManager] �յ���������֪ͨ");

        // ��Ǳ����ѽ���
        raceEnded = true;

        // ��ֹ��Ҽ������Ʋ��ر� CarController
        canControlCar = false;
        if (playerCar != null)
        {
            var controller = playerCar.GetComponent<CarController>();
            if (controller != null)
                controller.enabled = false;  // ֹͣ��ҿ���
        }

        // ��ֹ AI �������Ʋ��ر� CarController
        GameObject[] aiCars = GameObject.FindGameObjectsWithTag("AI");
        foreach (var aiCar in aiCars)
        {
            var aiController = aiCar.GetComponent<CarController>();
            var aiinput = aiCar.GetComponent<AIInput>();
            if (aiController != null)
            {
                aiController.enabled = false;  // ֹͣ AI ����
                aiinput.enabled = false;
            }
        }

        // ��ȡ AI ��ɵ�Ȧ��
        AICheckpointManager aiCheckpointManager = FindObjectOfType<AICheckpointManager>();
        if (aiCheckpointManager != null)
        {
            int aiCrossCount = aiCheckpointManager.GetAiCrossCount(); // ��ȡ AI ��ɵ�Ȧ��

            // ��ȡ��ҵ������Ȧ��
            int playerCompletedLaps = raceManager.CurrentLap;

            // �ж���Һ� AI �Ƿ���ɱ���������ʾ��Ӧ��Ϣ
            if (playerCompletedLaps >= raceManager.TotalLaps)
            {
                // ��� AI ��ɱ������������ɱ���
                if (aiCrossCount >= raceManager.TotalLaps)
                {
                    // ������ˣ���ʾʧ����Ϣ
                    if (finishTimeText != null)
                    {
                        finishTimeText.text = $"You lost! Your time is {GetFormattedTime()}";
                    }
                    Debug.Log("You lost! Better luck next time.");
                }
                else
                {
                    // ���Ӯ�ˣ���ʾ�ɹ���Ϣ
                    if (finishTimeText != null)
                    {
                        finishTimeText.text = $"You win! Your time is {GetFormattedTime()}";
                    }
                    Debug.Log("Congratulations! You won the race.");
                }
            }
        }

        // ��ʾ�������
        if (finishPanel != null)
        {
            finishPanel.SetActive(true);
        }
    }

    // ���ڸ�ʽ��ʱ��ĸ�������
    private string GetFormattedTime()
    {
        int minutes = Mathf.FloorToInt(gameTime / 60f);
        int seconds = Mathf.FloorToInt(gameTime % 60f);
        int milliseconds = Mathf.FloorToInt((gameTime * 1000f) % 1000f);
        return $"{minutes:D2}:{seconds:D2}:{milliseconds:D3}";
    }
    /// <summary>
    /// ������һ�֡� ��ť�ص������¼��ص�ǰ����
    /// </summary>
    private void OnRestartButtonClicked()
    {
        if (!raceEnded) return;
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

    /// <summary>
    /// �������˵��� ��ť�ص����������˵�����
    /// </summary>
    private void OnMainMenuButtonClicked()
    {
        if (!raceEnded) return;
        // �������˵�������Ϊ "MainMenu"
        SceneManager.LoadScene("MainMenuScene");
    }

    private void OnDestroy()
    {
        // ��ע����������¼�
        if (raceManager != null)
        {
            raceManager.OnRaceCompleted -= HandleRaceEnd;
        }
    }
    // ���� AI ����
    void EnableAIControl()
    {
        // �ҵ����е� AI �����������ǵĿ���
        GameObject[] aiCars = GameObject.FindGameObjectsWithTag("AI");
        foreach (var aiCar in aiCars)
        {
            CarController aiCarController = aiCar.GetComponent<CarController>();
            if (aiCarController != null)
            {
                aiCarController.enabled = true;  // ���� AI ���Ŀ���
            }
        }
    }
}