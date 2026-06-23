using UnityEngine;

public class BossRoomManager : MonoBehaviour
{
    [Header("Boss配置")]
    public GameObject timeGuardianPrefab;
    public Transform bossSpawnPoint;

    [Header("Boss击败奖励")]
    public GameObject timePortalPrefab;
    public GameObject treasureChestPrefab;
    public bool spawnPortalOnDefeat = true;
    public bool spawnChestOnDefeat = true;

    [Header("裂隙配置")]
    public bool enableRiftsInBossRoom = false;
    public GameObject riftPrefab;

    private GameObject spawnedBoss;
    private bool bossDefeated;

    void Start()
    {
        GlobalEventManager.Instance.OnBossDefeated += OnBossDefeated;
    }

    void OnDestroy()
    {
        GlobalEventManager.Instance.OnBossDefeated -= OnBossDefeated;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (bossDefeated) return;

        SpawnBoss();
    }

    public void SpawnBoss()
    {
        if (spawnedBoss != null) return;
        if (timeGuardianPrefab == null)
        {
            Debug.LogError("[BossRoomManager] 未设置Boss预制件！");
            return;
        }

        Vector3 spawnPos = bossSpawnPoint != null ? bossSpawnPoint.position : transform.position;
        spawnedBoss = Instantiate(timeGuardianPrefab, spawnPos, Quaternion.identity);
        spawnedBoss.name = "TimeGuardian";

        GlobalEventManager.Instance.TriggerBossEncounter(spawnedBoss);

        Debug.Log("[BossRoomManager] 时空守护者出现！");
    }

    void OnBossDefeated(GameObject boss)
    {
        if (spawnedBoss == null || boss != spawnedBoss) return;

        bossDefeated = true;
        Debug.Log("[BossRoomManager] Boss被击败，生成奖励");

        if (spawnPortalOnDefeat && timePortalPrefab != null)
        {
            // 在Boss房间中心生成传送门
            Vector3 portalPos = transform.position;
            var portal = Instantiate(timePortalPrefab, portalPos, Quaternion.identity);
            portal.name = "TimePortal";
            var tp = portal.GetComponent<TimePortal>();
            if (tp != null) tp.Unlock();
        }

        if (spawnChestOnDefeat && treasureChestPrefab != null)
        {
            Vector3 chestPos = transform.position + Vector3.right * 2f;
            Instantiate(treasureChestPrefab, chestPos, Quaternion.identity);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = bossDefeated ? Color.green : Color.red;
        Vector3 pos = bossSpawnPoint != null ? bossSpawnPoint.position : transform.position;
        Gizmos.DrawWireCube(pos, Vector3.one * 2f);
        Gizmos.DrawIcon(pos, "boss_icon", true);
    }
}
