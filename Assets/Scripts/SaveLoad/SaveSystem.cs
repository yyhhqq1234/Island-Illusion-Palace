using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

[System.Serializable]
public class GameSaveData
{
    public int saveSlot;
    public string saveName;
    public string timestamp;
    public int weekCount;
    public int playerLevel;
    public int playerExperience;
    public float playerHealth;
    public float playerMana;
    public float playerBurden;
    public Vector3 playerPosition;
    public string currentScene;
    public List<InventoryItemData> inventoryItems;
    public List<MemoryFragmentData> memoryFragments;
    public List<SoulCoreData> soulCores;
    public PlayerStatsData playerStats;
    public PermanentProgressData permanentProgress;

    public GameSaveData(int slot)
    {
        saveSlot = slot;
        saveName = "存档 " + slot;
        timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        weekCount = 1;
        playerLevel = 1;
        playerExperience = 0;
        playerHealth = 100f;
        playerMana = 100f;
        playerBurden = 0f;
        playerPosition = Vector3.zero;
        currentScene = "MainScene";
        inventoryItems = new List<InventoryItemData>();
        memoryFragments = new List<MemoryFragmentData>();
        soulCores = new List<SoulCoreData>();
        playerStats = new PlayerStatsData();
        permanentProgress = new PermanentProgressData();
    }
}

[System.Serializable]
public class PlayerStatsData
{
    public int strength;
    public int vitality;
    public int intelligence;
    public int resonance;
    public float maxHealth;
    public float maxMana;
    public float maxBurden;

    public PlayerStatsData()
    {
        strength = 10;
        vitality = 10;
        intelligence = 10;
        resonance = 10;
        maxHealth = 100f;
        maxMana = 100f;
        maxBurden = 100f;
    }
}

[System.Serializable]
public class InventoryItemData
{
    public string itemName;
    public string itemType;
    public int quantity;
    public bool isEquipped;

    public InventoryItemData(string name, string type, int qty, bool equipped)
    {
        itemName = name;
        itemType = type;
        quantity = qty;
        isEquipped = equipped;
    }
}

[System.Serializable]
public class MemoryFragmentData
{
    public string fragmentName;
    public string fragmentType;
    public string rarity;
    public int level;
    public bool isActivated;

    public MemoryFragmentData(string name, string type, string rare, int lvl, bool activated)
    {
        fragmentName = name;
        fragmentType = type;
        rarity = rare;
        level = lvl;
        isActivated = activated;
    }
}

[System.Serializable]
public class SoulCoreData
{
    public EnemyAI.EnemyType enemyType;
    public string quality;
    public int level;
    public float attributeMultiplier = 1f;

    public SoulCoreData(EnemyAI.EnemyType enemy, string qual, int lvl)
    {
        enemyType = enemy;
        quality = qual;
        level = lvl;
        attributeMultiplier = CalculateBaseMultiplier(quality);
    }
    
    // 为了保持向后兼容性，添加字符串构造函数
    public SoulCoreData(string enemy, string qual, int lvl)
    {
        EnemyAI.EnemyType type;
        if (System.Enum.TryParse<EnemyAI.EnemyType>(enemy, out type))
        {
            enemyType = type;
        }
        quality = qual;
        level = lvl;
        attributeMultiplier = CalculateBaseMultiplier(quality);
    }

    private float CalculateBaseMultiplier(string quality)
    {
        switch (quality)
        {
            case "Common": return 1f;
            case "Uncommon": return 1.2f;
            case "Rare": return 1.5f;
            case "Epic": return 2f;
            case "Legendary": return 2.5f;
            default: return 1f;
        }
    }

    public bool CanBreakthrough()
    {
        // 简单的突破条件：等级为10的倍数
        return level % 10 == 0;
    }

    public void Breakthrough()
    {
        if (CanBreakthrough())
        {
            level++;
            attributeMultiplier *= 1.1f; // 突破后属性倍率提升10%
        }
    }
}

[System.Serializable]
public class PermanentProgressData
{
    public int totalWeeks;
    public int totalEnemiesDefeated;
    public int totalBossesDefeated;
    public int totalItemsCollected;
    public int totalFragmentsCollected;
    public List<string> unlockedAchievements;
    public List<string> unlockedMemories;
    public List<SkillMasteryData> skillMasteries;
    public int permanentStrengthBonus;
    public int permanentVitalityBonus;
    public int permanentIntelligenceBonus;
    public int permanentResonanceBonus;

