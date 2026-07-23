using UnityEngine;

/// <summary>
/// 物品收集器 - 处理玩家收集各种物品的逻辑
/// </summary>
public class ItemCollector : MonoBehaviour
{
    [Header("收集设置")]
    public float collectRange = 2f; // 收集范围

    private ICollectible nearbyItem = null; // 附近的可收集物品

    // 自动拾取开关缓存（PlayerPrefs: IIP_AutoPickup），每 0.5s 重读一次，避免每帧读 PlayerPrefs
    private bool autoPickupEnabled = false;
    private float autoPickupCheckTimer = 0f;

    void Update()
    {
        // 定时刷新自动拾取开关状态
        autoPickupCheckTimer -= Time.deltaTime;
        if (autoPickupCheckTimer <= 0f)
        {
            autoPickupCheckTimer = 0.5f;
            autoPickupEnabled = PlayerPrefs.GetInt(IIPConstants.PrefKeyAutoPickup, 0) == 1;
        }

        // 自动拾取开启：靠近即收，无需按E
        if (autoPickupEnabled)
        {
            if (nearbyItem != null) TryCollectItem();
            return;
        }

        // 检查E键输入
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryCollectItem();
        }
    }

    /// <summary>
    /// 尝试收集物品
    /// </summary>
    void TryCollectItem()
    {
        if (nearbyItem != null)
        {
            nearbyItem.OnCollected();
            nearbyItem = null;
        }
    }

    /// <summary>
    /// 设置附近的可收集物品
    /// </summary>
    public void SetNearbyItem(ICollectible item)
    {
        if (nearbyItem == null)
        {
            nearbyItem = item;
            Debug.Log($"靠近{item.GetItemName()}，按E收集");
        }
    }

    /// <summary>
    /// 清除附近的可收集物品
    /// </summary>
    public void ClearNearbyItem(ICollectible item)
    {
        if (nearbyItem == item)
        {
            nearbyItem = null;
        }
    }
}

/// <summary>
/// 可收集物品接口
/// </summary>
public interface ICollectible
{
    string GetItemName();
    void OnCollected();
}
