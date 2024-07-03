
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;




[RequireComponent(typeof(CharacterController))]
public class PlayerMoveAnimation : MonoBehaviour
{
    [Header("Player")]
    public float MoveSpeed = 2.0f;
    public float SprintSpeed = 5.335f;
    [Range(0.0f, 0.3f)]
    public float RotationSmoothTime = 0.12f;
    public float SpeedChangeRate = 10.0f;
    public float JumpHeight = 1.2f;
    public float Gravity = -15.0f;
    [Space(10)]
    public float JumpTimeout = 0.50f;
    public float FallTimeout = 0.15f;
    [Header("Player Grounded")]
    public bool Grounded = true;
    public float GroundedOffset = -0.14f;
    public float GroundedRadius = 0.28f;
    public LayerMask GroundLayers;
    [Header("Cinemachine")]
    public GameObject CinemachineCameraTarget;
    public float TopClamp = 70.0f;
    public float BottomClamp = -30.0f;
    public float CameraAngleOverride = 0.0f;
    public bool LockCameraPosition = false;
    public Vector3 CameraOffset;


    // cinemachine
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;
    // player
    private float _speed;
    private float _animationBlend;
    private float _targetRotation = 0.0f;
    private float _rotationVelocity;
    private float _verticalVelocity;
    private float _terminalVelocity = 53.0f;
    // timeout deltatime
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;
    // animation IDs
    private int _animIDSpeed;
    private int _animIDGrounded;
    private int _animIDJump;
    private int _animIDFreeFall;
    private int _animIDMotionSpeed;
    private int _animIDPlayerAngle;

    private Quaternion LerpTargetPos;


    private Animator an;
    private CharacterController _controller;
    private GameObject _mainCamera;
    private LockOn lockon;

    private const float _threshold = 0.01f;

    public bool jump;
    public Vector2 move;
    public bool sprint;
    public Vector2 look;

