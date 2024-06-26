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

    public GameObject prefab_WireVine;

    //TODO: temp. delete this
    LineRenderer lineRenderer;

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
        if (knockback.IsKnockbacked)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
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
        //TODO: temp. delete this.
        if (!TryGetComponent(out lineRenderer))
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        lineRenderer.enabled = true;
        lineRenderer.positionCount = 2;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.green;
        lineRenderer.endColor = Color.green;
        lineRenderer.startWidth = 0.2f;
        lineRenderer.endWidth = 0.2f;
        //============================================================

        anim.SetBool("IsWireLaunching", true);
        controller.Move(Vector3.zero);

        float dirX = (target.transform.position - transform.position).normalized.x;
        float dirZ = (target.transform.position - transform.position).normalized.z;
        transform.rotation = Quaternion.LookRotation(new Vector3(dirX, 0, dirZ));

        float progress = 0;

        while (progress < WIRE_LAUNCHING_DURATION)
        {
            progress += Time.deltaTime;

            //TODO: temp. delete this.
            lineRenderer.SetPosition(0, transform_RightHand.transform.position);
            lineRenderer.SetPosition(1, target.transform.position);
            //============================

            if (target == null)
            {
                RemoveWire();

                yield break;
            }

            //TODO: wire launching graphic

            yield return null;
        }

        StartCoroutine(PullWire(target));
    }

    IEnumerator PullWire(WireTarget target)
    {
        //TODO: pulling anim. wire curves and stretchs.

        float progress = 0f;

        while (progress < WIRE_PULLING_DURATION)
        {
            progress += Time.deltaTime;

            //TODO: temp. delete this.
            lineRenderer.SetPosition(0, transform_RightHand.transform.position);
            lineRenderer.SetPosition(1, target.transform.position);
            //============================

            if (target == null)
            {
                RemoveWire();

                yield break;
            }

            //TODO: wire pulling graphic

            yield return null;
        }

        RemoveWire();

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

        if (target.transform.root.TryGetComponent(out Knockback targetKnockback))
        {
            targetKnockback.ApplyKnockback(transform.position - target.transform.position, 4);
        }

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

    void RemoveWire()
    {
        //TODO: temp. delete this.
        lineRenderer.enabled = false;
        //===============================

        print("removing wire.");
    }
}
