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
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            an.SetInteger("weaponID", 0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            an.SetInteger("weaponID", 1);
        }
    }

    public void changeWeapon(int weaponID)
    {
        switch (weaponID)
        {
            case 0:
                //Ä®
                break;
            case 1:
                //µµ³¢
                break;
        }
    }
}
