using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class ShootBehaviourScript : MonoBehaviour
{
    InputManager input;
    public CinemachineVirtualCamera _cinemachine;
    Cinemachine3rdPersonFollow ThirdPersonFollow;
    Animator an;
    public MultiAimConstraint rig;
    Camera mainCamera;
    public Transform aimPos;

    void Awake()
    {
        input = GetComponent<InputManager>();
        ThirdPersonFollow = _cinemachine.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
        an = GetComponent<Animator>();
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (input.aiming)
        {
            an.SetBool("aiming", true);
            ThirdPersonFollow.CameraDistance = 1.5f;
            ThirdPersonFollow.CameraSide = 1f;
            ThirdPersonFollow.ShoulderOffset = new Vector3(0.6f,0,0);
            rig.weight = 1f;
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
            ThirdPersonFollow.CameraDistance = 6;
            ThirdPersonFollow.CameraSide = 0.5f;
            ThirdPersonFollow.ShoulderOffset = new Vector3(1f, 0, 0);
            rig.weight = 0f;
        }
    }
}
