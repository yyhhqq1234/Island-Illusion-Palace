using UnityEngine;

public class PlayerController : MonoBehaviour, IDashProvider, IDieHandler
{
    [Header("移动参数")]
    public float moveSpeed = 5f;
    public float runSpeed = 8f;
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    public float rotationSmoothing = 0.1f;

    [Header("组件引用")]
    public Rigidbody2D rb;
    public Animator anim;
    public Camera playerCamera;
    public WeaponSystem weaponSystem;
    public SummonSystem summonSystem;
    public SummonWheelUI summonWheelUI;
    public CharacterStats characterStats;
    public BurdenSystem burdenSystem;
    public InventorySystem inventorySystem;

    [Header("调试选项")]
    public bool showDebugInfo = false;

    [Header("外观设置")]
    [Tooltip("玩家缩放倍率")]
    public float scaleMultiplier = 1.0f;

    // 依赖注入字段（由PlayerControllerDependencyInjector注入）
    private PlayerSpawnManager spawnManager;
    private InventoryUI injectedInventoryUI;
    private AlchemyUI injectedAlchemyUI;
    private PauseMenu injectedPauseMenu;

    private Vector2 movement;
    private Vector2 currentDirection;
    private Vector2 targetDirection;
    private bool isMoving = false;
    private bool isRunning = false;
    private bool isDashing = false;
    bool IDashProvider.isDashing => isDashing;
    float IDashProvider.dashSpeed => dashSpeed;
    float IDashProvider.dashDuration => dashDuration;
    float IDashProvider.dashCooldown => dashCooldown;

    void IDieHandler.OnDie()
    {
        Debug.LogWarning("[PlayerController] 玩家死亡");
        enabled = false;
        if (TryGetComponent<Rigidbody2D>(out var rbDie))
            rbDie.velocity = Vector2.zero;
        Invoke(nameof(Respawn), 2f);
    }

    void Respawn()
    {
        // 传送到安全区生成点（保留所有状态）— 使用注入的依赖，降级到查找
        PlayerSpawnManager activeSpawnMgr = spawnManager;
        if (activeSpawnMgr == null)
        {
            activeSpawnMgr = FindObjectOfType<PlayerSpawnManager>();
            if (activeSpawnMgr != null)
                Debug.LogWarning("[PlayerController] 依赖注入未就绪，降级查找PlayerSpawnManager");
        }

        if (activeSpawnMgr != null)
            activeSpawnMgr.MovePlayerToSafeRoom(gameObject);
        else
            transform.position = Vector3.zero;

        // 满血复活
        var hs = GetComponent<HealthSystem>();
        if (hs != null) hs.currentHealth = hs.maxHealth;

        // 重置所有Boss房间
        var bossRooms = FindObjectsOfType<BossRoomManager>();
        foreach (var room in bossRooms)
            room.ResetBossRoom();

        enabled = true;
        moveSpeed = originalMoveSpeed;
        runSpeed = originalRunSpeed;
        Debug.LogWarning($"[PlayerController] 已在安全区复活 位置={transform.position}，Boss房间已重置");
    }

    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;

    private bool summonWheelActive = false;

    // 速度保护：防止 Boss 减速等外部修改造成数值错乱
    private float originalMoveSpeed;
    private float originalRunSpeed;
    private int activeSlowCount = 0;
    private int playerLayer, enemyLayer; // Dash 无敌用缓存

    // ── 缓存引用（避免每帧 FindObjectOfType） ──
    private InventoryUI cachedInventoryUI;
    private AlchemyUI cachedAlchemyUI;
    private PauseMenu cachedPauseMenu;