    public PermanentProgressData()
    {
        totalWeeks = 0;
        totalEnemiesDefeated = 0;
        totalBossesDefeated = 0;
        totalItemsCollected = 0;
        totalFragmentsCollected = 0;
        unlockedAchievements = new List<string>();
        unlockedMemories = new List<string>();
        skillMasteries = new List<SkillMasteryData>();
        permanentStrengthBonus = 0;
        permanentVitalityBonus = 0;
        permanentIntelligenceBonus = 0;
        permanentResonanceBonus = 0;
    }
}

[System.Serializable]
public class SkillMasteryData
{
    public string skillName;
    public int masteryLevel;

    public SkillMasteryData(string name, int level)
    {
        skillName = name;
        masteryLevel = level;
    }
}

public class SaveSystem : MonoBehaviour, ISaveService
{
    [Header("存档设置")]
    public int maxSaveSlots = 5;
    public string saveFolder = "Saves";

    [Header("引用")]
    public CharacterStats characterStats;
    public HealthSystem playerHealth;
    public ManaSystem playerMana;
    public BurdenSystem playerBurden;
    public InventorySystem inventorySystem;
    public MemoryFragmentSystem memoryFragmentSystem;
    public SummonSystem summonSystem;
    public IntegratedMapSystem mapSystem;

    [Header("状态")]
    private List<GameSaveData> saveDataList = new List<GameSaveData>();

    void Start()
    {
        InitializeReferences();
        LoadSaveDataList();
    }

    void InitializeReferences()
    {
        characterStats = GetComponentInParent<CharacterStats>();
        playerHealth = FindObjectOfType<HealthSystem>();
        playerMana = FindObjectOfType<ManaSystem>();
        playerBurden = FindObjectOfType<BurdenSystem>();
        inventorySystem = FindObjectOfType<InventorySystem>();
        memoryFragmentSystem = FindObjectOfType<MemoryFragmentSystem>();
        summonSystem = FindObjectOfType<SummonSystem>();
        mapSystem = FindObjectOfType<IntegratedMapSystem>();
        
        // 验证关键组件
        if (characterStats == null) Debug.LogWarning("SaveSystem: CharacterStats component not found!");
        if (playerHealth == null) Debug.LogWarning("SaveSystem: HealthSystem component not found!");
        if (playerMana == null) Debug.LogWarning("SaveSystem: ManaSystem component not found!");
        if (playerBurden == null) Debug.LogWarning("SaveSystem: BurdenSystem component not found!");
    }

    public void SaveGame(int saveSlot)
    {
        if (saveSlot < 1 || saveSlot > maxSaveSlots)
        {
            Debug.LogError($"SaveSystem: Invalid save slot {saveSlot}");
            return;
        }
        
        try
        {
            GameSaveData saveData = CreateSaveData(saveSlot);
            SaveDataToFile(saveData);
            Debug.Log($"游戏已保存到槽位 {saveSlot}");
        }
        catch (System.Exception e)
        {
            Debug.LogError("SaveSystem: Error saving game: " + e.Message);
        }
    }

