using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllyHealth : MonoBehaviour
{
    [SerializeField]
    private int maxHealth = 20;

    [SerializeField]
    private GameObject deathVFXPrefab;

    [SerializeField]
    private float knockBackThrust = 10f;

    private int currentHealth;
    private Knockback knockback;
    private Flash flash;
    private bool isDead = false;

    public delegate void OnHealthChangedDelegate(int currentHealth, int maxHealth);
    public event OnHealthChangedDelegate OnHealthChanged;

    public delegate void OnDeathDelegate();
    public event OnDeathDelegate OnDeath;

    private void Awake()
    {
        flash = GetComponent<Flash>();
        knockback = GetComponent<Knockback>();
    }

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage, Transform attacker)
    {
        // Không nhận sát thương nếu đã chết
        if (isDead)
            return;

        currentHealth -= damage;

        if (knockback != null)
        {
            knockback.GetKnockedBack(attacker, knockBackThrust);
        }

        if (flash != null)
        {
            StartCoroutine(flash.FlashRoutine());
        }

        // Thông báo sự thay đổi máu
        if (OnHealthChanged != null)
        {
            OnHealthChanged(currentHealth, maxHealth);
        }

        StartCoroutine(CheckDetectDeathRoutine());
    }

    private IEnumerator CheckDetectDeathRoutine()
    {
        if (flash != null)
        {
            yield return new WaitForSeconds(flash.GetRestoreMatTime());
        }
        else
        {
            yield return null;
        }

        DetectDeath();
    }

    private void DetectDeath()
    {
        if (currentHealth <= 0 && !isDead)
        {
            Die();
        }
    }

    // Phương thức công khai để kích hoạt cái chết trực tiếp
    public void Die()
    {
        if (isDead)
            return; // Tránh gọi nhiều lần

        isDead = true;

        // Thông báo sự kiện chết
        if (OnDeath != null)
        {
            OnDeath();
        }

        // Chơi animation chết nếu có
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Death");
            StartCoroutine(DeathWithAnimationRoutine());
        }
        else
        {
            // Tạo hiệu ứng và hủy ngay lập tức nếu không có animation
            InstantiateDeathEffect();
            Destroy(gameObject);
        }
    }

    // Coroutine để đợi animation chết kết thúc
    private IEnumerator DeathWithAnimationRoutine()
    {
        // Vô hiệu hóa các component khác
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        // Vô hiệu hóa component di chuyển nếu có
        MonoBehaviour[] components = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour comp in components)
        {
            if (comp != this && comp.GetType().Name.Contains("Movement"))
            {
                comp.enabled = false;
            }
        }

        // Đợi animation chết chạy xong
        yield return new WaitForSeconds(1.0f); // Thời gian animation chết

        InstantiateDeathEffect();
        Destroy(gameObject);
    }

    private void InstantiateDeathEffect()
    {
        if (deathVFXPrefab != null)
        {
            Instantiate(deathVFXPrefab, transform.position, Quaternion.identity);
        }
    }

    public float GetHealthPercent()
    {
        return (float)currentHealth / maxHealth;
    }

    public void Heal(int amount)
    {
        if (isDead)
            return; // Không hồi máu nếu đã chết

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);

        if (OnHealthChanged != null)
        {
            OnHealthChanged(currentHealth, maxHealth);
        }
    }

    // Lấy giá trị hiện tại và tối đa của máu
    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }

    // Kiểm tra nếu đã chết
    public bool IsDead()
    {
        return isDead;
    }
}
