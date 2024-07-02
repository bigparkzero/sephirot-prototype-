using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent)), RequireComponent(typeof(Knockback)), RequireComponent(typeof(Animator))]
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

    [Description("공격 후 대기 등 랜덤 딜레이의 한계치")]
    public float delayLimit = 0.8f;
    float delayDuration;
    bool mustAct;

    [HideInInspector] public Vector3 originPos;

    [Description("비전투 상태에서 배회하는지 여부")]
    public bool IsWanderable = true;
    [Description("활동 반경. -1일 경우 배회하지 않고, 전투 시작 시 풀리지 않음")]
    public float activityRange = -1f;

    float SMOOTH_ROTATION_SPEED = 1.2f; //스킬 쿨타임을 기다리는 동안 타겟을 향해 회전하는 속도.

    public float sightAngle = 30f;
    public float sightRange = 30f;

    [Description("비전투 중 배회 시의 이동 속도")]
    public float wanderingSpeed = 10f;
    public float movementSpeed = 20f;

    NavMeshAgent agent;
    [HideInInspector] public Knockback knockback;
    [HideInInspector] public Animator anim;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        knockback = GetComponent<Knockback>();

        NavMeshHit hit;
        if (agent.FindClosestEdge(out hit))
        {
            originPos = hit.position;
        }
        else
        {
            throw new System.Exception("No approachable nav mesh point! Maybe this enemy is placed on wrong position.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        //사망 시 동작 정지.
        if (currentState == State.Dead)
        {
            return;
        }

        //TODO: 활동반경 + 시야보다 적이 멀어지면 전투 해제 및 복귀. 전투 해제 거리가 이상할 수 있으니 확인 요망.
        if (activityRange > 0 && target != null && Vector3.Distance(originPos, target.transform.position) > activityRange + sightRange)
        {
            target = null;
            currentState = State.Returning;
        }

        //타겟이 사라지면 복귀.
        if (target == null)
        {
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

        //딜레이 처리
        if (delayDuration > 0)
        {
            //TODO: special sth이 있으면 딜레이 해제(ex: 체력% 특수기믹 실행 등)

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

        //공격 쿨타임 중일 경우 대상에게 천천히 회전
        //if (!isAttacking)
        {
            SmoothLookTarget(Time.deltaTime);

            //TODO: Turning Anim
        }

        //상태에 따른 행동
        switch (currentState)
        {
            case State.Idle:
                if (!mustAct)
                {
                    //딜레이 적용
                    ApplyDelay();
                    return;
                }

                //시야 내 적 식별. 적이 없으면 배회.
                if (CheckEnemyInSight())
                {
                    currentState = State.Alert;
                    mustAct = true;
                }
                else
                {
                    WanderAround();
                }

                break;
            case State.Wandering:
                //시야 내 적 식별.
                if (CheckEnemyInSight())
                {
                    currentState = State.Alert;
                    mustAct = true;
                    return;
                }

                //도착하면 딜레이 적용
                if (Vector3.Distance(transform.position, agent.destination) < 0.01f)
                {
                    mustAct = false;
                    currentState = State.Idle;

                    //TODO: anim -> Idle
                }

                break;
            //case State.Delayed: //딜레이 시 여기까지 진입하지 않음.
            //    break;
            case State.Alert:
                if (!mustAct)
                {
                    //딜레이 적용
                    ApplyDelay();
                    return;
                }

                //TODO: select skill

                break;
            //case State.Attacking: //필요 시 공격 중 효과. 상태이상 면역 등은 공격에서 직접 처리함.
            //    break;
            //case State.Dead: //사망 시 여기까지 진입하지 않음.
            //    break;
            case State.Returning:
                ReturnToOriginPos();

                //도착하면 Idle & 딜레이 적용
                if (Vector3.Distance(transform.position, agent.destination) < 0.01f)
                {
                    ApplyDelay(0.5f);

                    //TODO: anim -> idle
                }

                break;
        }
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
}
