using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public enum FollowMode
    {
        SpringPhysics,  // 弹簧物理模拟（有惯性感）
        SmoothDamp      // 平滑插值（更稳定）
    }

    [Header("基础设置")]
    public Transform target;                      // 赛车目标
    public Vector3 offset = new Vector3(0, 3, -6); // 相机相对偏移
    public FollowMode mode = FollowMode.SmoothDamp; // 跟随模式

    [Header("弹簧物理参数")]
    public float springStrength = 20f;            // 弹力系数（建议15~25）
    public float damping = 12f;                   // 阻尼系数（建议10~15）
    private Vector3 physicsVelocity = Vector3.zero; // 物理速度

    [Header("平滑插值参数")]
    public float smoothTime = 0.1f;               // 平滑时间（越小跟随越快）
    public float maxSpeed = Mathf.Infinity;       // 最大跟随速度
    private Vector3 smoothVelocity = Vector3.zero; // 平滑速度

    [Header("旋转设置")]
    public float rotationSmooth = 5f;             // 旋转平滑度
    public bool lookAhead = true;                 // 是否预判车辆前方方向
    public float lookAheadDistance = 2f;          // 预判距离

    void Awake()
    {
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = 144;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 1. 计算目标位置（含偏移）
        Vector3 desiredPosition = target.TransformPoint(offset);

        // 2. 根据模式更新相机位置
        switch (mode)
        {
            case FollowMode.SpringPhysics:
                // 弹簧物理模拟（带惯性）
                Vector3 displacement = desiredPosition - transform.position;
                Vector3 springForce = displacement * springStrength;
                Vector3 dampingForce = -physicsVelocity * damping;
                physicsVelocity += (springForce + dampingForce) * Time.deltaTime;
                transform.position += physicsVelocity * Time.deltaTime;
                break;

            case FollowMode.SmoothDamp:
                // 平滑插值（更稳定）
                transform.position = Vector3.SmoothDamp(
                    transform.position,
                    desiredPosition,
                    ref smoothVelocity,
                    smoothTime,
                    maxSpeed,
                    Time.deltaTime
                );
                break;
        }

        // 3. 计算目标旋转方向
        Vector3 lookDirection = target.forward;
        if (lookAhead)
        {
            // 预判车辆前方方向（减少急转弯时的镜头滞后）
            lookDirection = (target.position + target.forward * lookAheadDistance) - transform.position;
        }

        Quaternion desiredRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            desiredRotation,
            rotationSmooth * Time.deltaTime
        );
    }

    void OnEnable()
    {
        // 初始化相机位置和速度
        if (target != null)
        {
            transform.position = target.TransformPoint(offset);
            transform.rotation = Quaternion.LookRotation(target.forward);
            physicsVelocity = Vector3.zero;
            smoothVelocity = Vector3.zero;
        }
    }

    // 调试辅助：在Scene窗口显示跟随偏移
    void OnDrawGizmosSelected()
    {
        if (target != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(target.position, target.TransformPoint(offset));
            Gizmos.DrawWireSphere(target.TransformPoint(offset), 0.5f);
        }
    }
}