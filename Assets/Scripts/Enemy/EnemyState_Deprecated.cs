using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions.Must;
using static TestForcingRagdoll;

// Animation states required : idle, wander, alert, trace, die, goLeft, goRight, backward
// Animations for other behaviors include in skill effect components. (EnemySkillEffectBase)
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Knockback))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(EnemySkills))]
[RequireComponent(typeof(CharacterController))]
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
    float currentDelayCooldown = 8f;
    float DELAY_COOLDOWN = 8f;

    [HideInInspector] public Vector3 originPos;

    [Description("������ ���¿��� ��ȸ�ϴ��� ����")]
    public bool IsWanderable = true;
    [Description("Ȱ�� �ݰ�. -1�� ��� ��ȸ���� �ʰ�, ���� ���� �� Ǯ���� ����")]
    public float activityRange = 30f;

    public float sightAngle = 30f;
    public float sightRange = 30f;

    [Description("������ �� ��ȸ ���� �̵� �ӵ�")]
    public float wanderingSpeed = 10f;
    public float movementSpeed = 20f;
    
    bool isSideWalkingToRight;
    float sideWalkDistance;

    [Description("Ÿ�ٰ� �ʹ� ����� ��� �������� �ӵ�")]
    public float backwardSpeed = 8f;
    bool isBackwarding;

    NavMeshAgent agent;
    Knockback knockback;
    Animator anim;
    EnemySkills skills;
    CharacterController controller;

    [HideInInspector] public bool isAttacking;

    public float tracingDistance;   // ���� �Ѿư��� �Ÿ�. �� �Ÿ����� �ָ� Ÿ���� ���� �̵���.
    bool isTracingTarget = true;    // Ÿ���� tracingDistance���� �ָ� ������ ���.

    bool isOn = false;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        knockback = GetComponent<Knockback>();
        anim = GetComponent<Animator>();
        skills = GetComponent<EnemySkills>();
        controller = GetComponent<CharacterController>();

        agent.updatePosition = false;
        agent.updateRotation = false;
        agent.isStopped = true;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 1f, NavMesh.AllAreas))
        {
            originPos = hit.position;
            controller.transform.position = originPos;
            transform.position = originPos;
            agent.Warp(originPos);
        }
        else
        {
            throw new System.Exception("No approachable nav mesh point! Maybe this enemy is placed on wrong position.");
        }

        isOn = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isOn)
        {
            return;
        }

        //��� �� ���� ����.
        if (currentState == State.Dead)
        {
            return;
        }

        if (currentDelayCooldown > 0) currentDelayCooldown -= Time.deltaTime;

        //Ȱ���ݰ� + �þߺ��� ���� �־����� ���� ���� �� ����.
        if (activityRange > 0 && target != null && Vector3.Distance(originPos, target.transform.position) > activityRange + sightRange)
        {
            target = null;
            ResetState();
            currentState = State.Returning;
        }

        //Ÿ���� ������� ����.
        if (currentState == State.Alert && target == null)
        {
            ResetState();
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
                        ResetState();
                        currentState = State.Alert;
                    }
                    else
                    {
                        ResetState();
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
            delayDuration -= Time.deltaTime;

            if (target != null)
            {
                if (isBackwarding || Vector3.Distance(transform.position, target.transform.position) <= backwardSpeed)
                {
                    SmoothBackward();
                }
                else
                {
                    anim.SetBool("IsGoingLeft", false);
                    anim.SetBool("IsGoingRight", false);
                    anim.SetBool("IsGoingBackward", false);

                    Vector3 dir = target.transform.position - transform.position;
                    dir -= Vector3.up * dir.y;
                    transform.rotation = Quaternion.LookRotation(dir);
                }
            }
            else
            {
                //�þ� �� �� �ĺ�.
                if (CheckEnemyInSight())
                {
                    currentState = State.Alert;
                    delayDuration = 0;
                    currentDelayCooldown = DELAY_COOLDOWN;
                }
            }

            if (delayDuration <= 0)
            {
                if (target != null)
                {
                    ResetState();
                    currentState = State.Alert;
                }
                else
                {
                    ResetState();
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

    private void LateUpdate()
    {
        if (!isOn)
        {
            return;
        }

        if (currentState == State.Idle || currentState == State.Attacking || currentState == State.Dead ||
            knockback.IsKnockbacked)
        {
            return;
        }

        if (Vector3.Distance(transform.position, agent.destination) > 0.1f)
        {
            if (agent.pathPending)
            {
                return;
            }

            Vector3 targetPosition = agent.nextPosition;
            Vector3 moveDirection = targetPosition - transform.position;
            print(moveDirection);
            // CharacterController�� �̵�
            if (controller.enabled)
            {
                controller.Move(moveDirection * movementSpeed * Time.deltaTime);
        
                // �̵� �������� ȸ��
                if (isBackwarding)
                {
                    transform.rotation = Quaternion.LookRotation(target.transform.position - transform.position);
                }
                else if (moveDirection != Vector3.zero && target != null)
                {
                    Vector3 dir = target.transform.position - transform.position;
                    dir -= Vector3.up * dir.y;
                    transform.rotation = Quaternion.LookRotation(dir);
                }
                else if (currentState == State.Wandering || currentState == State.Returning)
                {
                    Vector3 dir = agent.destination - transform.position;
                    dir -= Vector3.up * dir.y;
                    transform.rotation = Quaternion.LookRotation(dir);
                }
        
                // CharacterController ��ġ�� NavMeshAgent�� ����ȭ
                if (NavMesh.SamplePosition(controller.transform.position, out NavMeshHit hit, 1f, NavMesh.AllAreas))
                {
                    controller.transform.position = hit.position;
                    transform.position = hit.position;
                    agent.transform.position = hit.position;
                }
            }
        }
    }

    public void ResetState()
    {
        if (target == null)
        {
            anim.SetBool("HasTarget", false);
            anim.SetBool("IsTracingTarget", false);
            anim.SetBool("IsGoingLeft", false);
            anim.SetBool("IsGoingRight", false);
            anim.SetBool("IsGoingBackward", false);
        }
        anim.SetInteger("ActionIndex", 0);

        knockback.EndKnockback();

        isTracingTarget = false;

        sideWalkDistance = 0;
        isBackwarding = false;
        
        delayDuration = 0;
    }

    void ApplyDelay()
    {
        delayDuration = Random.Range(0, delayLimit);
        currentState = State.Delayed;
        currentDelayCooldown = DELAY_COOLDOWN;
    }

    void HandleIdleState()
    {
        if (currentDelayCooldown <= 0)
        {
            //������ ����
            ApplyDelay();
            return;
        }

        //�þ� �� �� �ĺ�. ���� ������ ��ȸ.
        if (CheckEnemyInSight())
        {
            currentState = State.Alert;
            currentDelayCooldown = DELAY_COOLDOWN;
        }
        else
        {
            WanderAround();
        }
    }

    void WanderAround()
    {
        if (activityRange < 0) return;

        agent.stoppingDistance = 0f;

        Vector3 desti = originPos;

        float xOffset = Random.Range(-activityRange, activityRange);
        float zOffset = Random.Range(-activityRange, activityRange);

        desti += Vector3.right * xOffset + Vector3.forward * zOffset;

        agent.SetDestination(desti);

        currentState = State.Wandering;

        anim.SetBool("IsWandering", true);
    }

    void HandleWanderingState()
    {
        //�þ� �� �� �ĺ�.
        if (CheckEnemyInSight())
        {
            currentState = State.Alert;
            currentDelayCooldown = DELAY_COOLDOWN;

            anim.SetBool("IsWandering", false);
            anim.SetBool("HasTarget", true);

            return;
        }

        if (Vector3.Distance(transform.position, agent.destination) < 0.1f)
        {
            anim.SetBool("IsWandering", false);

            currentState = State.Idle;
            ApplyDelay();
        }
    }

    void HandleAlertState()
    {
        if (currentDelayCooldown <= 0)
        {
            //������ ����
            ApplyDelay();
            return;
        }

        agent.SetDestination(target.transform.position);

        float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);

        if (distanceToTarget <= tracingDistance)
        {
            isTracingTarget = false;
        }
        else if (distanceToTarget >= tracingDistance * 1.5f)
        {
            isTracingTarget = true;

            anim.SetBool("IsGoingLeft", false);
            anim.SetBool("IsGoingRight", false);
            anim.SetBool("IsGoingBackward", false);
        }

        anim.SetBool("IsTracingTarget", isTracingTarget);

        if (!isTracingTarget)
        {
            agent.SetDestination(transform.position);

            if (skills.PrepareSkill())
            {
                StartSkill();
            }
            else
            {
                float angleToTarget = Vector3.Angle(transform.forward, target.transform.position - transform.position);

                if (isBackwarding || Vector3.Distance(transform.position, target.transform.position) <= tracingDistance * 0.3f)
                {
                    SmoothBackward();
                }
                else if (Mathf.Abs(angleToTarget) <= 1f)
                {
                    SmoothSideWalk();
                }
                else
                {
                    Vector3 dir = target.transform.position - transform.position;
                    dir -= Vector3.up * dir.y;
                    transform.rotation = Quaternion.LookRotation(dir);
                }
            }
        }
    }

    void HandleReturningState()
    {
        anim.SetInteger("ActionIndex", 0);
        anim.SetBool("IsWandering", true);

        currentDelayCooldown = DELAY_COOLDOWN;
        isAttacking = false;

        ResetState();

        ReturnToOriginPos();

        //�����ϸ� Idle & ������ ����
        if (Vector3.Distance(transform.position, agent.destination) < 0.5f)
        {
            anim.SetBool("IsWandering", false);

            currentState = State.Idle;
            ApplyDelay();
        }
    }

    void SmoothBackward()
    {
        if (target == null)
        {
            currentState = State.Returning;
            return;
        }

        if (isBackwarding)
        {
            if (Vector3.Distance(transform.position, target.transform.position) >= backwardSpeed)
            {
                isBackwarding = false;
                anim.SetBool("IsGoingBackward", false);
                return;
            }
        }

        anim.SetBool("IsGoingLeft", false);
        anim.SetBool("IsGoingRight", false);
        anim.SetBool("IsGoingBackward", true);

        isBackwarding = true;

        Vector3 dir = target.transform.position - transform.position;
        dir -= Vector3.up * dir.y;

        if (dir.sqrMagnitude < 0.001f) return;

        if (NavMesh.SamplePosition(transform.position - dir * backwardSpeed * Time.deltaTime, out NavMeshHit hit, 1f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    void SmoothSideWalk()
    {
        if (target == null) return;

        anim.SetBool("IsGoingBackward", false);

        // ������ �� ���� �� �缳��
        if (sideWalkDistance <= 0)
        {
            anim.SetBool("IsGoingLeft", false);
            anim.SetBool("IsGoingRight", false);

            sideWalkDistance = Random.Range(1f, 4.5f);
            isSideWalkingToRight = Random.Range(0, 2) == 1;

            ApplyDelay();

            return;
        }

        if (isSideWalkingToRight)
        {
            anim.SetBool("IsGoingLeft", false);
            anim.SetBool("IsGoingRight", true);
        }
        else
        {
            anim.SetBool("IsGoingLeft", true);
            anim.SetBool("IsGoingRight", false);
        }

        // ������ ����
        Vector3 pos = transform.position + (isSideWalkingToRight ? transform.right : -transform.right) * wanderingSpeed * Time.deltaTime;

        if (NavMesh.SamplePosition(pos, out NavMeshHit hit, 1f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }

        // ������ �Ÿ� ����
        sideWalkDistance -= wanderingSpeed * Time.deltaTime;
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
            anim.SetBool("HasTarget", true);
            target = closest;

            return true;
        }

        return true;
    }

    void ReturnToOriginPos()
    {
        agent.SetDestination(originPos);
        agent.stoppingDistance = 0f;
    }

    void StartSkill()
    {
        anim.SetBool("IsGoingLeft", false);
        anim.SetBool("IsGoingRight", false);
        anim.SetBool("IsGoingBackward", false);
        anim.SetBool("IsTracingTarget", false);

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

    public void Die()
    {
        anim.SetBool("IsDead", true);

        currentState = State.Dead;
    }
}
