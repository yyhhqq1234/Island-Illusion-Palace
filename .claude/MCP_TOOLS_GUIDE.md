# MCP 工具按需启用指南

## 背景

本项目通过 `.mcp.json` 连接 `ai-game-developer` MCP Server（119 个 Unity 编辑器工具）。
工具的 JSON Schema 定义会在**每次对话请求**中随 system prompt 注入主 Agent 上下文，
主模型 `glm_for_coding` 上下文上限 200K token，119 个工具 schema + 944KB skills + 历史消息
容易触发 `API Error: 400 context length exceeds limit 200000`。

## 已落地的减负措施

1. **`.claude/skills/` 下 123 个 MCP 包装型 SKILL.md 已 `git mv` 到 `.claude/skills-disabled/`**
   - 这些 SKILL.md 由 `unity-skill-generate` 自动生成，内容是 MCP 工具 schema 的复述
   - 944KB ≈ 236K token，移走后每个会话固定开销大幅下降
   - MCP 工具本身仍可通过 `mcp__ai-game-developer__*` 直接调用，不依赖 SKILL.md
2. **运行时用 `tool-set-enabled-state` 临时禁用 92 个低频工具**
   - 仅保留 27 个高频工具的 schema 注入（见下表）
   - ⚠️ 此状态存在 Unity 编辑器内存中，**Unity 重启后失效**，需重新禁用

## 当前保留的高频工具（27 个）

| 类别 | 工具 |
|------|------|
| 资产 | `assets-find` `assets-get-data` `assets-refresh` `assets-prefab-open` `assets-prefab-save` `assets-prefab-close` |
| 脚本 | `script-read` `script-execute` `script-update-or-create` |
| GameObject | `gameobject-find` `gameobject-component-get` `gameobject-component-modify` `gameobject-component-add` |
| 场景 | `scene-list-opened` `scene-open` `scene-get-data` `scene-save` |
| 对象 | `object-get-data` `object-modify` |
| 编辑器 | `editor-application-get-state` `editor-application-set-state` |
| 日志 | `console-get-logs` `console-clear-logs` |
| 元工具 | `unity-tool-list` `tool-set-enabled-state` `ping` |

## 按需启用被禁用工具

当任务需要 tilemap / timeline / cinemachine / screenshot / profiler / animation / package 等工具时：

**方法 A：单工具按需启用**（推荐，开销最小）
```
调用 mcp__ai-game-developer__tool-set-enabled-state
  tools: [{ "Name": "tilemap-set-tile", "Enabled": true }]
```

**方法 B：按类别批量启用**（适合某子系统连续操作）
```
# 例如要做地图编辑，启用整个 tilemap 类别
tools: [
  {"Name":"tilemap-create","Enabled":true},
  {"Name":"tilemap-set-tile","Enabled":true},
  {"Name":"tilemap-box-fill","Enabled":true},
  {"Name":"tilemap-list","Enabled":true}
]
```

**方法 C：全量恢复**（调试或不确定时）
```bash
# 在对话中要求 Agent 调用 tool-set-enabled-state 把所有工具设为 true
# 或重启 Unity 编辑器（运行时禁用状态自然丢失）
```

## 工具类别速查（被禁用的 92 个）

- **tilemap-\*** (13)：地图创建/填充/清除/瓦片资产/碰撞体/朝向
- **timeline-\*** (13)：时间轴创建/轨道/片段/标记/绑定
- **cinemachine-\*** (13)：虚拟相机创建/Body/Aim/Lens/Noise/Priority/Targets
- **screenshot-\*** (4)：⚠️ 禁止直接用，见 CLAUDE.md「本地视觉分析」章节
- **profiler-\*** (13)：性能分析器
- **animation-\*** (3) / **animator-\*** (3)：动画与动画控制器
- **assets-\*** (8)：复制/创建文件夹/删除/材质/移动/内置资源/shader
- **gameobject-\*** (6)：创建/销毁/复制/修改/设父/组件销毁/组件列表
- **package-\*** (4)：包管理
- **scene-\*** (3)：创建/设激活/卸载
- **reflection-\*** (2)：反射调用
- **其他**：`tests-run` `type-get-json-schema` `editor-selection-*` `script-delete`

## 重新生成 skills（如需要）

若日后想恢复 `.claude/skills/`（不推荐，会重新吃 236K token）：
```bash
git mv .claude/skills-disabled/* .claude/skills/
```
或调用 MCP `unity-skill-generate` 重新生成（需先启用该工具）。
