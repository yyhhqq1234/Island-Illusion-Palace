# 项目记忆文档

> 此文档记录 AI Agent 与用户的多轮对话关键信息，用于维护跨会话上下文。
> 版本：1 | 最近更新：2026-06-15

---

## 1. 项目概览

- **项目**: 幻宫：时空回响 (Island: Illusion Palace) — Unity 2D top-down Roguelite Action RPG
- **Unity 项目路径**: `d:\Program Files\Unity\U3Dproject\Island-Illusion-Palace`
- **客户端**: `Scripts/comfyui_client.py` (Python GUI, Tkinter)
- **服务器**: `10.150.164.64:8189` (v3.2 统一 API)

## 2. ComfyUI 客户端状态

### 2.1 服务器架构
- **8189 统一入口**: 提供 HTTP REST + WebSocket + 蓝图展平
- **ComfyUI 原生 8188**: 不推荐直接访问，通过 8189 代理
- **SDK**: `comms_client.py` (19835 bytes, 从服务器 GET /client-sdk 获取)
- **工作流总量**: 9 个（5 注册工作流 + 4 蓝图）
- **GPU**: NVIDIA RTX 3060 (12GB VRAM)
- **模型总量**: ~89GB

### 2.2 客户端版本
- `comfyui_client.py` 约 4400+ 行，Tkinter GUI
- `HAS_COMMS_SDK` 由 `from comms_client import CommsClient` 决定
- 配置持久化: `comfyui_client_config.json`

### 2.3 关键 UI 功能
| 区域 | 说明 |
|------|------|
| 工具栏 | 连接状态/VRAM/队列/详情面板 + 4按钮（测试连接/检查队列/中断/释放显存）|
| Tab 1 (生成) | 资产选择/阶段/提示词/参数/生成按钮 |
| Tab 2 (AI) | LLM 配置 + 优化提示词/生成提示词/推荐工作流/自定义对话 |
| Tab 3 (资源) | 服务器资源浏览器（模型/VAE/LoRA/节点）|
| Tab 4 (分析) | 项目扫描 + AI 需求提取 + 资产缺口分析 |
| Tab 5 (工作流) | 工作流编辑器（新建/加载/保存/验证/AI 生成）|

### 2.4 工作流执行流程
```
用户选择资产 → 选择工作流 → 点击生成
  → _do_generate()
    → _get_logical_workflow_id()  # 根据资产/阶段选择工作流
    → _fetch_workflow_params()    # 查询蓝图参数 schema
    → _map_params_to_workflow()   # 映射参数字段名
    → POST /workflows/execute     # 提交到 8189 统一入口
    → _poll_result()              # 轮询 /comfy/queue + /comfy/history
    → _handle_completed()         # 检查错误 → 下载文件 → 保存到本地
```

### 2.5 可用的游戏美术工作流（v3.2 精简版）

| 工作流 | 类型 | 速度 | 参数字段 | 用途 |
|--------|------|------|----------|------|
| `game-concept-art` | 注册 | quality | `prompt`, `negative_prompt`, `seed`, `steps`, `cfg` | 概念图/宣传画（推荐 steps 25-40, cfg 7-10）|
| `game-character-design` | 注册 | quality | `prompt`, `negative_prompt`, `seed`, `steps`, `cfg` | 角色立绘单帧（steps 20-30, cfg 5-8）|
| `game-sprite-sheet` | **注册** | **quality** | `prompt`, `negative_prompt`, `seed`, `steps`, `cfg` | **精灵帧动画/条带**（多帧动态，推荐 steps 20-25, cfg 6-8, prompt 需含帧描述）|
| `game-environment` | 注册 | quality | `prompt`, `negative_prompt`, `seed`, `steps`, `cfg` | 场景/关卡背景/Tile（steps 20-30）|
| `game-item-icon` | 注册 | ultra-fast | `prompt`, `seed` | 道具/图标/UI 元素（仅 prompt+seed，不含步数/cfg）|

蓝图（仅剩 4 个 image-utility）: `image-inpainting`, `image-depth-map`, `image-blur`, `image-sharpen`

### 2.6 提示词双阶段设计
每个 ASSET_TEMPLATES 包含两套提示词：
- `prompt`: 概念图阶段（dark fantasy concept art, digital painting, 单对象居中）
- `sprite_prompt`: 精灵帧阶段（simple pixel art, minimalist game sprite, 纯白背景, 居中）
- `_update_prompts()` 根据 stage 自动选择对应字段


## 3. 当前已知问题

### 3.1 服务器端
- ComfyUI 后端（8188）偶尔挂掉导致 8189 代理返回 502 / 空 prompt_id，需重启 ComfyUI
- `game-item-icon` 工作流仅支持 prompt+seed 参数，不含 steps/cfg 控制


### 3.2 客户端
- 精灵帧 img2img 流程尚未完整测试（需概念图生成成功作为参考底图）

## 4. 近期重要变更

| 日期 | 变更 | 详情 |
|------|------|------|
| 2026-06-15 | 服务器 v3.2 精简重构 | 工作流从 41→8（4 注册: game-concept-art/character-design/environment/item-icon + 4 蓝图: image-utility）; 客户端 SDK 更新; _get_logical_workflow_id 映射重写; _fetch_workflow_params 增加 /workflows/{id} 查询策略; 移除旧 z-image/qwen 引用 |
| 2026-06-15 | LLM 提示词更新 | 系统提示词从 SD 权重语法改为自然语言格式 |
| 2026-06-15 | 项目分析融合 | `ProjectScanner` 扩展了指导文档扫描 + `get_context_for_asset()` |
| 2026-06-15 | 服务器 v3.2 完全集成 | SDK 更新、工作流选择器、参数自动映射、错误分类 |
| 2026-06-15 | 工具栏新增按钮 | ⏹ 中断 + 🧹 释放显存 |
| 2026-06-15 | `comms_client.py` 替换 | 从服务器下载新版 SDK（19835 bytes）|
| 2026-06-15 | `启动ComfyUI客户端.bat` | 一键启动脚本 |

## 5. ASSET_TEMPLATES 汇总

所有模板提示词已改为自然语言格式（无 SD 权重语法）。
- **Player**: 墨语, 莎娜 (64x64)
- **Enemy 小怪**: 腐化村民, 暗影之刃, 水晶寄生体, 沼泽潜伏者 (32x32)
- **Enemy 精英**: 灵魂吞噬者, 熔岩元素, 机械构造体 (48x48)
- **Boss**: 时空守护者 (64x64), 记忆守护者 (80x80), S-SN (96x96)
- **MapTile**: 废墟都市Tile, 遗忘庄园Tile, 古代神殿Tile, 实验室碎片Tile (64x64)
- **Prop**: 营火, 炼药锅, 时空门, 时空裂隙 (32x32)

## 6. 提示词规则
- 概念图：自然语言逗号分隔 + `dark fantasy concept art, digital painting`
- 精灵帧：`simple pixel art, minimalist game sprite` + 纯白背景 + 居中
- 统一视觉语言：blue-purple soul glow (#4A3A8C), cyan crystal erosion (#00D4FF)
- 反向提示词：排除 photorealistic, 3D render, hand-drawn lineart, cel-shading 等
