using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceManager : MonoBehaviour
{
    public static RaceManager Instance { get; private set; }

    [Header("比赛设置")]
    public int TotalLaps = 3;
    [Header("延迟显隐秒数")]
    public float checkpointDelay = 1.5f;
    [Header("可视检查点对象")]
    public GameObject postLapCheckpoint;
    [Header("所有 Checkpoint 脚本")]
    public Checkpoint[] allCheckpoints;
    [Header("起点/终点 ID")]
    public int startFinishId = 0;

    public int CurrentLap => Mathf.Max(0, crossCount - 1);
    public System.Action OnRaceCompleted;

    private int crossCount = 0;
    private HashSet<int> nextAllowed = new HashSet<int>();
    private HashSet<int> visitedThisLap = new HashSet<int>();

    // —— 新增：UI 提示用
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
        // 警告倒计时
        if (warningTimer > 0f)
            warningTimer -= Time.deltaTime;
    }

    private void OnGUI()
    {
        if (warningTimer > 0f && !string.IsNullOrEmpty(warningMessage))
        {
            // 新建一个 GUIStyle 让文字居中、字号合适
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 32
            };
            // 矩形宽度撑满屏幕，高度 50，垂直居中
            Rect rect = new Rect(0, Screen.height / 2 - 25, Screen.width, 50);
            GUI.Label(rect, warningMessage, style);
        }
    }

    /// <summary>
    /// 外部调用：显示一条屏幕中间的警告文字，2 秒后自动消失
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
            Debug.Log($"[RaceManager] 过线次数：{crossCount}，已完成圈数：{CurrentLap}/{TotalLaps}");

            if (crossCount == 1 && postLapCheckpoint != null)
                StartCoroutine(DelayedSetActive(postLapCheckpoint, false, checkpointDelay));
            else if (crossCount == TotalLaps && postLapCheckpoint != null)
                StartCoroutine(DelayedSetActive(postLapCheckpoint, true, checkpointDelay));
            else if (CurrentLap >= TotalLaps)
                OnRaceCompleted?.Invoke();

            // 新一圈开始，清空已访问集合
            visitedThisLap.Clear();
        }

        visitedThisLap.Add(id);

        // 更新下一步允许集合
        var cp = System.Array.Find(allCheckpoints, c => c.id == id);
        nextAllowed = new HashSet<int>(cp.nextIds);

        return true;
    }

    public bool IsVisitedInCurrentLap(int id) => visitedThisLap.Contains(id);

    public void HandleIllegalReverse(int id, GameObject car)
    {
        Debug.LogWarning("[RaceManager] 检测到逆行，已强制纠正！");

        // 停车
        var rb = car.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // 弹 UI 警告
        ShowWarning("禁止逆行！");

        // 瞬移并朝向
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