    void Start()
    {
        // 初始化组件引用
        if (playerCamera == null) playerCamera = Camera.main;
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (rb != null) { rb.freezeRotation = true; rb.mass = 10f; } // 适中质量：可推敌人但费劲
        if (anim == null) anim = GetComponent<Animator>();
        if (weaponSystem == null)
        {
            weaponSystem = GetComponent<WeaponSystem>() ?? transform.Find("Weapon")?.GetComponent<WeaponSystem>();
        }
        if (summonSystem == null)
        {
            summonSystem = GetComponent<SummonSystem>();
        }
        if (summonWheelUI == null)
        {
            // 组件与 PlayerController 同挂 Player 根节点，先取同物体组件；
            // 注意不能用 FindObjectOfType 找未激活对象（轮盘旧版 Awake 会自隐藏）
            summonWheelUI = GetComponent<SummonWheelUI>();
            if (summonWheelUI == null) summonWheelUI = FindObjectOfType<SummonWheelUI>();
        }
        if (characterStats == null) characterStats = GetComponent<CharacterStats>();
        if (burdenSystem == null) burdenSystem = GetComponent<BurdenSystem>();
        if (inventorySystem == null)
        {
            inventorySystem = GetComponent<InventorySystem>() ?? transform.Find("Inventory")?.GetComponent<InventorySystem>();
        }

        // 缓存 UI 引用（一次性查找）
        cachedInventoryUI = FindObjectOfType<InventoryUI>();
        cachedAlchemyUI = FindObjectOfType<AlchemyUI>();
        cachedPauseMenu = FindObjectOfType<PauseMenu>();

        // 保存原始速度（用于 Boss 减速恢复）
        originalMoveSpeed = moveSpeed;
        originalRunSpeed = runSpeed;

        // 缓存层索引（Dash 无敌用）
        playerLayer = LayerMask.NameToLayer("Player");
        enemyLayer = LayerMask.NameToLayer("Enemy");

        // 初始化方向
        currentDirection = targetDirection = Vector2.down;

        // 设置玩家初始大小
        transform.localScale = Vector3.one * scaleMultiplier;

        // 验证关键组件
        if (rb == null)
        {
            Debug.LogWarning("PlayerController: Rigidbody2D component not found!");
        }

        // 如果依赖注入未完成，尝试自动注入（降级保护）
        if (spawnManager == null)
        {
            PlayerControllerDependencyInjector.InjectToController(this);
        }
    }

    /// <summary>
    /// 设置依赖项 - 由PlayerControllerDependencyInjector调用
    /// </summary>
    public void SetDependencies(
        PlayerSpawnManager spawnMgr,
        InventoryUI invUI,
        AlchemyUI alcUI,
        PauseMenu pauseMenu)
    {
        spawnManager = spawnMgr;
        injectedInventoryUI = invUI;
        injectedAlchemyUI = alcUI;
        injectedPauseMenu = pauseMenu;

        // 同步更新缓存，确保Update()在注入完成后立即生效
        if (invUI != null) cachedInventoryUI = invUI;
        if (alcUI != null) cachedAlchemyUI = alcUI;
        if (pauseMenu != null) cachedPauseMenu = pauseMenu;

        Debug.Log("[PlayerController] 依赖项已设置");
    }

    void Update()
    {
        // 防止意外禁用
        if (!enabled) { enabled = true; Debug.LogWarning("[PlayerController] 自动恢复 enabled"); }

        // 兜底：轮盘开启期间被暂停/面板遮挡 → 强制关闭轮盘，防止 KeyUp 丢失导致轮盘卡死
        // 场景：按住 R 开轮盘 → ESC 暂停/背包/炼金面板 → Update() 早退 → 松 R 的 KeyUp 未处理
        // → summonWheelActive 永久 true → WheelRoot (sortOrder=400) 盖在暂停菜单上方
        if (summonWheelActive && !Input.GetKey(IIPConstants.KeySummonWheel))
        {
            summonWheelActive = false;
            if (summonWheelUI != null) summonWheelUI.HideWheel();
        }

        // UI状态检查：优先使用注入依赖，降级到缓存
        PauseMenu activePauseMenu = injectedPauseMenu != null ? injectedPauseMenu : cachedPauseMenu;
        InventoryUI activeInventoryUI = injectedInventoryUI != null ? injectedInventoryUI : cachedInventoryUI;
        AlchemyUI activeAlchemyUI = injectedAlchemyUI != null ? injectedAlchemyUI : cachedAlchemyUI;

        // 暂停时屏蔽所有游戏输入（但允许 UI 自身的按键处理，如 ESC 关暂停菜单）
        if (activePauseMenu != null && activePauseMenu.IsPaused()) return;

        // 背包/炼金面板打开时屏蔽游戏输入，但仍允许切换开关键（I/E）通过，以便再按一次关闭面板
        bool inventoryOpen = activeInventoryUI != null && activeInventoryUI.IsInventoryOpen();
        bool alchemyOpen = activeAlchemyUI != null && activeAlchemyUI.IsAlchemyPanelOpen();
        if (inventoryOpen || alchemyOpen)
        {
            // 面板打开时只处理切换键，跳过其余游戏输入
            if (inventoryOpen && Input.GetKeyDown(KeyCode.I))
            {
                activeInventoryUI.HideInventory();
            }
            else if (alchemyOpen && Input.GetKeyDown(KeyCode.E) && activeAlchemyUI.PlayerInCauldronRange)
            {
                activeAlchemyUI.HideAlchemyPanel();
            }
            return;
        }

        HandleMouseInput();
        HandleKeyboardInput();
        HandleDashInput();
        HandleRunInput();
        HandleInteractionInput();
        HandleSummonWheelInput();
        HandleQuickSummonInput();
        HandleRecallAllInput();
        HandleQuickItemInput();
        UpdateAnimation();
        UpdateTimers();

        if (showDebugInfo)
        {
            Debug.Log($"移动状态: 移动{isMoving}, 奔跑{isRunning}, 冲刺{isDashing}, 方向{currentDirection}");
        }
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    void UpdateTimers()
    {
        if (dashTimer > 0 && (dashTimer -= Time.deltaTime) <= 0) isDashing = false;
        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
            // 实时广播冷却剩余进度给 HUD（每帧）
            if (GlobalEventManager.Instance != null)
                GlobalEventManager.Instance.TriggerDashCooldownChanged(Mathf.Max(0f, dashCooldownTimer), dashCooldown);
        }
    }

