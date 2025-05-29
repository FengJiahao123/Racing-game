using UnityEngine;

[ExecuteInEditMode] // 使得脚本在编辑模式下也能执行
public class WheelColliderAligner : MonoBehaviour
{
    [Header("拖入引用")]
    public GameObject wheelModel;  // 轮子的3D模型（必须有MeshRenderer）
    public WheelCollider wheelCollider; // 对应的WheelCollider

    [Header("调试选项")]
    public bool showDebugGizmos = true; // 显示调试球
    public Color modelCenterColor = Color.green; // 轮子模型中心颜色
    public Color colliderPosColor = Color.red;   // WheelCollider位置颜色

    // 更新轮子和WheelCollider对齐
    void Update()
    {
        if (wheelModel == null || wheelCollider == null) return;

        // 获取轮子模型的精确几何中心（世界坐标）
        Renderer renderer = wheelModel.GetComponent<Renderer>();
        if (renderer == null)
        {
            Debug.LogError("轮子模型缺少MeshRenderer！");
            return;
        }

        Bounds bounds = renderer.bounds;
        Vector3 modelWorldCenter = bounds.center;

        // 计算WheelCollider应有的本地中心点（综合考虑悬挂距离的影响）
        Vector3 colliderCenter = wheelCollider.transform.InverseTransformPoint(modelWorldCenter);

        // 使用正值来修正Y轴偏移，使得WheelCollider的位置与模型几何中心对齐
        colliderCenter.y = colliderCenter.y + wheelCollider.suspensionDistance / 2f; // 修正Y轴值

        // 应用修正后的Center
        wheelCollider.center = colliderCenter;

        // 自动计算完美半径（取模型高度作为半径）
        wheelCollider.radius = bounds.extents.y;
    }

    // 绘制调试信息
    void OnDrawGizmos()
    {
        if (!showDebugGizmos || wheelModel == null || wheelCollider == null) return;

        // 绘制轮子模型中心（绿色）
        Renderer renderer = wheelModel.GetComponent<Renderer>();
        if (renderer != null)
        {
            Gizmos.color = modelCenterColor;
            Gizmos.DrawSphere(renderer.bounds.center, 0.03f);
        }

        // 绘制WheelCollider实际位置（红色）
        wheelCollider.GetWorldPose(out Vector3 colliderPos, out Quaternion _);
        Gizmos.color = colliderPosColor;
        Gizmos.DrawSphere(colliderPos, 0.03f);

        // 绘制连线（方便查看偏移）
        Gizmos.color = Color.white;
        Gizmos.DrawLine(renderer.bounds.center, colliderPos);
    }
}
