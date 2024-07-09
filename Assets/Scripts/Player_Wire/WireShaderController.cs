using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WireShaderController : MonoBehaviour
{
    Renderer r;
    Material mat;

    bool isGrowing;
    bool isShrinking;
    bool isDissolving;
    float growthTime;
    float shrinkTime;
    float dissolveTime;
    float GROW_DURATION;
    float SHRINK_DURATION;
    //current growth
    //grow speed

    public Transform hand;
    Transform target;

    float vineLength;

    float shrinkCurrentSize;
    float SHRINK_SIZE = 0.2f;

    // Start is called before the first frame update
    void Start()
    {
        r = GetComponentInChildren<Renderer>();
        mat = r.material;

        vineLength = r.bounds.size.z;
        r.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        mat.SetVector("_TopWorldPos", transform.position);

        if (target != null)
        {
            float distance = Vector3.Distance(target.position, transform.position);
            float size = distance / vineLength;
            float x = transform.localScale.x;
            float y = transform.localScale.y;

            if (isShrinking)
            {
                x -= shrinkCurrentSize * Time.deltaTime;
                y -= shrinkCurrentSize * Time.deltaTime;
                shrinkCurrentSize -= Time.deltaTime;
            }

            transform.localScale = new Vector3(x, y, size);
        }

        if ((target == null || hand == null) && (isGrowing || isShrinking))
        {
            isGrowing = false;
            isShrinking = false;
            DissolveWire();
        }

        if (isGrowing)
        {
            transform.position = hand.position;
            transform.LookAt(target);
            growthTime += Time.deltaTime;
            mat.SetFloat("_GrowthTime", growthTime);

            if (growthTime > GROW_DURATION)
            {
                isGrowing = false;
                ShrinkWire();
            }
        }

        if (isShrinking)
        {
            transform.position = hand.position;
            transform.LookAt(target);
            shrinkTime += Time.deltaTime;

            if (shrinkTime > SHRINK_DURATION)
            {
                isShrinking = false;
                DissolveWire();
            }
        }

        if (isDissolving)
        {
            dissolveTime += Time.deltaTime;
            mat.SetFloat("_DissolveTime", dissolveTime);

            if (dissolveTime > 1)
            {
                isDissolving = false;
                mat.SetFloat("_IsDissolving", 0);
                target = null;
                r.enabled = false;
            }
        }
    }

    public void ActivateWire(Transform t, float growSpeed, float growDuration, float shrinkTime)
    {
        isShrinking = false;
        isDissolving = false;
        mat.SetFloat("_IsDissolving", 0f);

        r.enabled = true;

        transform.position = hand.position;
        transform.LookAt(t);
        target = t;
        growthTime = 0;
        GROW_DURATION = growDuration;
        SHRINK_DURATION = shrinkTime;

        float distance = Vector3.Distance(target.position, transform.position);
        float size = distance / vineLength;
        transform.localScale = new Vector3(1, 1, size);
        float growingSpeed = distance / GROW_DURATION;

        mat.SetFloat("_GrowthTime", growthTime);
        mat.SetFloat("_GrowSpeed", growingSpeed);

        isGrowing = true;
    }

    void ShrinkWire()
    {
        shrinkTime = 0;
        transform.localScale = new Vector3(transform.localScale.x - 0.2f, transform.localScale.y - 0.2f, transform.localScale.z);

        shrinkCurrentSize = SHRINK_SIZE; 
        isShrinking = true;
    }

    void DissolveWire()
    {
        dissolveTime = 0;
        isDissolving = true;
        mat.SetFloat("_IsDissolving", 1f);
    }

    public void DoForcedDissolve()
    {
        isGrowing = false;
        isShrinking = false;
        DissolveWire();
    }
}
