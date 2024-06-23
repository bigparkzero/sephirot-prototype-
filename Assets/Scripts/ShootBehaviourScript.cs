using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class ShootBehaviourScript : MonoBehaviour
{
    public float defaultCameraDistance;
    public float defaultCameraSide;
    public Vector3 defaultShoulderOffset;

    public float aimingCameraDistance;
    public float aimingCameraSide;
    public Vector3 aimingShoulderOffset;

    public float smoothspeed;
    Animator an;
    Camera mainCamera;
    Cinemachine3rdPersonFollow ThirdPersonFollow;
    public CinemachineVirtualCamera _cinemachine;
    public MultiAimConstraint rig;
    public Transform aimPos;

    void Awake()
    {
        ThirdPersonFollow = _cinemachine.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
        an = GetComponent<Animator>();
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(1))
        {
            an.SetBool("aiming", true);
            Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100))
            {
                aimPos.position = hit.point;
            }
            else
            {
                Vector3 maxDistancePoint = ray.origin + ray.direction * 100;
                aimPos.position = maxDistancePoint;
            }
        }
        else
        {
            an.SetBool("aiming", false);
        }
        rig.weight = Mathf.Lerp(rig.weight, Input.GetMouseButton(1) ? 1 : 0, smoothspeed);
        ThirdPersonFollow.CameraDistance = Mathf.Lerp(ThirdPersonFollow.CameraDistance, Input.GetMouseButton(1) ? aimingCameraDistance : defaultCameraDistance, smoothspeed);
        ThirdPersonFollow.CameraSide = Mathf.Lerp(ThirdPersonFollow.CameraSide, Input.GetMouseButton(1) ? aimingCameraSide : defaultCameraSide, smoothspeed);
        ThirdPersonFollow.ShoulderOffset = Vector3.Lerp(ThirdPersonFollow.ShoulderOffset, Input.GetMouseButton(1) ? aimingShoulderOffset : defaultShoulderOffset, smoothspeed);
    }
}
