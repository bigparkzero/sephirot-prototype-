using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WireConstraint : MonoBehaviour
{
    public Transform[] bones;

    [HideInInspector]
    public Transform firstBonePin;
    
    [HideInInspector]
    public Transform lastBonePin;

    [HideInInspector]
    public bool isFirst;

    [HideInInspector]
    public float speed;

    [HideInInspector]
    public bool isOn;

    float segmentLength;

    private void Start()
    {
        segmentLength = Vector3.Distance(bones[0].position, bones[1].position);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            isOn = !isOn;
        }

        if (!isOn)
        {
            return;
        }

        ApplyConstraints();
    }

    void ApplyConstraints()
    {

        for (int i = 0; i < bones.Length - 1; i++)
        {
            Transform bone = bones[i];
            Transform next = bones[i + 1];

            Vector3 delta = next.position - bone.position;
            float distance = delta.magnitude;
            float difference = (distance - segmentLength) / distance;
            Vector3 adjustment = delta * 0.5f * difference;

            bone.position += adjustment;
            next.position -= adjustment;

            if (i < bones.Length - 1)
            {
                //bones[i].rotation = Quaternion.LookRotation(delta);
            }

            if (i == 0 && firstBonePin != null)
            {
                Vector3 moveTo = firstBonePin.position - bone.position;
                bone.position += moveTo * speed;
                bone.LookAt(next);
            }

            if (lastBonePin != null && i == bones.Length - 2)
            {
                Vector3 diff = lastBonePin.position - next.position;
                next.position += diff;
                next.LookAt(bones[i]);
            }
        }
    }
}
