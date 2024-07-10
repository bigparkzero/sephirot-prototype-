using BehaviorTree;
using Pathfinding;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    Transform desti;

    BehaviorTreeNode behaviorRoot;
    float behaviorTime = 0f;

    Animator anim;
    AIPath aiPath;
    AIDestinationSetter destinationSetter;
    Knockback knockback;
    [HideInInspector] public EnemySkills skills;
    [HideInInspector] public Stats stats;

    [HideInInspector] public GameObject target;

    [HideInInspector] public Vector3 originPos;

    [HideInInspector] public bool isAttacking;
    [HideInInspector] public bool isSideWalking;
    [HideInInspector] public bool isBackwarding;
    [HideInInspector] public bool isTracing;
    [HideInInspector] public bool isReturning;
    [HideInInspector] public bool isWandering;

    [HideInInspector] public bool isDead;

    #region Delay
    [HideInInspector] public float delayTimer;
    public float MIN_DELAY_TIME = 1f;
    public float MAX_DELAY_TIME = 3.5f;
    [HideInInspector] public float delayActivationCooldown;
    public float DELAY_COOLDOWN_AMOUNT = 8f;
    #endregion

    #region Range
    [Description("활동 반경. 0 이하로 설정할 경우 배회하지 않고, 전투 시작 시 풀리지 않음")]
    public float activityRange = 30f;

    public float sightAngle = 45f;
    public float sightRange = 30f;

    public float tracingDistance = 10f;
    [HideInInspector] public float tracingDistanceOrigin;
    #endregion

    #region Movement
    public float tracingSpeed = 10f;
    public float wanderingSpeed = 8f;
    public float backwardSpeed = 4f;
    public float sideWalkSpeed = 6f;

    float wanderingCooldown;
    Quaternion rotationWhenKnockbackStarted;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        // Turn off Astar Logs.
        AstarPath.active.logPathResults = PathLog.OnlyErrors;

        #region GetComponent
        anim = GetComponent<Animator>();
        aiPath = GetComponent<AIPath>();
        destinationSetter = GetComponent<AIDestinationSetter>();
        knockback = GetComponent<Knockback>();
        skills = GetComponent<EnemySkills>();
        stats = GetComponent<Stats>();
        #endregion

        #region Behavior Tree Generation
        behaviorRoot = GenerateBehaviorTree();
        #endregion

        #region Save Origin Values
        NNInfo nearestNode = AstarPath.active.GetNearest(transform.position);
        Vector3 validPosition = nearestNode.position;
        aiPath.Teleport(validPosition);
        originPos = validPosition;

        tracingDistanceOrigin = tracingDistance;
        #endregion

        #region Instantiate GameObject for indicate Destination
        desti = new GameObject().transform;
        desti.gameObject.name = gameObject.name + "_destination";
        desti.position = transform.position;
        desti.SetParent(transform);
        destinationSetter.target = desti;
        #endregion

        #region AIPath Stat Settings
        aiPath.maxSpeed = wanderingSpeed;
        #endregion
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead) return;

        if (target == null)
        {
            anim.SetBool("HasTarget", false);
        }
        else
        {
            anim.SetBool("HasTarget", true);
        }

        if (behaviorTime > 0)
        {
            behaviorTime -= Time.deltaTime;
        }

        if (delayTimer > 0)
        {
            delayTimer -= Time.deltaTime;
        }

        if (knockback.IsKnockbacked)
        {
            CancelAttack();
            EndMovement();
            transform.rotation = rotationWhenKnockbackStarted;
        }
        else
        {
            rotationWhenKnockbackStarted = transform.rotation;
        }

        if (!aiPath.pathPending && behaviorTime <= 0)
        {
            behaviorTime = 0.08f;
            behaviorRoot.Execute();
        }
    }

    private void LateUpdate()
    {
        if (knockback.IsKnockbacked)
        {
            CancelAttack();
            EndMovement();
            transform.rotation = rotationWhenKnockbackStarted;
        }
        else if (target != null && !isAttacking)
        {
            Vector3 dir = (target.transform.position - transform.position).normalized;
            dir -= Vector3.up * dir.y;
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 50);

            rotationWhenKnockbackStarted = transform.rotation;
        }

        if (isReturning)
        {
            stats.Healed(stats.maxHP);
        }
    }

    BehaviorTreeNode GenerateBehaviorTree()
    {
        List<BehaviorTreeNode> rootNodes = new List<BehaviorTreeNode>();

        // Death
        List<BehaviorTreeNode> deathNodes = new List<BehaviorTreeNode>();
        DeathConditionNode deathCondition = new DeathConditionNode(stats);
        DeathActionNode deathAction = new DeathActionNode(this);
        deathNodes.Add(deathCondition);

        deathNodes.Add(deathAction);
        SequenceNode deathSequence = new SequenceNode(deathNodes);

        // Knockback
        List<BehaviorTreeNode> knockbackSequenceNodes = new List<BehaviorTreeNode>();
        KnockbackConditionNode knockbackCondition = new KnockbackConditionNode(knockback);
        CancleAttackActionNode cancelAttackAction = new CancleAttackActionNode(this);
        knockbackSequenceNodes.Add(knockbackCondition);
        knockbackSequenceNodes.Add(cancelAttackAction);
        SequenceNode knockbackSequence = new SequenceNode(knockbackSequenceNodes);

        // Attacking
        AttackingConditionNode attackingCondition = new AttackingConditionNode(this);

        // Returning
        List<BehaviorTreeNode> returnNodes = new List<BehaviorTreeNode>();

        List<BehaviorTreeNode> returnSequenceNodes1 = new List<BehaviorTreeNode>();
        NowReturningConditionNode nowReturningCondition = new NowReturningConditionNode(this);
        EndMovementActionNode endMovementAction = new EndMovementActionNode(this, aiPath);
        returnSequenceNodes1.Add(nowReturningCondition);
        returnSequenceNodes1.Add(endMovementAction);
        SequenceNode returnSequence1 = new SequenceNode(returnSequenceNodes1);

        List<BehaviorTreeNode> returnSequenceNodes2 = new List<BehaviorTreeNode>();
        StartReturningConditionNode startReturningCondition = new StartReturningConditionNode(this, anim);
        StartReturningActionNode startReturningAction = new StartReturningActionNode(this, aiPath);
        returnSequenceNodes2.Add(startReturningCondition);
        returnSequenceNodes2.Add(startReturningAction);
        SequenceNode returnSequence2 = new SequenceNode(returnSequenceNodes2);

        returnNodes.Add(returnSequence1);
        returnNodes.Add(returnSequence2);
        SelectorNode returnSelector = new SelectorNode(returnNodes);

        // Delay
        List<BehaviorTreeNode> delaySequenceNodes = new List<BehaviorTreeNode>();
        DelayedConditionNode delayedCondition = new DelayedConditionNode(this);

        List<BehaviorTreeNode> delaySucceederNodes = new List<BehaviorTreeNode>();

        List<BehaviorTreeNode> delaySucceederSelectorNodes = new List<BehaviorTreeNode>();
        List<BehaviorTreeNode> delaySucceederSelectorSequenceNodes = new List<BehaviorTreeNode>();

        TargetConditionNode delayTargetCondition = new TargetConditionNode(this);

        CombatMovingConditionNode combatMovingCondition = new CombatMovingConditionNode(this);
        ConverterNode delay_combatMoveConverter = new ConverterNode(combatMovingCondition);

        CombatMovementActionNode combatMovementAction = new CombatMovementActionNode(this, aiPath, anim);

        delaySucceederSelectorSequenceNodes.Add(delayTargetCondition);
        delaySucceederSelectorSequenceNodes.Add(delay_combatMoveConverter);
        delaySucceederSelectorSequenceNodes.Add(combatMovementAction);

        SequenceNode delaySucceederSelectorSequence = new SequenceNode(delaySucceederSelectorSequenceNodes);

        EndMovementActionNode delay_endMovementAction = new EndMovementActionNode(this, aiPath);

        delaySucceederSelectorNodes.Add(delaySucceederSelectorSequence);
        delaySucceederSelectorNodes.Add(delay_endMovementAction);

        SelectorNode delaySucceederSelector = new SelectorNode(delaySucceederSelectorNodes);

        delaySucceederNodes.Add(delaySucceederSelector);
        SucceederNode delaySucceeder = new SucceederNode(delaySucceederNodes);

        delaySequenceNodes.Add(delayedCondition);
        delaySequenceNodes.Add(delaySucceeder);

        SequenceNode delaySequence = new SequenceNode(delaySequenceNodes);

        // Combat
        List<BehaviorTreeNode> combatSequenceNodes = new List<BehaviorTreeNode>();

        TargetConditionNode targetCondition = new TargetConditionNode(this);
        combatSequenceNodes.Add(targetCondition);

        List<BehaviorTreeNode> combatSelectorNodes = new List<BehaviorTreeNode>();

        List<BehaviorTreeNode> combatSelectorSequence1Nodes = new List<BehaviorTreeNode>();
        AttackableConditionNode attackableCondition = new AttackableConditionNode(this);
        AttackActionNode attackAction = new AttackActionNode(this, aiPath);
        combatSelectorSequence1Nodes.Add(attackableCondition);
        combatSelectorSequence1Nodes.Add(attackAction);
        SequenceNode combatSelectorSequence1 = new SequenceNode(combatSelectorSequence1Nodes);

        List<BehaviorTreeNode> combatSelectorSequence2Nodes = new List<BehaviorTreeNode>();
        CombatMovingConditionNode combatCombatMovingCondition = new CombatMovingConditionNode(this);
        ConverterNode combatCombatMovingConverter = new ConverterNode(combatCombatMovingCondition);
        CombatMovementActionNode combatCombatMovementAction = new CombatMovementActionNode(this, aiPath, anim);
        combatSelectorSequence2Nodes.Add(combatCombatMovingConverter);
        combatSelectorSequence2Nodes.Add(combatCombatMovementAction);
        SequenceNode combatSelectorSequence2 = new SequenceNode(combatSelectorSequence2Nodes);

        combatSelectorNodes.Add(combatSelectorSequence1);
        combatSelectorNodes.Add(combatSelectorSequence2);

        EndMovementActionNode combatSelectorLastEndMovement = new EndMovementActionNode(this, aiPath);
        combatSelectorNodes.Add(combatSelectorLastEndMovement);

        SelectorNode combatSelector = new SelectorNode(combatSelectorNodes);
        combatSequenceNodes.Add(combatSelector);

        SequenceNode combatSequence = new SequenceNode(combatSequenceNodes);

        // Detect
        List<BehaviorTreeNode> detectSequenceNodes = new List<BehaviorTreeNode>();
        EnemyDetectedConditionNode detectedCondition = new EnemyDetectedConditionNode(this);
        StartCombatActionNode startCombatAction = new StartCombatActionNode(this);
        detectSequenceNodes.Add(detectedCondition);
        detectSequenceNodes.Add(startCombatAction);
        SequenceNode detectSequence = new SequenceNode(detectSequenceNodes);

        // Wander
        List<BehaviorTreeNode> wanderSequenceNodes = new List<BehaviorTreeNode>();
        WanderableConditionNode wanderableCondition = new WanderableConditionNode(this);

        List<BehaviorTreeNode> wanderSelectorNodes = new List<BehaviorTreeNode>();

        List<BehaviorTreeNode> wanderSelectorSequenceNodes = new List<BehaviorTreeNode>();

        WanderingConditionNode wanderingCondition = new WanderingConditionNode(this);
        ConverterNode wanderingConverter = new ConverterNode(wanderingCondition);

        WanderActionNode wanderAction = new WanderActionNode(this, aiPath, anim);
        wanderSelectorSequenceNodes.Add(wanderingConverter);
        wanderSelectorSequenceNodes.Add(wanderAction);
        SequenceNode wanderSelectorSequence = new SequenceNode(wanderSelectorSequenceNodes);

        EndMovementActionNode wanderEndMovementAction = new EndMovementActionNode(this, aiPath);
        wanderSelectorNodes.Add(wanderSelectorSequence);
        wanderSelectorNodes.Add(wanderEndMovementAction);

        SelectorNode wanderSelector = new SelectorNode(wanderSelectorNodes);

        wanderSequenceNodes.Add(wanderableCondition);
        wanderSequenceNodes.Add(wanderSelector);
        SequenceNode wanderSequence = new SequenceNode(wanderSequenceNodes);

        // Root
        rootNodes.Add(deathSequence);
        rootNodes.Add(knockbackSequence);
        rootNodes.Add(attackingCondition);
        rootNodes.Add(returnSelector);
        rootNodes.Add(delaySequence);
        rootNodes.Add(combatSequence);
        rootNodes.Add(detectSequence);
        rootNodes.Add(wanderSequence);
        SelectorNode rootNode = new SelectorNode(rootNodes);

        return rootNode;
    }

    public void Die()
    {
        //TODO: ragdoll, shader
        CancelAttack();
        EndMovement();

        anim.SetBool("IsDead", true);

        isDead = true;
    }

    public void StartReturning()
    {
        stats.Healed(stats.maxHP);

        anim.SetBool("IsWandering", true);
        anim.SetBool("IsGoingLeft", false);
        anim.SetBool("IsGoingRight", false);
        anim.SetBool("IsGoingBackward", false);
        anim.SetBool("IsTracingTarget", false);
        anim.SetBool("HasTarget", false);
        anim.SetInteger("ActionIndex", 0);

        target = null;

        delayTimer = 0f;

        isSideWalking = false;
        isBackwarding = false;
        isTracing = false;

        isReturning = true;

        SetDestination(originPos);
    }

    public void SetDestination(Vector3 destination)
    {
        desti.SetParent(null);
        desti.position = destination;
    }

    public void EndMovement()
    {
        if (isReturning)
        {
            isReturning = false;
            anim.SetBool("IsWandering", false);
        }
        if (isSideWalking)
        {
            isSideWalking = false;
            anim.SetBool("IsGoingLeft", false);
            anim.SetBool("IsGoingRight", false);
        }
        if (isBackwarding)
        {
            isBackwarding = false;
            anim.SetBool("IsGoingBackward", false);
        }
        if (isTracing)
        {
            isTracing = false;
            anim.SetBool("IsTracingTarget", false);
        }
        if (isWandering)
        {
            isWandering = false;
            anim.SetBool("IsWandering", false);

            wanderingCooldown = Random.Range(4f, 8f);
        }

        desti.SetParent(transform);
        desti.transform.position = transform.position;
    }

    public void StartFight()
    {
        EndMovement();

        // 원래는 불필요해야하는데 이상하게 bool값들이 바뀌지 않은 상태로 남는 경우가 있어서 안전하게 넣어둠.
        anim.SetBool("IsWandering", false);
        anim.SetBool("HasTarget", true);
    }

    public void Attack()
    {
        EndMovement();

        isAttacking = true;

        Vector3 dir = (target.transform.position - transform.position).normalized;
        dir -= Vector3.up * dir.y;
        transform.rotation = Quaternion.LookRotation(dir);

        skills.UseSkill();
    }

    public void EndAttack()
    {
        if (isAttacking)
        {
            isAttacking = false;

            anim.SetInteger("ActionIndex", 0);
        }
    }

    public void CancelAttack()
    {
        if (isAttacking)
        {
            skills.StopSkill();

            EndAttack();
        }
    }
}
