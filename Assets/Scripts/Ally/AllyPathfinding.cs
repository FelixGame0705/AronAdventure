using UnityEngine;

public class AllyPathfinding : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 3f;

    [SerializeField]
    private bool faceMovementDirection = true;

    private Rigidbody2D rb;
    private Vector2 moveDir;
    private Knockback knockback;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        knockback = GetComponent<Knockback>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (knockback != null && knockback.GettingKnockedBack)
            return;

        rb.MovePosition(rb.position + moveDir * (moveSpeed * Time.fixedDeltaTime));

        if (faceMovementDirection && moveDir.magnitude > 0.1f)
        {
            if (moveDir.x < 0)
            {
                spriteRenderer.flipX = true;
            }
            else if (moveDir.x > 0)
            {
                spriteRenderer.flipX = false;
            }
        }
    }

    public void MoveTo(Vector2 targetPosition)
    {
        moveDir = (targetPosition - rb.position).normalized;
    }

    public void StopMoving()
    {
        moveDir = Vector2.zero;
    }

    public void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
    }
}
