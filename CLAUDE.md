# CLAUDE.md

## Project Overview

**幻宫：时空回响** (Island: Illusion Palace) is a 2D top-down Roguelite Action RPG built in Unity, combining real-time combat, soul summoning, alchemy crafting, and memory fragment narrative. Set in the ISLAND universe (ISLAND1 "if" timeline), the player explores procedurally generated maps across multiple cycles to collect Shana's soul fragments and determine the ending.

- **Engine**: Unity (2D, top-down perspective, XY plane)
- **Language**: C#
- **Platform**: PC (Steam)
- **Genre**: 2D Roguelite Action RPG + Strategy Alchemy + Soul Summoning

## Build & Run

- Open the project in Unity Editor at `d:\Program Files\Unity\U3Dproject\Island-Illusion-Palace`
- The `GamePlay` scene is the main gameplay scene (`Assets/Scenes/GamePlay.unity`)
- The `MainMenu` scene is the title screen (`Assets/Scenes/MainMenu.unity`)
- Build target: Windows (StandaloneWindows64)
- Use `manage_editor` MCP tool for play/pause/stop operations

---

## AGENTS 多角色策划系统 (Solo Mode)

本系统基于 **AGENTS**（AI Game design assistaNT TEam System）框架，将游戏策划工作拆解为 8 个专业角色。在 Solo 模式下，Agent 需要根据任务类型**自主切换角色思维**，以对应角色的专业视角完成工作。

> 完整提示词参考：[ai_agents_prompts/](./ai_agents_prompts/00_AGENTS_团队总览与协作协议.md) | 详细工作流见 [AGENTS.md](./AGENTS.md)
>
> **强制要求**：在 Solo 模式下，每次执行任务前，Agent 必须先读取以下两个核心文件以建立角色认知与协作上下文：
> 1. [ai_agents_prompts/00_AGENTS_团队总览与协作协议.md](./ai_agents_prompts/00_AGENTS_团队总览与协作协议.md) — 团队架构、决策模式、沟通协议
> 2. [ai_agents_prompts/00_主策划AI_Lead_Designer.md](./ai_agents_prompts/00_主策划AI_Lead_Designer.md) — 主策划角色定位、职责边界、评审体系

### 角色速查表

| # | 角色 | 一句话定位 | 何时切换到此角色 |
|---|------|-----------|-----------------|
| 0 | **主策划AI** (Lead Designer) | 统筹全局，最终决策 | 需要做方向性判断、审核方案、制定里程碑 |
| 1 | **系统策划AI** (System Designer) | 底层规则、模块架构 | 设计/修改玩法系统、功能架构、交互逻辑 |
| 2 | **数值策划AI** (Numerical Designer) | 数学模型、平衡验证 | 涉及数值表、伤害公式、成长曲线、经济循环 |
| 3 | **文案/剧情策划AI** (Narrative Designer) | 世界观、故事、文本 | 写剧情、角色对话、任务文本、世界观设定 |
| 4 | **关卡策划AI** (Level Designer) | 地图布局、难度节奏 | 设计地图空间、刷怪配置、难度曲线 |
| 5 | **美术风格AI** (Art Style) | 视觉语言、色彩光影、ComfyUI 资产生成 | 定义美术方向、配色方案、风格参考、ComfyUI 资产生成 |
| 6 | **UI/UX策划AI** (UI/UX Designer) | 界面布局、交互流程 | 设计 UI 线框、交互逻辑、用户体验 |
| 7 | **运营策略AI** (Operation Strategy) | 商业化、留存、活动 | 设计付费模型、活动排期、用户激励 |

### Solo 模式使用规则

1. **任务前必读核心文档**：每次执行任务前，必须先读取 `00_AGENTS_团队总览与协作协议.md` 和 `00_主策划AI_Lead_Designer.md` 以建立角色认知与协作上下文
2. **单任务单角色**：每个具体任务对应一个主角色，以该角色的思维框架输出
3. **主策划统筹**：涉及多系统协调、最终决策时，以主策划视角汇总
4. **角色切换显式声明**：切换角色时在回复开头标注 `[角色：XXX]`
5. **交付物符合角色标准**：每个角色的输出格式见 AGENTS.md 或 ai_agents_prompts/ 中的对应文件
6. **必须委托子Agent**：每一轮对话中，主Agent 不得亲自执行任务，必须将所有任务委托给专业子Agent 执行。识别任务类型后，立即通过 `Skill` 或 `Task` 工具调用对应的子Agent，主Agent 仅负责汇总结果与最终决策，不直接编写代码、修改文档或执行其他具体操作。
   - **例外**：以下内容由主Agent 亲自负责，不委托子Agent：
     - 子Agent 的职责划分与调度规则修改
     - 关键架构文件（`AGENTS.md`、`CLAUDE.md`、`.trae/`、`ai_agents_prompts/`）的维护

