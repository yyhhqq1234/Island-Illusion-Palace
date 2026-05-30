using UnityEngine;
using System.Collections.Generic;

public class TimeRiftSpawner : MonoBehaviour
{
    [Header("裂隙生成参数")]
    public GameObject riftPrefab;

    [Header("生成概率设置")]
    [Range(0f, 100f)]
    public float minSpawnChance = 30f;
    [Range(0f, 100f)]
    public float maxSpawnChance = 60f;
    public float spawnCheckInterval = 15f;

    [Header("生成数量")]
    public int minRiftsPerMap = 1;
    public int maxRiftsPerMap = 3;

    [Header("概率修正（基于游戏状态）")]
    [Range(0f, 50f)] public float burdenBonus = 5f;
    [Range(0f, 20f)] public float fragmentBonus = 2f;
    [Range(0f, 20f)] public float cycleBonus = 3f;

    [Header("位置生成设置")]
    [Tooltip("距离玩家最小距离")]
    public float minDistanceFromPlayer = 8f;
    [Tooltip("距离玩家最大距离")]
    public float maxDistanceFromPlayer = 20f;
    [Tooltip("是否避开安全区")]
    public bool avoidSafeZone = true;
    [Tooltip("是否避开Boss房间")]
    public bool avoidBossRoom = true;
    [Tooltip("安全区检测半径")]
    public float safeZoneAvoidRadius = 5f;
    [Tooltip("Boss房间检测半径")]
    public float bossRoomAvoidRadius = 8f;

    [Header("障碍物检测设置")]
    [Tooltip("是否避开障碍物")]
    public bool avoidObstacles = true;
    [Tooltip("障碍物检测半径")]
    public float obstacleAvoidRadius = 2f;
    [Tooltip("障碍物检测层")]
    public LayerMask obstacleLayer = ~0;
    [Tooltip("连续障碍物检测次数上限")]
    public int maxObstacleChecks = 5;

    [Header("地图边界限制")]
    public float mapBoundaryPadding = 5f;

    [Header("渲染设置")]
    [Tooltip("裂隙渲染排序层")]
    public int riftSortingOrder = 100;

    private IntegratedMapSystem mapSystem;
    private float checkTimer;
    private int riftsSpawnedThisMap;
    private float currentSpawnChance;

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
        currentSpawnChance = CalculateSpawnChance();

        float roll = Random.Range(0f, 100f);
        if (roll > currentSpawnChance)
        {
            Debug.Log($"[TimeRiftSpawner] 本次未生成裂隙 (掷骰: {roll:F1} > 概率: {currentSpawnChance:F1}%)");
            return;
        }

        if (riftPrefab == null)
        {
            Debug.LogWarning("[TimeRiftSpawner] Rift Prefab 未设置！");
            return;
        }

        Vector3 spawnPos = FindValidRiftPosition();
        if (spawnPos == Vector3.zero)
        {
            Debug.Log("[TimeRiftSpawner] 未找到有效的生成位置");
            return;
        }

        var rift = Instantiate(riftPrefab, spawnPos, Quaternion.identity);
        rift.name = $"TimeRift_{riftsSpawnedThisMap}";
        riftsSpawnedThisMap++;

        SetRiftToTopLayer(rift);

