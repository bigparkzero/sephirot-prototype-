using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

public class CameraController : MonoBehaviour
{
    InputManager input;

    public GameObject CinemachineCameraTarget;

    public float TopClamp;
    public float BottomClamp;

    public float CameraAngleOverride = 0.0f;
    public bool LockCameraPosition = false;


    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;



    private void Awake()
    {
        input = GetComponent<InputManager>();

    }
       
    private void LateUpdate()
    {
        if (input.look.sqrMagnitude >= 0.01f && !LockCameraPosition)
        {
            _cinemachineTargetYaw += input.look.x;
            _cinemachineTargetPitch += input.look.y;
        }

        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

        CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
            _cinemachineTargetYaw, 0.0f);
    }


    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }
}
