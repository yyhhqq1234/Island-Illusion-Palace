using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Portal 系统静态工具类 — 自动扫描 Build Settings 中的所有场景，动态构建传送池
/// </summary>
public static class PortalUtility
{
    private static IntegratedMapSystem cachedMapSystem;
    private static MapType[] teleportPool;
    private static Dictionary<MapType, string> sceneMap;
    private static bool poolBuilt = false;

    /// <summary>从 Build Settings 自动扫描场景并构建传送池</summary>
    static void BuildPoolIfNeeded()
    {
        if (poolBuilt) return;
        poolBuilt = true;

        var pool = new List<MapType>();
        var map = new Dictionary<MapType, string>();

        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(path);

            // 跳过非地图场景（MainMenu 等）
            if (sceneName == "MainMenu") continue;

            // 场景名 → MapType 枚举匹配（大小写不敏感）
            if (System.Enum.TryParse<MapType>(sceneName, true, out MapType mapType))
            {
                pool.Add(mapType);
                map[mapType] = sceneName;
                Debug.Log($"[PortalUtility] 扫描到传送目标: {sceneName} → {mapType}");
            }
            else
            {
                Debug.LogWarning($"[PortalUtility] 场景 '{sceneName}' 无法匹配 MapType 枚举，已跳过");
            }
        }

        teleportPool = pool.ToArray();
        sceneMap = map;

        if (teleportPool.Length == 0)
        {
            Debug.LogError("[PortalUtility] 未找到任何有效的传送目标！请检查 Build Settings 和场景命名");
            teleportPool = new[] { MapType.Forest };
            sceneMap[MapType.Forest] = "Forest";
        }
    }

    public static MapType GetCurrentMapType()
    {
        var m = GetMapSystem();
        return m != null ? m.currentMapType : MapType.Forest;
    }

    public static MapType GetRandomDestination()
    {
        BuildPoolIfNeeded();
        return teleportPool[Random.Range(0, teleportPool.Length)];
    }

    public static void TeleportTo(MapType destination)
    {
        BuildPoolIfNeeded();
        if (sceneMap.TryGetValue(destination, out string sceneName))
        {
            Debug.Log($"[PortalUtility] 传送至 {destination} → 加载场景 {sceneName}");
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning($"[PortalUtility] {destination} 无对应场景，回退第一个可用");
            SceneManager.LoadScene(sceneMap.Values.First());
        }
    }

    public static MapType[] GetAvailableDestinations()
    {
        BuildPoolIfNeeded();
        return (MapType[])teleportPool.Clone();
    }

    public static IntegratedMapSystem GetMapSystem()
    {
        if (cachedMapSystem == null)
            cachedMapSystem = Object.FindObjectOfType<IntegratedMapSystem>();
        return cachedMapSystem;
    }

    static PortalUtility()
    {
        SceneManager.sceneLoaded += (_, _) => { cachedMapSystem = null; poolBuilt = false; };
    }
}
