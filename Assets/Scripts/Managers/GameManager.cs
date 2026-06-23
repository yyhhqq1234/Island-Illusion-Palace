    using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using GameSystems;

public class GameManager : MonoBehaviour
{
    [Header("UI元素")]
    public Text systemStatusText;
    public Text battleStatsText;
    public Slider healthSlider;
    public Slider burdenSlider;
    public Text summonCountText;
    public Text alchemyInventoryText;

    [Header("玩家和系统引用")]
    public GameObject player;
    public HealthSystem playerHealth;
    public BurdenSystem playerBurden;
    public BattleSystem battleSystem;
    public SummonSystem summonSystem;
    public AlchemySystem alchemySystem;

    [Header("测试设置")]
    public bool enableAutoTesting = false;
    public float testInterval = 5f;

    private bool systemsInitialized = false;
    private Coroutine testCoroutine;

    void Start()
    {
        _ = GlobalEventManager.Instance;
        StartCoroutine(InitializeSystemsWithRetry());
    }

    /// <summary>
    /// 延迟初始化：每隔 1 秒重试查找系统组件，最多尝试 30 次。
    /// Player 可能由 MapGenerator 动态生成，系统组件不一定在 Start 时就存在。
    /// </summary>
    IEnumerator InitializeSystemsWithRetry()
    {
        int maxAttempts = 30;
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            if (TryInitializeSystems())
            {
                Debug.Log($"[GameManager] 所有系统初始化完成！（尝试次数: {attempt}）");
                if (enableAutoTesting)
                    testCoroutine = StartCoroutine(RunSystemTests());
                yield break;
            }

            if (attempt == 1)
                Debug.Log("[GameManager] 系统组件尚未就绪，等待地图生成...");
            yield return new WaitForSeconds(1f);
        }

