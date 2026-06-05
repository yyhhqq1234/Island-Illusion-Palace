using UnityEngine;

/// <summary>
/// 时空裂隙 — 不稳定随机传送
/// 设计文档参考：幻宫_时空回响_地图探索和生成系统拆解.md 4.2节
/// - 基础生成概率5%
/// - 负担>50 +10%, 负担>70 +10%
/// - 每个记忆碎片 +3%
/// - 多周目(cycle>=2) +3%
/// - 80%通往高难地图，20%通往记忆区域
/// </summary>
public class TimeRift : MonoBehaviour
{
    [Header("裂隙设置")]
    public MapType destinationType = MapType.MemoryFragment;
    public float lifetime = 45f;

    [Header("视觉效果")]
    public ParticleSystem riftParticles;
    public Animator animator;

    [Header("传送设置")]
    [Tooltip("是否使用概率分配目的地（80%高难/20%记忆区域）")]
    public bool useProbabilityDestination = true;
    [Tooltip("高难地图比例（0~1）")]
    [Range(0f, 1f)]
    public float highDifficultyChance = 0.8f;

    private float age;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (useProbabilityDestination)
            SelectDestinationByProbability();
    }

    void SelectDestinationByProbability()
    {
        float roll = Random.value;
        if (roll < highDifficultyChance)
        {
            destinationType = GetRandomHighDifficultyMap();
            Debug.Log($"[时空裂隙] 通往高难地图: {destinationType}");
        }
        else
        {
            destinationType = MapType.MemoryFragment;
            Debug.Log($"[时空裂隙] 通往记忆碎片区域");
        }
    }

    MapType GetRandomHighDifficultyMap()
    {
        MapType[] highDiffMaps = {
            MapType.Volcano,
            MapType.IceField,
            MapType.RockLand,
            MapType.RuinCity,
            MapType.AncientTemple,
            MapType.ForgottenManor,
        };

        var currentMap = GetCurrentMapType();
        var available = new System.Collections.Generic.List<MapType>();
        foreach (var m in highDiffMaps)
        {
            if (m != currentMap)
                available.Add(m);
        }

        return available.Count > 0 ? available[Random.Range(0, available.Count)] : MapType.Volcano;
    }

    MapType GetCurrentMapType()
    {
        var map = FindObjectOfType<IntegratedMapSystem>();
        return map != null ? map.currentMapType : MapType.Forest;
    }

    void Update()
    {
        age += Time.deltaTime;

        // 最后10秒触发Animator淡出
        float remainingTime = lifetime - age;
        if (remainingTime <= 10f && animator != null)
            animator.SetBool("IsFadingOut", true);

        if (age >= lifetime)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (age < 1f) return;

        Debug.Log($"[时空裂隙] 玩家进入裂隙，传送到 {destinationType}");

        var map = FindObjectOfType<IntegratedMapSystem>();
        if (map != null)
        {
            map.SetMapType(destinationType);
            map.GenerateNewMap();
        }
        Destroy(gameObject);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 1, 1, 0.5f);
        Gizmos.DrawWireSphere(transform.position, 0.8f);
    }
}
