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
    public float coastDrag = 0.3f;  // 优化直线滑行
    public float idleDrag = 1f;     // 优化怠速阻力

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

    [Header("Drift Recovery - 新增修复参数")]
    public float frictionRecoverySpeed = 8f;    // 摩擦力恢复速度
    public float stabilityRecoverySpeed = 5f;  // 扭矩清除速度
    private float _driftCompensation;          // 当前漂移补偿力

    [Header("Nitro System")]
    public NitroSystem nitroSystem;  // 引用 NitroSystem

    [Header("输入源（留空则自动找 PlayerInput）")]
    public MonoBehaviour inputSource;  // Inspector 里拖 PlayerInput 或 AIInput

    private IDriverInput _driver;

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
    public Rigidbody GetRigidbody()
    {
        return _rb;
    }
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.mass = 1200f;
        _rb.angularDrag = 4f;
        _rb.centerOfMass = new Vector3((float)0.33, -0.4f, 0);

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

        // 如果车辆有 NitroSystem 脚本，就赋值给 nitroSystem
        if (nitroSystem == null)
        {
            nitroSystem = GetComponent<NitroSystem>();  // 通过 GetComponent 查找
        }
        if (inputSource != null && inputSource is IDriverInput di)
            _driver = di;
        else
            _driver = GetComponent<PlayerInput>();  // 或 new PlayerInput()，用同物体上的
    }

    void Update()
    {
        //_handbrake = Input.GetKey(KeyCode.LeftShift) && Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f;
        _handbrake = _driver.Handbrake;
    }

    void FixedUpdate()
    {
        //float throttleInput = Input.GetAxis("Vertical");
        //float steerInput = Input.GetAxis("Horizontal");
        //bool brakeInput = Input.GetKey(KeyCode.Space);
        // 改成从 _driver 拿：
        float throttleInput = _driver.Throttle;
        float steerInput = _driver.Steer;
        bool brakeInput = _driver.Brake;

        _targetThrottle = Mathf.Lerp(_targetThrottle, throttleInput, Time.fixedDeltaTime * throttleResponse);
        _targetBrake = Mathf.Lerp(_targetBrake, brakeInput ? 1f : 0f, Time.fixedDeltaTime / brakeResponseTime);

        currentThrottle = _targetThrottle;
        currentBrakeForce = _targetBrake * maxBrakeForce;

        // 如果氮气系统存在并且激活，就应用氮气加速
        if (nitroSystem != null && nitroSystem.IsNitroActive())
        {
            HandleNitroBoost();  // 使用氮气加速
        }

        HandleSteering(steerInput);
        HandleMotor();
        HandleBrakes();
        HandleDrift();
        HandleAutoBrake();
        HandleDrag();

        // 仅在没有激活氮气时，才应用最大速度限制
        if (nitroSystem == null || !nitroSystem.IsNitroActive())
        {
            LimitMaxSpeed();  // 限制最大速度
        }

        UpdateDebugInfo();
    }


    void HandleNitroBoost()
    {
        // 增加氮气加速效果
        _rb.AddForce(transform.forward * nitroSystem.nitroBoost, ForceMode.Impulse);  // 使用 Impulse，使其为瞬时力
    }
    void HandleSteering(float rawInput)
    {
        // === 自动归零阈值修正 ===
        if (Mathf.Abs(rawInput) < 0.01f)
        {
            _steeringInput = Mathf.Lerp(_steeringInput, 0f, Time.fixedDeltaTime * 6f);
            _steeringInputVelocity = 0f; // 防止持续漂移
        }
        else
        {
            _steeringInput = Mathf.SmoothDamp(_steeringInput, rawInput, ref _steeringInputVelocity, 0.1f);
        }

        // === 后续逻辑不变 ===
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

    // === 这里是 HandleDrift() 的新版本 ===
    void HandleDrift()
    {
        // 检查是否按下了 Shift 和方向输入
        if (_handbrake && Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f && KPH > minDriftSpeed)
        {
            // 漂移激活逻辑
            foreach (var wheel in allWheels)
            {
                if (System.Array.IndexOf(steerWheels, wheel) < 0)
                {
                    // 后轮打滑
                    var friction = wheel.sidewaysFriction;
                    friction.stiffness = rearWheelSlip;
                    wheel.sidewaysFriction = friction;
                }
                else
                {
                    // 前轮摩擦力增强
                    var friction = wheel.sidewaysFriction;
                    friction.stiffness = _originalFriction * 1.2f; // 比正常前轮稍微更抓地
                    wheel.sidewaysFriction = friction;
                }
            }

            // 计算车辆的局部速度，基于车速和输入的方向来计算漂移的补偿
            Vector3 localVel = transform.InverseTransformDirection(_rb.velocity);

            // 实时根据方向输入更新漂移补偿
            float directionMultiplier = 0f;

            // 如果按下了左方向键（A），漂移补偿反向
            if (Input.GetAxis("Horizontal") < 0)
            {
                directionMultiplier = -1f;  // 左
            }
            // 如果按下了右方向键（D），漂移补偿正常
            else if (Input.GetAxis("Horizontal") > 0)
            {
                directionMultiplier = 1f;  // 右
            }

            // 更新漂移补偿方向
            _driftCompensation = localVel.x * driftStability * _rb.mass * 0.01f * directionMultiplier;

            // 施加漂移补偿
            _rb.AddTorque(transform.up * _driftCompensation, ForceMode.Force);

            isDrifting = true;
        }
        else if (isDrifting)
        {
            // 精确恢复检测
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
                    // 恢复前轮摩擦力
                    var friction = wheel.sidewaysFriction;
                    friction.stiffness = Mathf.Lerp(friction.stiffness, _originalFriction,
                        Time.fixedDeltaTime * frictionRecoverySpeed);
                    wheel.sidewaysFriction = friction;
                }
            }

            // 扭矩清零
            if (!torqueCleared)
            {
                _driftCompensation = Mathf.Lerp(_driftCompensation, 0,
                    Time.fixedDeltaTime * stabilityRecoverySpeed * 2f);
                _rb.AddTorque(transform.up * -_driftCompensation, ForceMode.Force);
                torqueCleared = Mathf.Abs(_driftCompensation) < 0.01f;
            }

            // 状态退出
            isDrifting = !(frictionRestored && torqueCleared);

            if (!isDrifting)
            {
                _driftCompensation = 0;
                UpdateWheelFriction(1f);
            }
        }

        // 增加方向恢复的平滑度
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

    // 修复点4：完全恢复轮胎摩擦力
    void UpdateWheelFriction(float multiplier)
    {
        float clampedMultiplier = Mathf.Clamp01(multiplier);
        foreach (var wheel in allWheels)
        {
            // 恢复侧向摩擦力
            WheelFrictionCurve sideFriction = wheel.sidewaysFriction;
            sideFriction.stiffness = _originalFriction * clampedMultiplier;
            wheel.sidewaysFriction = sideFriction;

            // 恢复前向摩擦力（关键修复！）
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
    }

}