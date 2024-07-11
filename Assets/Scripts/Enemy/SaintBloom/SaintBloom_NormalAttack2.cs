using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaintBloom_NormalAttack2 : EnemySkillEffectBase
{
    struct TransformData
    {
        public Vector3 pos;
        public Quaternion rot;
    }

    public Transform transform_effect;
    public Transform transform_normalAttack2;

    bool isEffectOn;

    TransformData currentEffectWorld;
    TransformData originColliderLocal;

    private void Awake()
    {
        originColliderLocal.pos = colliderObject.transform.localPosition;
        originColliderLocal.rot = colliderObject.transform.localRotation;
    }


    public override void OnActivate()
    {
        base.OnActivate();

        owner.GetComponent<Knockback>().DisableKnockback();

        currentEffectWorld.pos = transform_normalAttack2.position;
        currentEffectWorld.rot = transform_normalAttack2.rotation;

        colliderObject.transform.SetParent(null);

        anim.SetInteger("ActionIndex", 4);

        durationTimer = DURATION;
    }

    public override void OnPlaying()
    {
        base.OnPlaying();

        if (!isEffectOn && DURATION - durationTimer >= 1.12f)
        {
            transform_effect.position = currentEffectWorld.pos;
            transform_effect.rotation = currentEffectWorld.rot;

            transform_effect.gameObject.SetActive(true);

            colliderObject.SetActive(true);

            isEffectOn = true;
        }

        if (isEffectOn)
        {
            transform_effect.position = currentEffectWorld.pos;
            transform_effect.rotation = currentEffectWorld.rot;
        }
    }

    public override void OnExit(bool isForcedStop)
    {
        base.OnExit(isForcedStop);

        isEffectOn = false;

        transform_effect.gameObject.SetActive(false);

        colliderObject.transform.SetParent(transform);
        colliderObject.transform.localPosition = originColliderLocal.pos;
        colliderObject.transform.localRotation = originColliderLocal.rot;
        colliderObject.SetActive(false);

        owner.GetComponent<Knockback>().EnableKnockback();
    }
}
