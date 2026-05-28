using UnityEngine;

/// <summary>
/// 物品收集器 - 处理玩家收集各种物品的逻辑
/// </summary>
public class ItemCollector : MonoBehaviour
{
    [Header("收集设置")]
    public float collectRange = 2f; // 收集范围

    private ICollectible nearbyItem = null; // 附近的可收集物品

    void Update()
    {
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
