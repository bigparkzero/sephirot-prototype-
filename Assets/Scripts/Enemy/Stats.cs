using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stats : MonoBehaviour
{
    float hp;
    public float maxHP = -1;

    public bool IsDead
    {
        get
        {
            return hp <= 0;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (maxHP < 1)
        {
            throw new System.Exception("Max HP can not be below 1. Please check inspector values of this GameObject(" + gameObject.name + ")'s Component(" + typeof(Stats).Name + ")");
        }

        hp = maxHP;
    }

    public void Healed(float heal)
    {
        if (IsDead) return;

        hp = Mathf.Min(hp + heal, maxHP);
    }

    public void Damaged(float damage)
    {
        if (IsDead) return;

        hp = Mathf.Max(0, hp - damage);
    }
}