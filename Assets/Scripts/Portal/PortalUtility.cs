using UnityEngine;

/// <summary>
/// Portal 系统静态工具类
/// 提供 GetCurrentMapType、TeleportTo、GetMapSystem 等通用方法
/// 消除 TimePortal/TimeRift 中的重复代码
/// </summary>
public static class PortalUtility
{
    private static IntegratedMapSystem cachedMapSystem;

    /// <summary>
    /// 获取当前地图类型
    /// 从 IntegratedMapSystem 读取 currentMapType，若不可用则返回默认值 Forest
    /// </summary>
    public static MapType GetCurrentMapType()
    {
        var map = GetMapSystem();
        return map != null ? map.currentMapType : MapType.Forest;
    }

    /// <summary>
    /// 执行传送：将当前地图切换到目标类型并生成新地图
    /// </summary>
    /// <param name="destination">目标地图类型</param>
    public static void TeleportTo(MapType destination)
    {
        var map = GetMapSystem();
        if (map != null)
        {
            map.SetMapType(destination);
            map.GenerateNewMap();
        }
        else
        {
            Debug.LogWarning("[PortalUtility] TeleportTo 失败：未找到 IntegratedMapSystem");
        }
    }

    /// <summary>
    /// 缓存式获取 IntegratedMapSystem 引用
    /// 首次调用时通过 FindObjectOfType 查找并缓存
    /// </summary>
    public static IntegratedMapSystem GetMapSystem()
    {
        if (cachedMapSystem == null)
            cachedMapSystem = Object.FindObjectOfType<IntegratedMapSystem>();
        return cachedMapSystem;
    }
}
