using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Singleton quản lý tất cả đồng minh
public class AllyManager : Singleton<AllyManager>
{
    [SerializeField]
    private int maxAllies = 3;

    [SerializeField]
    private float allyDuration = 30f; // Thời gian tồn tại mặc định

    [SerializeField]
    private GameObject[] allyPrefabs; // Các loại đồng minh có thể triệu hồi

    private List<GameObject> activeAllies = new List<GameObject>();

    // Triệu hồi đồng minh mới
    public void SummonAlly(int allyType, Vector3 position, float? customDuration = null)
    {
        if (activeAllies.Count >= maxAllies || allyType >= allyPrefabs.Length)
            return;

        // Tạo đồng minh mới
        GameObject newAlly = Instantiate(allyPrefabs[allyType], position, Quaternion.identity);
        activeAllies.Add(newAlly);

        // Đặt thời gian tồn tại nếu có
        float duration = customDuration ?? allyDuration;
        if (duration > 0)
        {
            StartCoroutine(DestroyAllyAfterTime(newAlly, duration));
        }
    }

    private IEnumerator DestroyAllyAfterTime(GameObject ally, float time)
    {
        yield return new WaitForSeconds(time);

        if (ally != null)
        {
            // Kiểm tra và chơi animation died nếu có
            AllyHealth allyHealth = ally.GetComponent<AllyHealth>();
            if (allyHealth != null)
            {
                allyHealth.Die();
            }
            else
            {
                DestroyAlly(ally);
            }
        }
    }

    public void DestroyAlly(GameObject ally)
    {
        if (ally != null)
        {
            activeAllies.Remove(ally);
            Destroy(ally);
        }
    }

    public List<GameObject> GetActiveAllies()
    {
        return activeAllies;
    }

    // Điều khiển tất cả đồng minh
    public void CommandAllAllies(Vector3 targetPosition)
    {
        foreach (GameObject ally in activeAllies)
        {
            AllyAI allyAI = ally.GetComponent<AllyAI>();
            if (allyAI != null)
            {
                allyAI.SetTargetPosition(targetPosition);
            }
        }
    }
}
