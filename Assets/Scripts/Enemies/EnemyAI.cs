using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [SerializeField]
    private float roamChangeDirFloat = 2f;

    [SerializeField]
    private float attackRange = 0f;

    [SerializeField]
    private MonoBehaviour enemyType;

    [SerializeField]
    private float attackCooldown = 2f;

    [SerializeField]
    private bool stopMovingWhileAttacking = false;

    [SerializeField]
    private Animator animator;

    [SerializeField]
    private bool canAttackAllies = true; // Có thể tấn công allies không

    [SerializeField]
    private float targetingRadius = 8f; // Phạm vi tìm kiếm mục tiêu

    [SerializeField]
    private LayerMask targetLayerMask; // Layer chứa player và allies

    private bool canAttack = true;
    private Transform currentTarget; // Player hoặc Ally hiện tại
    private float nextTargetSearchTime = 0f;
    private const float TARGET_SEARCH_INTERVAL = 0.5f; // Thời gian giữa các lần tìm kiếm mục tiêu

    private enum State
    {
        Roaming,
        Attacking,
    }

    private Vector2 roamPosition;
    private float timeRoaming = 0f;

    private State state;
    private EnemyPathfinding enemyPathfinding;

    private void Awake()
    {
        enemyPathfinding = GetComponent<EnemyPathfinding>();
        state = State.Roaming;
    }

    private void Start()
    {
        roamPosition = GetRoamingPosition();
        animator = GetComponent<Animator>();

        // Mặc định mục tiêu là player
        currentTarget = PlayerController.Instance.transform;
    }

    private void Update()
    {
        // Định kỳ tìm kiếm mục tiêu
        if (Time.time >= nextTargetSearchTime)
        {
            FindBestTarget();
            nextTargetSearchTime = Time.time + TARGET_SEARCH_INTERVAL;
        }

        MovementStateControl();
    }

    private void MovementStateControl()
    {
        switch (state)
        {
            default:
            case State.Roaming:
                Roaming();
                break;
            case State.Attacking:
                Attacking();
                break;
        }
    }

    private void Roaming()
    {
        timeRoaming += Time.deltaTime;
        enemyPathfinding.MoveTo(roamPosition);

        // Kiểm tra mục tiêu hiện tại
        if (currentTarget != null)
        {
            float distanceToTarget = Vector2.Distance(transform.position, currentTarget.position);
            if (distanceToTarget < attackRange)
            {
                state = State.Attacking;
            }
        }

        if (timeRoaming > roamChangeDirFloat)
        {
            roamPosition = GetRoamingPosition();
        }
    }

    private void Attacking()
    {
        // Kiểm tra nếu mục tiêu không còn tồn tại hoặc ngoài tầm tấn công
        if (currentTarget == null)
        {
            state = State.Roaming;
            return;
        }

        float distanceToTarget = Vector2.Distance(transform.position, currentTarget.position);
        if (distanceToTarget > attackRange)
        {
            state = State.Roaming;
            return;
        }

        if (attackRange != 0 && canAttack)
        {
            canAttack = false;
            (enemyType as IEnemy).Attack();

            // Xoay sprite về phía mục tiêu
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = transform.position.x > currentTarget.position.x;
            }

            if (stopMovingWhileAttacking)
            {
                enemyPathfinding.StopMoving();
            }
            else
            {
                enemyPathfinding.MoveTo(roamPosition);
            }

            StartCoroutine(AttackCooldownRoutine());
        }
    }

    // Tìm mục tiêu tốt nhất (player hoặc ally gần nhất)
    private void FindBestTarget()
    {
        // Mặc định mục tiêu là player
        Transform bestTarget = PlayerController.Instance.transform;
        float bestDistance = Vector2.Distance(transform.position, bestTarget.position);

        // Nếu không thể tấn công ally, chỉ sử dụng player làm mục tiêu
        if (!canAttackAllies)
        {
            currentTarget = bestTarget;
            return;
        }

        // Tìm kiếm ally gần nhất
        Collider2D[] nearbyTargets = Physics2D.OverlapCircleAll(
            transform.position,
            targetingRadius,
            targetLayerMask
        );

        foreach (Collider2D target in nearbyTargets)
        {
            // Bỏ qua nếu là chính mình
            if (target.transform == transform)
                continue;

            // Kiểm tra xem đối tượng có phải là ally không
            AllyHealth allyHealth = target.GetComponent<AllyHealth>();
            if (allyHealth != null)
            {
                float distance = Vector2.Distance(transform.position, target.transform.position);

                // Cập nhật mục tiêu nếu ally gần hơn
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestTarget = target.transform;
                }
            }
        }

        currentTarget = bestTarget;
    }

    private IEnumerator AttackCooldownRoutine()
    {
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    private Vector2 GetRoamingPosition()
    {
        timeRoaming = 0f;
        return new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
    }

    // Hàm này cho phép các thành phần khác đặt mục tiêu cho enemy
    public void SetTarget(Transform target)
    {
        if (target != null)
        {
            currentTarget = target;
        }
    }

    // Lấy mục tiêu hiện tại
    public Transform GetCurrentTarget()
    {
        return currentTarget;
    }

    // Vẽ phạm vi phát hiện mục tiêu và tấn công trong Editor
    private void OnDrawGizmosSelected()
    {
        // Phạm vi tìm kiếm mục tiêu
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, targetingRadius);

        // Phạm vi tấn công
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
