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
        Idle,           //비전투 상태
        Wandering,      //배회 중
        Delayed,        //강제딜레이 중
        Alert,          //전투 중
        Attacking,      //공격 중
        Dead,           //사망
        Returning,      //복귀
    }

    [HideInInspector] public GameObject target;
    [HideInInspector] public State currentState = State.Idle;

    [Description("랜덤 딜레이의 한계치")]
    public float delayLimit = 0.8f;
    float delayDuration;
    float currentDelayCooldown = 8f;
    float DELAY_COOLDOWN = 8f;

    [HideInInspector] public Vector3 originPos;

    [Description("비전투 상태에서 배회하는지 여부")]
    public bool IsWanderable = true;
    [Description("활동 반경. -1일 경우 배회하지 않고, 전투 시작 시 풀리지 않음")]
    public float activityRange = 30f;

    public float sightAngle = 30f;
    public float sightRange = 30f;

    [Description("비전투 중 배회 시의 이동 속도")]
    public float wanderingSpeed = 10f;
    public float movementSpeed = 20f;
    
    bool isSideWalkingToRight;
    float sideWalkDistance;

    [Description("타겟과 너무 가까울 경우 물러나는 속도")]
    public float backwardSpeed = 8f;
    bool isBackwarding;

    NavMeshAgent agent;
    Knockback knockback;
    Animator anim;
    EnemySkills skills;
    CharacterController controller;

    [HideInInspector] public bool isAttacking;

    public float tracingDistance;   // 적을 쫓아가는 거리. 이 거리보다 멀면 타겟을 향해 이동함.
    bool isTracingTarget = true;    // 타겟이 tracingDistance보다 멀리 존재할 경우.

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

        //사망 시 동작 정지.
        if (currentState == State.Dead)
        {
            return;
        }

        if (currentDelayCooldown > 0) currentDelayCooldown -= Time.deltaTime;

        //활동반경 + 시야보다 적이 멀어지면 전투 해제 및 복귀.
        if (activityRange > 0 && target != null && Vector3.Distance(originPos, target.transform.position) > activityRange + sightRange)
        {
            target = null;
            ResetState();
            currentState = State.Returning;
        }

        //타겟이 사라지면 복귀.
        if (currentState == State.Alert && target == null)
        {
            ResetState();
            currentState = State.Returning;
        }

        //상태이상 처리
        if (knockback != null && knockback.IsKnockbacked)
        {
            //TODO: special sth이 있으면 딜레이, 상태이상 해제(ex: 체력% 특수기믹 실행 등)
        
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

        //딜레이 처리
        if (delayDuration > 0)
        {
            //TODO: special sth이 있으면 딜레이 해제(ex: 체력% 특수기믹 실행 등)
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
                //시야 내 적 식별.
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

        //상태에 따른 행동
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
            // CharacterController로 이동
            if (controller.enabled)
            {
                controller.Move(moveDirection * movementSpeed * Time.deltaTime);
        
                // 이동 방향으로 회전
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
        
                // CharacterController 위치를 NavMeshAgent에 동기화
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
            //딜레이 적용
            ApplyDelay();
            return;
        }

        //시야 내 적 식별. 적이 없으면 배회.
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
        //시야 내 적 식별.
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
            //딜레이 적용
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

        //도착하면 Idle & 딜레이 적용
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

        // 옆걸음 끝 도달 시 재설정
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

        // 옆걸음 동작
        Vector3 pos = transform.position + (isSideWalkingToRight ? transform.right : -transform.right) * wanderingSpeed * Time.deltaTime;

        if (NavMesh.SamplePosition(pos, out NavMeshHit hit, 1f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }

        // 옆걸음 거리 측정
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
