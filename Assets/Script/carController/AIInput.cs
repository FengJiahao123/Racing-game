using UnityEngine;

public class AIInput : MonoBehaviour, IDriverInput
{
    private GameManager gameManager;
    [Header("·���㣨�ֶ����룩")]
    public Transform[] waypoints;  // ������� Inspector ���ֶ�������Щ·����
    public float reachRadius = 5f; // ����Ŀ������㵽��
    public float targetSpeedKPH = 180f;  // AI Ŀ���ٶȣ���λ km/h
    public float maxSteerAngle = 30f;    // ���ת��Ƕ�

    private int currentWP = 0;  // ��ǰĿ��·��������
    private int previousWP = 0; // ��һ��·��������
    private Rigidbody rb;  // ������ Rigidbody ���

    private bool isAccelerating = false; // �Ƿ����ڼ���
    private float speedIncreaseTime = 0f; // ����ʱ��
    
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    void Start()
    {
        // �Զ����� RaceManager ʵ��
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
            // ��ǰ�ٶȣ�km/h��
            float speed = rb.velocity.magnitude * 3.6f;

            // ����Ƿ���Ҫ���ٵ� 220 km/h
            if (currentWP == 18 || currentWP == 40)
            {
                // ����Ѿ�����Ŀ���18��40����������
                isAccelerating = true;
                speedIncreaseTime = 5f; // 3�����
                targetSpeedKPH = 220f;  // ���ٵ�220 km/h
            }

            // ���Ա������٣��ٶ�Խ�ӽ�Ŀ�꣬����ԽС
            return Mathf.Clamp01((targetSpeedKPH - speed) / targetSpeedKPH);
        }
    }

    public float Steer
    {
        get
        {
            // ��ȡ��ǰĿ��·����
            Vector3 targetPosition = waypoints[currentWP].position;

            // ����ӳ�����ǰλ�õ�Ŀ��·����ķ���
            Vector3 toTarget = targetPosition - transform.position;
            Vector3 dirFlat = Vector3.ProjectOnPlane(toTarget, Vector3.up).normalized;

            // ���㳵����ǰ������Ŀ��㷽��ļн�
            float angleError = Vector3.SignedAngle(transform.forward, dirFlat, Vector3.up);

            // ����Ѿ�����Ŀ��㣬�л�����һ��Ŀ���
            if (toTarget.magnitude < reachRadius)
            {
                previousWP = currentWP; // ������һ��·����
                currentWP = (currentWP + 1) % waypoints.Length;  // ѭ��·����
            }

            // ����ת��Ƕȣ�-1 �� 1 ֮�䣩
            return Mathf.Clamp(angleError / maxSteerAngle, -1f, 1f);
        }
    }

    public bool Brake => false;     // �� AI������Ҫ����ɲ��
    public bool Handbrake => false; // ����Ҫ��ɲ��Ư�ƣ�

    // ��ײ��⣺�� AI ײ��ǽ�����ٶ�С�� 20 ʱ������λ��
    private void OnCollisionEnter(Collision collision)
    {
        // �ж��Ƿ���ǽ�ڣ�������ĳ��������Զ��壩
        if (collision.gameObject.CompareTag("Wall"))
        {
            // ����ٶ�С�� 20 km/h��ִ�о���λ��
            if (rb.velocity.magnitude * 3.6f < 20f)
            {
                CorrectPosition();
                Debug.Log($"[AI] ײǽ�˲����ٶ�С��20�����ص�·���� {previousWP}");
            }
        }
    }

    // ����λ�ã����ٶ�С��20�ҷ�����ײʱ�����µ�������
    private void CorrectPosition()
    {
        // ��ȡ��һ��·����λ��
        Vector3 correctedPosition = waypoints[previousWP].position;
        // ���� Y ��ĸ߶ȣ���ֹ��ģ�͵���ȥ
        correctedPosition.y += 1f;  // ��������߶� +1

        // �� AI ����λ�õ������µ�����λ��
        transform.position = correctedPosition;

        // ǿ�ƾ��� AI ���ķ��򣬳�����һ��·����ķ���
        Vector3 directionToPreviousWP = waypoints[previousWP].position - transform.position;
        transform.rotation = Quaternion.LookRotation(directionToPreviousWP);  // ��������

        rb.velocity = Vector3.zero; // �����ٶ�
    }

    void Update()
    {
        // ����ʱ��ļ�ʱ
        if (isAccelerating)
        {
            speedIncreaseTime -= Time.deltaTime;
            if (speedIncreaseTime <= 0f)
            {
                targetSpeedKPH = gameManager.speed;  // �ָ������ٶ�
                isAccelerating = false;  // ��������
                Debug.Log("[AI] ������ɣ��ָ������ٶ�");
            }
        }
    }
}