    void HandleMouseInput()
    {
        if (playerCamera == null) return;

        Vector3 mouseWorldPosition = playerCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.z = 0f;

        Vector2 directionToMouse = (mouseWorldPosition - transform.position).normalized;
        targetDirection = GetDiscreteDirection(directionToMouse);

        if (targetDirection != Vector2.zero)
        {
            currentDirection = Vector2.Lerp(currentDirection, targetDirection, rotationSmoothing);
            if (currentDirection.magnitude > 1f) currentDirection.Normalize();
        }
    }

    Vector2 GetDiscreteDirection(Vector2 direction)
    {
        if (direction.magnitude < 0.1f)
            return currentDirection;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (angle < 0) angle += 360;

        if (angle >= 315f || angle < 45f)
            return Vector2.right;
        else if (angle >= 45f && angle < 135f)
            return Vector2.up;
        else if (angle >= 135f && angle < 225f)
            return Vector2.left;
        else
            return Vector2.down;
    }

    void HandleKeyboardInput()
    {
        float horizontal = 0f;
        float vertical = 0f;

        if (Input.GetKey(IIPConstants.KeyMoveUp) || Input.GetKey(IIPConstants.KeyMoveUpAlt)) vertical = 1f;
        else if (Input.GetKey(IIPConstants.KeyMoveDown) || Input.GetKey(IIPConstants.KeyMoveDownAlt)) vertical = -1f;

        if (Input.GetKey(IIPConstants.KeyMoveRight) || Input.GetKey(IIPConstants.KeyMoveRightAlt)) horizontal = 1f;
        else if (Input.GetKey(IIPConstants.KeyMoveLeft) || Input.GetKey(IIPConstants.KeyMoveLeftAlt)) horizontal = -1f;

        movement = new Vector2(horizontal, vertical);
        isMoving = movement.magnitude > 0.1f;
    }

    void HandleRunInput()
    {
        isRunning = (Input.GetKey(IIPConstants.KeyRun) || Input.GetKey(IIPConstants.KeyRunAlt)) && isMoving && !isDashing;
    }

    void HandleDashInput()
    {
        if (Input.GetKeyDown(IIPConstants.KeyDash) && !isDashing && dashCooldownTimer <= 0 && isMoving)
        {
            StartDash();
        }
    }

    void HandleInteractionInput()
    {
        if (Input.GetKeyDown(IIPConstants.KeyInteract))
        {
            // E键单点分发：炼金锅范围内由 PlayerController 统一开关炼金面板
            // （AlchemyUI 不再自检E键，避免脚本执行顺序漂移导致双重处理）；
            // 范围外才走常规交互（拾取等）。
            InventoryUI activeInventoryUI = injectedInventoryUI != null ? injectedInventoryUI : cachedInventoryUI;
            AlchemyUI activeAlchemyUI = injectedAlchemyUI != null ? injectedAlchemyUI : cachedAlchemyUI;
            bool alchemyHandlesE = activeAlchemyUI != null && activeAlchemyUI.PlayerInCauldronRange;
            if (alchemyHandlesE)
            {
                if (activeAlchemyUI.IsAlchemyPanelOpen())
                    activeAlchemyUI.HideAlchemyPanel();
                else
                    activeAlchemyUI.ShowAlchemyPanel();
            }
            else
            {
                TryInteract();
            }
        }

        if (Input.GetKeyDown(IIPConstants.KeyInventory))
        {
            OpenInventory();
        }
    }

