using UnityEngine;

/// <summary>
/// 敌人掉落组件 - 负责掉落表和灵魂之核的生成
/// </summary>
public class EnemyLootComponent : MonoBehaviour
{
    [Header("掉落设置")]
    public int soulDropAmount = 15;
    public int essenceDropAmount = 0;
    [Tooltip("ScriptableObject 掉落表（优先于下方的硬编码掉落）")]
    public DropTableData dropTable;

    [Header("灵魂之核设置")]
    public GameObject soulCorePrefab;
    public Sprite[] soulCoreSprites;

    /// <summary>
    /// 处理掉落（死亡时调用）
    /// </summary>
    public void HandleLoot(EnemyAI.EnemyType enemyType, EnemyAI.EnemyQuality enemyQuality)
    {
        SpawnSoulCore(enemyType, enemyQuality);
        RollDropTableLoot();
    }

    /// <summary>
    /// 生成灵魂之核
    /// </summary>
    public void SpawnSoulCore(EnemyAI.EnemyType enemyType, EnemyAI.EnemyQuality enemyQuality)
    {
        Debug.Log($"生成灵魂之核：{enemyType} 在位置 {transform.position}");

        if (soulCorePrefab != null)
        {
            GameObject prefabInstance = Instantiate(soulCorePrefab, transform.position, Quaternion.identity);
            var prefabSoulCore = prefabInstance.GetComponent(System.Reflection.Assembly.GetExecutingAssembly().GetType("SoulCore"));
            if (prefabSoulCore != null)
            {
                var soulCoreType = prefabSoulCore.GetType();
                var setEnemyTypeMethod = soulCoreType.GetMethod("SetEnemyType");
                var setQualityMethod = soulCoreType.GetMethod("SetQuality");
                if (setEnemyTypeMethod != null)
                {
                    setEnemyTypeMethod.Invoke(prefabSoulCore, new object[] { enemyType });
                }
                if (setQualityMethod != null)
                {
                    setQualityMethod.Invoke(prefabSoulCore, new object[] { enemyQuality });
                }
            }
        }
        else
        {
            GameObject soulCoreObj = new GameObject("SoulCore_" + enemyType);
            soulCoreObj.transform.position = transform.position;

            var soulCore = soulCoreObj.AddComponent(System.Reflection.Assembly.GetExecutingAssembly().GetType("SoulCore"));
            var soulCoreType1 = soulCore.GetType();
            var setEnemyTypeMethod = soulCoreType1.GetMethod("SetEnemyType");
            var setQualityMethod = soulCoreType1.GetMethod("SetQuality");
            if (setEnemyTypeMethod != null)
            {
                setEnemyTypeMethod.Invoke(soulCore, new object[] { enemyType });
            }
            if (setQualityMethod != null)
            {
                setQualityMethod.Invoke(soulCore, new object[] { enemyQuality });
            }
        }
    }

    private void RollDropTableLoot()
    {
        if (dropTable == null) return;
        var results = dropTable.RollLoot();
        var inventory = FindObjectOfType<InventorySystem>();
        if (inventory == null) return;

        foreach (var r in results)
        {
            switch (r.type)
            {
                case DropTableData.DropType.Material:
                    inventory.AddMaterial(r.materialType, r.amount);
                    break;
                case DropTableData.DropType.Recipe:
                    inventory.AddPotion(r.recipeType, r.amount);
                    break;
                case DropTableData.DropType.Soul:
                    if (soulDropAmount > 0)
                        Debug.Log($"[Loot] 灵魂 +{soulDropAmount}");
                    break;
            }
        }
    }
}