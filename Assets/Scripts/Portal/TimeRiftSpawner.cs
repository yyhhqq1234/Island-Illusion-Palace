using UnityEngine;

public class TimeRiftSpawner : MonoBehaviour
{
    [Header("裂隙生成参数")]
    public GameObject riftPrefab;
    public float baseSpawnChance = 0.05f;
    public float spawnCheckInterval = 15f;
    public int minRiftsPerMap = 0;
    public int maxRiftsPerMap = 3;

    [Header("概率修正")]
    [Range(0f, 0.5f)] public float burdenBonus = 0.05f;
    [Range(0f, 0.1f)] public float fragmentBonus = 0.02f;
    [Range(0f, 0.1f)] public float cycleBonus = 0.03f;

    private IntegratedMapSystem mapSystem;
    private float checkTimer;
    private int riftsSpawnedThisMap;

    void Start()
    {
        mapSystem = FindObjectOfType<IntegratedMapSystem>();
        ResetForNewMap();
    }

    void Update()
    {
        if (riftsSpawnedThisMap >= maxRiftsPerMap) return;

        checkTimer += Time.deltaTime;
        if (checkTimer >= spawnCheckInterval)
        {
            checkTimer = 0f;
            TrySpawnRift();
        }
    }

    public void ResetForNewMap()
    {
        riftsSpawnedThisMap = 0;
        checkTimer = 0f;

        for (int i = 0; i < minRiftsPerMap; i++)
            TrySpawnRift();
    }

    void TrySpawnRift()
    {
        float chance = CalculateSpawnChance();
        if (Random.value > chance) return;
        if (riftPrefab == null) return;

        Vector3 spawnPos = FindValidRiftPosition();
        if (spawnPos == Vector3.zero) return;

        var rift = Instantiate(riftPrefab, spawnPos, Quaternion.identity);
        rift.name = $"TimeRift_{riftsSpawnedThisMap}";
        riftsSpawnedThisMap++;

        Debug.Log($"[TimeRiftSpawner] 裂隙生成！概率={chance:P0}，总数={riftsSpawnedThisMap}/{maxRiftsPerMap}");
    }

    float CalculateSpawnChance()
    {
        float chance = baseSpawnChance;

        var bs = FindObjectOfType<BurdenSystem>();
        if (bs != null)
        {
            if (bs.currentBurden > 50f) chance += burdenBonus;
            if (bs.currentBurden > 70f) chance += burdenBonus;
        }

        var mf = FindObjectOfType<MemoryFragmentSystem>();
        if (mf != null)
            chance += mf.GetFragmentCount() * fragmentBonus;

        if (mapSystem != null && mapSystem.currentCycle >= 2)
            chance += cycleBonus;

        return Mathf.Clamp(chance, 0f, 0.5f);
    }

    Vector3 FindValidRiftPosition()
    {
        Vector3 playerPos = GameObject.FindGameObjectWithTag("Player")?.transform.position ?? Vector3.zero;

        for (int attempt = 0; attempt < 20; attempt++)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float dist = Random.Range(5f, 15f);
            Vector3 pos = playerPos + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * dist;

            Collider2D hit = Physics2D.OverlapCircle(pos, 0.5f);
            if (hit == null) return pos;
        }
        return Vector3.zero;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}
