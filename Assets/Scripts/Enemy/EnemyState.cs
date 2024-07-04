using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.AI;

// Animation states required : idle, wander, alert, trace, die, goLeft, goRight, backward
// Animations for other behaviors include in skill effect components. (EnemySkillEffectBase)
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Knockback))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(EnemySkills))]
public class EnemyState : MonoBehaviour
{
    public enum State
    {
        Idle,           //������ ����
        Wandering,      //��ȸ ��
        Delayed,        //���������� ��
        Alert,          //���� ��
        Attacking,      //���� ��
        Dead,           //���
        Returning,      //����
    }

    [HideInInspector] public GameObject target;
    [HideInInspector] public State currentState = State.Idle;

    [Description("���� �������� �Ѱ�ġ")]
    public float delayLimit = 0.8f;
    float delayDuration;
    [HideInInspector] public bool mustAct;

    [HideInInspector] public Vector3 originPos;

    [Description("������ ���¿��� ��ȸ�ϴ��� ����")]
    public bool IsWanderable = true;
    [Description("Ȱ�� �ݰ�. -1�� ��� ��ȸ���� �ʰ�, ���� ���� �� Ǯ���� ����")]
    public float activityRange = -1f;

    float SMOOTH_ROTATION_SPEED = 1.2f; //��ų ��Ÿ���� ��ٸ��� ���� Ÿ���� ���� ȸ���ϴ� �ӵ�.

    public float sightAngle = 30f;
    public float sightRange = 30f;

    [Description("������ �� ��ȸ ���� �̵� �ӵ�")]
    public float wanderingSpeed = 10f;
    public float movementSpeed = 20f;
    
    bool isSideWalkingToRight;
    float sideWalkDistance;
    float sideWalkFrontOffset;

    public float backwardDistance = 8f;
    bool isBackwarding;

    NavMeshAgent agent;
    Knockback knockback;
    Animator anim;
    EnemySkills skills;

    [HideInInspector] public bool isAttacking;

    public float tracingDistance;   // ���� �Ѿư��� �Ÿ�. �� �Ÿ����� �ָ� Ÿ���� ���� �̵���.
    bool isTracingTarget = true;    // Ÿ���� tracingDistance���� �ָ� ������ ���.

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        knockback = GetComponent<Knockback>();
        anim = GetComponent<Animator>();
        skills = GetComponent<EnemySkills>();

