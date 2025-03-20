using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapeLandSplatter : MonoBehaviour
{
    private SpriteFade spriteFade;
    private bool canTargetAllies = true;

    private void Awake()
    {
        spriteFade = GetComponent<SpriteFade>();
    }

    private void Start()
    {
        StartCoroutine(spriteFade.SlowFadeRoutine());
        Invoke("DisableCollider", 0.2f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Kiểm tra nếu là player
        PlayerHealth playerHealth = other.gameObject.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(1, transform);
            return;
        }

        // Kiểm tra nếu là ally và có thể tấn công allies
        if (canTargetAllies)
        {
            AllyHealth allyHealth = other.gameObject.GetComponent<AllyHealth>();
            if (allyHealth != null)
            {
                allyHealth.TakeDamage(1, transform);
            }
        }
    }

    private void DisableCollider()
    {
        GetComponent<CapsuleCollider2D>().enabled = false;
    }

    // Thiết lập khả năng tấn công allies
    public void SetCanTargetAllies(bool value)
    {
        canTargetAllies = value;
    }
}
