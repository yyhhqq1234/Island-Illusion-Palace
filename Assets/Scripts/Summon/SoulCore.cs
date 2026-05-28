using UnityEngine;
using System.Collections;

/// <summary>
/// 灵魂之核 - 敌人死亡后生成的收集物
/// </summary>
public class SoulCore : MonoBehaviour, ICollectible
{
    [Header("灵魂之核设置")]
    public EnemyAI.EnemyType enemyType; // 对应的敌人类型
    public float collectRange = 2f; // 收集范围
    public float floatSpeed = 1f; // 浮动速度
    public float floatAmplitude = 0.3f; // 浮动幅度

    [Header("精灵动画")]
    public Sprite[] soulCoreSprites; // 灵魂之核精灵序列（10张图片）
    public float animationFrameRate = 12f; // 动画帧率

    private SpriteRenderer spriteRenderer;
    private Vector3 startPosition;
    private float floatTimer = 0f;
    private int currentSpriteIndex = 0;
    private float spriteTimer = 0f;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>() ?? gameObject.AddComponent<SpriteRenderer>();
        startPosition = transform.position;

        if (soulCoreSprites != null && soulCoreSprites.Length > 0)
        {
            spriteRenderer.sprite = soulCoreSprites[0];
            spriteRenderer.sortingOrder = 5;
        }

        CircleCollider2D collider = GetComponent<CircleCollider2D>() ?? gameObject.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = collectRange;
        gameObject.tag = "Collectible";
    }

    void Update()
    {
        floatTimer += Time.deltaTime * floatSpeed;
        transform.position = startPosition + Vector3.up * Mathf.Sin(floatTimer) * floatAmplitude;

        if (soulCoreSprites != null && soulCoreSprites.Length > 0)
        {
            spriteTimer += Time.deltaTime;
            float frameTime = 1f / animationFrameRate;
            if (spriteTimer >= frameTime)
            {
                spriteTimer = 0f;
                currentSpriteIndex = (currentSpriteIndex + 1) % soulCoreSprites.Length;
                spriteRenderer.sprite = soulCoreSprites[currentSpriteIndex];
            }
        }
    }

    public void SetEnemyType(EnemyAI.EnemyType type) => enemyType = type;
    public EnemyAI.EnemyType GetEnemyType() => enemyType;

    // ICollectible接口实现
    public string GetItemName()
    {
        return $"灵魂之核({enemyType})";
    }

    public void OnCollected()
    {
        // 将灵魂之核添加到召唤系统库存
        SummonSystem summonSystem = FindObjectOfType<SummonSystem>();
        if (summonSystem != null)
        {
            summonSystem.AddSoulCore(enemyType, SoulCoreQuality.Common);

            // 如果有空的出战槽位，自动装备到第一个空槽位
            int firstEmptySlot = summonSystem.GetFirstAvailableSlot();
            if (firstEmptySlot >= 0)
            {
                var inventory = summonSystem.GetSoulCoreInventory();
                if (inventory.Count > 0)
                {
                    var newCore = inventory[inventory.Count - 1];
                    summonSystem.SetBattleSummon(firstEmptySlot, newCore);
                    Debug.Log($"自动装备到槽位 {firstEmptySlot}");
                }
            }
        }

        Debug.Log($"灵魂之核被收集：{enemyType}");
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            collision.GetComponent<ItemCollector>()?.SetNearbyItem(this);
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            collision.GetComponent<ItemCollector>()?.ClearNearbyItem(this);
    }

    // 绘制收集范围（仅在编辑器中可见）
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, collectRange);
    }
}

