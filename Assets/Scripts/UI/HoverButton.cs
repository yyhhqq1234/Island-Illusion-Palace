using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HoverButton : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private Image buttonImage;
    
    [SerializeField]
    private Image normalImage;
    
    [SerializeField]
    private Animator buttonAnimator;
    
    private void Start()
    {
        // 如果没有指定buttonImage，尝试从自身获取
        if (buttonImage == null)
        {
            buttonImage = GetComponent<Image>();
        }
        
        // 初始设置为默认状态
        UpdateButtonState();
    }
    
    private void UpdateButtonState()
    {
        if (buttonImage == null || normalImage == null)
            return;
        
        // 始终显示默认状态
        buttonImage.sprite = normalImage.sprite;
        buttonImage.color = normalImage.color;
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        // 点击时播放动画
        if (buttonAnimator != null)
        {
            buttonAnimator.SetBool("Click", true);
            // 延迟重置布尔值，确保动画有足够时间播放
            StartCoroutine(ResetClickBool());
        }
    }
    
    private System.Collections.IEnumerator ResetClickBool()
    {
        // 等待一帧确保动画开始播放
        yield return null;
        // 重置布尔值
        if (buttonAnimator != null)
        {
            buttonAnimator.SetBool("Click", false);
        }
    }
    
    // 公共方法，用于外部设置状态
    public void SetImage(Image normal)
    {
        normalImage = normal;
        
        // 更新显示
        UpdateButtonState();
    }
}