### 各角色核心职责卡片

#### 0. 主策划AI — 统筹与决策
- 制定游戏核心愿景、市场定位与长期发展战略
- 设计整体概念、世界观基调与核心玩法循环
- 统筹各子系统设计，审核所有策划方案，做出最终决策
- 七维评审：创新性 / 可玩性 / 技术可行性 / 美术契合度 / 商业化潜力 / 用户体验 / 风险可控性
- 输出：GDD 初稿、核心循环示意图、视觉参考集

#### 1. 系统策划AI — 规则与架构
- 设计角色、战斗、经济、社交、任务等核心系统的底层规则与交互逻辑
- 运用 Source-Pool-Sink 模型构建资源闭环
- 高内聚低耦合、单一职责的模块化设计
- 采用事件驱动架构实现系统间解耦
- 输出：系统设计文档（含功能需求、UI 流程图、数据结构）

#### 2. 数值策划AI — 建模与平衡
- 设计战斗公式、属性成长曲线与伤害计算模型
- 锚定期望战斗时长作为数值体系核心锚点
- 构建并验证经济系统，控制通胀/通缩风险
- 蒙特卡洛模拟、极端值校验、等级碾压测试
- 输出：数值表（Excel）、战力模型脚本、经济平衡报告

#### 3. 文案/剧情策划AI — 叙事与世界观
- 构建文明史、地理、势力、器物等世界观设定
- 撰写主线/支线剧情、角色背景、对话文本及任务说明
- 将叙事转化为可玩任务链，融合玩法与情绪体验
- 输出：世界观设定文档、任务台本、角色档案、UI 文案包

#### 4. 关卡策划AI — 空间与节奏
- 设计地图空间结构、动线规划与拓扑关系（线性/分支/轮辐）
- 控制难度曲线，使挑战与技能匹配于"心流通道"
- 设置兴趣点、引导与奖励机制
- 三步教学法：引入→练习→组合
- 输出：关卡平面图、白盒模型、难度评分表、Beat Chart

#### 5. 美术风格AI — 视觉定义
- 定义造型倾向、色彩搭配、光影分布与材质表现
- 制作情绪板（Mood Board）、色彩板与图形参考集
- 互补色/类似色/三分色等配色原则
- 全权负责 ComfyUI 美术资产生成与提示词管理
- 输出：Mood Board、色彩搭配方案、风格指导手册

#### 6. UI/UX策划AI — 交互与体验
- 设计界面布局、导航结构与操作流程
- 应用网格系统规范信息层级与视觉秩序
- 即时反馈、容错设计、Fitts' Law 等交互原则
- 多平台适配（PC 快捷键、移动端触控、手柄导航）
- 输出：UI 线框图、交互流程图、动效说明文档

#### 7. 运营策略AI — 商业化与留存
- 制定用户获取、留存与付费转化策略
- 设计签到、活跃任务、Battle Pass 等日常激励体系
- 规划版本更新节奏、节日活动与 IP 联动方案
- 监控 LTV、ARPPU 等商业指标
- 输出：活动排期表、商业化方案 PPT、买量策略

### 全周期开发流程（主策划AI 主导）

| 阶段 | 内容 | 主导角色 |
|------|------|---------|
| 前期准备 | 构思核心玩法、纸面原型验证 | 主策划 + 系统 + 文案 |
| 立项规划 | 制定里程碑、确定技术栈 | 主策划 + 运营 |
| 预生产 | 垂直切片、细化 GDD、建立规范 | 全员 + 美术 |
| 正式生产 | 按模块并行推进、迭代测试 | 各子角色协同 |
| 测试优化 | 功能测试、平衡性调整 | 测试 + 主策划 + 数值 |
| 上线运营 | 监控数据、版本更新 | 运营 + 数值 |
| 复盘总结 | 经验沉淀、方法论优化 | 主策划 + 全员 |

