using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyState))]
public class Stats : MonoBehaviour
{
    EnemyState enemyState;

    float hp;
    public float maxHP = -1;

    // Start is called before the first frame update
    void Start()
    {
        enemyState = GetComponent<EnemyState>();

        if (maxHP < 0)
        {
            throw new System.Exception("Max HP can not be below zero. Please check inspector values of this GameObject(" + gameObject.name + ")'s Component(" + typeof(Stats).Name + ")");
        }

        hp = maxHP;
    }

    public void Damaged(float damage)
    {
        hp = Mathf.Max(0, hp - damage);

        if (hp <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        enemyState.Die();
    }
}
