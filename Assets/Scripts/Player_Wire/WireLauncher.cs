using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class WireLauncher : MonoBehaviour
{
    public Transform transform_RightHand;

    public PlayerWireTargetRadar radar;

    PlayerMoveAnimation move;
    Animator anim;
    CharacterController controller;
    Knockback knockback;

    float WIRE_LAUNCHING_DURATION = 0.5f;
    float WIRE_PULLING_DURATION = 0.5f;

    public WireShaderController wireController;

    float wireCurrentCooldown;
    float WIRE_COOLDOWN = 1.5f;

    // Start is called before the first frame update
    void Start()
    {
        move = GetComponent<PlayerMoveAnimation>();
        anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        knockback = GetComponent<Knockback>();
    }

    // Update is called once per frame
    void Update()
    {
        if (wireCurrentCooldown > 0)
        {
            wireCurrentCooldown -= Time.deltaTime;
            return;
        }

        if (knockback.IsKnockbacked)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            wireCurrentCooldown = WIRE_COOLDOWN;

            GameObject targetPoint = radar.closest;

            if (targetPoint == null)
            {
                return;
            }

            WireTarget target = targetPoint.GetComponent<WireTarget>();

            //TODO: invincible state, cannot control

            move.isWireActivated = true;

            StartCoroutine(LaunchWire(target));
        }
    }

    IEnumerator LaunchWire(WireTarget target)
    {
        anim.SetBool("IsWireLaunching", true);
        controller.Move(Vector3.zero);

        float dirX = (target.transform.position - transform.position).normalized.x;
        float dirZ = (target.transform.position - transform.position).normalized.z;
        transform.rotation = Quaternion.LookRotation(new Vector3(dirX, 0, dirZ));

        float progress = 0;

        float distance = Vector3.Distance(target.transform.position, transform_RightHand.transform.position);
        float wireSpeed = distance / WIRE_LAUNCHING_DURATION;

        wireController.ActivateWire(target.transform, wireSpeed, WIRE_LAUNCHING_DURATION, WIRE_PULLING_DURATION);

        while (progress < WIRE_LAUNCHING_DURATION)
        {
            progress += Time.deltaTime;

            if (target == null)
            {
                DoForcedDissolveWire();

                yield break;
            }

            yield return null;
        }

        if (target.isPullable)
        {
            if (target.transform.root.TryGetComponent(out Knockback targetKnockback))
            {
                targetKnockback.ApplyKnockback(transform.position - target.transform.position, 4);
            }
        }

        StartCoroutine(PullWire(target));
    }

    IEnumerator PullWire(WireTarget target)
    {
        float progress = 0f;

        while (progress < WIRE_PULLING_DURATION)
        {
            progress += Time.deltaTime;

            if (target == null)
            {
                DoForcedDissolveWire();

                yield break;
            }

            yield return null;
        }

        if (target.isPullable)
        {
            PullEnemy(target);
        }
        else
        {
            DashTo(target);
        }
    }

    void PullEnemy(WireTarget target)
    {
        //TODO: remove invincibility. can control.

        anim.SetBool("IsWireLaunching", false);
        move.isWireActivated = false;
    }

    void DashTo(WireTarget target)
    {
        //TODO: remove invincibility.

        anim.SetBool("IsWireDashing", true);
        anim.SetBool("IsWireLaunching", false);
        Physics.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Enemy"));
        move.WireDash(target);
    }

    void DoForcedDissolveWire()
    {
        wireController.DoForcedDissolve();
    }
}
