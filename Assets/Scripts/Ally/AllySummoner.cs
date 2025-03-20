using System.Collections;
using UnityEngine;

public class AllySummoner : MonoBehaviour
{
    [SerializeField]
    private KeyCode summonKey = KeyCode.F;

    [SerializeField]
    private float cooldown = 15f;

    [SerializeField]
    private GameObject summonEffectPrefab;

    private bool canSummon = true;

    private void Update()
    {
        if (Input.GetKeyDown(summonKey) && canSummon)
        {
            SummonAlly();
        }
    }

    private void SummonAlly()
    {
        // Lấy vị trí chuột trong thế giới
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        // Hiệu ứng triệu hồi
        if (summonEffectPrefab != null)
        {
            Instantiate(summonEffectPrefab, mousePos, Quaternion.identity);
        }

        // Triệu hồi đồng minh
        AllyManager.Instance.SummonAlly(0, mousePos); // 0 là loại đồng minh đầu tiên

        // Thiết lập cooldown
        StartCoroutine(SummonCooldownRoutine());
    }

    private IEnumerator SummonCooldownRoutine()
    {
        canSummon = false;
        yield return new WaitForSeconds(cooldown);
        canSummon = true;
    }
}
