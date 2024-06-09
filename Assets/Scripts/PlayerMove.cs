using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

public class PlayerMove : MonoBehaviour
{
    InputManager input;
    CharacterController _controller;
    GameObject mainCamera;


    public float MoveSpeed = 2.0f;

    public float SprintSpeed = 5.335f;

    [Range(0.0f, 0.3f)]
    public float RotationSmoothTime = 0.12f;

    public float SpeedChangeRate = 10.0f;

    private float _speed;
    private float _targetRotation = 0.0f;
    private float _rotationVelocity;
    private float _verticalVelocity;
    private float _terminalVelocity = 53.0f;



    private void Awake()
    {
        input = GetComponent<InputManager>();
        _controller = GetComponent<CharacterController>();
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
    }
    private void Update()
    {
        Move();
    }
    private void Move()
    {
        float targetSpeed = input.sprint ? SprintSpeed : MoveSpeed;

        if (input.move == Vector2.zero) targetSpeed = 0.0f;

        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

        float speedOffset = 0.1f;

        if (currentHorizontalSpeed < targetSpeed - speedOffset ||
            currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * input.move.magnitude,
                    Time.deltaTime * SpeedChangeRate);

            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else
        {
            _speed = targetSpeed;
        }

        Vector3 inputDirection = new Vector3(input.move.x, 0.0f, input.move.y).normalized;

        if (input.move != Vector2.zero)
        {
            _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                              mainCamera.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                RotationSmoothTime);

            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }


        Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

        _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                         new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

       
    }
}