### 沟通原则
- **清晰高于文采**：精确、无歧义的语言
- **预期清单驱动**：每项交付物附带功能点、行为逻辑与验收标准
- **结构化交付**：输入输出以结构化文本或配置文件形式提供
- **版本可追溯**：所有文档包含版本记录（日期、修改人、变更摘要）

---

## 智能体调度规则 (Agent Orchestration)

### 每轮对话前置检查
**每次对话开始前**，Agent 必须执行以下前置检查：
1. 使用 `LS` 工具列出 `Assets/I-IP markdown/` 文件夹内容，确认当前可用的策划文档清单
2. 根据任务类型，读取相关策划文档作为上下文参考
3. 如果任务涉及已有策划案修改，必须先读取对应的 markdown 文件全文

### 子Agent 调用规则

Solo Agent 必须根据任务类型，通过 `Task` 工具或 `Skill` 工具调用对应的专业子Agent：

| 任务类型 | 调用方式 | 触发条件 |
|---------|---------|---------|
| **策划案修改** | `Skill` → `AI团队协调` | 任何涉及策划文档内容修改、新增策划案、系统设计变更 |
| **文案/文学创作** | `Skill` → `文学小说创作专家` | 任何涉及世界观构建、剧情写作、角色对话、任务文本、文学润色等文字创作 |
| **代码开发/修改** | `Task` → `unity-topdown-developer` | 任何涉及 Unity C# 代码的新增、修改、重构 |
| **代码审查** | `Task` → `TRAE-code-review` | 代码开发完成后，对所有变更进行审查 |
| **安全审查** | `Task` → `TRAE-security-review` | 涉及用户数据、存档、网络等敏感代码变更 |
| **运行时调试** | `Task` → `TRAE-debugger` | 仅通过静态分析无法诊断的 Bug |

### 策划案修改流程
1. 识别修改范围，确定涉及哪些子系统的策划文档
2. 调用 `AI策划团队` Skill（`AI团队协调`），将策划任务分发给对应的专业角色Agent
3. 各角色Agent 以各自专业视角输出修改方案
4. 主策划AI 汇总各角色意见，进行七维评审
5. 确认无误后，修改对应的 markdown 文件

### 代码开发流程
1. 读取相关策划文档（`I-IP markdown/`）和现有代码
2. 调用 `unity-topdown-developer` Agent 执行代码修改
3. 代码修改完成后，调用 `GetDiagnostics` 检查编译错误
4. **主Agent 全内容审查**：代码开发结束后，主Agent 必须进行全内容审查，包括：
   - 检查所有修改的代码文件，确认代码质量和规范符合性
   - 检查所有修改的策划文档，确认策划案一致性
   - 运行 `GetDiagnostics` 确认无编译错误
   - 输出审查报告，列出所有变更点和潜在风险

### 审查报告模板
```
## 全内容审查报告

### 代码变更审查
- 修改文件列表
- 代码规范检查（命名、注释、事件驱动架构）
- 潜在问题（空引用、性能、逻辑错误）

### 策划文档变更审查
- 修改文件列表
- 策划一致性检查（跨系统数值、叙事逻辑）
- 跨系统影响分析

### 编译状态
- 编译错误数
- 警告信息

### 风险评估
- 风险等级：低 / 中 / 高
- 建议措施
```

---

## Architecture

### Project Structure