    public void LoadGame(int saveSlot)
    {
        if (saveSlot < 1 || saveSlot > maxSaveSlots)
        {
            Debug.LogError($"SaveSystem: Invalid save slot {saveSlot}");
            return;
        }
        
        try
        {
            GameSaveData saveData = LoadDataFromFile(saveSlot);
            if (saveData != null)
            {
                ApplySaveData(saveData);
                Debug.Log($"从槽位 {saveSlot} 加载游戏");
            }
            else
            {
                Debug.LogError($"无法加载槽位 {saveSlot} 的存档");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("SaveSystem: Error loading game: " + e.Message);
        }
    }

    public void DeleteSave(int saveSlot)
    {
        if (saveSlot < 1 || saveSlot > maxSaveSlots)
        {
            Debug.LogError($"SaveSystem: Invalid save slot {saveSlot}");
            return;
        }
        
        try
        {
            string savePath = GetSaveFilePath(saveSlot);
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
                LoadSaveDataList();
                Debug.Log($"槽位 {saveSlot} 的存档已删除");
            }
            else
            {
                Debug.LogWarning($"SaveSystem: Save slot {saveSlot} does not exist");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("SaveSystem: Error deleting save: " + e.Message);
        }
    }

    public List<GameSaveData> GetSaveDataList()
    {
        LoadSaveDataList();
        return saveDataList;
    }

    public GameSaveData GetSaveData(int saveSlot)
    {
        return saveDataList.Find(save => save.saveSlot == saveSlot);
    }

    public bool SaveSlotExists(int saveSlot)
    {
        string savePath = GetSaveFilePath(saveSlot);
        return File.Exists(savePath);
    }

    GameSaveData CreateSaveData(int saveSlot)
    {
        try
        {
            GameSaveData saveData = new GameSaveData(saveSlot);
            
            // 保存游戏状态
            if (characterStats != null)
            {
                saveData.playerLevel = characterStats.level;
                saveData.playerExperience = characterStats.currentExperience;
                saveData.playerStats = new PlayerStatsData()
                {
                    strength = characterStats.strength,
                    vitality = characterStats.vitality,
                    intelligence = characterStats.intelligence,
                    resonance = characterStats.resonance,
                    maxHealth = playerHealth != null ? playerHealth.maxHealth : 100f,
                    maxMana = playerMana != null ? playerMana.maxMana : 100f,
                    maxBurden = playerBurden != null ? playerBurden.maxBurden : 100f
                };
            }

            if (playerHealth != null)
            {
                saveData.playerHealth = playerHealth.currentHealth;
            }

            if (playerMana != null)
            {
                saveData.playerMana = playerMana.currentMana;
            }

            if (playerBurden != null)
            {
                saveData.playerBurden = playerBurden.currentBurden;
            }

            // 保存玩家位置
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                saveData.playerPosition = player.transform.position;
            }

            // 保存当前场景
            saveData.currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            // 保存物品
            if (inventorySystem != null)
            {
                saveData.inventoryItems = new List<InventoryItemData>();
                try
                {
                    foreach (var item in inventorySystem.GetItems())
                    {
                        if (item != null)
                        {
                            saveData.inventoryItems.Add(new InventoryItemData(
                                item.itemName,
                                item.itemType.ToString(),
                                item.quantity,
                                item.isEquipped
                            ));
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning("SaveSystem: Error saving inventory: " + e.Message);
                }
            }

            // 保存记忆碎片
            if (memoryFragmentSystem != null)
            {
                saveData.memoryFragments = new List<MemoryFragmentData>();
                // 这里需要根据实际的记忆碎片系统实现
            }

            // 保存灵魂之核
            if (summonSystem != null)
            {
                saveData.soulCores = new List<SoulCoreData>();
                // 这里需要根据实际的召唤系统实现
            }

            // 保存永久进度
            saveData.permanentProgress = new PermanentProgressData();
            // 这里需要根据实际的游戏进度实现

            return saveData;
        }
        catch (System.Exception e)
        {
            Debug.LogError("SaveSystem: Error creating save data: " + e.Message);
            return new GameSaveData(saveSlot); // 返回默认数据作为后备
        }
    }

    void ApplySaveData(GameSaveData saveData)
    {
        // 应用玩家状态
        if (characterStats != null)
        {
            characterStats.level = saveData.playerLevel;
            characterStats.currentExperience = saveData.playerExperience;
            characterStats.strength = saveData.playerStats.strength;
            characterStats.vitality = saveData.playerStats.vitality;
            characterStats.intelligence = saveData.playerStats.intelligence;
            characterStats.resonance = saveData.playerStats.resonance;
        }

        if (playerHealth != null)
        {
            playerHealth.currentHealth = saveData.playerHealth;
            playerHealth.maxHealth = saveData.playerStats.maxHealth;
        }

        if (playerMana != null)
        {
            playerMana.currentMana = saveData.playerMana;
            playerMana.maxMana = saveData.playerStats.maxMana;
        }

        if (playerBurden != null)
        {
            playerBurden.currentBurden = saveData.playerBurden;
            playerBurden.maxBurden = saveData.playerStats.maxBurden;
        }

        // 应用玩家位置
        if (GameObject.FindGameObjectWithTag("Player") != null)
        {
            GameObject.FindGameObjectWithTag("Player").transform.position = saveData.playerPosition;
        }

        // 加载场景
        if (!string.IsNullOrEmpty(saveData.currentScene))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(saveData.currentScene);
        }

        // 应用物品
        if (inventorySystem != null && saveData.inventoryItems != null)
        {
            // 这里需要根据实际的物品系统实现
        }

        // 应用记忆碎片
        if (memoryFragmentSystem != null && saveData.memoryFragments != null)
        {
            // 这里需要根据实际的记忆碎片系统实现
        }

        // 应用灵魂之核
        if (summonSystem != null && saveData.soulCores != null)
        {
            // 这里需要根据实际的召唤系统实现
        }

        // 应用永久进度
        ApplyPermanentProgress(saveData.permanentProgress);
    }

    public void ApplyPermanentProgress(PermanentProgressData progress)
    {
        // 应用永久属性加成
        if (characterStats != null)
        {
            characterStats.strength += progress.permanentStrengthBonus;
            characterStats.vitality += progress.permanentVitalityBonus;
            characterStats.intelligence += progress.permanentIntelligenceBonus;
            characterStats.resonance += progress.permanentResonanceBonus;
        }

        // 应用其他永久进度
        // 这里需要根据实际的游戏进度实现
    }

    void SaveDataToFile(GameSaveData saveData)
    {
        if (saveData == null)
        {
            Debug.LogError("SaveSystem: Cannot save null save data!");
            return;
        }
        
        try
        {
            string savePath = GetSaveFilePath(saveData.saveSlot);
            string directory = Path.GetDirectoryName(savePath);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream stream = new FileStream(savePath, FileMode.Create))
            {
                formatter.Serialize(stream, saveData);
            }

            LoadSaveDataList();
        }
        catch (System.Exception e)
        {
            Debug.LogError("SaveSystem: Error saving game data: " + e.Message);
        }
    }

