using UnityEngine;

public class AllyMelee : MonoBehaviour, IAlly
{
    [SerializeField]
    private int damage = 15;

    [SerializeField]
    private float attackRadius = 1.2f;

    [SerializeField]
    private LayerMask enemyLayer;

    [SerializeField]
    private Vector3 attackOffset = new Vector3(0.5f, 0f, 0f);

    [SerializeField]
    private GameObject hitEffect;

    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void PerformAction()
    {
        // Tấn công được gọi từ AllyAI
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
        else
        {
            // Nếu không có animator, gọi ngay lập tức
            DealDamageAnimEvent();
        }
    }

    // Được gọi từ animation event
    public void DealDamageAnimEvent()
    {
        // Tính toán vị trí tấn công dựa trên hướng của sprite
        Vector3 attackPosition = transform.position;
        if (spriteRenderer != null)
        {
            if (spriteRenderer.flipX)
            {
                attackPosition += new Vector3(-attackOffset.x, attackOffset.y, attackOffset.z);
            }
            else
            {
                attackPosition += attackOffset;
            }
        }

        // Phát hiện kẻ địch xung quanh
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
            attackPosition,
            attackRadius,
            enemyLayer
        );

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                // Gây sát thương cho kẻ địch
                enemyHealth.TakeDamage(damage);

                // Hiệu ứng đánh trúng
                if (hitEffect != null)
                {
                    Instantiate(hitEffect, enemy.transform.position, Quaternion.identity);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Vẽ phạm vi tấn công trong editor
        Vector3 attackPosition = transform.position + attackOffset;
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(attackPosition, attackRadius);
    }
}

// Lớp đồng minh tấn công từ xa (tái sử dụng Grape)