        NavMeshHit hit;
        if (agent.FindClosestEdge(out hit))
        {
            originPos = hit.position;
            transform.position = originPos;
        }
        else
        {
            throw new System.Exception("No approachable nav mesh point! Maybe this enemy is placed on wrong position.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        //��� �� ���� ����.
        if (currentState == State.Dead)
        {
            return;
        }

        //Ȱ���ݰ� + �þߺ��� ���� �־����� ���� ���� �� ����.
        if (activityRange > 0 && target != null && Vector3.Distance(originPos, target.transform.position) > activityRange + sightRange)
        {
            target = null;
            currentState = State.Returning;
        }

        //Ÿ���� ������� ����.
        if (target == null)
        {
            currentState = State.Returning;
        }

        //�����̻� ó��
        if (knockback != null && knockback.IsKnockbacked)
        {
            //TODO: special sth�� ������ ������, �����̻� ����(ex: ü��% Ư����� ���� ��)

            if (delayDuration > 0)
            {
                delayDuration -= Time.deltaTime;

                if (delayDuration <= 0)
                {
                    if (target != null)
                    {
                        currentState = State.Alert;
                    }
                    else
                    {
                        currentState = State.Returning;
                    }
                }
            }

            return;
        }

        //������ ó��
        if (delayDuration > 0)
        {
            //TODO: special sth�� ������ ������ ����(ex: ü��% Ư����� ���� ��)

            if (currentState == State.Alert)
            {
                if (isBackwarding || Vector3.Distance(transform.position, target.transform.position) <= backwardDistance)
                {
                    SmoothBackward(Time.deltaTime);
                }
                else if (Vector3.Angle(transform.forward, target.transform.position - transform.position) <= 0.1f)
                {
                    SmoothSideWalk(Time.deltaTime);
                }
                else
                {
                    SmoothLookTarget(Time.deltaTime);
                }
            }

            delayDuration -= Time.deltaTime;

            if (delayDuration <= 0)
            {
                if (target != null)
                {
                    currentState = State.Alert;
                }
                else
                {
                    currentState = State.Returning;
                }
            }
            return;
        }

        //���¿� ���� �ൿ
        switch (currentState)
        {
            case State.Idle:
                HandleIdleState();
                break;
            case State.Wandering:
                HandleWanderingState();
                break;
            case State.Alert:
                HandleAlertState();
                break;
            case State.Returning:
                HandleReturningState();
                break;
        }
    }

    public void ResetState()
    {
        knockback.EndKnockback();

        isTracingTarget = true;

        sideWalkDistance = 0;
        isBackwarding = false;
        
        delayDuration = 0;
    }

    void HandleIdleState()
    {
        if (!mustAct)
        {
            //������ ����
            ApplyDelay();
            return;
        }

        //�þ� �� �� �ĺ�. ���� ������ ��ȸ.
        if (CheckEnemyInSight())
        {
            currentState = State.Alert;
            mustAct = true;
        }
        else
        {
            WanderAround();
        }
    }

    void HandleWanderingState()
    {
        //�þ� �� �� �ĺ�.
        if (CheckEnemyInSight())
        {
            currentState = State.Alert;
            mustAct = true;
            return;
        }

        //�����ϸ� ������ ����
        if (agent.remainingDistance < 0.01f)
        {
            mustAct = false;
            currentState = State.Idle;

            //TODO: anim -> Idle
        }
    }

    void HandleAlertState()
    {
        if (!mustAct)
        {
            //������ ����
            ApplyDelay();
            return;
        }

        agent.SetDestination(target.transform.position);

        if (agent.remainingDistance <= tracingDistance)
        {
            isTracingTarget = false;
        }

        if (agent.remainingDistance >= tracingDistance * 1.3f)
        {
            isTracingTarget = true;
        }

        if (!isTracingTarget)
        {
            agent.SetDestination(transform.position);

            if (skills.ActivateSkill())
            {
                StartSkill();
            }
            else
            {
                if (isBackwarding || Vector3.Distance(transform.position, target.transform.position) <= backwardDistance)
                {
                    SmoothBackward(Time.deltaTime);
                }
                else if (Vector3.Angle(transform.forward, target.transform.position - transform.position) <= 0.1f)
                {
                    SmoothSideWalk(Time.deltaTime);
                }
                else
                {
                    SmoothLookTarget(Time.deltaTime);
                }
            }
        }
    }

    void HandleReturningState()
    {
        mustAct = false;
        isAttacking = false;

        ResetState();

        ReturnToOriginPos();

        //�����ϸ� Idle & ������ ����
        if (agent.remainingDistance < 0.01f)
        {
            ApplyDelay(0.5f);

            //TODO: anim -> idle
        }
    }

    void SmoothBackward(float deltaTime)
    {
        if (target == null)
        {
            currentState = State.Returning;
            return;
        }

        if (isBackwarding)
        {
            if (Vector3.Distance(transform.position, target.transform.position) >= backwardDistance * 1.15f)
            {
                isBackwarding = false;
            }
        }

        Vector3 dir = target.transform.position - transform.position;
        dir -= Vector3.up * dir.y;

        if (dir.sqrMagnitude < 0.001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(dir);

        agent.Move(-transform.forward * wanderingSpeed * deltaTime);

        //TODO: anim -> backward
    }

    void SmoothLookTarget(float deltaTime)
    {
        if (target == null) return;

        Vector3 dir = target.transform.position - transform.position;
        dir -= Vector3.up * dir.y;

        if (dir.sqrMagnitude < 0.001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(dir);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, SMOOTH_ROTATION_SPEED * deltaTime);

        //TODO: anim -> rotate
    }

    void SmoothSideWalk(float deltaTime)
    {
        if (target == null) return;

        // ������ �� ���� �� �缳��
        if (sideWalkDistance <= 0)
        {
            sideWalkDistance = Random.Range(1f, 4.5f);
            isSideWalkingToRight = Random.Range(0, 2) == 1;
            sideWalkFrontOffset = Random.Range(-0.5f, 0.5f);
        }

        // ������ ����
        Vector3 pos = transform.position + (isSideWalkingToRight ? transform.right : -transform.right);
        pos += transform.forward * sideWalkFrontOffset;
        agent.Move(pos * wanderingSpeed * deltaTime);

        // Ÿ���� ���� ȸ��
        Vector3 dir = target.transform.position - transform.position;
        dir -= Vector3.up * dir.y;

        if (dir.sqrMagnitude < 0.001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(dir);
        transform.rotation = targetRotation;

        // ������ �Ÿ� ����
        sideWalkDistance -= wanderingSpeed * deltaTime;

        //TODO: anim -> side walk
    }

    void ApplyDelay()
    {
        delayDuration = Random.Range(0, delayLimit);
        currentState = State.Delayed;
        mustAct = true;
    }

    void ApplyDelay(float time)
    {
        delayDuration = time;
        currentState = State.Delayed;
        mustAct = true;
    }

    bool CheckEnemyInSight()
    {
        List<GameObject> possibleTargets = new();
        Collider[] cols = Physics.OverlapSphere(transform.position, sightRange, LayerMask.GetMask("Player"));

        foreach (Collider col in cols)
        {
            Vector3 dir = col.transform.position - transform.position;
            if (Vector3.Angle(transform.forward, dir) > sightAngle)
            {
                continue;
            }
            else
            {
                possibleTargets.Add(col.transform.root.gameObject);
            }
        }

        if (possibleTargets.Count <= 0) return false;

        GameObject closest = null;
        float closestDistance = float.MaxValue;

        foreach (GameObject possibleTarget in possibleTargets)
        {
            float distance = Vector3.Distance(transform.position, possibleTarget.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = possibleTarget;
            }
        }

        if (closest != null)
        {
            target = closest;
            return true;
        }

        return true;
    }

    void WanderAround()
    {
        if (activityRange < 0) return;

        agent.stoppingDistance = 0f;

        Vector3 desti = originPos;

        float xOffset = Random.Range(-activityRange, activityRange);
        float zOffset = Random.Range(-activityRange, activityRange);

        desti += Vector3.right * zOffset + Vector3.forward * zOffset;

        currentState = State.Wandering;

        //TODO: anim -> walk
    }

    void ReturnToOriginPos()
    {
        agent.SetDestination(originPos);
        agent.stoppingDistance = 0f;

        currentState = State.Returning;

        //TODO: anim -> walk
    }

    void StartSkill()
    {
        isAttacking = true;
        currentState = State.Attacking;
    }

    public void ExitSkill()
    {
        isAttacking = false;

        if (target != null)
        {
            currentState = State.Alert;
        }
        else
        {
            currentState = State.Returning;
        }
    }
}