```
Assets/
├── Scripts/
│   ├── Alchemy/        # AlchemySystem, AlchemyUI, RecipeType enum
│   ├── Battle/         # BattleSystem, BattleEventManager
│   ├── Boss/           # BossAI, TimeGuardianBoss, BossRoomManager
│   ├── Core/           # HealthSystem, BurdenSystem, CharacterStats, ManaSystem
│   ├── DataAssets/     # ScriptableObjects: DropTableData, WeaponData, etc.
│   ├── Difficulty/     # DifficultyManager
│   ├── Editor/         # Custom inspectors (IntegratedMapSystemEditor, etc.)
│   ├── Ending/         # EndingSystem
│   ├── Enemy/          # EnemyAI, EnemySpawner, BossAI, SummonedCreatureAI
│   ├── Interfaces/     # Service contracts (IAlchemyService, IBurdenProvider, etc.)
│   ├── Inventory/      # InventorySystem, InventoryUI, MaterialItem, ItemSlot
│   ├── Managers/       # Singletons: GlobalEventManager, GameplayAudioManager, GameManager, ResourceManager
│   ├── Map/            # IntegratedMapSystem, SafeZoneDetector, CampfireSystem, CauldronSystem, etc.
│   ├── Memory/         # MemoryFragmentSystem, NarrativeEventSystem
│   ├── Player/         # PlayerController, WeaponSystem, AttackTrigger
│   ├── Portal/         # TimePortal, TimeRift, TimeRiftSpawner
│   ├── SaveLoad/       # SaveSystem
│   ├── Summon/         # SummonSystem, SummonWheelUI, SoulCore
│   ├── Test/           # InventoryTest
│   └── UI/             # UIManager, PauseMenu, SceneController
├── ArtMaterials/       # Sprites and textures organized by category
├── Music/              # Audio organized by type (Battle, Effect, GlobalMusic, MapMusic, UIEffect)
├── Animations/         # Animation assets (Player, Enemies, Map, Effect, Weapon)
└── I-IP markdown/      # Design documents (read-only reference)
```

### Naming Conventions

- **Namespace**: `GameSystems` is used for shared types (enums, configs like `MaterialTypeEnum`, `RecipeType`, `MapMusicType`, `MapMusicConfig`). Most scripts are in the global namespace.
- **Scripts**: PascalCase, one class per file, filename matches class name
- **Enums**: In-class for type-specific enums (e.g., `EnemyAI.EnemyType`), or in the `GameSystems` namespace for shared enums
- **Interfaces**: Prefixed with `I` (e.g., `IAlchemyService`, `IBurdenProvider`)

### Core Design Patterns

1. **Singleton Managers**: `GlobalEventManager`, `GameplayAudioManager`, `ResourceManager`, `GameManager` all follow the `Instance` + `DontDestroyOnLoad` pattern
2. **Event Bus**: `GlobalEventManager` is the central event bus. All cross-system communication goes through it — never directly call methods across systems. Subscribe via `OnEnable`/`OnDisable`, trigger via public `Trigger*` methods
3. **Interface Contracts**: `Interfaces/` folder contains service contracts like `IInventoryService`, `IHealthProvider`, `IBurdenProvider`. Components implement interfaces for loose coupling
4. **ScriptableObject Data**: `DataAssets/` contains `ScriptableObject` assets for data-driven design (drop tables, weapon data, recipe effects)

### Key Event Flow (GlobalEventManager)

```
Scene Lifecycle:  OnSceneWillLoad → OnSceneDidLoad → OnSceneWillUnload
Game State:       OnGamePaused / OnGameResumed / OnPlayerDeath
Safe Zone:        OnPlayerEnterSafeZone / OnPlayerExitSafeZone
Combat:           OnBattleStart / OnBattleEnd / OnBossDefeated / OnBossEncounter
Burden:           OnBurdenChanged / OnBurdenHigh / OnBurdenCritical / OnBurdenCleared
Map:              OnMapTypeChanged (broadcasts MapMusicType)
Music:            OnMusicStateChange (MusicState: MainMenu, Exploration, Battle, BossBattle, Camp, Silence)
```

## Key Systems

### Map System (`IntegratedMapSystem`)
- 5x5 grid of sub-maps (each 39x26 tiles)
- Map types: `MapCategory` (Natural/Human/Special/Final) + `MapType` (Forest, Wasteland, Desert, etc.)
- Generates walls, boss rooms, safe zones, treasure chests
- Broadcasts `OnMapTypeChanged` when a new map generates
- Implements `IMapSystem` and `ITimeRiftProvider`

### Player (`PlayerController`)
- WASD movement, Shift to run, Space to dash (invincibility frames)
- Uses `Rigidbody2D` (Dynamic) — control via velocity, never directly modify Transform
- References: WeaponSystem, SummonSystem, BurdenSystem, InventorySystem, CharacterStats

### Combat
- **WeaponSystem**: Sword/Staff/Scythe/Crystal Armament, each with element type
- **Weakness system**: 7 elements (Frost, Fire, Thunder, Water, Soul, Holy, Physical), ×1.8 weakness, ×0.4 resistance
- **BattleEventManager**: Battle lifecycle events
- **Damage**: AttackTrigger on weapon colliders

