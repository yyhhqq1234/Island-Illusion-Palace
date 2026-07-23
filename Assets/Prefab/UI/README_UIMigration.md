# UI 跨场景迁移指南（暂停菜单 / HUD / 背包 / 炼金）

> 适用场景：新建游戏地图场景时，让 暂停菜单 + 全部 HUD + 背包/炼金面板 开箱即用。
> 验证基准：`Assets/Scenes/UITransferTest.unity`（最小迁移验证场景，可直接参考）。
> 最后更新：2026-07-24

## 一、新场景需要拖入的预制体（共 4 个）

| 预制体 | 内容 | 说明 |
|---|---|---|
| `Assets/Prefab/UI/HUDCanvas.prefab` | 全部 HUD（HP/MP、负担、经验、快捷栏、小地图、武器图标、召唤状态、区域名）+ **内嵌 InventoryPanel / AlchemyPanel 嵌套预制体** | 自带 Canvas/CanvasScaler/GraphicRaycaster，根上挂 `EventSystemEnsurer` |
| `Assets/Prefab/UI/PauseMenu.prefab` | 暂停菜单 + 内嵌设置面板 + 确认对话框 | 自带 Canvas，根上挂 `EventSystemEnsurer` |
| `Assets/Prefab/UI/InventoryManager.prefab` | InventorySystem + InventoryUI | 普通 GameObject（非 Canvas），放场景根即可 |
| `Assets/Prefab/UI/AlchemyManager.prefab` | AlchemySystem + AlchemyUI | 同上 |

## 二、必需的场景组件

| 组件 | 是否必须 | 说明 |
|---|---|---|
| EventSystem | **无需手工添加** | HUDCanvas / PauseMenu 根上的 `EventSystemEnsurer` 会在 Awake 自动创建（幂等，已有则不重复建） |
| Main Camera | 必须（正交） | 任何地图场景本来就需要；Screen Space - Overlay 的 UI 不依赖它渲染 |
| InventoryManager / AlchemyManager | 见下 | 不用背包/炼金的纯战斗场景可省略；PauseMenu 的 ESC 优先级判断会自动判空跳过 |

## 三、引用绑定方式（迁移安全性设计）

- **面板引用**：InventoryUI.inventoryPanel / AlchemyUI.alchemyPanel 在预制体中为 null，
  Start 时按名字自动查找场景中的 `InventoryPanel` / `AlchemyPanel`（含未激活节点，HUDCanvas 内嵌的那两个），
  并输出一条 Warning 提示。属设计内行为，无需手工绑定。
- **系统组件引用**：所有 HUD 脚本（HealthManaUI/BurdenWarningUI/PlayerLevelUI/QuickSlotUI/
  MinimapUI/WeaponIconUI/SummonStatusUI/AreaNameUI）均通过 `FindObjectOfType` / 单例 /
  `GlobalEventManager` 事件总线获取外部系统（HealthSystem、CharacterStats 等），
  **无任何序列化场景引用**，预制体拖入即自动绑定。
- **玩家系统缺失时**：新场景还没有 Player 时，HUD 会输出一次性 Warning（如
  "[HealthManaUI] Start 时未找到 HealthSystem，将在 Update 中延迟补拉"），
  玩家生成后自动补拉，不会报错也不会刷屏。

## 四、注意事项

1. **不要重复拖面板**：HUDCanvas 预制体内部已嵌套 InventoryPanel / AlchemyPanel，
   不需要（也不允许）再单独拖 `InventoryPanel.prefab` / `AlchemyPanel.prefab` 进场景，
   否则会出现同名面板重复（历史遗留问题，Wasteland 场景已于 2026-07-24 清理）。
   这两个面板预制体只作为 HUDCanvas 的嵌套源存在。
2. **暂停菜单依赖**：PauseMenu 的"返回主菜单"按钮走 `IIPBootstrap.SwitchToScene`，
   新场景需保证主菜单场景在 Build Settings 中（SceneMainMenu 常量）。
   ESC 键优先级：背包/炼金打开时 ESC 先关它们（找不到这两个 UI 时自动跳过，不报错）。
3. **设置面板**：内嵌在 PauseMenu 预制体中，首开与切 Tab 均自动置顶
   （SettingsPanelController.ResetContentToTop + 延迟一帧兜底）。
4. **Time.timeScale**：背包/炼金/暂停菜单都会操作 timeScale（打开=0，关闭=1），
   测试时注意退出 Play 前恢复，避免残留到编辑模式。
5. **字体**：IIPUIFont 在 OS 微软雅黑与 Resources/UI/MainFont 间自动回退，
   警告"IIPUIFont 未找到内置字体"属已知提示，不影响功能。

## 五、迁移验证清单（参考 UITransferTest.unity）

1. 新建场景 → 拖入上述 4 个预制体 + 一个 Orthographic Camera
2. 进 PlayMode：
   - Console 无 NullReferenceException / MissingReferenceException
   - 出现 Warning "[InventoryUI]/[AlchemyUI] …未绑定，已按名字自动查找"（预期行为）
3. HUD 显示正常（左上 HP/MP/负担条、右上小地图、右下快捷栏）
4. ESC 打开暂停菜单 → 设置 → 条目从顶部排起 → 返回 → 恢复游戏
