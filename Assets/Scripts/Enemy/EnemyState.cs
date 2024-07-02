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

    [Description("���� �� ��� �� ���� �������� �Ѱ�ġ")]
    public float delayLimit = 0.8f;
    float delayDuration;
    bool mustAct;

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
        //��� �� ���� ����.
        if (currentState == State.Dead)
        {
            return;
        }

        //TODO: Ȱ���ݰ� + �þߺ��� ���� �־����� ���� ���� �� ����. ���� ���� �Ÿ��� �̻��� �� ������ Ȯ�� ���.
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

        //���� ��Ÿ�� ���� ��� ��󿡰� õõ�� ȸ��
        //if (!isAttacking)
        {
            SmoothLookTarget(Time.deltaTime);

            //TODO: Turning Anim
        }

        //���¿� ���� �ൿ
        switch (currentState)
        {
            case State.Idle:
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

                break;
            case State.Wandering:
                //�þ� �� �� �ĺ�.
                if (CheckEnemyInSight())
                {
                    currentState = State.Alert;
                    mustAct = true;
                    return;
                }

                //�����ϸ� ������ ����
                if (Vector3.Distance(transform.position, agent.destination) < 0.01f)
                {
                    mustAct = false;
                    currentState = State.Idle;

                    //TODO: anim -> Idle
                }

                break;
            //case State.Delayed: //������ �� ������� �������� ����.
            //    break;
            case State.Alert:
                if (!mustAct)
                {
                    //������ ����
                    ApplyDelay();
                    return;
                }

                //TODO: select skill

                break;
            //case State.Attacking: //�ʿ� �� ���� �� ȿ��. �����̻� �鿪 ���� ���ݿ��� ���� ó����.
            //    break;
            //case State.Dead: //��� �� ������� �������� ����.
            //    break;
            case State.Returning:
                ReturnToOriginPos();

                //�����ϸ� Idle & ������ ����
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
