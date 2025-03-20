using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grape : MonoBehaviour, IEnemy
{
    [SerializeField]
    private GameObject grapeProjectilePrefab;

    [SerializeField]
    private bool canTargetAllies = true; // Có thể nhắm vào allies

    private Animator myAnimator;
    private SpriteRenderer spriteRenderer;
    private EnemyAI enemyAI;

    readonly int ATTACK_HASH = Animator.StringToHash("Attack");

    private void Awake()
    {
        myAnimator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        enemyAI = GetComponent<EnemyAI>();
    }

    public void Attack()
    {
        myAnimator.SetTrigger(ATTACK_HASH);

        // Lấy mục tiêu hiện tại từ EnemyAI nếu có thể
        Transform target =
            enemyAI != null ? enemyAI.GetCurrentTarget() : PlayerController.Instance.transform;

        // Nếu không có mục tiêu, mặc định về player
        if (target == null)
        {
            target = PlayerController.Instance.transform;
        }

        // Xoay sprite dựa trên vị trí mục tiêu
        if (transform.position.x > target.position.x)
        {
            spriteRenderer.flipX = true;
        }
        else
        {
            spriteRenderer.flipX = false;
        }
    }

    public void SpawnProjectileAnimEvent()
    {
        // Lấy mục tiêu hiện tại từ EnemyAI nếu có thể
        Transform target =
            enemyAI != null ? enemyAI.GetCurrentTarget() : PlayerController.Instance.transform;

        // Nếu không có mục tiêu, mặc định về player
        if (target == null)
        {
            target = PlayerController.Instance.transform;
        }

        // Tạo projectile và truyền thông tin mục tiêu
        GameObject projectile = Instantiate(
            grapeProjectilePrefab,
            transform.position,
            Quaternion.identity
        );

        GrapeProjectile grapeProjectile = projectile.GetComponent<GrapeProjectile>();
        if (grapeProjectile != null)
        {
            grapeProjectile.SetTarget(target);
            grapeProjectile.SetCanTargetAllies(canTargetAllies);
        }
    }
}
