using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour
{
    public float KPH => _rb.velocity.magnitude * 3.6f;

    [Header("Wheel Settings")]
    public WheelCollider[] driveWheels;
    public WheelCollider[] steerWheels;
    public WheelCollider[] allWheels;

    [Header("Engine Settings")]
    public float maxMotorTorque = 3000f;
    public float maxSpeed = 180f;
    public AnimationCurve torqueCurve;
    public float throttleResponse = 10f;

    [Header("Steering Settings")]
    public float maxSteerAngle = 25f;
    public float steerResponseSpeed = 5f;
    public float naturalRecoverySpeed = 15f;
    [Range(0.1f, 1f)]
    public float speedBasedSteeringReduction = 0.5f;
    public float minSteerAngleAtSpeed = 10f;

    [Header("Brake Settings")]
    public float maxBrakeForce = 8000f;
    public float brakeResponseTime = 0.15f;
    public float autoBrakeForce = 400f;
    public float coastDrag = 0.3f;  // �Ż�ֱ�߻���
    public float idleDrag = 1f;     // �Ż���������

    [Header("Drift Settings")]
    public float minDriftSpeed = 40f;
    public float driftSpeedMaintain = 0.8f;
    public float driftSteerMultiplier = 1.7f;
    public float rearWheelSlip = 0.4f;
    public float handbrakeDriftMultiplier = 3.5f;
    public float driftStability = 2f;
    public float driftTorque = 350f;
    public bool autoThrottleInDrift = true;
    public float minDriftThrottle = 0.4f;

    [Header("Drift Recovery - �����޸�����")]
    public float frictionRecoverySpeed = 8f;    // Ħ�����ָ��ٶ�
    public float stabilityRecoverySpeed = 5f;  // Ť������ٶ�
    private float _driftCompensation;          // ��ǰƯ�Ʋ�����

    [Header("Debug Info")]
    public float currentSteerAngle;
    public float currentThrottle;
    public float currentBrakeForce;
    public float slipRatio;
    public float wheelRPM;
    public bool isDrifting;

    private Rigidbody _rb;
    private float _targetThrottle;
    private float _targetBrake;
    private float _originalFriction;
    private float _steeringInput;
    private float _steeringInputVelocity;
    private bool _handbrake;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.mass = 1200f;
        _rb.angularDrag = 4f;
        _rb.centerOfMass = new Vector3(0, -0.4f, 0);

        if (torqueCurve == null || torqueCurve.length == 0)
        {
            torqueCurve = new AnimationCurve(
                new Keyframe(0, 1.2f),
                new Keyframe(0.4f, 1f),
                new Keyframe(0.8f, 0.7f),
                new Keyframe(1f, 0.4f)
            );
        }

        if (allWheels.Length > 0)
        {
            _originalFriction = allWheels[0].sidewaysFriction.stiffness;
            UpdateWheelFriction(1f);
        }
    }

    void Update()
    {
        _handbrake = Input.GetKey(KeyCode.LeftShift) && Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f;
    }

    void FixedUpdate()
    {
        float throttleInput = Input.GetAxis("Vertical");
        float steerInput = Input.GetAxis("Horizontal");
        bool brakeInput = Input.GetKey(KeyCode.Space);

        _targetThrottle = Mathf.Lerp(_targetThrottle, throttleInput, Time.fixedDeltaTime * throttleResponse);
        _targetBrake = Mathf.Lerp(_targetBrake, brakeInput ? 1f : 0f, Time.fixedDeltaTime / brakeResponseTime);

        currentThrottle = _targetThrottle;
        currentBrakeForce = _targetBrake * maxBrakeForce;

        HandleSteering(steerInput);
        HandleMotor();
        HandleBrakes();
        HandleDrift();
        HandleAutoBrake();
        HandleDrag();
        LimitMaxSpeed();
        UpdateDebugInfo();
    }

    void HandleSteering(float rawInput)
    {
        // === �Զ�������ֵ���� ===
        if (Mathf.Abs(rawInput) < 0.01f)
        {
            _steeringInput = Mathf.Lerp(_steeringInput, 0f, Time.fixedDeltaTime * 6f);
            _steeringInputVelocity = 0f; // ��ֹ����Ư��
        }
        else
        {
            _steeringInput = Mathf.SmoothDamp(_steeringInput, rawInput, ref _steeringInputVelocity, 0.1f);
        }

        // === �����߼����� ===
        float speedFactor = Mathf.Clamp01(KPH / maxSpeed);
        float steerLimit = Mathf.Lerp(1f, speedBasedSteeringReduction, Mathf.Pow(speedFactor, 1.5f));
        steerLimit = Mathf.Max(steerLimit, minSteerAngleAtSpeed / maxSteerAngle);

        float targetAngle = _steeringInput * maxSteerAngle * steerLimit;

        if (isDrifting)
        {
            currentSteerAngle = targetAngle * driftSteerMultiplier;
            naturalRecoverySpeed = 8f;
        }
        else
        {
            currentSteerAngle = Mathf.Lerp(
                currentSteerAngle,
                targetAngle,
                Time.fixedDeltaTime * (Mathf.Abs(_steeringInput) > 0.1f ? steerResponseSpeed : naturalRecoverySpeed)
            );
        }

        foreach (var wheel in steerWheels)
        {
            wheel.steerAngle = currentSteerAngle;
        }

        if (driveWheels.Length == 4)
        {
            Vector3 localVel = transform.InverseTransformDirection(_rb.velocity);
            float compensation = -localVel.x * 0.3f * _rb.mass * 0.01f;
            _rb.AddTorque(transform.up * compensation, ForceMode.Force);
        }
    }


    void HandleMotor()
    {
        float speedRatio = Mathf.Clamp01(KPH / maxSpeed);
        float torque = _targetThrottle * maxMotorTorque * torqueCurve.Evaluate(speedRatio);

        foreach (var wheel in driveWheels)
        {
            if (isDrifting)
            {
                wheel.motorTorque = torque * (System.Array.IndexOf(steerWheels, wheel) >= 0 ? 1f : driftSpeedMaintain);
            }
            else
            {
                wheel.motorTorque = torque;
            }
        }
    }

    void HandleBrakes()
    {
        foreach (var wheel in allWheels)
        {
            wheel.brakeTorque = _targetBrake > 0.05f ? currentBrakeForce : 0f;
        }
    }

    // === ������ HandleDrift() ���°汾 ===
    void HandleDrift()
    {
        if (_handbrake && KPH > minDriftSpeed)
        {
            // Ư�Ƽ����߼�
            foreach (var wheel in allWheels)
            {
                if (System.Array.IndexOf(steerWheels, wheel) < 0)
                {
                    // ���ִ�
                    var friction = wheel.sidewaysFriction;
                    friction.stiffness = rearWheelSlip;
                    wheel.sidewaysFriction = friction;
                }
                else
                {
                    // ������ǰ��Ħ������ǿ
                    var friction = wheel.sidewaysFriction;
                    friction.stiffness = _originalFriction * 1.2f; // ������ǰ����΢��ץ��
                    wheel.sidewaysFriction = friction;
                }
            }

            Vector3 localVel = transform.InverseTransformDirection(_rb.velocity);
            _driftCompensation = -localVel.x * driftStability * _rb.mass * 0.01f;
            _rb.AddTorque(transform.up * _driftCompensation, ForceMode.Force);

            isDrifting = true;
        }
        else if (isDrifting)
        {
            // ��ȷ�ָ����
            bool frictionRestored = true;
            bool torqueCleared = Mathf.Abs(_driftCompensation) < 0.01f;

            foreach (var wheel in allWheels)
            {
                if (System.Array.IndexOf(steerWheels, wheel) < 0)
                {
                    var friction = wheel.sidewaysFriction;
                    friction.stiffness = Mathf.Lerp(friction.stiffness, _originalFriction,
                        Time.fixedDeltaTime * frictionRecoverySpeed);
                    wheel.sidewaysFriction = friction;

                    if (Mathf.Abs(friction.stiffness - _originalFriction) > 0.01f)
                        frictionRestored = false;
                }
                else
                {
                    // ������ǰ��Ҳ�ָ�
                    var friction = wheel.sidewaysFriction;
                    friction.stiffness = Mathf.Lerp(friction.stiffness, _originalFriction,
                        Time.fixedDeltaTime * frictionRecoverySpeed);
                    wheel.sidewaysFriction = friction;
                }
            }

            // Ť������
            if (!torqueCleared)
            {
                _driftCompensation = Mathf.Lerp(_driftCompensation, 0,
                    Time.fixedDeltaTime * stabilityRecoverySpeed * 2f);
                _rb.AddTorque(transform.up * -_driftCompensation, ForceMode.Force);
                torqueCleared = Mathf.Abs(_driftCompensation) < 0.01f;
            }

            // ״̬�˳�
            isDrifting = !(frictionRestored && torqueCleared);

            if (!isDrifting)
            {
                _driftCompensation = 0;
                UpdateWheelFriction(1f);
            }
        }

        if (_rb.angularVelocity.magnitude > 2f)
        {
            _rb.angularVelocity *= 0.95f;
        }
    }


    void HandleAutoBrake()
    {
        if (Mathf.Abs(_targetThrottle) < 0.1f && KPH > 5f)
        {
            foreach (var wheel in allWheels)
            {
                wheel.brakeTorque = autoBrakeForce;
            }
        }
    }

    void HandleDrag()
    {
        _rb.drag = _targetBrake > 0.1f ? 2f :
                  Mathf.Abs(_targetThrottle) > 0.1f ? 0.05f :
                  KPH > 3f ? coastDrag : idleDrag;
    }

    void LimitMaxSpeed()
    {
        float maxSpeedMs = maxSpeed / 3.6f;
        if (_rb.velocity.magnitude > maxSpeedMs)
        {
            _rb.velocity = _rb.velocity.normalized * maxSpeedMs;
        }
    }

    // �޸���4����ȫ�ָ���̥Ħ����
    void UpdateWheelFriction(float multiplier)
    {
        float clampedMultiplier = Mathf.Clamp01(multiplier);
        foreach (var wheel in allWheels)
        {
            // �ָ�����Ħ����
            WheelFrictionCurve sideFriction = wheel.sidewaysFriction;
            sideFriction.stiffness = _originalFriction * clampedMultiplier;
            wheel.sidewaysFriction = sideFriction;

            // �ָ�ǰ��Ħ�������ؼ��޸�����
            WheelFrictionCurve forwardFriction = wheel.forwardFriction;
            forwardFriction.stiffness = _originalFriction * clampedMultiplier;
            wheel.forwardFriction = forwardFriction;
        }
    }

    void UpdateDebugInfo()
    {
        if (driveWheels.Length > 0 && driveWheels[0].GetGroundHit(out WheelHit hit))
        {
            slipRatio = hit.forwardSlip;
            wheelRPM = driveWheels[0].rpm;
        }
        Debug.Log($"����Ť��: {_driftCompensation:F2} | ǰ��Ħ��: {allWheels[0].forwardFriction.stiffness:F2}");
        Debug.Log($"Drift״̬: {isDrifting} | Ť��: {_driftCompensation:F4} | " +
                  $"����Ħ����: {allWheels[3].sidewaysFriction.stiffness - _originalFriction:F4}");
        Debug.Log($"SteerInput: {_steeringInput:F4} | RawInput: {Input.GetAxis("Horizontal"):F4}");


    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 450, 300));
        GUILayout.Label($"Speed: {KPH:F1} km/h");
        GUILayout.Label($"Throttle: {currentThrottle:F2}");
        GUILayout.Label($"Brake: {currentBrakeForce:F0} N");
        GUILayout.Label($"Steer Angle: {currentSteerAngle:F1}��");
        GUILayout.Label($"Drifting: {isDrifting}");
        GUILayout.EndArea();
    }
}