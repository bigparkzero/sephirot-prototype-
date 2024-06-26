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
            
        }
        if (Input.GetMouseButtonDown(1))
        {
            an.SetTrigger("specialslash");

<<<<<<< Updated upstream
        }
=======
        
>>>>>>> Stashed changes
    }
    public int AttackCount
    {
        get { return an.GetInteger("attackcount"); }
        set { an.SetInteger("attackcount",value); }
    }
}