    void OpenInventory()
    {
        InventoryUI activeInventoryUI = injectedInventoryUI != null ? injectedInventoryUI : cachedInventoryUI;
        if (activeInventoryUI != null)
        {
            if (activeInventoryUI.IsInventoryOpen())
                activeInventoryUI.HideInventory();
            else
                activeInventoryUI.ShowInventory();
        }
        else
        {
            Debug.LogWarning("[PlayerController] InventoryUI 未初始化，无法打开背包");
        }
    }

    void HandleSummonWheelInput()
    {
        if (Input.GetKeyDown(IIPConstants.KeySummonWheel))
        {
            summonWheelActive = true;
            if (summonWheelUI != null)
            {
                summonWheelUI.ShowWheel();
                summonWheelUI.RefreshDisplay();
            }
        }

        if (Input.GetKeyUp(IIPConstants.KeySummonWheel) && summonWheelActive)
        {
            summonWheelActive = false;
            if (summonWheelUI != null)
            {
                int selectedSlot = summonWheelUI.GetSelectedSlot();
                if (selectedSlot >= 0 && summonSystem != null)
                {
                    Vector2 wheelDirection = GetWheelDirectionFromSlot(selectedSlot);
                    summonSystem.SelectSummonFromWheel(wheelDirection);
                }
                // 松开 R 必定收起轮盘（修复旧版选中召唤后轮盘逻辑上常开不收的问题）
                summonWheelUI.HideWheel();
            }
        }
    }

    Vector2 GetWheelDirectionFromSlot(int slotIndex)
    {
        return slotIndex switch
        {
            0 => Vector2.up,
            1 => Vector2.left,
            2 => Vector2.right,
            _ => Vector2.up
        };
    }

    void HandleQuickSummonInput()
    {
        if (Input.GetKeyDown(IIPConstants.KeyQuickSummon))
        {
            if (summonSystem != null)
            {
                summonSystem.QuickSummon();
            }
        }
    }

    void HandleRecallAllInput()
    {
        if (Input.GetKeyDown(IIPConstants.KeyRecallAll) || Input.GetKeyDown(IIPConstants.KeyRecallAllAlt))
        {
            if (summonSystem != null)
            {
                summonSystem.RecallAllSummons();
            }
        }
    }

    void HandleQuickItemInput()
    {
        if (Input.GetKeyDown(IIPConstants.KeyQuickItem1))
        {
            UseQuickItem(0);
        }
        else if (Input.GetKeyDown(IIPConstants.KeyQuickItem2))
        {
            UseQuickItem(1);
        }
        else if (Input.GetKeyDown(IIPConstants.KeyQuickItem3))
        {
            UseQuickItem(2);
        }
    }

    Vector2 GetWheelDirection()
    {
        if (playerCamera == null) return Vector2.up;

        Vector3 mouseWorldPos = playerCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;
        Vector2 direction = (mouseWorldPos - transform.position).normalized;

        if (direction.y > 0.5f) return Vector2.up;
        if (direction.y < -0.5f) return Vector2.down;
        if (direction.x > 0.5f) return Vector2.right;
        return Vector2.left;
    }

