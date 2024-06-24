using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WireLauncher : MonoBehaviour
{
    public Transform transform_RightHand;

    public PlayerWireTargetRadar radar;

    PlayerMoveAnimation move;

    float WIRE_LAUNCHING_DURATION = 0.3f;
    float WIRE_PULLING_DURATION = 0.15f;

    // Start is called before the first frame update
    void Start()
    {
        move = GetComponent<PlayerMoveAnimation>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            GameObject targetPoint = radar.closest;

            if (targetPoint == null)
            {
                return;
            }

            WireTarget target = targetPoint.GetComponent<WireTarget>();

            //invincible state, cannot control

            move.isWireActivated = true;

            StartCoroutine(LaunchWire(target));
        }
    }

    IEnumerator LaunchWire(WireTarget target)
    {
        //launching anim.

        float dirY = (target.transform.position - transform.position).normalized.y;
        transform.rotation = Quaternion.Euler(0, dirY, 0);

        float progress = 0;

        while (progress < WIRE_LAUNCHING_DURATION)
        {
            progress += Time.deltaTime;

            if (target == null)
            {
                RemoveWire();

                yield break;
            }

            //wire launching graphic

            yield return null;
        }

        StartCoroutine(PullWire(target));
    }

    IEnumerator PullWire(WireTarget target)
    {
        //pulling anim. wire curves and stretchs.

        float progress = 0f;

        while (progress < WIRE_PULLING_DURATION)
        {
            progress += Time.deltaTime;

            if (target == null)
            {
                RemoveWire();

                yield break;
            }

            //wire pulling graphic

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
        //make enemy pulled. remove invincibility. can control.

        print(target.transform.root.name + " is pulled!");
    }

    void DashTo(WireTarget target)
    {
        move.WireDash(target);
    }

    void RemoveWire()
    {
        print("removing wire.");
    }
}
