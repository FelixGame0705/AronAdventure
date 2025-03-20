using UnityEngine;

public class DepthSorting : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void LateUpdate()
    {
        // Cập nhật sorting order của nhân vật dựa trên vị trí Y
        spriteRenderer.sortingOrder = Mathf.RoundToInt(transform.position.y * -10);
    }
}
