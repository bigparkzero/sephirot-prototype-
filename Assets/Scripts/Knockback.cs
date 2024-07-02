using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//TODO: down ���� �߰��ؾ���.
//���� �ð� �ʹ� ��� ���� ��� �ι� ����Ǵ� �� �̻�����. ���������� �ִ� �ð� ���ؾ��ҵ�.
public class Knockback : MonoBehaviour
{
    CharacterController controller;
    Animator an;

    //TODO: for test. delete this.
    public bool isKnockbackedForTest;
    public float knockbackPowerForTest = 2f;
    public float knockbackDurationForTest = 0.7f;
    //===========================

    private Vector3 knockbackDirection;
    private float knockbackDuration;
    public bool IsKnockbacked => knockbackDuration > 0;
    float KNOCKBACK_MOVE_DURATION = 0.5f;

    float knockbackResistGauge = 0f;
    float KNOCKBACK_RESIST_DECREASE_AMOUNT = 0.4f;
    public float knockbackResist = 10f;
    float currentImmuneTime = 0f;
    float KNOCKBACK_IMMUNE_TIME = 3f;

    //TODO: from PlayerMoveAnimation. �ϳ��� �ý������� �����ؾ��ҵ�.
    public float GroundedOffset = 0.15f;
    public float GroundedRadius = 0.74f;
    public LayerMask GroundLayers;
    public float Gravity = -60.0f;

    public bool isKnockbackable = true;

    bool Grounded
    {
        get
        {
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);
            bool isGrounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);

            return isGrounded;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!TryGetComponent(out an) || !TryGetComponent(out controller))
        {
            throw new System.Exception("no animator or controller! knockback component needs both.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        //TODO: for test. delete this.
        if (Input.GetKeyDown(KeyCode.K))
        {
            isKnockbackedForTest = true;
        }

        if (isKnockbackedForTest)
        {
            isKnockbackedForTest = false;
            int i = Random.Range(0, 2);
            if (i == 0) i = -1;

            ApplyKnockback(i * transform.forward * -knockbackPowerForTest, knockbackPowerForTest, knockbackDurationForTest);
        }
        //====================================

        if (knockbackResistGauge > 0)
        {
            knockbackResistGauge -= Time.deltaTime * KNOCKBACK_RESIST_DECREASE_AMOUNT;
        }

        if (currentImmuneTime > 0)
        {
            currentImmuneTime -= Time.deltaTime;
        }

        HandleKnockback();
    }

    public void ApplyKnockback(Vector3 dir, float power, float duration = 0.7f)
    {
        if (currentImmuneTime > 0)
        {
            return;
        }

        if (dir == Vector3.zero || power == 0)
        {
            dir = -transform.forward;
            power = 0.1f;
        }

        knockbackResistGauge += power;

        if (knockbackResistGauge >= knockbackResist)
        {
            currentImmuneTime = KNOCKBACK_IMMUNE_TIME;
            knockbackResistGauge = 0;
        }

        KNOCKBACK_MOVE_DURATION = 0.5f;

        knockbackDuration = duration;

        knockbackDirection = dir.normalized * power;

        //�˹� ����� ĳ���� ���� ��
        bool isFromBack = Vector3.Dot(dir, transform.forward) > 0;
        int currentDir = 0; // 1: from front, 2: from back

        //���߿� ���ְų� �˹� �߿��� ������ 15 �̻��� �˹� ����.
        if (!Grounded || an.GetBool("IsKnockbackedFromFront") || an.GetBool("IsKnockbackedFromBack"))
        {
            power = Mathf.Max(power, 15f);

            if (an.GetBool("IsKnockbackedFromFront"))
            {
                currentDir = 1;
                an.SetBool("IsKnockbackedFromFront", false);
            }
            else if (an.GetBool("IsKnockbackedFromBack"))
            {
                currentDir = 2;
                an.SetBool("IsKnockbackedFromBack", false);
            }

        }

        if (an.GetBool("IsHitFromFront"))
        {
            currentDir = 1;
            an.SetBool("IsHitFromFront", false);
        }
        else if (an.GetBool("IsHitFromBack"))
        {
            currentDir = 2;
            an.SetBool("IsHitFromBack", false);
        }

        //5 �̻� �з����� �˹�. �ƴϸ� ����.
        if (isKnockbackable && power > 5f)
        {
            if (isFromBack)
            {
                if (currentDir == 2)
                {
                    //�̹� ���� ���� �˹� ���� ��� normalized time�� 0.2�� �ʱ�ȭ.
                    an.Play("Knockbacked from back", 0, 0.2f);
                }
                else
                {
                    an.CrossFade("Knockbacked from back", 0.05f);
                }
                an.SetBool("IsKnockbackedFromBack", true);
            }
            else
            {
                if (currentDir == 1)
                {
                    //�̹� ���� ���� �˹� ���� ��� normalized time�� 0.2�� �ʱ�ȭ.
                    an.Play("Knockbacked from front", 0, 0.2f);
                }
                else
                {
                    an.CrossFade("Knockbacked from front", 0.05f);
                }
                an.SetBool("IsKnockbackedFromFront", true);
            }
        }
        else
        {
            //�ڷ� �з����� �ǰ� ����� ��� �ð� 1.2�ʸ� �˹� �ð����� ����.
            float hitSpeed = 1.2f / duration;

            //������ �з����� �ǰ� ����� �޹��⺸�� ���� ������ ª��
            if (isFromBack)
            {
                hitSpeed /= 1.8f;
            }

            an.SetFloat("BeHitSpeed", hitSpeed);
            if (isFromBack)
            {
                if (currentDir == 2)
                {
                    //�̹� ���� ���� �˹� ���� ��� normalized time�� 0���� �ʱ�ȭ.
                    an.Play("Be Hit from Back", 0, 0);
                }
                else
                {
                    an.CrossFade("Be Hit from Back", 0);
                }
                an.SetBool("IsHitFromBack", true);
            }
            else
            {
                if (currentDir == 1)
                {
                    //�̹� ���� ���� �˹� ���� ��� normalized time�� 0���� �ʱ�ȭ.
                    an.Play("Be Hit from Front", 0, 0);
                }
                else
                {
                    an.CrossFade("Be Hit from Front", 0);
                }
                an.SetBool("IsHitFromFront", true);
            }
        }
    }

    private void HandleKnockback()
    {
        //�з����� �ð� ������ ó��
        if (KNOCKBACK_MOVE_DURATION > 0)
        {
            //�߷� ����
            knockbackDirection += Vector3.up * Gravity * Time.deltaTime;

            controller.Move(knockbackDirection * Time.deltaTime);
            KNOCKBACK_MOVE_DURATION -= Time.deltaTime;

            knockbackDuration -= Time.deltaTime;
        }
        else //�з��� ���� ���ۺҰ� �ð� ó��
        {
            if (knockbackDuration > 0)
            {
                knockbackDuration -= Time.deltaTime;
            }
        }

        if (knockbackDuration <= 0)
        {
            EndKnockback();
        }
    }

    private void EndKnockback()
    {
        an.SetBool("IsKnockbackedFromFront", false);
        an.SetBool("IsKnockbackedFromBack", false);
        an.SetBool("IsHitFromFront", false);
        an.SetBool("IsHitFromBack", false);
        knockbackDirection = Vector3.zero;

        // �˹� ���� �� ��ġ ����(ĳ���� ��Ʈ�ѷ��� ��ġ�� ���ӿ�����Ʈ�� ���� ��ġ�� ����ġ�� �� ����.)
        Vector3 finalPosition = controller.transform.position;
        transform.position = finalPosition;
    }
}
