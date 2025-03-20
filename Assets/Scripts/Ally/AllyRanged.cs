using UnityEngine;

public class AllyRanged : MonoBehaviour, IAlly
{
    [SerializeField]
    private GameObject projectilePrefab;

    [SerializeField]
    private Transform firePoint;

    [SerializeField]
    private float projectileSpeed = 8f;

    [SerializeField]
    private int damaged = 2;

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
            // Nếu không có animator, bắn ngay lập tức
            FireAllyProjectileAnimEvent();
        }

        // Xoay sprite theo hướng của mục tiêu
        Transform targetEnemy = FindNearestEnemy();
        if (targetEnemy != null)
        {
            if (transform.position.x > targetEnemy.position.x)
            {
                spriteRenderer.flipX = true;
            }
            else
            {
                spriteRenderer.flipX = false;
            }
        }
    }

    // Được gọi từ animation event
    public void FireAllyProjectileAnimEvent()
    {
        if (projectilePrefab == null)
            return;

        // Tìm kẻ địch gần nhất để nhắm bắn
        Transform targetEnemy = FindNearestEnemy();

        if (targetEnemy != null)
        {
            // Xác định vị trí bắn
            Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position;

            // Tạo đạn
            GameObject projectile = Instantiate(
                projectilePrefab,
                spawnPosition,
                Quaternion.identity
            );

            // Thiết lập hướng và vận tốc
            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 direction = (targetEnemy.position - spawnPosition).normalized;
                rb.linearVelocity = direction * projectileSpeed;

                // Xoay đạn theo hướng di chuyển
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                projectile.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }

            // Nếu đạn có script AllyProjectile, thiết lập thông tin
            AllyProjectile allyProjectile = projectile.GetComponent<AllyProjectile>();
            if (allyProjectile != null)
            {
                allyProjectile.SetDamage(damaged);
                allyProjectile.SetOwner(gameObject);
            }
        }
    }

    private Transform FindNearestEnemy()
    {
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

        return closest;
    }
}
