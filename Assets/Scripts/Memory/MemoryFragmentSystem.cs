using UnityEngine;
using System.Collections.Generic;

public enum MemoryFragmentType { Childhood, Battle, Emotion, Knowledge, Sacrifice, Hope, Truth }

public enum FragmentRarity { Common, Uncommon, Rare, Epic, Legendary }

[System.Serializable]
public class MemoryFragment
{
    public string fragmentName;
    public MemoryFragmentType fragmentType;
    public FragmentRarity rarity;
    public int level;
    public float burdenCost;
    public string description;
    public string memoryText;
    public bool isActivated;

    public MemoryFragment(string name, MemoryFragmentType type, FragmentRarity fragmentRarity, int fragmentLevel)
    {
        fragmentName = name;
        fragmentType = type;
        rarity = fragmentRarity;
        level = fragmentLevel;
        burdenCost = CalculateBurdenCost();
        description = GenerateDescription();
        memoryText = GenerateMemoryText();
        isActivated = false;
    }

    float CalculateBurdenCost()
    {
        float rarityMultiplier = rarity switch
        {
            FragmentRarity.Common => 1f,
            FragmentRarity.Uncommon => 1.5f,
            FragmentRarity.Rare => 2f,
            FragmentRarity.Epic => 2.5f,
            FragmentRarity.Legendary => 3f,
            _ => 1f
        };
        return 10f * level * rarityMultiplier;
    }

    string GenerateDescription()
    {
        string typeDesc = fragmentType switch
        {
            MemoryFragmentType.Childhood => "童年记忆，带来温暖与力量",
            MemoryFragmentType.Battle => "战斗记忆，增强战斗能力",
            MemoryFragmentType.Emotion => "情感记忆，提升精神属性",
            MemoryFragmentType.Knowledge => "知识记忆，增加智慧",
            MemoryFragmentType.Sacrifice => "牺牲记忆，带来特殊效果",
            MemoryFragmentType.Hope => "希望记忆，增强恢复能力",
            MemoryFragmentType.Truth => "真相记忆，揭示隐藏力量",
            _ => "未知记忆"
        };

        string rarityDesc = rarity switch
        {
            FragmentRarity.Common => "普通",
            FragmentRarity.Uncommon => "优秀",
            FragmentRarity.Rare => "稀有",
            FragmentRarity.Epic => "史诗",
            FragmentRarity.Legendary => "传说",
            _ => "普通"
        };

        return $"{rarityDesc}的{typeDesc}，等级 {level}";
    }

    string GenerateMemoryText()
    {
        return fragmentType switch
        {
            MemoryFragmentType.Childhood => "那是一个阳光明媚的下午，我和朋友们在田野里奔跑...",
            MemoryFragmentType.Battle => "战场上，我与敌人激烈交锋，每一次攻击都充满了决心...",
            MemoryFragmentType.Emotion => "她的笑容如同春天的阳光，温暖了我内心的寒冷...",
            MemoryFragmentType.Knowledge => "古老的书籍中记载着不为人知的秘密，我沉浸在知识的海洋中...",
            MemoryFragmentType.Sacrifice => "为了保护重要的人，我愿意付出一切，即使是自己的生命...",
            MemoryFragmentType.Hope => "即使在最黑暗的时刻，我也从未放弃希望，因为我知道光明终会到来...",
            MemoryFragmentType.Truth => "当真相揭开的那一刻，我才明白一切都是命运的安排...",
            _ => "一段模糊的记忆，似乎隐藏着重要的信息..."
        };
    }

    public float GetEffectValue()
    {
        float rarityMultiplier = rarity switch
        {
            FragmentRarity.Common => 0.05f,
            FragmentRarity.Uncommon => 0.1f,
            FragmentRarity.Rare => 0.15f,
            FragmentRarity.Epic => 0.2f,
            FragmentRarity.Legendary => 0.3f,
            _ => 0.05f
        };
        return level * rarityMultiplier;
    }

    public void LevelUp()
    {
        level++;
        burdenCost = CalculateBurdenCost();
        description = GenerateDescription();
    }

    public void Activate()
    {
        isActivated = true;
    }

    public void Deactivate()
    {
        isActivated = false;
    }
}

public class MemoryFragmentSystem : MonoBehaviour
{
    [Header("记忆碎片设置")]
    public int maxFragmentSlots = 3;
    public int resonanceSlotLimit = 3;
    public float resonanceBonusMultiplier = 0.5f;

    [Header("记忆碎片库存")]
    public List<MemoryFragment> collectedFragments = new List<MemoryFragment>();
    public List<MemoryFragment> activatedFragments = new List<MemoryFragment>();
    public List<MemoryFragment> resonanceFragments = new List<MemoryFragment>();

    [Header("剧情触发")]
    public List<string> unlockedMemories = new List<string>();
    public int currentMemoryIndex = 0;

    [Header("引用")]
    public CharacterStats characterStats;
    public BurdenSystem burdenSystem;
    public SummonSystem summonSystem;

    [Header("状态")]
    private bool isProcessingResonance = false;
    private int fragmentsCollected = 0;

