using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public float damage = 15f;

    List<GameObject> alreadyHitObjects = new List<GameObject>();

    public GameObject go_hitEffect;
    public GameObject go_owner;

    private void OnTriggerEnter(Collider other)
    {
        GameObject target = other.transform.root.gameObject;

        if (alreadyHitObjects.Contains(target)) return;

        if (target.layer == LayerMask.NameToLayer("Enemy"))
        {
            if (target.TryGetComponent(out Stats targetStats))
            {
                targetStats.Damaged(damage);
            }

            if (target.TryGetComponent(out Knockback targetKnockback))
            {
                targetKnockback.ApplyKnockback(target.transform.position - go_owner.transform.position, 5f, 0.4f);
            }

            //Instantiate(go_hitEffect, other.ClosestPoint(transform.position), Quaternion.identity);
        }

        alreadyHitObjects.Add(target);
    }

    private void OnDisable()
    {
        alreadyHitObjects.Clear();
    }
}