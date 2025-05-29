using UnityEngine;

[ExecuteInEditMode] // ʹ�ýű��ڱ༭ģʽ��Ҳ��ִ��
public class WheelColliderAligner : MonoBehaviour
{
    [Header("��������")]
    public GameObject wheelModel;  // ���ӵ�3Dģ�ͣ�������MeshRenderer��
    public WheelCollider wheelCollider; // ��Ӧ��WheelCollider

    [Header("����ѡ��")]
    public bool showDebugGizmos = true; // ��ʾ������
    public Color modelCenterColor = Color.green; // ����ģ��������ɫ
    public Color colliderPosColor = Color.red;   // WheelColliderλ����ɫ

    // �������Ӻ�WheelCollider����
    void Update()
    {
        if (wheelModel == null || wheelCollider == null) return;

        // ��ȡ����ģ�͵ľ�ȷ�������ģ��������꣩
        Renderer renderer = wheelModel.GetComponent<Renderer>();
        if (renderer == null)
        {
            Debug.LogError("����ģ��ȱ��MeshRenderer��");
            return;
        }

        Bounds bounds = renderer.bounds;
        Vector3 modelWorldCenter = bounds.center;

        // ����WheelColliderӦ�еı������ĵ㣨�ۺϿ������Ҿ����Ӱ�죩
        Vector3 colliderCenter = wheelCollider.transform.InverseTransformPoint(modelWorldCenter);

        // ʹ����ֵ������Y��ƫ�ƣ�ʹ��WheelCollider��λ����ģ�ͼ������Ķ���
        colliderCenter.y = colliderCenter.y + wheelCollider.suspensionDistance / 2f; // ����Y��ֵ

        // Ӧ���������Center
        wheelCollider.center = colliderCenter;

        // �Զ����������뾶��ȡģ�͸߶���Ϊ�뾶��
        wheelCollider.radius = bounds.extents.y;
    }

    // ���Ƶ�����Ϣ
    void OnDrawGizmos()
    {
        if (!showDebugGizmos || wheelModel == null || wheelCollider == null) return;

        // ��������ģ�����ģ���ɫ��
        Renderer renderer = wheelModel.GetComponent<Renderer>();
        if (renderer != null)
        {
            Gizmos.color = modelCenterColor;
            Gizmos.DrawSphere(renderer.bounds.center, 0.03f);
        }

        // ����WheelColliderʵ��λ�ã���ɫ��
        wheelCollider.GetWorldPose(out Vector3 colliderPos, out Quaternion _);
        Gizmos.color = colliderPosColor;
        Gizmos.DrawSphere(colliderPos, 0.03f);

        // �������ߣ�����鿴ƫ�ƣ�
        Gizmos.color = Color.white;
        Gizmos.DrawLine(renderer.bounds.center, colliderPos);
    }
}
