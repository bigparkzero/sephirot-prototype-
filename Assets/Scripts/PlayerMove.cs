using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

public class PlayerMove : MonoBehaviour
{
    InputManager input;
    CharacterController _controller;
    GameObject mainCamera;
    CameraController _cameraController;

    public float MoveSpeed = 2.0f;
    public float SprintSpeed = 5.335f;
    [Range(0.0f, 0.3f)]
    public float RotationSmoothTime = 0.12f;
    public float SpeedChangeRate = 10.0f;


    public float GroundedRadius = 0.28f;
    public LayerMask GroundLayers;
    public float GroundedOffset = -0.14f;


    public float gravity = -9;
    public float gravityMultiplier;
    public float jumpPower;

    [HideInInspector] public float _speed;
    [HideInInspector] public float _targetRotation = 0.0f;
    [HideInInspector] public float _rotationVelocity;
    [HideInInspector] public float _verticalVelocity;
    [HideInInspector] public float targetSpeed;
    [HideInInspector] public float currentHorizontalSpeed;
    [HideInInspector] public float speedOffset;


    

    private void Awake()
    {
        input = GetComponent<InputManager>();
        _controller = GetComponent<CharacterController>();
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        _cameraController = GetComponent<CameraController>();
    }
    private void Update()
    {
        Move();
        ApplyGravity();
        Jump();
        
    }
    
    private void Move()
    {
        targetSpeed = input.sprint ? SprintSpeed : MoveSpeed;
        if (input.move == Vector2.zero) targetSpeed = 0.0f;
        currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;
        speedOffset = 0.1f;
        if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * input.move.magnitude,
                    Time.deltaTime * SpeedChangeRate);
            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else
            _speed = targetSpeed;
        Vector3 inputDirection = new Vector3(input.move.x, 0.0f, input.move.y).normalized;

        if (input.move != Vector2.zero)
        {
            _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                     mainCamera.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                RotationSmoothTime);        
            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }
        if (input.aiming)
            transform.rotation = Quaternion.Euler(0.0f, mainCamera.transform.eulerAngles.y, 0.0f);
        Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;
        _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                         new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
    }
    private void Jump()
    {
        if (GroundedCheck() && input.jump)
            _verticalVelocity = jumpPower;
        
    }
    private void ApplyGravity()
    {
        if (_controller.isGrounded && _verticalVelocity < 0.01f)
        {
            _verticalVelocity = -0.1f;
        }
        else
        {
            _verticalVelocity += gravity * gravityMultiplier * Time.deltaTime;
        }
    }
    public bool GroundedCheck()
    {
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
            transform.position.z);
        bool Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
            QueryTriggerInteraction.Ignore);

        return Grounded;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawSphere(
            new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
            GroundedRadius);

    }
}
