# AGENTS.md

## Project: 幻宫：时空回响 (Island: Illusion Palace)

A Unity 2D top-down Roguelite Action RPG. Read [CLAUDE.md](./CLAUDE.md) for the full architecture and system reference.

---

## AGENTS 多角色策划系统 (Solo Mode)

本系统基于 **AGENTS**（AI Game design assistaNT TEam System）框架，将游戏策划工作拆解为 8 个专业角色。在 Solo 模式下，你作为单一 Agent 需要根据任务类型**自主切换角色思维**，以对应角色的专业视角完成工作。

完整提示词参考：[ai_agents_prompts/](./ai_agents_prompts/00_AGENTS_团队总览与协作协议.md)

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
5. **交付物符合角色标准**：每个角色的输出格式见下方各角色卡片
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

## Agent Workflow

### Before Any Code Change
1. Read the relevant script(s) with `Read` tool first — never edit code you haven't read
2. Check `Assets/I-IP markdown/` for the design document if the task involves game mechanics
3. Use MCP Tilemap 工具 (`tilemap-list`, `tilemap-get`) to inspect scene state before modifying

### Making Changes
1. **Prefer editing existing files** — do not create new scripts unless absolutely necessary
2. **Use MCP 工具**进行 Unity 编辑器操作：Tilemap 用 `tilemap-*`，Timeline 用 `timeline-*`，Cinemachine 用 `cinemachine-*`
3. **Use `Edit` tool** for code modifications (not shell commands)
4. After code changes, check for compilation errors via `GetDiagnostics`

### After Changes
1. Verify no compilation errors exist
2. If runtime testing is needed, suggest the user enter Play Mode in Unity

## Key Constraints

### DO NOT
- Modify files in `Assets/I-IP markdown/` without explicit design task context — 代码实现阶段视为只读参考；策划迭代阶段通过 AI策划团队 进行修改
- Modify the scene during Play Mode
- Delete or overwrite assets without user confirmation
- Create new ScriptableObject files unless the user explicitly requests it
- Create documentation files (\*.md) unless the user explicitly requests it

### Unity 2D Rules
- Movement must use `Rigidbody2D` velocity, never direct Transform modification
- Z-axis is for rendering sorting only, not gameplay positioning
- Camera must stay orthographic
- Use Tilemap system for map construction when possible

### Code Style
- Inspector fields: English names, `[Header("中文")]` for grouping, `[Tooltip("中文提示")]` for descriptions
- Debug logs: `Debug.Log($"[ClassName] message")` pattern
- Shared types (enums, config classes) go in the `GameSystems` namespace
- Comments can be in Chinese or English — Chinese preferred for team communication

## Event-Driven Communication

All inter-system communication flows through `GlobalEventManager.Instance`:

```
To broadcast:  GlobalEventManager.Instance.TriggerXxx(args)
To listen:     GlobalEventManager.Instance.OnXxx += handler   (in OnEnable)
               GlobalEventManager.Instance.OnXxx -= handler   (in OnDisable)
```

Key events available:
- `OnMapTypeChanged(MapMusicType)` — map type changes
- `OnMusicStateChange(MusicState)` — music state request
- `OnBurdenChanged(float)` — burden value changed
- `OnBattleStart/End(GameObject)` — combat lifecycle
- `OnBossDefeated(GameObject)` — boss killed
- `OnPlayerEnterSafeZone/ExitSafeZone(GameObject)` — safe zone detection

## MCP Tool Quick Reference

项目配置了 **AI Game Developer MCP Server** (`gamedev-mcp-server v8.0.1.0`)，通过 `.mcp.json` 定义，地址 `http://localhost:23035`，提供 Unity 编辑器的 3 大类 ~30+ 个工具。

### Tilemap 工具 (地图编辑)

| 任务 | 工具 |
|------|------|
| 列出/创建 Tilemap | `tilemap-list`, `tilemap-create` |
| 读取/设置单个格子 | `tilemap-get-tile`, `tilemap-set-tile` |
| 区域填充/清除 | `tilemap-box-fill`, `tilemap-clear` |
| 创建 Tile 资产 | `tilemap-create-tile-asset`, `tilemap-create-rule-tile` |
| 设置碰撞/方向/颜色 | `tilemap-set-collider-type`, `tilemap-set-orientation`, `tilemap-set-tile-flags` |
| 通用读写 | `tilemap-get`, `tilemap-modify` |

### Timeline 工具 (动画时间轴)

| 任务 | 工具 |
|------|------|
| 列出/创建 Timeline | `timeline-list`, `timeline-create` |
| 轨道管理 | `timeline-track-add`, `timeline-track-list`, `timeline-track-remove` |
| 绑定对象 | `timeline-track-bind`, `timeline-director-bind` |
| Clip 操作 | `timeline-clip-add`, `timeline-clip-move`, `timeline-clip-set-timing` |
| 标记点 | `timeline-marker-add` |

### Cinemachine 工具 (虚拟相机)

| 任务 | 工具 |
|------|------|
| 列出/创建相机 | `cinemachine-camera-list`, `cinemachine-camera-create` |
| 设置 Body/Aim | `cinemachine-set-body`, `cinemachine-set-aim` |
| 镜头参数 | `cinemachine-set-lens`, `cinemachine-set-noise`, `cinemachine-set-default-blend` |
| 目标绑定 | `cinemachine-set-targets`, `cinemachine-set-priority` |
| 扩展组件 | `cinemachine-add-extension` |
| Brain 确保 | `cinemachine-brain-ensure` |

> 所有工具前缀为 `mcp_ai-game-developer_*`，由 Trae IDE 自动注册。

## Common Task Patterns

### Adding a new game mechanic
1. Read the relevant system breakdown markdown in `I-IP markdown/`
2. Identify which existing script to extend (or if a new one is truly needed)
3. Implement using the event bus pattern — use `GlobalEventManager` for cross-system signals
4. Add `[Header]`/`[Tooltip]` attributes for Inspector visibility
5. Test via `GetDiagnostics` for compilation, then suggest Play Mode testing

### Fixing a bug
1. Read console output via `GetDiagnostics`
2. Read the relevant script files
3. Locate the root cause (null reference, logic error, missing reference)
4. Fix minimally — don't refactor unrelated code
5. Verify with `GetDiagnostics`

### Adding Inspector-configurable fields
- Field name: English (camelCase or PascalCase)
- `[Header("中文分类名")]` for grouping
- `[Tooltip("鼠标悬停时的中文说明")]` for hover tooltip
- Enum values must be in English (not Chinese)

## Music & Map Integration

When working on map/music features:
- Map types are defined in `IntegratedMapSystem.MapType` enum
- Music types are defined in `GameSystems.MapMusicType` enum (in `GameplayAudioManager.cs`)
- `IntegratedMapSystem` broadcasts `OnMapTypeChanged` when a new map generates
- `GameplayAudioManager` listens and switches music accordingly
- Music state priority: SafeZone > Boss > Battle > Exploration
- Music files organized in `Assets/Music/MapMusic/{MapType}/`