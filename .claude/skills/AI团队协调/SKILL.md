---
name: AI团队协调
description: AGENTS 策划任务分发入口。当需要修改策划文档、新增策划案、进行系统设计变更、多角色协同策划时调用。读取团队总览与主策划定义，将任务分发给对应专业角色 Agent（lead-designer/system-designer/numerical-designer/narrative-designer/level-designer/art-style/uiux-designer/operation-strategy），由主策划汇总七维评审后输出最终方案。
---

# AI 团队协调 — AGENTS 策划任务分发入口

本 Skill 是 **AGENTS 多角色策划系统** 的任务分发入口。主 Agent 识别到策划类任务后调用本 Skill，由本 Skill 协调 8 个专业角色 Agent 完成策划工作。

## 强制前置阅读

调用本 Skill 后，须先 `Read` 以下两份核心文件建立角色认知：
1. [ai_agents_prompts/00_AGENTS_团队总览与协作协议.md](../../../ai_agents_prompts/00_AGENTS_团队总览与协作协议.md) — 团队架构、决策模式、沟通协议
2. [ai_agents_prompts/00_主策划AI_Lead_Designer.md](../../../ai_agents_prompts/00_主策划AI_Lead_Designer.md) — 主策划角色定位、评审体系

## 可用角色 Agent（.claude/agents/）

| # | Agent 名 | 一句话定位 | 触发条件 |
|---|---------|-----------|---------|
| 0 | `lead-designer` | 主策划，统筹与最终决策 | 方向性判断、审核方案、里程碑 |
| 1 | `system-designer` | 系统策划，底层规则与架构 | 玩法系统、功能架构、交互逻辑 |
| 2 | `numerical-designer` | 数值策划，数学模型与平衡 | 数值表、伤害公式、成长曲线、经济 |
| 3 | `narrative-designer` | 文案/剧情策划，世界观与故事 | 剧情、对话、任务文本、世界观 |
| 4 | `level-designer` | 关卡策划，地图与难度节奏 | 地图空间、刷怪配置、难度曲线 |
| 5 | `art-style` | 美术风格，视觉与 ComfyUI 资产 | 美术方向、配色、ComfyUI 生成 |
| 6 | `uiux-designer` | UI/UX 策划，界面与交互 | UI 线框、交互逻辑、用户体验 |
| 7 | `operation-strategy` | 运营策略，商业化与留存 | 付费模型、活动排期、用户激励 |

> 文学性深度润色（世界观文学化、角色对话精修）额外可调用「文学小说创作专家」（`.trae/skills/文学小说创作专家/`）。

## 分发流程

1. **识别任务范围**：根据任务类型确定主角色与协作角色（见下表）
2. **前置检查**：`LS` 列 `Assets/I-IP markdown/`，`Read` 相关策划文档全文
3. **分发**：通过 `Agent` 工具调用对应角色 Agent，传入任务上下文 + 已读文档摘要
4. **汇总评审**：多角色方案汇集到 `lead-designer` 做七维评审（创新性/可玩性/技术可行性/美术契合度/商业化潜力/用户体验/风险可控性）
5. **落盘**：主 Agent 据最终决策修改对应 markdown 文件
6. **跨系统校验**：若涉数值/逻辑变更，提示主 Agent 同步代码（走 unity-topdown-developer 流程）

## 任务-角色映射

| 任务类型 | 主角色 | 协作角色 |
|---------|--------|---------|
| 新增/修改玩法系统 | system-designer | numerical / level / uiux |
| 战斗/经济数值平衡 | numerical-designer | system / operation |
| 剧情/世界观/任务文本 | narrative-designer | art / uiux |
| 地图/关卡/刷怪配置 | level-designer | system / numerical / art |
| 美术方向/ComfyUI 资产 | art-style | narrative / uiux |
| UI 线框/交互流程 | uiux-designer | system / art |
| 商业化/活动/留存 | operation-strategy | numerical |
| 跨系统重大决策 | lead-designer | 全员辩论 |

## 决策模式

- **自由辩论模式**（重大方向）：lead-designer 提构想 → 各角色挑战 → 多轮攻防 → lead-designer 决策
- **严格串行模式**（模块级）：lead-designer 定边界约束 → 单角色限定范围详规 → lead-designer 评估合规性

## 交付规范

每个角色 Agent 输出须含：
- 结构化文档（按角色模板，见各 agent 定义）
- 「预期表现清单」（功能点/行为逻辑/验收标准）
- 版本记录（日期、修改人、变更摘要）
- 与现有系统交互点 / 跨系统冲突提示

## 原则

- 清晰高于文采，精确无歧义
- 结构化交付，版本可追溯
- 策划文档与代码冲突时以策划案为准，需改策划案走本流程
- 主 Agent 不亲自写策划内容，委托角色 Agent；仅负责汇总与最终决策落盘

## 视觉能力（所有策划角色已内置）

8 个策划角色 Agent 均已具备「看图」能力（通过本地 VL 小模型 `mcp__local-vision__analyze_image`），可主动调用核对实际画面。完整规范见 [.claude/agents/_shared/vision-capability.md](../../agents/_shared/vision-capability.md)。

- **能直接截图的角色**：`uiux-designer` / `art-style` / `level-designer` / `narrative-designer`（有 `script-execute` 权限跑反射读 `m_RenderTexture` 截 GameView）
- **仅看已有图的角色**：`lead-designer` / `system-designer` / `numerical-designer` / `operation-strategy`（需新截图时委托上述角色或 developer/debugger 落盘后传路径）

分发任务时，若涉及「核对实际呈现」，明确提示对应角色主动用视觉能力。各角色会根据专业视角**自行优化传给 VL 模型的 `question`**（具体、可验证、可迭代重问）。
