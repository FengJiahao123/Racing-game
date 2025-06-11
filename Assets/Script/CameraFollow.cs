using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public enum FollowMode
    {
        SpringPhysics,  // ��������ģ�⣨�й��ԸУ�
        SmoothDamp      // ƽ����ֵ�����ȶ���
    }

    [Header("��������")]
    public Transform target;                      // ��ҳ�����Ŀ��
    public Vector3 offset = new Vector3(0, 3, -6); // ������ƫ��
    public FollowMode mode = FollowMode.SmoothDamp; // ����ģʽ

    [Header("�����������")]
    public float springStrength = 20f;            // ����ϵ��������15~25��
    public float damping = 12f;                   // ����ϵ��������10~15��
    private Vector3 physicsVelocity = Vector3.zero; // �����ٶ�

    [Header("ƽ����ֵ����")]
    public float smoothTime = 0.1f;               // ƽ��ʱ�䣨ԽС����Խ�죩
    public float maxSpeed = Mathf.Infinity;       // �������ٶ�
    private Vector3 smoothVelocity = Vector3.zero; // ƽ���ٶ�

    [Header("��ת����")]
    public float rotationSmooth = 5f;             // ��תƽ����
    public bool lookAhead = true;                 // �Ƿ�Ԥ�г���ǰ������
    public float lookAheadDistance = 2f;          // Ԥ�о���

    void Awake()
    {
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = 144;
    }

    void Start()
    {
        // ���û���ֶ�����Ŀ�공�����Զ����Ҵ� "Player" ��ǩ�ĳ���
        if (target == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                target = player.transform;  // ��Ŀ������Ϊ���ѡ��ĳ���
            }
        }

        // ���û���ҵ���ҳ������������
        if (target == null)
        {
            Debug.LogWarning("Player vehicle not found. Make sure the vehicle has 'Player' tag.");
        }
    }

    void LateUpdate()
    {
        if (target == null) return;  // ���û��Ŀ�꣬���˳�����

        // ����Ŀ��λ�ã�����ƫ������
        Vector3 desiredPosition = target.TransformPoint(offset);

        // ����ģʽ�������λ��
        switch (mode)
        {
            case FollowMode.SpringPhysics:
                // ��������ģ�⣨�����ԣ�
                Vector3 displacement = desiredPosition - transform.position;
                Vector3 springForce = displacement * springStrength;
                Vector3 dampingForce = -physicsVelocity * damping;
                physicsVelocity += (springForce + dampingForce) * Time.deltaTime;
                transform.position += physicsVelocity * Time.deltaTime;
                break;

            case FollowMode.SmoothDamp:
                // ƽ����ֵ�����ȶ���
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

        // ����Ŀ����ת����
        Vector3 lookDirection = target.forward;
        if (lookAhead)
        {
            // Ԥ�г���ǰ�����򣨼��ټ�ת��ʱ�ľ�ͷ�ͺ�
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
        // ��ʼ�����λ�ú��ٶ�
        if (target != null)
        {
            transform.position = target.TransformPoint(offset);
            transform.rotation = Quaternion.LookRotation(target.forward);
            physicsVelocity = Vector3.zero;
            smoothVelocity = Vector3.zero;
        }
    }

    // ���Ը�������Scene������ʾ����ƫ��
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
