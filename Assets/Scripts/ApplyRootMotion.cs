using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyRootMotion : StateMachineBehaviour
{
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.applyRootMotion = false;
        animator.GetComponent<PlayerMoveAnimation>().dontmove = false;
    }
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.applyRootMotion = true;
        animator.GetComponent<PlayerMoveAnimation>().dontmove = true;
    }
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.applyRootMotion = true;
        animator.GetComponent<PlayerMoveAnimation>().dontmove = true;
    }
}
