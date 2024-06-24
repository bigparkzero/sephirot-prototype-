using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WireLauncher : MonoBehaviour
{
    public Transform transform_RightHand;

    public PlayerWireTargetRadar radar;

    float WIRE_LAUNCHING_DURATION = 1f;
    float WIRE_PULLING_DURATION = 0.5f;

    // Start is called before the first frame update
    void Start()
    {

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

            StartCoroutine(LaunchWire(target));
        }
    }

    IEnumerator LaunchWire(WireTarget target)
    {
        //launching anim.

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
        //dash jump start anim. jump state. remove invincibility. can control.

        print("Dash to " + target.transform.root.name + "!");
    }

    void RemoveWire()
    {
        print("removing wire.");
    }
}
