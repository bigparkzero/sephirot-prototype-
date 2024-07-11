using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaintBloom_NormalAttack1 : EnemySkillEffectBase
{
    struct TransformData
    {
        public Vector3 pos;
        public Quaternion rot;
    }

    int idx = 0;

    float slashTime;

    public Transform transform_effect;
    public Transform transform_normalAttack1_1and3;
    public Transform transform_normalAttack1_2;

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

        idx = Random.Range(0, 3);

        switch (idx)
        {
            case 0:
                currentEffectWorld.pos = transform_normalAttack1_1and3.position;
                currentEffectWorld.rot = transform_normalAttack1_1and3.rotation;
                slashTime = 1.42f;
                anim.SetInteger("ActionIndex", 1);
                break;
            case 1:
                currentEffectWorld.pos = transform_normalAttack1_2.position;
                currentEffectWorld.rot = transform_normalAttack1_2.rotation;
                slashTime = 1.12f;
                anim.SetInteger("ActionIndex", 2);
                break;
            case 2:
                currentEffectWorld.pos = transform_normalAttack1_1and3.position;
                currentEffectWorld.rot = transform_normalAttack1_1and3.rotation;
                slashTime = 0.75f;
                anim.SetInteger("ActionIndex", 3);
                break;
        }

        colliderObject.transform.SetParent(null);

        durationTimer = DURATION;
    }

    public override void OnPlaying()
    {
        base.OnPlaying();

        if (!isEffectOn && DURATION - durationTimer >= slashTime)
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

        if (!isForcedStop)
        {
            Stats ownerStats = owner.GetComponent<Stats>();
            if (ownerStats.HP <= ownerStats.maxHP / 2)
            {
                skills.ActivateSkillChain();
            }
            else
            {
                skills.ExitSkill();
                owner.GetComponent<Knockback>().EnableKnockback();
            }
        }
        else
        {
            skills.ExitSkill();
            owner.EndAttack();

            owner.GetComponent<Knockback>().EnableKnockback();
        }
    }
}
