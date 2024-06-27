using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combo : MonoBehaviour
{
    Animator an;
    private void Awake()
    {
        an = GetComponent<Animator>();
    }
    private void Update()
    {
        
        if (Input.GetMouseButtonDown(0))
        {
            an.SetTrigger("attack");
<<<<<<< HEAD
<<<<<<< HEAD
            
=======
        }

        if (an.GetCurrentAnimatorStateInfo(0).IsName("basic slash"))
        {
            an.applyRootMotion = true;
        }
        else
        {
            an.applyRootMotion = false;
>>>>>>> parent of 931dd87 (콤보 구현 1)
        }
        if (Input.GetMouseButtonDown(1))
        {
            an.SetTrigger("specialslash");

<<<<<<< Updated upstream
        }
=======
        
>>>>>>> Stashed changes
=======
        }

        if (an.GetCurrentAnimatorStateInfo(0).IsName("basic slash"))
        {
            an.applyRootMotion = true;
        }
        else
        {
            an.applyRootMotion = false;
        }
>>>>>>> parent of 931dd87 (콤보 구현 1)
    }
}