    void Start()
    {
        InitializeReferences();
        LoadInitialFragments();
        UpdateResonanceEffects();
    }

    void InitializeReferences()
    {
        characterStats = GetComponentInParent<CharacterStats>();
        burdenSystem = GetComponentInParent<BurdenSystem>();
        summonSystem = GetComponentInParent<SummonSystem>();
    }

    void LoadInitialFragments()
    {
        // 添加一些初始记忆碎片用于测试
        AddFragment(new MemoryFragment("童年的阳光", MemoryFragmentType.Childhood, FragmentRarity.Common, 1));
        AddFragment(new MemoryFragment("第一次战斗", MemoryFragmentType.Battle, FragmentRarity.Uncommon, 1));
    }

    public void AddFragment(MemoryFragment fragment)
    {
        collectedFragments.Add(fragment);
        fragmentsCollected++;
        Debug.Log($"获得记忆碎片：{fragment.fragmentName}");

        // 检查是否触发剧情
        CheckMemoryTrigger();
    }

    public bool ActivateFragment(MemoryFragment fragment)
    {
        if (activatedFragments.Count >= maxFragmentSlots)
        {
            Debug.Log("记忆碎片槽位已满");
            return false;
        }

        if (!collectedFragments.Contains(fragment))
        {
            Debug.Log("记忆碎片不存在");
            return false;
        }

        if (fragment.isActivated)
        {
            Debug.Log("记忆碎片已经激活");
            return false;
        }

        // 检查负担是否足够
        if (burdenSystem != null)
        {
            float totalBurden = GetTotalFragmentBurden() + fragment.burdenCost;
            if (totalBurden > burdenSystem.maxBurden)
            {
                Debug.Log("激活该记忆碎片会超出负担上限");
                return false;
            }

            // 添加负担
            burdenSystem.AddBurden(fragment.burdenCost);
        }

        fragment.Activate();
        activatedFragments.Add(fragment);
        Debug.Log($"激活记忆碎片：{fragment.fragmentName}");

        // 更新共鸣效果
        UpdateResonanceEffects();

        return true;
    }

    public bool DeactivateFragment(MemoryFragment fragment)
    {
        if (!activatedFragments.Contains(fragment))
        {
            Debug.Log("记忆碎片未激活");
            return false;
        }

        fragment.Deactivate();
        activatedFragments.Remove(fragment);

        // 移除负担
        if (burdenSystem != null)
        {
            burdenSystem.RemoveBurden(fragment.burdenCost);
        }

        Debug.Log($"停用记忆碎片：{fragment.fragmentName}");

        // 更新共鸣效果
        UpdateResonanceEffects();

        return true;
    }

    public bool AddToResonance(MemoryFragment fragment)
    {
        if (resonanceFragments.Count >= resonanceSlotLimit)
        {
            Debug.Log("共鸣槽位已满");
            return false;
        }

        if (!fragment.isActivated)
        {
            Debug.Log("记忆碎片未激活，无法加入共鸣");
            return false;
        }

        resonanceFragments.Add(fragment);
        Debug.Log($"记忆碎片加入共鸣：{fragment.fragmentName}");

        // 更新共鸣效果
        UpdateResonanceEffects();

        return true;
    }

    public bool RemoveFromResonance(MemoryFragment fragment)
    {
        if (!resonanceFragments.Contains(fragment))
        {
            Debug.Log("记忆碎片不在共鸣槽中");
            return false;
        }

        resonanceFragments.Remove(fragment);
        Debug.Log($"记忆碎片移出共鸣：{fragment.fragmentName}");

        // 更新共鸣效果
        UpdateResonanceEffects();

        return true;
    }

    void UpdateResonanceEffects()
    {
        if (resonanceFragments.Count == 0)
        {
            // 清除所有共鸣效果
            ClearResonanceEffects();
            return;
        }

        // 计算共鸣效果
        CalculateResonanceEffects();
    }

    void CalculateResonanceEffects()
    {
        // 统计各类型记忆碎片数量
        Dictionary<MemoryFragmentType, int> typeCounts = new Dictionary<MemoryFragmentType, int>();
        foreach (var fragment in resonanceFragments)
        {
            if (typeCounts.ContainsKey(fragment.fragmentType))
            {
                typeCounts[fragment.fragmentType]++;
            }
            else
            {
                typeCounts[fragment.fragmentType] = 1;
            }
        }

        // 应用共鸣效果
        foreach (var kvp in typeCounts)
        {
            int count = kvp.Value;
            if (count >= 2)
            {
                ApplyTypeResonance(kvp.Key, count);
            }
        }

        // 检查是否有全类型共鸣
        if (typeCounts.Count >= 3)
        {
            ApplyMultiTypeResonance(typeCounts.Count);
        }
    }

