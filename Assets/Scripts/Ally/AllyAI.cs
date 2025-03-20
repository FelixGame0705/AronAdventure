using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllyAI : MonoBehaviour
{
    public enum State
    {
        Following,
        Attacking,
        Guarding,
        Moving,
    }

    [SerializeField]
    private float followDistance = 3f; // Khoảng cách theo sau player

    [SerializeField]
    private float attackRange = 2f;

    [SerializeField]
    private float attackCooldown = 2f;

    [SerializeField]
    private MonoBehaviour allyType; // Component triển khai IAlly

    [SerializeField]
    private bool autoAttack = true; // Tự động tấn công kẻ địch gần nhất

    [SerializeField]
    private bool stopMovingWhileAttacking = false;

    [SerializeField]
    private float targetUpdateRate = 0.5f; // Tần số cập nhật mục tiêu

    private State currentState;
    private Transform player;
    private Transform targetEnemy;
    private Vector3 targetPosition;
    private AllyPathfinding pathfinding;
    private Animator animator;
    private bool canAttack = true;
    private float lastTargetSearch;

    private void Awake()
    {
        pathfinding = GetComponent<AllyPathfinding>();
        animator = GetComponent<Animator>();
        currentState = State.Following;
    }

    private void Start()
    {
        player = PlayerController.Instance.transform;
    }

    private void Update()
    {
        if (player == null)
            return;

        // Cập nhật kẻ địch mục tiêu theo tần số
        if (autoAttack && Time.time > lastTargetSearch + targetUpdateRate)
        {
            FindNearestEnemy();
            lastTargetSearch = Time.time;
        }

        // Máy trạng thái đơn giản
        switch (currentState)
        {
            case State.Following:
                FollowPlayer();
                CheckForEnemies();
                break;

            case State.Attacking:
                AttackTarget();
                break;

            case State.Guarding:
                GuardPosition();
                CheckForEnemies();
                break;

            case State.Moving:
                MoveToPosition();
                break;
        }
    }

    // Tìm kẻ địch gần nhất
    private void FindNearestEnemy()
    {
        // Tìm tất cả các đối tượng có EnemyHealth trong phạm vi
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 10f);
        float closestDistance = float.MaxValue;
        Transform closest = null;

        foreach (Collider2D col in colliders)
        {
            EnemyHealth enemyHealth = col.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                float distance = Vector2.Distance(transform.position, col.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = col.transform;
                }
            }
        }

        targetEnemy = closest;
    }

    // Theo sau người chơi
    private void FollowPlayer()
    {
        if (Vector2.Distance(transform.position, player.position) > followDistance)
        {
            Vector2 directionToPlayer = (player.position - transform.position).normalized;
            Vector2 targetPos = (Vector2)player.position - directionToPlayer * followDistance;
            pathfinding.MoveTo(targetPos);

            if (animator != null)
            {
                animator.SetBool("Moving", true);
            }
        }
        else
        {
            pathfinding.StopMoving();

            if (animator != null)
            {
                animator.SetBool("Moving", false);
            }
        }
    }

    // Tấn công mục tiêu
    private void AttackTarget()
    {
        if (targetEnemy == null)
        {
            currentState = State.Following;
            return;
        }

        float distanceToTarget = Vector2.Distance(transform.position, targetEnemy.position);

        if (distanceToTarget > attackRange)
        {
            // Di chuyển đến mục tiêu
            pathfinding.MoveTo(targetEnemy.position);

            if (animator != null)
            {
                animator.SetBool("Moving", true);
            }
        }
        else
        {
            // Trong tầm tấn công
            if (stopMovingWhileAttacking)
            {
                pathfinding.StopMoving();
            }

            if (canAttack)
            {
                // Thực hiện tấn công
                if (allyType != null && allyType is IAlly)
                {
                    (allyType as IAlly).PerformAction();

                    if (animator != null)
                    {
                        animator.SetTrigger("Attack");
                    }

                    // Thiết lập cooldown
                    StartCoroutine(AttackCooldownRoutine());
                }
            }
        }
    }

    // Bảo vệ vị trí
    private void GuardPosition()
    {
        // Logic đứng yên và tấn công nếu có kẻ địch trong tầm
        if (
            targetEnemy != null
            && Vector2.Distance(transform.position, targetEnemy.position) <= attackRange
        )
        {
            if (canAttack)
            {
                // Thực hiện tấn công
                if (allyType != null && allyType is IAlly)
                {
                    (allyType as IAlly).PerformAction();

                    if (animator != null)
                    {
                        animator.SetTrigger("Attack");
                    }

                    // Thiết lập cooldown
                    StartCoroutine(AttackCooldownRoutine());
                }
            }
        }
    }

    // Di chuyển đến vị trí chỉ định
    private void MoveToPosition()
    {
        if (Vector2.Distance(transform.position, targetPosition) > 0.5f)
        {
            pathfinding.MoveTo(targetPosition);

            if (animator != null)
            {
                animator.SetBool("Moving", true);
            }
        }
        else
        {
            // Đã đến vị trí, chuyển sang chế độ bảo vệ
            currentState = State.Guarding;
            pathfinding.StopMoving();

            if (animator != null)
            {
                animator.SetBool("Moving", false);
            }
        }
    }

    // Kiểm tra kẻ địch xung quanh
    private void CheckForEnemies()
    {
        if (targetEnemy != null && autoAttack)
        {
            currentState = State.Attacking;
        }
    }

    // Cooldown tấn công
    private IEnumerator AttackCooldownRoutine()
    {
        canAttack = false;
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    // Các phương thức công khai để điều khiển đồng minh

    public void SetTargetPosition(Vector3 position)
    {
        targetPosition = position;
        currentState = State.Moving;
    }

    public void SetTargetEnemy(Transform enemy)
    {
        targetEnemy = enemy;
        currentState = State.Attacking;
    }

    public void FollowPlayerCommand()
    {
        currentState = State.Following;
    }

    public void GuardCurrentPosition()
    {
        targetPosition = transform.position;
        currentState = State.Guarding;
    }
}
