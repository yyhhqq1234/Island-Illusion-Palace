using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public enum UIPanelType { MainMenu, GameHUD, Inventory, Skills, Map, Pause, GameOver, Dialog }

public class UIManager : MonoBehaviour
{
    [Header("UI面板")]
    public GameObject mainMenuPanel;
    public GameObject gameHUDPanel;
    public GameObject inventoryPanel;
    public GameObject skillsPanel;
    public GameObject mapPanel;
    public GameObject pausePanel;
    public GameObject gameOverPanel;
    public GameObject dialogPanel;

    [Header("游戏HUD元素")]
    public Slider healthSlider;
    public Slider manaSlider;
    public Slider burdenSlider;
    public Text healthText;
    public Text manaText;
    public Text burdenText;
    public Text levelText;
    public Text experienceText;

    [Header("背包元素")]
    public Transform inventoryGrid;
    public GameObject inventoryItemPrefab;
    public Text itemDescriptionText;

    [Header("地图元素")]
    public RawImage mapImage;
    public RectTransform playerMapIcon;

    [Header("对话元素")]
    public Text dialogText;
    public Text dialogNameText;

    [Header("引用")]
    public HealthSystem playerHealth;
    public ManaSystem playerMana;
    public BurdenSystem playerBurden;
    public CharacterStats characterStats;
    public InventorySystem inventorySystem;
    public IntegratedMapSystem mapSystem;

    [Header("状态")]
    private UIPanelType currentPanel = UIPanelType.MainMenu;
    private bool isPaused = false;

    void Start()
    {
        InitializeReferences();
        InitializeUI();
        ShowPanel(UIPanelType.MainMenu);
    }

    void Update()
    {
        UpdateHUD();
        HandleInput();
    }

    void InitializeReferences()
    {
        playerHealth = FindObjectOfType<HealthSystem>();
        playerMana = FindObjectOfType<ManaSystem>();
        playerBurden = FindObjectOfType<BurdenSystem>();
        characterStats = FindObjectOfType<CharacterStats>();
        inventorySystem = FindObjectOfType<InventorySystem>();
        mapSystem = FindObjectOfType<IntegratedMapSystem>();
    }

    void InitializeUI()
    {
        // 初始化所有面板状态
        SetAllPanelsActive(false);

        // 初始化HUD元素
        if (healthSlider != null) healthSlider.maxValue = 100;
        if (manaSlider != null) manaSlider.maxValue = 100;
        if (burdenSlider != null) burdenSlider.maxValue = 100;

        // 初始化背包网格
        if (inventoryGrid != null)
        {
            foreach (Transform child in inventoryGrid)
            {
                Destroy(child.gameObject);
            }
        }
    }

