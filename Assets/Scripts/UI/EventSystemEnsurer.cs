using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// EventSystem 兜底器 - 挂在 UI 预制体根节点上（HUDCanvas / PauseMenu）。
/// 场景缺少 EventSystem 时自动创建一个，保证 UI 预制体拖入任意新场景后
/// 按钮点击 / 滑块拖拽 / 滚轮滚动立即可用，无需手工接线。
/// 幂等：场景中已有 EventSystem（包括其他预制体创建的）时不重复创建。
/// </summary>
public class EventSystemEnsurer : MonoBehaviour
{
    void Awake()
    {
        if (FindObjectOfType<EventSystem>() == null)
        {
            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            Debug.Log($"[EventSystemEnsurer] 场景缺少 EventSystem，已由 {gameObject.name} 自动创建");
        }
    }
}
