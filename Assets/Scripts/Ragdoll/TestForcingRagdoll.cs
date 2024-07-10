using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestForcingRagdoll : MonoBehaviour
{
    public enum Dir
    {
        up,
        down,
        left,
        right,
        forward,
        backward,
    }

    public bool Activate;

    public float power;
    
    public Rigidbody rb;
    public Dir[] dirs;

    // Update is called once per frame
    void Update()
    {
        if (Activate)
        {
            if (rb != null)
            {
                Vector3 d = Vector3.zero;

                foreach (var dir in dirs)
                {
                    switch (dir)
                    {
                        case Dir.up:
                            d += transform.up;
                            break;
                        case Dir.down:
                            d -= transform.up;
                            break;
                        case Dir.left:
                            d -= transform.right;
                            break;
                        case Dir.right:
                            d += transform.right;
                            break;
                        case Dir.forward:
                            d += transform.forward;
                            break;
                        case Dir.backward:
                            d -= transform.forward;
                            break;
                    }
                }

                d.Normalize();

                rb.AddForce(d * power);
            }

            Activate = false;
        }
    }
}