    void ApplyTypeResonance(MemoryFragmentType type, int count)
    {
        float bonus = count * resonanceBonusMultiplier * 0.1f;

        switch (type)
        {
            case MemoryFragmentType.Childhood:
                // 增加生命值
                if (characterStats != null)
                {
                    characterStats.vitality += Mathf.FloorToInt(bonus * 10);
                }
                break;
            case MemoryFragmentType.Battle:
                // 增加攻击力
                if (characterStats != null)
                {
                    characterStats.strength += Mathf.FloorToInt(bonus * 10);
                }
                break;
            case MemoryFragmentType.Emotion:
                // 增加精神属性
                if (characterStats != null)
                {
                    characterStats.resonance += Mathf.FloorToInt(bonus * 10);
                }
                break;
            case MemoryFragmentType.Knowledge:
                // 增加智慧
                if (characterStats != null)
                {
                    characterStats.intelligence += Mathf.FloorToInt(bonus * 10);
                }
                break;
            case MemoryFragmentType.Sacrifice:
                // 增加特殊效果
                if (summonSystem != null)
                {
                    // 这里可以添加特殊效果
                }
                break;
            case MemoryFragmentType.Hope:
                // 增加恢复能力
                if (burdenSystem != null)
                {
                    // 这里可以添加恢复效果
                }
                break;
            case MemoryFragmentType.Truth:
                // 增加全属性
                if (characterStats != null)
                {
                    characterStats.strength += Mathf.FloorToInt(bonus * 5);
                    characterStats.vitality += Mathf.FloorToInt(bonus * 5);
                    characterStats.intelligence += Mathf.FloorToInt(bonus * 5);
                    characterStats.resonance += Mathf.FloorToInt(bonus * 5);
                }
                break;
        }

        Debug.Log($"{type}类型共鸣效果激活，等级：{count}");
    }

    void ApplyMultiTypeResonance(int typeCount)
    {
        float bonus = typeCount * resonanceBonusMultiplier * 0.05f;

        if (characterStats != null)
        {
            characterStats.strength += Mathf.FloorToInt(bonus * 10);
            characterStats.vitality += Mathf.FloorToInt(bonus * 10);
            characterStats.intelligence += Mathf.FloorToInt(bonus * 10);
            characterStats.resonance += Mathf.FloorToInt(bonus * 10);
        }

        Debug.Log($"多类型共鸣效果激活，类型数量：{typeCount}");
    }

    void ClearResonanceEffects()
    {
        // 清除所有共鸣效果
        Debug.Log("共鸣效果已清除");
    }

    void CheckMemoryTrigger()
    {
        // 检查是否触发记忆剧情
        if (fragmentsCollected >= 3 && currentMemoryIndex == 0)
        {
            TriggerMemory("记忆的开始");
            currentMemoryIndex++;
        }
        else if (fragmentsCollected >= 7 && currentMemoryIndex == 1)
        {
            TriggerMemory("真相的碎片");
            currentMemoryIndex++;
        }
        else if (fragmentsCollected >= 12 && currentMemoryIndex == 2)
        {
            TriggerMemory("命运的抉择");
            currentMemoryIndex++;
        }
    }

    void TriggerMemory(string memoryName)
    {
        if (!unlockedMemories.Contains(memoryName))
        {
            unlockedMemories.Add(memoryName);
            Debug.Log($"触发记忆：{memoryName}");
            // 这里可以添加剧情触发逻辑，比如播放对话、动画等
        }
    }

    public float GetTotalFragmentBurden()
    {
        float total = 0f;
        foreach (var fragment in activatedFragments)
        {
            total += fragment.burdenCost;
        }
        return total;
    }

    public List<MemoryFragment> GetFragmentsByType(MemoryFragmentType type)
    {
        return collectedFragments.FindAll(fragment => fragment.fragmentType == type);
    }

    public List<MemoryFragment> GetActivatedFragments()
    {
        return activatedFragments;
    }

    public List<MemoryFragment> GetResonanceFragments()
    {
        return resonanceFragments;
    }

    public List<MemoryFragment> GetUnactivatedFragments()
    {
        return collectedFragments.FindAll(fragment => !fragment.isActivated);
    }

    public int GetFragmentCount()
    {
        return collectedFragments.Count;
    }

    public int GetActivatedFragmentCount()
    {
        return activatedFragments.Count;
    }

    public int GetResonanceFragmentCount()
    {
        return resonanceFragments.Count;
    }

    public bool HasFragment(string fragmentName)
    {
        return collectedFragments.Exists(fragment => fragment.fragmentName == fragmentName);
    }

    public MemoryFragment GetFragment(string fragmentName)
    {
        return collectedFragments.Find(fragment => fragment.fragmentName == fragmentName);
    }

    // 调试方法
    public void DebugPrintFragments()
    {
        Debug.Log("=== 记忆碎片库存 ===");
        foreach (var fragment in collectedFragments)
        {
            string status = fragment.isActivated ? "[激活]" : "[未激活]";
            Debug.Log($"{status} {fragment.fragmentName} - {fragment.fragmentType}，等级 {fragment.level}");
        }
        Debug.Log($"总碎片数：{GetFragmentCount()}，激活数：{GetActivatedFragmentCount()}，共鸣数：{GetResonanceFragmentCount()}");
    }

    public void DebugPrintMemories()
    {
        Debug.Log("=== 解锁的记忆 ===");
        foreach (var memory in unlockedMemories)
        {
            Debug.Log($"- {memory}");
        }
    }
}