    private void Awake()
    {
        // get a reference to our main camera
        if (_mainCamera == null)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }
    }

    private void Start()
    {
        _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;

        an = GetComponent<Animator>();
        _controller = GetComponent<CharacterController>();

        AssignAnimationIDs();

        // reset our timeouts on start
        _jumpTimeoutDelta = JumpTimeout;
        _fallTimeoutDelta = FallTimeout;
         
        lockon = GetComponent<LockOn>();
    }

    public void ApplyRootMotion(int RootMotion)
    {
        an.applyRootMotion = RootMotion == 1 ? true : false;
    }
    private void Update()
    {

        JumpAndGravity();
        GroundedCheck();
        Move();
        input();
        roll();
    }

    private void LateUpdate()
    {
        CameraRotation();
    }

    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        _animIDPlayerAngle = Animator.StringToHash("PlayerAngle");
    }
    void input()
    {
        jump = Input.GetKeyDown(KeyCode.Space);
        move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        sprint = Input.GetKey(KeyCode.LeftShift);
        look = new Vector2(Input.GetAxisRaw("Mouse X"), -Input.GetAxisRaw("Mouse Y"));
    }
    private void GroundedCheck()
    {
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
            transform.position.z);
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
            QueryTriggerInteraction.Ignore);
        an.SetBool(_animIDGrounded, Grounded);
    }
    private void CameraRotation()
    {
        CinemachineCameraTarget.transform.position = transform.position + CameraOffset;

        if (look.sqrMagnitude >= _threshold && !LockCameraPosition)
        {
            _cinemachineTargetYaw += look.x;
            _cinemachineTargetPitch += look.y;
        }
        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

        if (lockon.isLockOn && lockon.currentTarget != null)
        {
            Vector3 direction = (lockon.currentTarget.targetPos.transform.position - CinemachineCameraTarget.transform.position).normalized;
            direction.y += lockon.cameraDirectionY;
            CinemachineCameraTarget.transform.forward = Vector3.Lerp
                (CinemachineCameraTarget.transform.forward, direction, Time.deltaTime * lockon.cameraDirectionSmoothTime);
            Vector3 camAngles = _mainCamera.transform.eulerAngles;
            _cinemachineTargetPitch = camAngles.x > TopClamp ? camAngles.x - 360 : camAngles.x;
            _cinemachineTargetYaw = camAngles.y;
        }
        else
        {
            CinemachineCameraTarget.transform.eulerAngles = new Vector3(_cinemachineTargetPitch + CameraAngleOverride,
                _cinemachineTargetYaw, 0.0f);
        }
    }

    private void Move()
    {
        float targetSpeed = sprint ? SprintSpeed : MoveSpeed;
        if (move == Vector2.zero) targetSpeed = 0.0f;
        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;
        float speedOffset = 0.1f;
        if (currentHorizontalSpeed < targetSpeed - speedOffset ||
            currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed,
                Time.deltaTime * SpeedChangeRate);
            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else
        {
            _speed = targetSpeed;
        }
        _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
        if (_animationBlend < 0.01f) _animationBlend = 0f;
        Vector3 inputDirection = new Vector3(move.x, 0.0f, move.y).normalized;
       
        if (move != Vector2.zero)
        {
            _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                              _mainCamera.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                RotationSmoothTime);
            if (!an.applyRootMotion)
            {
                if (lockon.isLockOn)
                {
                    transform.rotation = Quaternion.Euler(0.0f, _mainCamera.transform.eulerAngles.y, 0.0f);
                }
                else
                {
                    transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                }
            }
        }
        if (an.applyRootMotion && lockon.currentTarget != null)
        {
            Vector3 lookvetor = lockon.currentTarget.transform.position - transform.position;
            if (Mathf.Abs(lookvetor.magnitude) >= 4 +lockon.currentTarget.transform.localScale.x)
            {
                if (Mathf.Abs(lookvetor.magnitude) < 25 + lockon.currentTarget.transform.localScale.x)
                {
                    _controller.Move(new Vector3( lookvetor.x,0, lookvetor.z).normalized * Time.deltaTime * 50);
                }
            }
            
            print(Mathf.Abs(lookvetor.magnitude));
                
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0.0f, Mathf.Atan2(lookvetor.normalized.x, lookvetor.normalized.z) * Mathf.Rad2Deg, 0.0f), 0.1f);
            if (Input.GetMouseButtonDown(1))
            {
                transform.rotation = Quaternion.Euler(0.0f, Mathf.Atan2(lookvetor.normalized.x, lookvetor.normalized.z) * Mathf.Rad2Deg, 0.0f);
            }
        }
        Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;
        if (!an.applyRootMotion)
        {
            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
        }
        an.SetFloat(_animIDSpeed, _animationBlend);
        print(Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg - _mainCamera.transform.eulerAngles.y);
        float velocity = 2;
        an.SetFloat(_animIDPlayerAngle, lockon.isLockOn ? Mathf.SmoothDamp(an.GetFloat(_animIDPlayerAngle),
                                                                           _targetRotation - _mainCamera.transform.eulerAngles.y,
                                                                           ref velocity, 0.05f) : 0);
        
    }

    void roll()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            an.SetTrigger("roll");
        }
    }
    public void applyRootMotion(int a)
    {
        an.applyRootMotion = a == 1 ? true : false; 
    }

    private void JumpAndGravity()
    {
        if (_controller.isGrounded)
        {
            _fallTimeoutDelta = FallTimeout;
                an.SetBool(_animIDJump, false);
                an.SetBool(_animIDFreeFall, false);
            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2f;
            }
            if (jump && _jumpTimeoutDelta <= 0.0f)
            {
                _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
                an.SetBool(_animIDJump, true);
            }

            if (_jumpTimeoutDelta >= 0.0f)
            {
                _jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else
        {
            _jumpTimeoutDelta = JumpTimeout;
            if (_fallTimeoutDelta >= 0.0f)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                an.SetBool(_animIDFreeFall, true);
            }
            jump = false;
        }
        if (_verticalVelocity < _terminalVelocity)
        {
            _verticalVelocity += Gravity * Time.deltaTime;
        }
    }
    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    private void OnDrawGizmosSelected()
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);
        if (Grounded) Gizmos.color = transparentGreen;
        else Gizmos.color = transparentRed;
        Gizmos.DrawSphere(
            new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
            GroundedRadius);
    }
}

