using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 按钮悬停反馈组件 — 挂在主菜单/暂停菜单按钮上。
/// 鼠标进入时：Image 高亮色 + 轻微放大；离开时还原。
/// 不依赖 Animator/外部 Image 引用，基于自身 Image 与 Transform。
/// 与 Button 自带的 ColorTint 可共存（本组件额外提供缩放反馈）。
/// </summary>
public class HoverButton : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler
{
    [Header("悬停反馈")]
    [Tooltip("悬停时的高亮色（叠加到 Image.color）")]
    public Color hoverColor = new Color(0.32f, 0.28f, 0.50f, 1f);

    [Tooltip("悬停时的缩放倍数（1=不缩放）")]
    [Range(1f, 1.2f)]
    public float hoverScale = 1.06f;

    [Tooltip("缩放过渡速度（越大越快）")]
    public float lerpSpeed = 12f;

    private Image targetImage;
    private Color normalColor;
    private Vector3 normalScale;
    private bool isHovering = false;
    private bool initialized = false;

    void Awake()
    {
        EnsureInit();
    }

    void EnsureInit()
    {
        if (initialized) return;
        targetImage = GetComponent<Image>();
        if (targetImage != null) normalColor = targetImage.color;
        normalScale = transform.localScale;
        initialized = true;
    }

    void OnEnable()
    {
        EnsureInit();
        isHovering = false;
        ApplyImmediate(false);
    }

    void Update()
    {
        if (!isHovering) return;
        // 悬停期间持续把缩放平滑拉到目标（防止外部布局重置）
        if (hoverScale > 1f)
        {
            transform.localScale = Vector3.Lerp(
                transform.localScale, normalScale * hoverScale, Time.unscaledDeltaTime * lerpSpeed);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        EnsureInit();
        isHovering = true;
        if (targetImage != null) targetImage.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        ApplyImmediate(false);
    }

    void ApplyImmediate(bool hover)
    {
        if (targetImage != null)
            targetImage.color = hover ? hoverColor : normalColor;
        transform.localScale = normalScale;
    }

    void OnDisable()
    {
        isHovering = false;
        ApplyImmediate(false);
    }
}
