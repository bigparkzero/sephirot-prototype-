using Pathfinding;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTree
{
    public enum BTNodeState
    {
        SUCCESS,
        FAILURE,
        RUNNING,
        ERROR,
    }

    public abstract class BehaviorTreeNode
    {
        protected BTNodeState _nowState;
        public BTNodeState nowState
        {
            get { return _nowState; }
        }

        public abstract BTNodeState Execute();
    }

    #region Composite Nodes
    public class SequenceNode : BehaviorTreeNode
    {
        List<BehaviorTreeNode> _children = new List<BehaviorTreeNode>();

        public SequenceNode(List<BehaviorTreeNode> children)
        {
            _children = children;
        }

        public override BTNodeState Execute()
        {
            foreach (BehaviorTreeNode child in _children)
            {
                BTNodeState childState = child.Execute();
                switch (childState)
                {
                    case BTNodeState.FAILURE:
                    case BTNodeState.RUNNING:
                        _nowState = childState;
                        return _nowState;
                }
            }
            _nowState = BTNodeState.SUCCESS;
            return _nowState;
        }
    }

    public class SelectorNode : BehaviorTreeNode
    {
        List<BehaviorTreeNode> _children = new List<BehaviorTreeNode>();

        public SelectorNode(List<BehaviorTreeNode> children)
        {
            _children = children;
        }

        public override BTNodeState Execute()
        {
            foreach (BehaviorTreeNode child in _children)
            {
                BTNodeState childState = child.Execute();
                switch (childState)
                {
                    case BTNodeState.SUCCESS:
                    case BTNodeState.RUNNING:
                        _nowState = childState;
                        return _nowState;
                }
            }
            _nowState = BTNodeState.FAILURE;
            return _nowState;
        }
    }
    #endregion

    #region Decorator Node
    public class SucceederNode : BehaviorTreeNode
    {
        List<BehaviorTreeNode> _children = new List<BehaviorTreeNode>();

        public SucceederNode(List<BehaviorTreeNode> children)
        {
            _nowState = BTNodeState.SUCCESS;
            _children = children;
        }

        public override BTNodeState Execute()
        {
            foreach (BehaviorTreeNode child in _children)
            {
                child.Execute();
            }
            return _nowState;
        }
    }

    public class ConverterNode : BehaviorTreeNode
    {
        BehaviorTreeNode _child;

        public ConverterNode(BehaviorTreeNode child)
        {
            _child = child;
        }

        public override BTNodeState Execute()
        {
            switch (_child.Execute())
            {
                case BTNodeState.SUCCESS:
                    return BTNodeState.FAILURE;
                case BTNodeState.FAILURE:
                    return BTNodeState.SUCCESS;
                case BTNodeState.RUNNING:
                    return BTNodeState.RUNNING;
            }
            return BTNodeState.ERROR;
        }
    }
    #endregion

    #region Condition Node
    public class DeathConditionNode : BehaviorTreeNode
    {
        Stats _stats;

        public DeathConditionNode(Stats stats)
        {
            _stats = stats;
        }

        public override BTNodeState Execute()
        {
            return _stats.IsDead ? BTNodeState.SUCCESS : BTNodeState.FAILURE;
        }
    }

    public class KnockbackConditionNode : BehaviorTreeNode
    {
        Knockback _knockback;

        public KnockbackConditionNode(Knockback knockback)
        {
            _knockback = knockback;
        }

        public override BTNodeState Execute()
        {
            return _knockback.IsKnockbacked ? BTNodeState.SUCCESS : BTNodeState.FAILURE;
        }
    }

    public class AttackingConditionNode : BehaviorTreeNode
    {
        EnemyBehavior _enemyBehavior;

        public AttackingConditionNode(EnemyBehavior enemyBehavior)
        {
            _enemyBehavior = enemyBehavior;
        }

        public override BTNodeState Execute()
        {
            return _enemyBehavior.isAttacking ? BTNodeState.SUCCESS : BTNodeState.FAILURE;
        }
    }

    public class TargetConditionNode : BehaviorTreeNode
    {
        EnemyBehavior _enemyBehavior;

        public TargetConditionNode(EnemyBehavior enemyBehavior)
        {
            _enemyBehavior = enemyBehavior;
        }

        public override BTNodeState Execute()
        {
            return _enemyBehavior.target != null ? BTNodeState.SUCCESS : BTNodeState.FAILURE;
        }
    }

    public class CombatMovingConditionNode : BehaviorTreeNode
    {
        EnemyBehavior _enemyBehavior;

        public CombatMovingConditionNode(EnemyBehavior enemyBehavior)
        {
            _enemyBehavior = enemyBehavior;
        }

        public override BTNodeState Execute()
        {
            if (_enemyBehavior.isSideWalking || _enemyBehavior.isBackwarding || _enemyBehavior.isTracing)
            {
                return BTNodeState.SUCCESS;
            }
            else
            {
                return BTNodeState.FAILURE;
            }
        }
    }

    public class NowReturningConditionNode : BehaviorTreeNode
    {
        EnemyBehavior _enemyBehavior;

        public NowReturningConditionNode(EnemyBehavior enemyBehavior)
        {
            _enemyBehavior = enemyBehavior;
        }

        public override BTNodeState Execute()
        {
            return _enemyBehavior.isReturning ? BTNodeState.SUCCESS : BTNodeState.FAILURE;
        }
    }

    public class StartReturningConditionNode : BehaviorTreeNode
    {
        EnemyBehavior _enemyBehavior;
        Animator _anim;

        public StartReturningConditionNode(EnemyBehavior enemyBehavior, Animator anim)
        {
            _enemyBehavior = enemyBehavior;
            _anim = anim;
        }

        public override BTNodeState Execute()
        {
            if (_enemyBehavior.activityRange < 0) return BTNodeState.FAILURE;
            if (!_anim.GetBool("HasTarget")) return BTNodeState.FAILURE;

            if (_enemyBehavior.target == null)
            {
                return BTNodeState.SUCCESS;
            }

            float enemyDistance = Vector3.Distance(_enemyBehavior.target.transform.position, _enemyBehavior.originPos);
            float totalRange = _enemyBehavior.activityRange + _enemyBehavior.sightRange;
            if (enemyDistance > totalRange)
            {
                return BTNodeState.SUCCESS;
            }

            return BTNodeState.FAILURE;
        }
    }

    public class DelayedConditionNode : BehaviorTreeNode
    {
        EnemyBehavior _enemyBehavior;

        public DelayedConditionNode(EnemyBehavior enemyBehavior)
        {
            _enemyBehavior = enemyBehavior;
        }

        public override BTNodeState Execute()
        {
            return _enemyBehavior.delayTimer > 0 ? BTNodeState.SUCCESS : BTNodeState.FAILURE;
        }
    }

    public class AttackableConditionNode : BehaviorTreeNode
    {
        EnemyBehavior _enemyBehavior;

        public AttackableConditionNode(EnemyBehavior enemyBehavior)
        {
            _enemyBehavior = enemyBehavior;
        }

        public override BTNodeState Execute()
        {
            return _enemyBehavior.skills.PrepareSkill() ? BTNodeState.SUCCESS : BTNodeState.FAILURE;
        }
    }

    public class EnemyDetectedConditionNode : BehaviorTreeNode
    {
        EnemyBehavior _enemyBehavior;

        public EnemyDetectedConditionNode(EnemyBehavior enemyBehavior)
        {
            _enemyBehavior = enemyBehavior;
        }

        public override BTNodeState Execute()
        {
            //TODO:notice
            if (CheckEnemyInSight())
            {
                return BTNodeState.SUCCESS;
            }

            return BTNodeState.FAILURE;
        }

        bool CheckEnemyInSight()
        {
            List<GameObject> possibleTargets = new();
            Collider[] cols = Physics.OverlapSphere(_enemyBehavior.transform.position, _enemyBehavior.sightRange, LayerMask.GetMask("Player"));

            foreach (Collider col in cols)
            {
                Vector3 dir = col.transform.position - _enemyBehavior.transform.position;
                dir -= Vector3.up * dir.y;
                float halfSightAngle = _enemyBehavior.sightAngle / 2;
                if (Vector3.Angle(_enemyBehavior.transform.forward, dir) > halfSightAngle)
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
                float distance = Vector3.Distance(_enemyBehavior.transform.position, possibleTarget.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = possibleTarget;
                }
            }

            if (closest != null && closestDistance <= _enemyBehavior.activityRange + _enemyBehavior.sightRange)
            {
                _enemyBehavior.target = closest;

                return true;
            }

            return false;
        }
    }

    public class WanderableConditionNode : BehaviorTreeNode
    {
        EnemyBehavior _enemyBehavior;

        public WanderableConditionNode(EnemyBehavior enemyBehavior)
        {
            _enemyBehavior = enemyBehavior;
        }

        public override BTNodeState Execute()
        {
            return _enemyBehavior.activityRange > 0 ? BTNodeState.SUCCESS : BTNodeState.FAILURE;
        }
    }

    public class WanderingConditionNode : BehaviorTreeNode
    {
        EnemyBehavior _enemyBehavior;

        public WanderingConditionNode(EnemyBehavior enemyBehavior)
        {
            _enemyBehavior = enemyBehavior;
        }

        public override BTNodeState Execute()
        {
            return _enemyBehavior.isWandering ? BTNodeState.SUCCESS : BTNodeState.FAILURE;
        }
    }

    public class DelayCooldowningConditionNode : BehaviorTreeNode
    {
        EnemyBehavior _enemyBehavior;

        public DelayCooldowningConditionNode(EnemyBehavior enemyBehavior)
        {
            _enemyBehavior = enemyBehavior;
        }

        public override BTNodeState Execute()
        {
            return _enemyBehavior.delayActivationCooldown > 0 ? BTNodeState.SUCCESS : BTNodeState.FAILURE;
        }
    }
    #endregion

    #region Action Node
    public class DeathActionNode : BehaviorTreeNode
    {
        EnemyBehavior _enemyBehavior;

        public DeathActionNode(EnemyBehavior enemyBehavior)
        {
            _enemyBehavior = enemyBehavior;
        }

        public override BTNodeState Execute()
        {
            _enemyBehavior.Die();

            return BTNodeState.SUCCESS;
        }
    }

    public class CancleAttackActionNode : BehaviorTreeNode
    {
        EnemyBehavior _enemyBehavior;

        public CancleAttackActionNode(EnemyBehavior enemyBehavior)
        {
            _enemyBehavior = enemyBehavior;
        }

        public override BTNodeState Execute()
        {
            _enemyBehavior.CancelAttack();

            return BTNodeState.SUCCESS;
        }
    }

    public class StartReturningActionNode : BehaviorTreeNode
    {
        EnemyBehavior _enemyBehavior;
        AIPath _aiPath;

        public StartReturningActionNode(EnemyBehavior enemyBehavior, AIPath aiPath)
        {
            _enemyBehavior = enemyBehavior;
            _aiPath = aiPath;
        }

        public override BTNodeState Execute()
        {
            _aiPath.maxSpeed = _enemyBehavior.wanderingSpeed;

            _aiPath.enableRotation = true;

            _enemyBehavior.StartReturning();

            return BTNodeState.SUCCESS;
        }
    }

    public class CombatMovementActionNode : BehaviorTreeNode
    {
        EnemyBehavior _enemyBehavior;
        AIPath _aiPath;
        Animator _anim;

        public CombatMovementActionNode(EnemyBehavior enemyBehavior, AIPath aiPath, Animator anim)
        {
            _enemyBehavior = enemyBehavior;
            _aiPath = aiPath;
            _anim = anim;
        }

        public override BTNodeState Execute()
        {
            if (_aiPath.pathPending) return BTNodeState.SUCCESS;

            float distance = Vector3.Distance(_enemyBehavior.target.transform.position, _enemyBehavior.transform.position);
            float distDiff = distance - _enemyBehavior.tracingDistance;
            Vector3 dir = (_enemyBehavior.target.transform.position - _enemyBehavior.transform.position).normalized;
            dir -= Vector3.up * dir.y;

            if (distance < _enemyBehavior.tracingDistance * 0.6f)
            {
                _aiPath.enableRotation = false;
                _aiPath.maxSpeed = _enemyBehavior.backwardSpeed;

                _enemyBehavior.isBackwarding = true;
                Vector3 validPosition = GetClosestValidPosition(_enemyBehavior.transform.position + dir * distDiff);

                _enemyBehavior.SetDestination(validPosition);

                _anim.SetBool("IsGoingBackward", true);
            }
            else if (distance > _enemyBehavior.tracingDistance * 1.4f)
            {
                _aiPath.maxSpeed = _enemyBehavior.tracingSpeed;
                _aiPath.enableRotation = true;

                _enemyBehavior.isTracing = true;
                Vector3 validPosition = GetClosestValidPosition(_enemyBehavior.transform.position + dir * distDiff);

                _enemyBehavior.SetDestination(validPosition);
                _anim.SetBool("IsTracingTarget", true);
            }
            else
            {
                if (_enemyBehavior.isSideWalking) return BTNodeState.SUCCESS;

                _aiPath.maxSpeed = _enemyBehavior.sideWalkSpeed;
                _aiPath.enableRotation = false;

                _enemyBehavior.isSideWalking = true;

                int right = Random.Range(0, 2) == 1 ? 1 : -1;
                float sideDist = Random.Range(3f, 5.5f);
                Vector3 validPosition = GetClosestValidPosition(_enemyBehavior.transform.position + _enemyBehavior.transform.right * (sideDist * right));

                _enemyBehavior.SetDestination(validPosition);

                if (right == -1)
                {
                    _anim.SetBool("IsGoingLeft", true);
                }
                else
                {
                    _anim.SetBool("IsGoingRight", true);
                }
            }

            return BTNodeState.SUCCESS;
        }

        Vector3 GetClosestValidPosition(Vector3 targetPosition)
        {
            NNConstraint nnConstraint = NNConstraint.Default;
            nnConstraint.constrainWalkability = true;
            nnConstraint.walkable = true;

            NNInfo nearestNode = AstarPath.active.GetNearest(targetPosition, nnConstraint);
            Vector3 validPosition = nearestNode.position;

            // 유효한 위치인지 추가 확인
            if (nearestNode.node == null || !nearestNode.node.Walkable)
            {
                validPosition = _enemyBehavior.transform.position; // 유효하지 않으면 현재 위치 반환
            }

            return validPosition;
        }
    }

    public class AttackActionNode : BehaviorTreeNode
    {
        EnemyBehavior _enemyBehavior;
        AIPath _aiPath;

        public AttackActionNode(EnemyBehavior enemyBehavior, AIPath aiPath)
        {
            _enemyBehavior = enemyBehavior;
            _aiPath = aiPath;
        }

        public override BTNodeState Execute()
        {
            _aiPath.enableRotation = true;

            _enemyBehavior.EndMovement();
            _enemyBehavior.Attack();

            return BTNodeState.SUCCESS;
        }
    }

    public class StartCombatActionNode : BehaviorTreeNode
    {
        EnemyBehavior _enemyBehavior;

        public StartCombatActionNode(EnemyBehavior enemyBehavior)
        {
            _enemyBehavior = enemyBehavior;
        }

        public override BTNodeState Execute()
        {
            _enemyBehavior.StartFight();

            return BTNodeState.SUCCESS;
        }
    }

    public class WanderActionNode : BehaviorTreeNode
    {
        EnemyBehavior _enemyBehavior;
        AIPath _aiPath;
        Animator _anim;

        public WanderActionNode(EnemyBehavior enemyBehavior, AIPath aiPath, Animator anim)
        {
            _enemyBehavior = enemyBehavior;
            _aiPath = aiPath;
            _anim = anim;
        }

        public override BTNodeState Execute()
        {
            if (_aiPath.pathPending) return BTNodeState.SUCCESS;

            _aiPath.maxSpeed = _enemyBehavior.wanderingSpeed;
            _aiPath.enableRotation = true;

            float range = _enemyBehavior.activityRange;
            float xOffset = Random.Range(-range, range);
            float zOffset = Random.Range(-range, range);

            NNInfo nearestNode = AstarPath.active.GetNearest(new Vector3(_enemyBehavior.originPos.x + xOffset, _enemyBehavior.originPos.y, _enemyBehavior.originPos.z + zOffset), NNConstraint.Default);
            Vector3 validPosition = nearestNode.position;
            _enemyBehavior.SetDestination(validPosition);
            _enemyBehavior.isWandering = true;

            _anim.SetBool("IsWandering", true);

            return BTNodeState.SUCCESS;
        }
    }

    public class EndMovementActionNode : BehaviorTreeNode
    {
        EnemyBehavior _enemyBehavior;
        AIPath _aiPath;

        public EndMovementActionNode(EnemyBehavior enemyBehavior, AIPath aiPath)
        {
            _enemyBehavior = enemyBehavior;
            _aiPath = aiPath;
        }

        public override BTNodeState Execute()
        {
            if (_aiPath.reachedEndOfPath)
            {
                _aiPath.enableRotation = true;
                _enemyBehavior.EndMovement();
            }

            return BTNodeState.SUCCESS;
        }
    }

    public class ActivateDelayActionNode : BehaviorTreeNode
    {
        EnemyBehavior _enemyBehavior;

        public ActivateDelayActionNode(EnemyBehavior enemyBehavior)
        {
            _enemyBehavior = enemyBehavior;
        }

        public override BTNodeState Execute()
        {
            _enemyBehavior.delayTimer = Random.Range(_enemyBehavior.MIN_DELAY_TIME, _enemyBehavior.MAX_DELAY_TIME);

            return BTNodeState.SUCCESS;
        }
    }
    #endregion
}