        Debug.Log($"[TimeRiftSpawner] 裂隙生成！概率={currentSpawnChance:F1}%，位置={spawnPos}，总数={riftsSpawnedThisMap}/{maxRiftsPerMap}");
    }

    void SetRiftToTopLayer(GameObject rift)
    {
        SpriteRenderer[] renderers = rift.GetComponentsInChildren<SpriteRenderer>();
        foreach (var renderer in renderers)
        {
            renderer.sortingOrder = riftSortingOrder;
        }

        ParticleSystem[] particles = rift.GetComponentsInChildren<ParticleSystem>();
        foreach (var particle in particles)
        {
            var renderer = particle.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sortingOrder = riftSortingOrder + 1;
            }
        }
    }

    float CalculateSpawnChance()
    {
        float baseChance = Random.Range(minSpawnChance, maxSpawnChance);

        var bs = FindObjectOfType<BurdenSystem>();
        if (bs != null)
        {
            if (bs.currentBurden > 50f) baseChance += burdenBonus;
            if (bs.currentBurden > 70f) baseChance += burdenBonus;
        }

        var mf = FindObjectOfType<MemoryFragmentSystem>();
        if (mf != null)
            baseChance += mf.GetFragmentCount() * fragmentBonus;

        if (mapSystem != null && mapSystem.currentCycle >= 2)
            baseChance += cycleBonus;

        return Mathf.Clamp(baseChance, 0f, 100f);
    }

    Vector3 FindValidRiftPosition()
    {
        Vector3 playerPos = GetPlayerPosition();
        Vector3 mapCenter = GetMapCenter();
        Vector2 mapBounds = GetMapBounds();

        for (int attempt = 0; attempt < 50; attempt++)
        {
            Vector3 candidatePos = GenerateRandomPosition(playerPos, mapCenter, mapBounds);

            if (!IsPositionValid(candidatePos, playerPos))
                continue;

            Collider2D hit = Physics2D.OverlapCircle(candidatePos, 0.5f);
            if (hit == null)
            {
                Debug.Log($"[TimeRiftSpawner] 第{attempt + 1}次尝试找到有效位置: {candidatePos}");
                return candidatePos;
            }
        }

        Debug.LogWarning($"[TimeRiftSpawner] 50次尝试均未找到有效位置");
        return Vector3.zero;
    }

    Vector3 GenerateRandomPosition(Vector3 playerPos, Vector3 mapCenter, Vector2 mapBounds)
    {
        int strategy = Random.Range(0, 3);
        Vector3 pos;

        switch (strategy)
        {
            case 0:
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float dist = Random.Range(minDistanceFromPlayer, maxDistanceFromPlayer);
                pos = playerPos + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * dist;
                break;

            case 1:
                pos = new Vector3(
                    Random.Range(mapCenter.x - mapBounds.x / 2 + mapBoundaryPadding,
                                 mapCenter.x + mapBounds.x / 2 - mapBoundaryPadding),
                    Random.Range(mapCenter.y - mapBounds.y / 2 + mapBoundaryPadding,
                                 mapCenter.y + mapBounds.y / 2 - mapBoundaryPadding),
                    0f
                );
                break;

            case 2:
                float edgeAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float edgeDist = Random.Range(mapBounds.x, mapBounds.y) / 2f - mapBoundaryPadding;
                pos = mapCenter + new Vector3(Mathf.Cos(edgeAngle), Mathf.Sin(edgeAngle), 0f) * edgeDist;
                break;

            default:
                pos = playerPos + new Vector3(Random.insideUnitSphere.x, Random.insideUnitSphere.y, 0f) * maxDistanceFromPlayer;
                break;
        }

        return pos;
    }

    bool IsPositionValid(Vector3 pos, Vector3 playerPos)
    {
        Vector3 mapCenter = GetMapCenter();
        Vector2 mapBounds = GetMapBounds();

        if (pos.x < mapCenter.x - mapBounds.x / 2 + mapBoundaryPadding ||
            pos.x > mapCenter.x + mapBounds.x / 2 - mapBoundaryPadding ||
            pos.y < mapCenter.y - mapBounds.y / 2 + mapBoundaryPadding ||
            pos.y > mapCenter.y + mapBounds.y / 2 - mapBoundaryPadding)
        {
            return false;
        }

        float distToPlayer = Vector3.Distance(pos, playerPos);
        if (distToPlayer < minDistanceFromPlayer)
            return false;

        if (avoidSafeZone && IsNearSafeZone(pos))
            return false;

        if (avoidBossRoom && IsNearBossRoom(pos))
            return false;

        if (avoidObstacles && HasNearbyObstacle(pos))
            return false;

        return true;
    }

    bool IsNearSafeZone(Vector3 pos)
    {
        SafeZoneDetector[] safeZones = FindObjectsOfType<SafeZoneDetector>();
        foreach (var safeZone in safeZones)
        {
            float dist = Vector3.Distance(pos, safeZone.transform.position);
            if (dist < safeZoneAvoidRadius)
                return true;
        }
        return false;
    }

    bool IsNearBossRoom(Vector3 pos)
    {
        if (mapSystem == null)
            return false;

        Transform bossRoom = mapSystem.transform.Find("BossRoom");
        if (bossRoom == null)
        {
            GameObject bossObj = GameObject.FindGameObjectWithTag("BossRoom");
            if (bossObj != null)
                bossRoom = bossObj.transform;
        }

        if (bossRoom != null)
        {
            float dist = Vector3.Distance(pos, bossRoom.position);
            return dist < bossRoomAvoidRadius;
        }

        return false;
    }

    bool HasNearbyObstacle(Vector3 pos)
    {
        if (mapSystem != null)
        {
            if (!mapSystem.IsPositionValidForRift(pos))
            {
                Debug.Log($"[TimeRiftSpawner] 位置 {pos} 在网格地图障碍物上");
                return true;
            }
        }

        int obstacleCheckCount = 0;
        float checkRadius = obstacleAvoidRadius;
        int checksPerformed = 0;

        while (obstacleCheckCount < maxObstacleChecks && checkRadius <= obstacleAvoidRadius * 3)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(pos, checkRadius, obstacleLayer);

            bool foundObstacle = false;
            foreach (var hit in hits)
            {
                if (hit.isTrigger)
                    continue;

                if (hit.CompareTag("Player") || hit.CompareTag("SafeZone") || hit.CompareTag("BossRoom"))
                    continue;

                if (hit.gameObject.layer == LayerMask.NameToLayer("Ground") ||
                    hit.gameObject.layer == LayerMask.NameToLayer("Water") ||
                    hit.gameObject.layer == LayerMask.NameToLayer("Wall"))
                {
                    Debug.Log($"[TimeRiftSpawner] 位置 {pos} 附近存在物理障碍物 {hit.name}");
                    foundObstacle = true;
                    break;
                }
            }

            if (foundObstacle)
            {
                obstacleCheckCount++;
                checkRadius += 0.5f;
            }
            else
            {
                if (checksPerformed > 0)
                    break;
            }

            checksPerformed++;
        }

        return obstacleCheckCount >= maxObstacleChecks;
    }

    Vector3 GetPlayerPosition()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        return player != null ? player.transform.position : Vector3.zero;
    }

    Vector3 GetMapCenter()
    {
        if (mapSystem != null)
            return mapSystem.transform.position;
        return Vector3.zero;
    }

    Vector2 GetMapBounds()
    {
        if (mapSystem != null)
        {
            return new Vector2(
                mapSystem.gridSize * 39f,
                mapSystem.gridSize * 26f
            );
        }
        return new Vector2(195f, 130f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.5f);

        Vector3 mapCenter = GetMapCenter();
        Vector2 mapBounds = GetMapBounds();
        Vector3 corner1 = mapCenter + new Vector3(-mapBounds.x / 2, -mapBounds.y / 2, 0);
        Vector3 corner2 = mapCenter + new Vector3(mapBounds.x / 2, mapBounds.y / 2, 0);

        Gizmos.color = new Color(0, 1, 1, 0.3f);
        Gizmos.DrawWireCube(mapCenter, new Vector3(mapBounds.x, mapBounds.y, 0));
    }
}
