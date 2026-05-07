using UnityEngine;
using TMPro;

/// <summary>
/// 伤害数字显示 - 显示浮动伤害数字
/// </summary>
public class DamageNumber : MonoBehaviour
{
    [Header("文本组件")]
    public TextMeshPro damageText;

    [Header("动画设置")]
    public float lifetime = 1f;
    public float moveSpeed = 2f;
    public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    public AnimationCurve alphaCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    private float timer = 0f;
    private Vector3 startPosition;
    private Color startColor;

    void Start()
    {
        startPosition = transform.position;
        if (damageText != null)
        {
            startColor = damageText.color;
        }
    }

    void Update()
    {
        timer += Time.deltaTime;
        float progress = timer / lifetime;

        if (progress >= 1f)
        {
            Destroy(gameObject);
            return;
        }

        // 向上移动
        transform.position = startPosition + Vector3.up * (moveSpeed * timer);

        // 缩放动画
        float scale = scaleCurve.Evaluate(progress);
        transform.localScale = Vector3.one * scale;

        // 透明度动画
        if (damageText != null)
        {
            Color color = startColor;
            color.a = alphaCurve.Evaluate(progress);
            damageText.color = color;
        }
    }

    /// <summary>
    /// 设置伤害值
    /// </summary>
    public void SetDamage(int damage, bool isCritical = false)
    {
        if (damageText != null)
        {
            damageText.text = damage.ToString();

            // 暴击显示为红色和更大字体
            if (isCritical)
            {
                damageText.color = Color.red;
                damageText.fontSize *= 1.5f;
            }
        }
    }
}

/// <summary>
/// 伤害数字生成器
/// </summary>
public class DamageNumberManager : MonoBehaviour
{
    public static DamageNumberManager Instance { get; private set; }

    [Header("伤害数字预制体")]
    public GameObject damageNumberPrefab;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 显示伤害数字
    /// </summary>
    public void ShowDamage(Vector3 position, int damage, bool isCritical = false)
    {
        if (damageNumberPrefab != null)
        {
            GameObject damageObj = Instantiate(damageNumberPrefab, position, Quaternion.identity);
            DamageNumber damageNumber = damageObj.GetComponent<DamageNumber>();
            if (damageNumber != null)
            {
                damageNumber.SetDamage(damage, isCritical);
            }
        }
    }
}