    void TryInteract()
    {
        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(transform.position, 2f);
        foreach (Collider2D collider in nearbyColliders)
        {
            IInteractable interactable = collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactable.Interact(gameObject);
                return;
            }
        }
    }

    void UseQuickItem(int slotIndex)
    {
        Debug.Log($"使用快捷物品槽 {slotIndex + 1}");
    }

    void HandleMovement()
    {
        if (rb == null || !enabled) return;

        // 速度自动修复
        if (moveSpeed <= 0f) moveSpeed = originalMoveSpeed;
        if (runSpeed <= 0f) runSpeed = originalRunSpeed;

        if (isDashing)
        {
            Vector2 dashDirection = movement.sqrMagnitude > 0 ? movement.normalized : (currentDirection != Vector2.zero ? currentDirection : Vector2.down);
            rb.velocity = dashDirection * dashSpeed;

            if (playerLayer >= 0 && enemyLayer >= 0)
            {
                if (dashTimer >= dashDuration - 0.02f)
                    Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);
                else if (dashTimer <= 0.02f)
                    Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false);
            }
        }
        else if (isMoving)
        {
            rb.velocity = movement * (isRunning ? runSpeed : moveSpeed);
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
    }

    void HandleDashingMovement() { /* moved inline */ }

    public void StartDash()
    {
        isDashing = true;
        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;
        if (anim != null) SetAnimatorTrigger("Dodge");
        Debug.Log("玩家冲刺！");
        // 广播闪避冷却开始给 HUD
        if (GlobalEventManager.Instance != null)
            GlobalEventManager.Instance.TriggerDashCooldownChanged(dashCooldown, dashCooldown);
        // 冲刺消耗负担
        if (burdenSystem != null)
        {
            burdenSystem.AddBurden(2f);
        }
    }

    void UpdateAnimation()
    {
        if (anim == null) return;

        SetAnimatorBool("IsMoving", isMoving);
        SetAnimatorBool("IsRunning", isRunning);
        SetAnimatorFloat("LastMoveX", currentDirection.x);
        SetAnimatorFloat("LastMoveY", currentDirection.y);

        if (isMoving && !isDashing)
        {
            SetAnimatorFloat("MoveX", movement.x);
            SetAnimatorFloat("MoveY", movement.y);
        }

        if (showDebugInfo)
        {
            Debug.Log($"动画参数: 移动({movement.x},{movement.y}), 朝向({currentDirection.x},{currentDirection.y})");
        }
    }

    // 动画参数设置方法
    void SetAnimatorBool(string paramName, bool value)
    {
        if (anim != null)
        {
            try
            {
                anim.SetBool(paramName, value);
            }
            catch (System.Exception e)
            {
                if (showDebugInfo)
                {
                    Debug.LogWarning($"SetAnimatorBool: {e.Message}");
                }
            }
        }
    }

    void SetAnimatorFloat(string paramName, float value)
    {
        if (anim != null)
        {
            try
            {
                anim.SetFloat(paramName, value);
            }
            catch (System.Exception e)
            {
                if (showDebugInfo)
                {
                    Debug.LogWarning($"SetAnimatorFloat: {e.Message}");
                }
            }
        }
    }

    void SetAnimatorTrigger(string paramName)
    {
        if (anim != null)
        {
            try
            {
                anim.SetTrigger(paramName);
            }
            catch (System.Exception e)
            {
                if (showDebugInfo)
                {
                    Debug.LogWarning($"SetAnimatorTrigger: {e.Message}");
                }
            }
        }
    }

    public bool IsCurrentlyDashing() => isDashing;
    public float GetDashCooldownProgress() => dashCooldownTimer / dashCooldown;
    public Vector2 GetCurrentDirection() => currentDirection;
    public bool IsMoving() => isMoving;
    public bool IsRunning() => isRunning;

    /// <summary>受击或其他系统设置无敌</summary>
    public void SetInvulnerable(bool invulnerable)
    {
        if (playerLayer < 0 || enemyLayer < 0) return;
        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, invulnerable);
    }

    /// <summary>应用减速效果（Boss/环境）— 防止叠加错乱</summary>
    public void ApplySlow(float multiplier, float duration)
    {
        if (multiplier >= 1f || multiplier <= 0f) return;
        activeSlowCount++;
        moveSpeed = Mathf.Max(originalMoveSpeed * multiplier, 1f); // 最低 1f
        runSpeed = Mathf.Max(originalRunSpeed * multiplier, 1.5f);
        int thisSlow = activeSlowCount;
        StartCoroutine(RemoveSlowAfterDelay(duration, thisSlow));
    }

    System.Collections.IEnumerator RemoveSlowAfterDelay(float duration, int slowId)
    {
        yield return new WaitForSeconds(duration);
        if (slowId == activeSlowCount)
        {
            moveSpeed = originalMoveSpeed;
            runSpeed = originalRunSpeed;
            activeSlowCount = 0;
        }
    }

    // 调整玩家大小的方法
    public void SetScale(float newScale)
    {
        scaleMultiplier = newScale;
        transform.localScale = Vector3.one * scaleMultiplier;
        Debug.Log($"玩家大小已调整为 {scaleMultiplier} 倍");
    }

    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying || !showDebugInfo) return;

        if (playerCamera != null)
        {
            Vector3 mouseWorldPosition = playerCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPosition.z = 0f;

            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, mouseWorldPosition);

            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, currentDirection * 2f);

            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, targetDirection * 1.5f);

            if (isMoving)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(transform.position, movement * 1f);
            }

            if (isDashing)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(transform.position, 0.5f);
            }
        }
    }
}

public interface IInteractable
{
    void Interact(GameObject interactor);
}
