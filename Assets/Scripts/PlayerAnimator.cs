using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

public class PlayerAnimator : MonoBehaviour
{
    PlayerMove _player;
    Animator an;
    InputManager input;
    private void Awake()
    {
        input = GetComponent<InputManager>();
        _player = GetComponent<PlayerMove>();
        an = GetComponent<Animator>();
        AssignAnimationIDs();
    }
    private int _animIDSpeed;
    private int _animIDGrounded;
    private int _animIDJump;
    private int _animIDFreeFall;
    private int _animIDMotionSpeed;
    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
    }



    float _animationBlend;
    float aimingoffset;
    void Update()
    {
        //걷기 & 뛰기 애니메이션 제어 
        _animationBlend = Mathf.Lerp(_animationBlend, _player.targetSpeed, Time.deltaTime * _player.SpeedChangeRate);
        if (_animationBlend < 0.01f) _animationBlend = 0f;
        an.SetFloat(_animIDSpeed, _animationBlend);
        an.SetFloat(_animIDMotionSpeed, input.move.normalized.magnitude);
        if (input.roll)
        {
            an.SetTrigger("roll");
        }

        if (an.GetCurrentAnimatorStateInfo(0).IsName("roll"))
        {
            an.applyRootMotion = true;
        }
        else
        {
            an.applyRootMotion = false;
        }
    }
}
