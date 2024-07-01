using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class ComboReset : StateMachineBehaviour
{
    public List<string> triggerName;
    
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.applyRootMotion = false;
        Debug.Log("false");
    }
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        for (int i = 0; i < triggerName.Count; i++)
        {
            animator.ResetTrigger(triggerName[i]);
        }
        animator.applyRootMotion = true;
        Debug.Log("true");
    }
    public override void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }
   
}
