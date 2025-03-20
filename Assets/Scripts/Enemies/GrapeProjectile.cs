using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapeProjectile : MonoBehaviour
{
    [SerializeField]
    private float duration = 1f;

    [SerializeField]
    private AnimationCurve animCurve;

    [SerializeField]
    private float heightY = 3f;

    [SerializeField]
    private GameObject grapeProjectileShadow;

    [SerializeField]
    private GameObject splatterPrefab;

    private Transform targetTransform;
    private bool canTargetAllies = true;

    private void Start()
    {
        // Nếu chưa có mục tiêu cụ thể, mặc định là player
        if (targetTransform == null)
        {
            targetTransform = PlayerController.Instance.transform;
        }

        GameObject grapeShadow = Instantiate(
            grapeProjectileShadow,
            transform.position + new Vector3(0, -0.3f, 0),
            Quaternion.identity
        );

        Vector3 targetPos = targetTransform.position;
        Vector3 grapeShadowStartPosition = grapeShadow.transform.position;

        StartCoroutine(ProjectileCurveRoutine(transform.position, targetPos));
        StartCoroutine(MoveGrapeShadowRoutine(grapeShadow, grapeShadowStartPosition, targetPos));
    }

    private IEnumerator ProjectileCurveRoutine(Vector3 startPosition, Vector3 endPosition)
    {
        float timePassed = 0f;

        while (timePassed < duration)
        {
            timePassed += Time.deltaTime;
            float linearT = timePassed / duration;
            float heightT = animCurve.Evaluate(linearT);
            float height = Mathf.Lerp(0f, heightY, heightT);

            transform.position =
                Vector2.Lerp(startPosition, endPosition, linearT) + new Vector2(0f, height);

            yield return null;
        }

        // Tạo splatter ở vị trí cuối cùng
        GameObject splatter = Instantiate(splatterPrefab, transform.position, Quaternion.identity);

        // Cập nhật GrapeLandSplatter để có thể tấn công allies
        GrapeLandSplatter landSplatter = splatter.GetComponent<GrapeLandSplatter>();
        if (landSplatter != null)
        {
            landSplatter.SetCanTargetAllies(canTargetAllies);
        }

        Destroy(gameObject);
    }

    private IEnumerator MoveGrapeShadowRoutine(
        GameObject grapeShadow,
        Vector3 startPosition,
        Vector3 endPosition
    )
    {
        float timePassed = 0f;

        while (timePassed < duration)
        {
            timePassed += Time.deltaTime;
            float linearT = timePassed / duration;
            grapeShadow.transform.position = Vector2.Lerp(startPosition, endPosition, linearT);
            yield return null;
        }

        Destroy(grapeShadow);
    }

    // Thiết lập mục tiêu cho projectile
    public void SetTarget(Transform target)
    {
        if (target != null)
        {
            targetTransform = target;
        }
    }

    // Thiết lập khả năng tấn công allies
    public void SetCanTargetAllies(bool value)
    {
        canTargetAllies = value;
    }
}
