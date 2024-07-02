using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//TODO: down 상태 추가해야함.
//경직 시간 너무 길면 경직 모션 두번 재생되는 등 이상현상. 경직같은거 최대 시간 정해야할듯.
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

    //TODO: from PlayerMoveAnimation. 하나의 시스템으로 통합해야할듯.
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

        //넉백 방향과 캐릭터 방향 비교
        bool isFromBack = Vector3.Dot(dir, transform.forward) > 0;
        int currentDir = 0; // 1: from front, 2: from back

        //공중에 떠있거나 넉백 중에는 강제로 15 이상의 넉백 적용.
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

        //5 이상 밀려나면 넉백. 아니면 경직.
        if (isKnockbackable && power > 5f)
        {
            if (isFromBack)
            {
                if (currentDir == 2)
                {
                    //이미 같은 방향 넉백 중인 경우 normalized time을 0.2로 초기화.
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
                    //이미 같은 방향 넉백 중인 경우 normalized time을 0.2로 초기화.
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
            //뒤로 밀려나는 피격 모션의 재생 시간 1.2초를 넉백 시간으로 변경.
            float hitSpeed = 1.2f / duration;

            //앞으로 밀려나는 피격 모션이 뒷방향보다 절반 정도로 짧음
            if (isFromBack)
            {
                hitSpeed /= 1.8f;
            }

            an.SetFloat("BeHitSpeed", hitSpeed);
            if (isFromBack)
            {
                if (currentDir == 2)
                {
                    //이미 같은 방향 넉백 중인 경우 normalized time을 0으로 초기화.
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
                    //이미 같은 방향 넉백 중인 경우 normalized time을 0으로 초기화.
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
        //밀려나는 시간 동안의 처리
        if (KNOCKBACK_MOVE_DURATION > 0)
        {
            //중력 적용
            knockbackDirection += Vector3.up * Gravity * Time.deltaTime;

            controller.Move(knockbackDirection * Time.deltaTime);
            KNOCKBACK_MOVE_DURATION -= Time.deltaTime;

            knockbackDuration -= Time.deltaTime;
        }
        else //밀려난 뒤의 조작불가 시간 처리
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

        // 넉백 종료 후 위치 설정(캐릭터 컨트롤러의 위치와 게임오브젝트의 실제 위치가 불일치할 수 있음.)
        Vector3 finalPosition = controller.transform.position;
        transform.position = finalPosition;
    }
}
