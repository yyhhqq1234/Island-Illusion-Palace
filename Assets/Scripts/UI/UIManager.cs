using UnityEngine;
using System.Collections.Generic;

public enum UIPanelType { MainMenu, GameHUD, Inventory, Skills, Map, Pause, GameOver, Dialog }

/// <summary>
/// UI 面板切换器（重构后）
/// 职责：仅负责顶层面板的显示/隐藏与快捷键切换。
/// HUD 数值显示已下沉到各专用 HUD 组件（事件驱动，订阅 GlobalEventManager），
/// UIManager 不再持有 healthSlider/manaSlider/burdenSlider/levelText 等 HUD 字段，也不再每帧轮询。
/// </summary>
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

    [Header("引用（仅用于面板内列表刷新，不再用于 HUD 数值）")]
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
        // 不再每帧 UpdateHUD()；HUD 由各专用组件事件驱动刷新
        HandleInput();
    }

    void InitializeReferences()
    {
        if (inventorySystem == null) inventorySystem = FindObjectOfType<InventorySystem>();
        if (mapSystem == null) mapSystem = FindObjectOfType<IntegratedMapSystem>();
    }

    void InitializeUI()
    {
        SetAllPanelsActive(false);
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

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.I)) ToggleInventory();
        else if (Input.GetKeyDown(KeyCode.C)) ToggleSkills();
        else if (Input.GetKeyDown(KeyCode.M)) ToggleMap();
        else if (Input.GetKeyDown(KeyCode.Escape)) TogglePause();
    }

    public void ShowPanel(UIPanelType panelType)
    {
        SetAllPanelsActive(false);

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
        if (currentPanel == UIPanelType.Inventory) ShowPanel(UIPanelType.GameHUD);
        else ShowPanel(UIPanelType.Inventory);
    }

    public void ToggleSkills()
    {
        if (currentPanel == UIPanelType.Skills) ShowPanel(UIPanelType.GameHUD);
        else ShowPanel(UIPanelType.Skills);
    }

    public void ToggleMap()
    {
        if (currentPanel == UIPanelType.Map) ShowPanel(UIPanelType.GameHUD);
        else ShowPanel(UIPanelType.Map);
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
        // 背包列表刷新交由 InventoryUI 自身管理，此处保留空实现以兼容旧调用
    }

    void UpdateMapUI()
    {
        // 地图面板刷新交由 Map 面板自身管理
    }

    public void StartGame()
    {
        ShowPanel(UIPanelType.GameHUD);
        Debug.Log("游戏开始！");

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

    public void ShowGameOver()
    {
        ShowPanel(UIPanelType.GameOver);
        Time.timeScale = 0f;
        Debug.Log("游戏结束！");
    }
}
