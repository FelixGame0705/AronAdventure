using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Enemies
{
    public class EnemyMelee : MonoBehaviour, IEnemy
    {
        [Header("Enemy Stats")]
        [SerializeField]
        private float attackRange = 1.5f;

        [SerializeField]
        private int attackDamage = 10;

        [SerializeField]
        private float attackCooldown = 1.5f;

        [SerializeField]
        private float knockbackForce = 3f;

        [Header("Attack Settings")]
        [SerializeField]
        private Vector3 attackOffset = new Vector3(0.5f, 0.5f, 0f); // Điều chỉnh vị trí

        [SerializeField]
        private float attackRadius = 1f;

        [SerializeField]
        private LayerMask attackMask; // Layer chứa cả player và allies

        private bool isAttacking = false;
        private Transform currentTarget; // Có thể là player hoặc ally
        private Animator animator;
        private EnemyAI enemyAI;
        private SpriteRenderer spriteRenderer;

        private readonly int ATTACK_HASH = Animator.StringToHash("Attack");

        private void Awake()
        {
            animator = GetComponent<Animator>();
            enemyAI = GetComponent<EnemyAI>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            // Mặc định mục tiêu là player
            if (PlayerController.Instance != null)
            {
                currentTarget = PlayerController.Instance.transform;
            }
            else
            {
                Debug.LogError("PlayerController.Instance is null! Kiểm tra lại hệ thống Player.");
            }
        }

        private void Update()
        {
            // Lấy mục tiêu hiện tại từ EnemyAI
            if (enemyAI != null)
            {
                currentTarget = enemyAI.GetCurrentTarget();
            }
        }

        public void Attack()
        {
            if (!isAttacking)
            {
                isAttacking = true;

                // Xoay sprite về phía mục tiêu
                if (currentTarget != null && spriteRenderer != null)
                {
                    spriteRenderer.flipX = transform.position.x > currentTarget.position.x;
                }

                StartCoroutine(AttackRoutine());
            }
        }

        private IEnumerator AttackRoutine()
        {
            yield return new WaitForSeconds(0.2f); // Delay trước khi gây sát thương

            if (animator != null)
            {
                animator.SetTrigger(ATTACK_HASH);
            }

            PerformAttack();
            yield return new WaitForSeconds(attackCooldown - 0.2f); // Trừ thời gian delay
            isAttacking = false;
        }

        private void PerformAttack()
        {
            // Tính toán vị trí tấn công dựa vào hướng nhìn
            Vector3 attackPosition;
            if (spriteRenderer != null && spriteRenderer.flipX)
            {
                // Nếu đang nhìn sang trái
                attackPosition =
                    transform.position
                    + new Vector3(-attackOffset.x, attackOffset.y, attackOffset.z);
            }
            else
            {
                // Nếu đang nhìn sang phải
                attackPosition = transform.position + attackOffset;
            }

            // Tìm các đối tượng trong phạm vi tấn công
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(
                attackPosition,
                attackRadius,
                attackMask
            );

            foreach (Collider2D hit in hitColliders)
            {
                // Kiểm tra nếu là player
                PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(attackDamage, transform);
                    ApplyKnockback(hit.transform);
                    continue; // Tiếp tục vòng lặp
                }

                // Kiểm tra nếu là ally
                AllyHealth allyHealth = hit.GetComponent<AllyHealth>();
                if (allyHealth != null)
                {
                    allyHealth.TakeDamage(attackDamage, transform);
                    ApplyKnockback(hit.transform);
                }
            }
        }

        private void ApplyKnockback(Transform hitTarget)
        {
            if (hitTarget.TryGetComponent(out Rigidbody2D targetRb))
            {
                Vector2 knockbackDirection = (hitTarget.position - transform.position).normalized;
                targetRb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Hiển thị phạm vi tấn công
            Vector3 attackPosition = transform.position + attackOffset;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPosition, attackRadius);
        }
    }
}
