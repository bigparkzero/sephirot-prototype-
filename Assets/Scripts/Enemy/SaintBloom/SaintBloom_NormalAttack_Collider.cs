using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaintBloom_NormalAttack_Collider : MonoBehaviour
{
    public float damage = 15f;

    List<GameObject> alreadyHitObjects = new List<GameObject>();

    public GameObject hitEffect;

    private void OnTriggerEnter(Collider other)
    {
        GameObject target = other.transform.root.gameObject;

        if (alreadyHitObjects.Contains(target)) return;

        if (target.layer == LayerMask.NameToLayer("Player"))
        {
            if (TryGetComponent(out Stats targetStats))
            {
                targetStats.Damaged(damage);
            }

            if (TryGetComponent(out Knockback targetKnockback))
            {
                targetKnockback.ApplyKnockback(target.transform.forward * -1f, 5f, 0.4f);
            }

            Instantiate(hitEffect, other.ClosestPoint(transform.position), Quaternion.identity);
        }
        
        alreadyHitObjects.Add(target);
    }

    private void OnDisable()
    {
        alreadyHitObjects.Clear();
    }
}