    void SetAllPanelsActive(bool active)
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(active);
        if (gameHUDPanel != null) gameHUDPanel.SetActive(active);
        if (inventoryPanel != null) inventoryPanel.SetActive(active);
        if (skillsPanel != null) skillsPanel.SetActive(active);
        if (mapPanel != null) mapPanel.SetActive(active);
        if (pausePanel != null) pausePanel.SetActive(active);
        if (gameOverPanel != null) gameOverPanel.SetActive(active);
        if (dialogPanel != null) dialogPanel.SetActive(active);
    }

    void UpdateHUD()
    {
        if (currentPanel == UIPanelType.GameHUD)
        {
            // 更新生命值
            if (playerHealth != null && healthSlider != null)
            {
                healthSlider.value = playerHealth.currentHealth;
                healthSlider.maxValue = playerHealth.maxHealth;
                if (healthText != null)
                {
                    healthText.text = $"{playerHealth.currentHealth:F0}/{playerHealth.maxHealth:F0}";
                }
            }

            // 更新魔法值
            if (playerMana != null && manaSlider != null)
            {
                manaSlider.value = playerMana.currentMana;
                manaSlider.maxValue = playerMana.maxMana;
                if (manaText != null)
                {
                    manaText.text = $"{playerMana.currentMana:F0}/{playerMana.maxMana:F0}";
                }
            }

            // 更新负担
            if (playerBurden != null && burdenSlider != null)
            {
                burdenSlider.value = playerBurden.currentBurden;
                burdenSlider.maxValue = playerBurden.maxBurden;
                if (burdenText != null)
                {
                    burdenText.text = $"{playerBurden.currentBurden:F0}/{playerBurden.maxBurden:F0}";
                }
            }

            // 更新等级和经验
            if (characterStats != null)
            {
                if (levelText != null)
                {
                    levelText.text = $"Lv.{characterStats.level}";
                }
                if (experienceText != null)
                {
                    experienceText.text = $"EXP: {characterStats.currentExperience}/{characterStats.requiredExperience}";
                }
            }
        }
    }

    void HandleInput()
    {
        // 处理UI面板切换
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventory();
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            ToggleSkills();
        }
        else if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleMap();
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void ShowPanel(UIPanelType panelType)
    {
        // 隐藏所有面板
        SetAllPanelsActive(false);

        // 显示目标面板
        switch (panelType)
        {
            case UIPanelType.MainMenu:
                if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
                break;
            case UIPanelType.GameHUD:
                if (gameHUDPanel != null) gameHUDPanel.SetActive(true);
                break;
            case UIPanelType.Inventory:
                if (inventoryPanel != null) inventoryPanel.SetActive(true);
                UpdateInventoryUI();
                break;
            case UIPanelType.Skills:
                if (skillsPanel != null) skillsPanel.SetActive(true);
                break;
            case UIPanelType.Map:
                if (mapPanel != null) mapPanel.SetActive(true);
                UpdateMapUI();
                break;
            case UIPanelType.Pause:
                if (pausePanel != null) pausePanel.SetActive(true);
                break;
            case UIPanelType.GameOver:
                if (gameOverPanel != null) gameOverPanel.SetActive(true);
                break;
            case UIPanelType.Dialog:
                if (dialogPanel != null) dialogPanel.SetActive(true);
                break;
        }

        currentPanel = panelType;
        Debug.Log($"显示面板：{panelType}");
    }

    public void HideAllPanels()
    {
        SetAllPanelsActive(false);
        currentPanel = UIPanelType.GameHUD;
    }

    public void ToggleInventory()
    {
        if (currentPanel == UIPanelType.Inventory)
        {
            ShowPanel(UIPanelType.GameHUD);
        }
        else
        {
            ShowPanel(UIPanelType.Inventory);
        }
    }

    public void ToggleSkills()
    {
        if (currentPanel == UIPanelType.Skills)
        {
            ShowPanel(UIPanelType.GameHUD);
        }
        else
        {
            ShowPanel(UIPanelType.Skills);
        }
    }

    public void ToggleMap()
    {
        if (currentPanel == UIPanelType.Map)
        {
            ShowPanel(UIPanelType.GameHUD);
        }
        else
        {
            ShowPanel(UIPanelType.Map);
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        if (isPaused)
        {
            ShowPanel(UIPanelType.Pause);
            Time.timeScale = 0f;
        }
        else
        {
            ShowPanel(UIPanelType.GameHUD);
            Time.timeScale = 1f;
        }
    }

    void UpdateInventoryUI()
    {
        if (inventoryGrid == null || inventoryItemPrefab == null || inventorySystem == null)
            return;

        // 清空现有物品
        foreach (Transform child in inventoryGrid)
        {
            Destroy(child.gameObject);
        }

        // 生成物品格子
        List<InventoryItem> items = inventorySystem.GetItems();
        for (int i = 0; i < items.Count; i++)
        {
            InventoryItem item = items[i];
            if (item != null)
            {
                GameObject itemObj = Instantiate(inventoryItemPrefab, inventoryGrid);
                InventoryItemUI itemUI = itemObj.GetComponent<InventoryItemUI>();
                if (itemUI != null)
                {
                    itemUI.SetItem(item);
                    itemUI.onItemClicked.AddListener(OnInventoryItemClicked);
                }
            }
        }
    }

    void OnInventoryItemClicked(InventoryItem item)
    {
        if (itemDescriptionText != null)
        {
            itemDescriptionText.text = item.GetDescription();
        }
        Debug.Log($"点击物品：{item.itemName}");
    }

    void UpdateMapUI()
    {
        if (mapSystem == null)
            return;

        // 更新地图图标位置
        if (playerMapIcon != null)
        {
            // 这里可以根据地图系统的当前房间位置更新玩家图标
        }
    }

    public void StartGame()
    {
        ShowPanel(UIPanelType.GameHUD);
        Debug.Log("游戏开始！");
        
        // 确保玩家对象处于激活状态
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && !player.activeSelf)
        {
            player.SetActive(true);
            Debug.Log("玩家对象已激活（UIManager）");
        }
    }

    public void ResumeGame()
    {
        isPaused = false;
        ShowPanel(UIPanelType.GameHUD);
        Time.timeScale = 1f;
        Debug.Log("游戏继续！");
    }

    public void RestartGame()
    {
        ShowPanel(UIPanelType.MainMenu);
        Time.timeScale = 1f;
        Debug.Log("游戏重新开始！");
    }

    public void QuitGame()
    {
        Debug.Log("退出游戏！");
        Application.Quit();
    }

    public void ShowDialog(string name, string text)
    {
        if (dialogPanel != null)
        {
            dialogPanel.SetActive(true);
        }
        if (dialogNameText != null)
        {
            dialogNameText.text = name;
        }
        if (dialogText != null)
        {
            dialogText.text = text;
        }
        currentPanel = UIPanelType.Dialog;
    }

    public void HideDialog()
    {
        if (dialogPanel != null)
        {
            dialogPanel.SetActive(false);
        }
        currentPanel = UIPanelType.GameHUD;
    }

    public void ShowGameOver()
    {
        ShowPanel(UIPanelType.GameOver);
        Time.timeScale = 0f;
        Debug.Log("游戏结束！");
    }

    // 调试方法
    public void DebugPrintUIStatus()
    {
        Debug.Log("=== UI状态 ===");
        Debug.Log($"当前面板：{currentPanel}");
        Debug.Log($"游戏暂停：{isPaused}");
        Debug.Log($"HUD激活：{gameHUDPanel != null && gameHUDPanel.activeSelf}");
        Debug.Log($"背包激活：{inventoryPanel != null && inventoryPanel.activeSelf}");
        Debug.Log($"技能激活：{skillsPanel != null && skillsPanel.activeSelf}");
        Debug.Log($"地图激活：{mapPanel != null && mapPanel.activeSelf}");
    }
}
