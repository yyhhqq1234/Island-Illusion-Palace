using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 武器图标UI - 显示当前装备的武器
/// 位置：屏幕左下角，状态条上方
/// </summary>
public class WeaponIconUI : MonoBehaviour
{
    [Header("UI组件")]
    [Tooltip("武器图标背景")]
    public Image background;
    
    [Tooltip("武器图标")]
    public Image weaponIcon;
    
    [Tooltip("武器名称文本")]
    public Text weaponNameText;
    
    [Tooltip("武器类型文本")]
    public Text weaponTypeText;

    [Header("样式配置")]
    [Tooltip("图标大小")]
    public float iconSize = 64f;
    
    [Tooltip("背景颜色")]
    public Color backgroundColor = new Color(0.2f, 0.2f, 0.3f, 0.8f);
    
    [Tooltip("边框颜色")]
    public Color borderColor = new Color(0.6f, 0.6f, 0.8f, 1f);

    private WeaponSystem weaponSystem;

    void Start()
    {
        EnsureContainerLayout();
        weaponSystem = FindObjectOfType<WeaponSystem>();

        if (weaponSystem == null)
        {
            Debug.LogWarning("[WeaponIconUI] 未找到WeaponSystem");
        }

        InitializeUI();
        // 主动拉一次初值
        if (weaponSystem != null)
            Refresh(weaponSystem.currentWeaponType, weaponSystem.enhancementLevel);
    }

    void OnEnable()
    {
        if (GlobalEventManager.Instance != null)
            GlobalEventManager.Instance.OnWeaponChanged += HandleWeaponChanged;
    }

    void OnDisable()
    {
        if (GlobalEventManager.Instance != null)
            GlobalEventManager.Instance.OnWeaponChanged -= HandleWeaponChanged;
    }

    void HandleWeaponChanged(WeaponType weaponType, int enhancementLevel)
    {
        Refresh(weaponType, enhancementLevel);
    }

    /// <summary>统一刷新入口</summary>
    void Refresh(WeaponType weaponType, int enhancementLevel)
    {
        if (weaponSystem == null) return;

        var weaponData = weaponSystem.weaponData;

        if (weaponData != null)
        {
            if (weaponIcon != null)
            {
                if (weaponData.icon != null)
                {
                    weaponIcon.sprite = weaponData.icon;
                    weaponIcon.color = Color.white;
                }
                else
                {
                    weaponIcon.sprite = null;
                    weaponIcon.color = Color.clear;
                }
            }

            if (weaponNameText != null)
            {
                string name = !string.IsNullOrEmpty(weaponData.weaponName) ? weaponData.weaponName : weaponType.ToString();
                weaponNameText.text = enhancementLevel > 0 ? $"{name} +{enhancementLevel}" : name;
            }

            if (weaponTypeText != null)
            {
                weaponTypeText.text = GetWeaponTypeText(weaponType);
            }
        }
        else
        {
            if (weaponIcon != null)
            {
                weaponIcon.sprite = null;
                weaponIcon.color = Color.clear;
            }

            if (weaponNameText != null)
            {
                weaponNameText.text = enhancementLevel > 0 ? $"{GetWeaponTypeText(weaponType)} +{enhancementLevel}" : "无武器";
            }

            if (weaponTypeText != null)
            {
                weaponTypeText.text = GetWeaponTypeText(weaponType);
            }
        }
    }

    /// <summary>
    /// 确保容器RectTransform布局正确（左下角）
    /// </summary>
    void EnsureContainerLayout()
    {
        RectTransform rt = GetComponent<RectTransform>();
        if (rt == null) return;
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(0, 0);
        rt.pivot = new Vector2(0, 0);
        rt.anchoredPosition = new Vector2(20, 200);
        rt.sizeDelta = new Vector2(iconSize, iconSize);
    }

    void Update()
    {
        // 事件驱动：WeaponSystem 通过 OnWeaponChanged 广播，无需每帧轮询
    }

    /// <summary>
    /// 初始化UI
    /// </summary>
    void InitializeUI()
    {
        if (background == null)
        {
            CreateDefaultUI();
        }
    }

    /// <summary>
    /// 获取武器类型文本
    /// </summary>
    string GetWeaponTypeText(WeaponType type)
    {
        switch (type)
        {
            case WeaponType.Sword:
                return "剑";
            case WeaponType.Staff:
                return "法杖";
            case WeaponType.Scythe:
                return "镰刀";
            case WeaponType.CrystalArm:
                return "水晶武装";
            default:
                return "未知";
        }
    }

    /// <summary>
    /// 创建默认UI
    /// </summary>
    void CreateDefaultUI()
    {
        GameObject weaponObj = new GameObject("WeaponIcon");
        weaponObj.transform.SetParent(transform, false);
        
        RectTransform rect = weaponObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(0, 0);
        rect.pivot = new Vector2(0, 0);
        rect.anchoredPosition = new Vector2(20, 200);
        rect.sizeDelta = new Vector2(iconSize, iconSize);
        
        // 背景
        background = weaponObj.AddComponent<Image>();
        background.color = backgroundColor;
        
        // 武器图标
        GameObject iconObj = new GameObject("WeaponIcon");
        iconObj.transform.SetParent(weaponObj.transform, false);
        weaponIcon = iconObj.AddComponent<Image>();
        weaponIcon.color = Color.white;
        RectTransform iconRect = iconObj.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.1f, 0.1f);
        iconRect.anchorMax = new Vector2(0.9f, 0.9f);
        iconRect.offsetMin = Vector2.zero;
        iconRect.offsetMax = Vector2.zero;
        
        // 武器名称文本
        GameObject nameObj = new GameObject("WeaponName");
        nameObj.transform.SetParent(weaponObj.transform, false);
        weaponNameText = nameObj.AddComponent<Text>();
        weaponNameText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        weaponNameText.fontSize = 12;
        weaponNameText.alignment = TextAnchor.MiddleCenter;
        weaponNameText.color = Color.white;
        RectTransform nameRect = nameObj.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, -0.5f);
        nameRect.anchorMax = new Vector2(1, 0);
        nameRect.offsetMin = Vector2.zero;
        nameRect.offsetMax = Vector2.zero;
        
        // 武器类型文本
        GameObject typeObj = new GameObject("WeaponType");
        typeObj.transform.SetParent(weaponObj.transform, false);
        weaponTypeText = typeObj.AddComponent<Text>();
        weaponTypeText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        weaponTypeText.fontSize = 10;
        weaponTypeText.alignment = TextAnchor.MiddleCenter;
        weaponTypeText.color = new Color(0.8f, 0.8f, 0.8f, 1f);
        RectTransform typeRect = typeObj.GetComponent<RectTransform>();
        typeRect.anchorMin = new Vector2(0, -0.8f);
        typeRect.anchorMax = new Vector2(1, -0.4f);
        typeRect.offsetMin = Vector2.zero;
        typeRect.offsetMax = Vector2.zero;
    }
}
