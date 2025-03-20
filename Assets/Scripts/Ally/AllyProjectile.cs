using UnityEngine;

public class AllyProjectile : MonoBehaviour
{
    [SerializeField]
    private float lifetime = 3f;

    [SerializeField]
    private GameObject hitEffectPrefab;

    [SerializeField]
    private bool destroyOnHit = true;

    private int damage = 10;
    private GameObject owner;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            // Gây sát thương
            enemyHealth.TakeDamage(damage);

            // Hiệu ứng trúng đạn
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            }

            // Hủy đạn nếu cần
            if (destroyOnHit)
            {
                Destroy(gameObject);
            }
        }
    }

    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }

    public void SetOwner(GameObject newOwner)
    {
        owner = newOwner;
    }
}