### Boss System
- `BossAI` base class → `TimeGuardianBoss` (with time-warp mechanics, slow fields, health regen)
- `BossRoomManager`: Spawns boss, handles defeat rewards (drops + TimePortal)
- TimePortal spawns at boss room center after defeat

### Summon System
- Enemies drop Soul Cores (White/Blue/Gold rarity)
- R key for summon wheel, F for quick summon, Alt to recall all
- Max 3 active summons, 2-second global cooldown
- `SummonSystem` manages slots and cooldowns

### Alchemy System (`AlchemySystem`)
- 24 materials across Basic/Rare/Epic/Legendary tiers (alchemical value 1-40)
- 14 recipes (4 basic known, 10 hidden to discover)
- Priority: match recipes first, then material synthesis, failure returns primes
- Success rate affected by Wisdom, Burden, Memory Fragments

### Burden System (`BurdenSystem`)
- 0-100 range; >70 = high (stat penalties), >90 = critical, 100 = forced teleport to campfire
- Increases from summon use, skill use, memory fragment activation
- Restored by campfires and alchemy items
- `IIPConstants` defines thresholds: `BurdenHighThreshold = 70`, `BurdenCriticalThreshold = 90`

### Audio System (`GameplayAudioManager`)
- Dynamic music switching based on: MapType → BurdenLevel → MusicState
- Music hierarchy: SafeZone > Boss > Battle > Exploration
- `MapMusicConfig` per map type in Inspector, fallback to defaults
- Map-type change triggers music refresh via `OnMapTypeChanged` event

### Portal System
- **TimePortal**: Stable connector, spawns at boss room center after defeat, connects 2-3 specific map types
- **TimeRift**: Unstable random portal, spawns by `TimeRiftSpawner` with configurable probability, lifetime 45s
- **TimeRiftSpawner**: Avoids safe zones, boss rooms, and grid map obstacles when spawning

### Save/Load
- `SaveSystem`: Serializes player state, inventory, map progress
- Multi-cycle progression: up to 30 attribute points carry over between cycles

## Development Rules

### Unity 2D Best Practices
- Always use `Rigidbody2D` + `Collider2D`, control via velocity/forces in `FixedUpdate`
- Z-axis used only for rendering sorting; set Transparency Sort Mode to Custom Axis (0,1,0)
- Camera must be Orthographic; use `manage_camera` tool for adjustments
- Tilemap system preferred for map construction

### Code Style
- C# comments in Chinese are acceptable (team is Chinese-speaking)
- `[Header]` and `[Tooltip]` attributes for Inspector: Header in Chinese, field names in English, Tooltip in Chinese
- Use `Debug.Log` with `[ClassName]` prefix for runtime diagnostics
- Follow single responsibility: one class = one responsibility

### MCP Tool Usage (Unity Editor Interaction)
- Use `mcp_unityMCP_batch_execute` for bulk operations (creating multiple objects, adding components)
- Use `mcp_unityMCP_find_gameobjects` for searching, `mcp_unityMCP_manage_gameobject` for CRUD
- Use `mcp_unityMCP_manage_components` for component management
- Use `mcp_unityMCP_manage_asset` for asset operations
- Use `mcp_unityMCP_execute_code` for ad-hoc C# code execution in the Editor
- Never modify scenes during Play Mode
- Always confirm with user before deleting/overwriting assets

### Design Document Reference
- All game design documents are in `Assets/I-IP markdown/` — 代码实现阶段视为只读参考；策划迭代阶段通过 AI策划团队 进行修改
- When implementing features, cross-reference the relevant system breakdown document
- 代码与策划案冲突时，优先以策划案为准修改代码；如需调整策划案，通过 AI策划团队 流程进行

## Important Constants (`IIPConstants`)
```csharp
MaxActiveSummons = 3
SummonGlobalCooldown = 2f
MaxMemoryFragmentSlots = 3
BurdenHighThreshold = 70f
BurdenCriticalThreshold = 90f
BurdenAbsoluteMax = 100f
MaxEnhancementLevel = 5
NewbieProtectionCount = 3
```

## Tags & Layers
- Tags: `Player`, `Enemy`, `SummonedCreature`, `SafeZone`, `Campfire`
- Layers: `Player`, `Enemy`, `Default`