        Debug.LogWarning($"[GameManager] 系统初始化超时（{maxAttempts}秒）。部分功能不可用。UI 仍可运行。");
    }

    void Update()
    {
        UpdateUI();
    }

    bool TryInitializeSystems()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");

        if (player != null && playerHealth == null)
            playerHealth = player.GetComponent<HealthSystem>();

        if (player != null && playerBurden == null)
            playerBurden = player.GetComponent<BurdenSystem>();

        if (battleSystem == null)
            battleSystem = FindObjectOfType<BattleSystem>();

        if (summonSystem == null)
            summonSystem = FindObjectOfType<SummonSystem>();

        if (alchemySystem == null)
            alchemySystem = FindObjectOfType<AlchemySystem>();

        systemsInitialized = player != null &&
                           playerHealth != null &&
                           playerBurden != null &&
                           battleSystem != null &&
                           summonSystem != null &&
                           alchemySystem != null;

        if (systemsInitialized)
        {
            GlobalEventManager.Instance.OnEnemyDefeated += OnEnemyDefeated;
            GlobalEventManager.Instance.OnDamageDealt += OnDamageDealt;
            GlobalEventManager.Instance.OnDamageTaken += OnDamageTaken;
            GlobalEventManager.Instance.OnPlayerDeath += OnPlayerDeath;
        }

        return systemsInitialized;
    }

    void UpdateUI()
    {
        if (!systemsInitialized) return;

        if (systemStatusText != null)
        {
            systemStatusText.text = "系统状态: 正常运行\n" +
                                  "玩家控制: " + (player != null ? "已连接" : "未找到") + "\n" +
                                  "战斗系统: " + (battleSystem != null ? "已激活" : "未激活") + "\n" +
                                  "召唤系统: " + (summonSystem != null ? "已激活" : "未激活") + "\n" +
                                  "炼金系统: " + (alchemySystem != null ? "已激活" : "未激活");
        }

        if (battleStatsText != null && battleSystem != null)
        {
            battleStatsText.text = battleSystem.GetBattleStats();
        }

        if (healthSlider != null && playerHealth != null)
        {
            healthSlider.value = playerHealth.GetHealthPercent();
        }

        if (burdenSlider != null && playerBurden != null)
        {
            burdenSlider.value = playerBurden.GetBurdenPercent();
        }

        if (summonCountText != null && summonSystem != null)
        {
            string summonText = "灵魂之核: " + summonSystem.GetSoulCoreCount() + " | ";
            summonText += "召唤物: " + summonSystem.GetActiveSummonCount() + "/" + summonSystem.maxActiveSummons;
            summonCountText.text = summonText;
        }

        if (alchemyInventoryText != null && alchemySystem != null)
        {
            alchemyInventoryText.text = "材料库存:\n";
            foreach (var item in alchemySystem.materialInventory)
            {
                if (item.Value > 0)
                {
                    alchemyInventoryText.text += GetMaterialName((MaterialTypeEnum)item.Key) + ": " + item.Value + "\n";
                }
            }
        }
    }

    string GetMaterialName(MaterialTypeEnum typeEnum)
    {
        return MaterialDatabase.GetMaterialName(typeEnum);
    }

    IEnumerator RunSystemTests()
    {
        yield return new WaitForSeconds(2f);

        while (true)
        {
            Debug.Log("=== 开始系统测试 ===");

            Debug.Log("测试1: 玩家控制 - 检查移动和闪避");
            if (player != null)
            {
                Debug.Log("  ✓ 玩家对象存在");
                PlayerController playerCtrl = player.GetComponent<PlayerController>();
                if (playerCtrl != null)
                {
                    Debug.Log("  ✓ 玩家控制器存在");
                    Debug.Log("  ✓ 闪避冷却进度: " + playerCtrl.GetDashCooldownProgress());
                }
            }

            Debug.Log("测试2: 战斗系统 - 检查武器切换和攻击");
            if (battleSystem != null)
            {
                Debug.Log("  ✓ 战斗系统存在");
                Debug.Log("  ✓ 当前武器: " + (battleSystem.weaponSystem != null ? battleSystem.weaponSystem.currentWeaponType.ToString() : "无"));
            }

            Debug.Log("测试3: 召唤系统 - 检查灵魂之核队列和召唤物管理");
            if (summonSystem != null)
            {
                Debug.Log("  ✓ 召唤系统存在");
                Debug.Log("  ✓ 灵魂之核队列长度: " + summonSystem.GetSoulCoreCount());
                Debug.Log("  ✓ 当前召唤数: " + summonSystem.GetActiveSummonCount() + "/" + summonSystem.maxActiveSummons);
                var inventory = summonSystem.GetSoulCoreInventory();
                string inventoryContent = inventory.Count > 0 ? string.Join(", ", inventory.ConvertAll(c => c.enemyType.ToString())) : "空";
                Debug.Log("  ✓ 库存内容: [" + inventoryContent + "]");
                Debug.Log("  ✓ 剩余召唤槽位: " + summonSystem.GetRemainingSummonSlots());
            }

            Debug.Log("测试4: 炼金系统 - 检查材料和配方");
            if (alchemySystem != null)
            {
                Debug.Log("  ✓ 炼金系统存在");
                Debug.Log("  ✓ 材料种类数: " + alchemySystem.materialInventory.Count);
                Debug.Log("  ✓ 可用配方数: " + alchemySystem.availableRecipes.Count);
            }

            Debug.Log("测试5: 生命值系统 - 检查健康状态");
            if (playerHealth != null && playerBurden != null)
            {
                Debug.Log("  ✓ 生命值系统存在");
                Debug.Log("  ✓ 生命值: " + playerHealth.GetHealthPercent() * 100 + "%");
                Debug.Log("  ✓ 负担值: " + playerBurden.GetBurdenPercent() * 100 + "%");
            }

            Debug.Log("=== 系统测试完成 ===\n");

            yield return new WaitForSeconds(testInterval);
        }
    }

    void OnEnemyDefeated(GameObject enemy)
    {
        Debug.Log("事件触发: 敌人被击败 - " + enemy.name);
    }

    void OnDamageDealt(GameObject attacker, float damage)
    {
        Debug.Log("事件触发: 造成伤害 - " + damage + " 点");
    }

    void OnDamageTaken(GameObject target, float damage)
    {
        Debug.Log("事件触发: 受到伤害 - " + damage + " 点");
    }

    void OnPlayerDeath()
    {
        Debug.Log("事件触发: 玩家死亡");

        if (testCoroutine != null)
        {
            StopCoroutine(testCoroutine);
        }
    }

    public void RunManualTest()
    {
        Debug.Log("=== 手动系统测试 ===");

        if (playerHealth != null)
        {
            float originalHealth = playerHealth.GetHealthPercent();
            playerHealth.TakeDamage(10f);
            Debug.Log("应用10点伤害，生命值变化: " + originalHealth + " -> " + playerHealth.GetHealthPercent());

            playerHealth.Heal(5f);
            Debug.Log("恢复5点生命值，当前生命值: " + playerHealth.GetHealthPercent());

            float originalBurden = playerBurden.GetBurdenPercent();
            playerBurden.ChangeBurden(10f);
            Debug.Log("增加10点负担值，负担值变化: " + originalBurden + " -> " + playerBurden.GetBurdenPercent());

            playerBurden.ChangeBurden(-5f);
            Debug.Log("减少5点负担值，当前负担值: " + playerBurden.GetBurdenPercent());
        }

        if (alchemySystem != null)
        {
            alchemySystem.AddMaterial(MaterialTypeEnum.SoulDust, 5);
            alchemySystem.AddMaterial(MaterialTypeEnum.PurifyingSalt, 3);
            alchemySystem.AddMaterial(MaterialTypeEnum.SoulEssence, 2);

            Debug.Log("添加测试材料，当前库存:");
            alchemySystem.DisplayInventory();

            var availableRecipes = alchemySystem.GetAvailableRecipes();
            Debug.Log("当前可用配方数: " + availableRecipes.Count);
        }

        Debug.Log("=== 手动测试完成 ===");
    }

    void OnDestroy()
    {
        GlobalEventManager.Instance.OnEnemyDefeated -= OnEnemyDefeated;
        GlobalEventManager.Instance.OnDamageDealt -= OnDamageDealt;
        GlobalEventManager.Instance.OnDamageTaken -= OnDamageTaken;
        GlobalEventManager.Instance.OnPlayerDeath -= OnPlayerDeath;
    }
}