    GameSaveData LoadDataFromFile(int saveSlot)
    {
        try
        {
            string savePath = GetSaveFilePath(saveSlot);

            if (File.Exists(savePath))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                using (FileStream stream = new FileStream(savePath, FileMode.Open))
                {
                    GameSaveData loadedData = formatter.Deserialize(stream) as GameSaveData;
                    if (loadedData == null)
                    {
                        Debug.LogError("SaveSystem: Failed to deserialize save data!");
                        return null;
                    }
                    return loadedData;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("SaveSystem: Error loading game data: " + e.Message);
        }

        return null;
    }

    void LoadSaveDataList()
    {
        saveDataList.Clear();

        try
        {
            for (int i = 1; i <= maxSaveSlots; i++)
            {
                if (SaveSlotExists(i))
                {
                    GameSaveData saveData = LoadDataFromFile(i);
                    if (saveData != null)
                    {
                        saveDataList.Add(saveData);
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("SaveSystem: Error loading save data list: " + e.Message);
        }
    }

    string GetSaveFilePath(int saveSlot)
    {
        string persistentDataPath = Application.persistentDataPath;
        string savePath = Path.Combine(persistentDataPath, saveFolder, "save_" + saveSlot + ".dat");
        return savePath;
    }

    public void StartNewWeek()
    {
        // 开始新的一周
        GameSaveData currentSave = GetCurrentSaveData();
        if (currentSave != null)
        {
            currentSave.weekCount++;
            currentSave.permanentProgress.totalWeeks++;
            // 重置游戏状态，但保留永久进度
            ResetWeekProgress(currentSave);
            SaveGame(currentSave.saveSlot);
        }
    }

    void ResetWeekProgress(GameSaveData saveData)
    {
        // 重置一周的进度，但保留永久属性
        saveData.playerPosition = Vector3.zero;
        saveData.playerHealth = saveData.playerStats.maxHealth;
        saveData.playerMana = saveData.playerStats.maxMana;
        saveData.playerBurden = 0f;
        saveData.inventoryItems.Clear();
        // 保留记忆碎片和灵魂之核
        // 这里需要根据实际的游戏设计实现
    }

    GameSaveData GetCurrentSaveData()
    {
        // 获取当前加载的存档
        // 这里需要根据实际的游戏实现
        return null;
    }

    // 调试方法
    public void DebugPrintSaveData(int saveSlot)
    {
        GameSaveData saveData = LoadDataFromFile(saveSlot);
        if (saveData != null)
        {
            Debug.Log("=== 存档信息 ===");
            Debug.Log($"存档槽位：{saveData.saveSlot}");
            Debug.Log($"存档名称：{saveData.saveName}");
            Debug.Log($"时间戳：{saveData.timestamp}");
            Debug.Log($"周数：{saveData.weekCount}");
            Debug.Log($"玩家等级：{saveData.playerLevel}");
            Debug.Log($"玩家位置：{saveData.playerPosition}");
            Debug.Log($"当前场景：{saveData.currentScene}");
            Debug.Log($"物品数量：{saveData.inventoryItems.Count}");
            Debug.Log($"记忆碎片数量：{saveData.memoryFragments.Count}");
            Debug.Log($"灵魂之核数量：{saveData.soulCores.Count}");
            Debug.Log($"永久力量加成：{saveData.permanentProgress.permanentStrengthBonus}");
            Debug.Log($"永久体质加成：{saveData.permanentProgress.permanentVitalityBonus}");
            Debug.Log($"永久智慧加成：{saveData.permanentProgress.permanentIntelligenceBonus}");
            Debug.Log($"永久共鸣加成：{saveData.permanentProgress.permanentResonanceBonus}");
        }
    }
}
