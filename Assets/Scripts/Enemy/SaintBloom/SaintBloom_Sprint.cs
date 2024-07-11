using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaintBloom_Sprint : EnemySkillEffectBase
{
    struct TransformData
    {
        public Vector3 pos;
        public Quaternion rot;
    }

    static int dashIndex = 0;
    float dashTime;
    float DASH_SPEED = 20f;
    float FINAL_DASH_SPEED = 35f;

    public Transform transform_effect;
    public Transform transform_effect3;
    public Transform transform_sprint;
    public SaintBloom_Sprint_Collider script_colliderScipt;

    GameObject currentEffect;
    Transform destinationTransform;

    bool isEffectOn;

    TransformData originColliderLocal;
    Vector3 colliderDiffFromOwner;

    private void Awake()
    {
        originColliderLocal.pos = colliderObject.transform.localPosition;
        originColliderLocal.rot = colliderObject.transform.localRotation;
    }

    public override void OnActivate()
    {
        base.OnActivate();

        colliderDiffFromOwner = colliderObject.transform.position - owner.transform.position;

        owner.GetComponent<Knockback>().DisableKnockback();

        int sprintDistance = 28;

        if (dashIndex == 2)
        {
            isChainable = false;
            sprintDistance += 10;
            currentEffect = transform_effect3.gameObject;
        }
        else
        {
            isChainable = true;
            currentEffect = transform_effect.gameObject;
        }

        Vector3 targetPos = owner.transform.position + owner.transform.forward * sprintDistance;

        NNInfo nearestNode = AstarPath.active.GetNearest(targetPos);
        Vector3 validPosition = nearestNode.position;

        destinationTransform = owner.GetComponent<AIDestinationSetter>().target;
        destinationTransform.SetParent(null);
        destinationTransform.position = validPosition;

        currentEffect.SetActive(true);
        colliderObject.SetActive(true);

        colliderObject.transform.SetParent(null);
        
        float dist = Vector3.Distance(targetPos, owner.transform.position);
        
        if (dashIndex < 2)
        {
            script_colliderScipt.damage = 20f;
            owner.GetComponent<AIPath>().maxSpeed = DASH_SPEED;
            dashTime = dist / DASH_SPEED;
            anim.SetInteger("ActionIndex", 6);
        }
        else
        {
            script_colliderScipt.damage = 40f;
            owner.GetComponent<AIPath>().maxSpeed = FINAL_DASH_SPEED;
            dashTime = dist / FINAL_DASH_SPEED;
            anim.SetInteger("ActionIndex", 7);
        }

        if (dashTime <= 1)
        {
            dashTime = 1.5f;
        }

        print(owner.isAttacking);

        durationTimer = dashTime;
    }

    public override void OnPlaying()
    {
        base.OnPlaying();

        colliderObject.transform.position = owner.transform.position + colliderDiffFromOwner;

        if (dashTime - durationTimer >= 1f)
        {
            currentEffect.SetActive(false);
        }
    }

    public override void OnExit(bool isForcedStop)
    {
        base.OnExit(isForcedStop);

        destinationTransform.SetParent(owner.transform);
        destinationTransform.position = Vector3.zero;

        colliderObject.transform.SetParent(transform);
        colliderObject.transform.localPosition = originColliderLocal.pos;
        colliderObject.transform.localRotation = originColliderLocal.rot;
        colliderObject.SetActive(false);

        if (owner.target == null)
        {
            dashIndex = 0;

            owner.EndAttack();

            owner.GetComponent<Knockback>().EnableKnockback();
            return;
        }

        if (!isForcedStop && dashIndex < 2)
        {
            dashIndex++;

            if (dashIndex > 2)
            {
                dashIndex = 0;
            }

            Vector3 dir = (owner.target.transform.position - owner.transform.position).normalized;
            dir -= Vector3.up * dir.y;
            Quaternion targetRot = Quaternion.LookRotation(dir);
            owner.transform.rotation = Quaternion.Slerp(owner.transform.rotation, targetRot, Random.Range(0.5f, 1.5f));

            owner.GetComponent<Knockback>().EnableKnockback();

            skills.ActivateSkillChain();

        }
        else
        {
            dashIndex = 0;
            
            owner.GetComponent<Knockback>().EnableKnockback();
        }
    }
}
