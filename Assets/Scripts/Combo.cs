using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combo : MonoBehaviour
{
    Animator an;
    public List<GameObject> weapons;
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
            changeWeapon(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            an.SetInteger("weaponID", 1);
            changeWeapon(1);
        }
    }

    public void changeWeapon(int weaponID)
    {
        for (int i = 0; i < weapons.Count; i++)
        {
            weapons[i].SetActive(false);
        }
        weapons[weaponID].SetActive(true);
        an.SetTrigger("changeweapon");
    }
}
