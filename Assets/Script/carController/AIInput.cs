using UnityEngine;

public class AIInput : MonoBehaviour, IDriverInput
{
    private GameManager gameManager;
    [Header("路径点（手动拖入）")]
    public Transform[] waypoints;  // 你可以在 Inspector 中手动设置这些路径点
    public float reachRadius = 5f; // 距离目标点多近算到达
    public float targetSpeedKPH = 180f;  // AI 目标速度，单位 km/h
    public float maxSteerAngle = 30f;    // 最大转向角度

    private int currentWP = 0;  // 当前目标路径点索引
    private int previousWP = 0; // 上一个路径点索引
    private Rigidbody rb;  // 车辆的 Rigidbody 组件

    private bool isAccelerating = false; // 是否正在加速
    private float speedIncreaseTime = 0f; // 加速时间
    
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    void Start()
    {
        // 自动查找 RaceManager 实例
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }
        if (gameManager == null)
        {
            Debug.LogError("RaceManager reference is missing!");
        }
        targetSpeedKPH = gameManager.speed;
    }

    public float Throttle
    {
        get
        {
            // 当前速度（km/h）
            float speed = rb.velocity.magnitude * 3.6f;

            // 检查是否需要加速到 220 km/h
            if (currentWP == 18 || currentWP == 40)
            {
                // 如果已经到达目标点18或40，启动加速
                isAccelerating = true;
                speedIncreaseTime = 5f; // 3秒加速
                targetSpeedKPH = 220f;  // 加速到220 km/h
            }

            // 线性比例减速：速度越接近目标，油门越小
            return Mathf.Clamp01((targetSpeedKPH - speed) / targetSpeedKPH);
        }
    }

    public float Steer
    {
        get
        {
            // 获取当前目标路径点
            Vector3 targetPosition = waypoints[currentWP].position;

            // 计算从车辆当前位置到目标路径点的方向
            Vector3 toTarget = targetPosition - transform.position;
            Vector3 dirFlat = Vector3.ProjectOnPlane(toTarget, Vector3.up).normalized;

            // 计算车辆当前朝向与目标点方向的夹角
            float angleError = Vector3.SignedAngle(transform.forward, dirFlat, Vector3.up);

            // 如果已经到达目标点，切换到下一个目标点
            if (toTarget.magnitude < reachRadius)
            {
                previousWP = currentWP; // 更新上一个路径点
                currentWP = (currentWP + 1) % waypoints.Length;  // 循环路径点
            }

            // 返回转向角度（-1 到 1 之间）
            return Mathf.Clamp(angleError / maxSteerAngle, -1f, 1f);
        }
    }

    public bool Brake => false;     // 简单 AI，不需要额外刹车
    public bool Handbrake => false; // 不需要手刹（漂移）

    // 碰撞检测：当 AI 撞到墙并且速度小于 20 时，纠正位置
    private void OnCollisionEnter(Collision collision)
    {
        // 判断是否是墙壁（根据你的场景可以自定义）
        if (collision.gameObject.CompareTag("Wall"))
        {
            // 如果速度小于 20 km/h，执行纠正位置
            if (rb.velocity.magnitude * 3.6f < 20f)
            {
                CorrectPosition();
                Debug.Log($"[AI] 撞墙了并且速度小于20，返回到路径点 {previousWP}");
            }
        }
    }

    // 纠正位置，当速度小于20且发生碰撞时，重新调整车辆
    private void CorrectPosition()
    {
        // 获取上一个路径点位置
        Vector3 correctedPosition = waypoints[previousWP].position;
        // 增加 Y 轴的高度，防止穿模和掉下去
        correctedPosition.y += 1f;  // 例如调整高度 +1

        // 将 AI 车的位置调整到新的修正位置
        transform.position = correctedPosition;

        // 强制纠正 AI 车的方向，朝向上一个路径点的方向
        Vector3 directionToPreviousWP = waypoints[previousWP].position - transform.position;
        transform.rotation = Quaternion.LookRotation(directionToPreviousWP);  // 方向修正

        rb.velocity = Vector3.zero; // 归零速度
    }

    void Update()
    {
        // 加速时间的计时
        if (isAccelerating)
        {
            speedIncreaseTime -= Time.deltaTime;
            if (speedIncreaseTime <= 0f)
            {
                targetSpeedKPH = gameManager.speed;  // 恢复正常速度
                isAccelerating = false;  // 结束加速
                Debug.Log("[AI] 加速完成，恢复正常速度");
            }
        }
    }
}
