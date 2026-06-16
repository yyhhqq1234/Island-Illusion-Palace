# 项目记忆文档

> 此文档记录 AI Agent 与用户的多轮对话关键信息，用于维护跨会话上下文。
> 版本：4 | 最近更新：2026-06-16

---

## 1. 项目概览

- **项目**: 幻宫：时空回响 (Island: Illusion Palace) — Unity 2D top-down Roguelite Action RPG
- **Unity 项目路径**: `d:\Program Files\Unity\U3Dproject\Island-Illusion-Palace`
- **客户端**: `Scripts/comfyui_client.py` (Python GUI, Tkinter)
- **服务器**: `10.150.164.64:8189` (v5.1 统一 API)

## 2. ComfyUI 客户端状态

### 2.1 服务器架构
- **8189 统一入口**: 提供 HTTP REST + WebSocket + 蓝图展平
- **ComfyUI 原生 8188**: 不推荐直接访问，通过 8189 代理
- **SDK**: `comms_client.py` (从服务器 GET /client-sdk 获取)
- **工作流总量**: **8 个**（全部蓝图）+ 4 个 image-utility 蓝图
- **GPU**: NVIDIA RTX 3060 (12GB VRAM) + NPU Intel AI Boost (13 TOPS) + CPU Core Ultra 9 275HX
- **加速**: CLIP 嵌入卸载 (NPU/CPU) + CPU VAE 解码
- **模型总量**: ~97GB
- **在线文档**: http://10.150.164.64:8189/docs (67208 bytes)

### 2.2 模型配置
| 模型 | 用途 | 特点 |
|------|------|------|
| **NoobAI-XL-Vpred** (SDXL) | 旗舰角色/概念/精灵帧 | 1300万张Danbooru训练, 角色识别能力SD1.5的5倍+, 发色控制优秀 |
| **AbyssOrangeMix3** (SD1.5) | 品质角色/场景/精灵帧 | 2.5D混合风格, 有白发神明偏见 |
| **z-image-turbo** | 极速道具图标 | 4步生成, 不适合角色/概念图 |

### 2.3 客户端版本
- `comfyui_client.py` **v5.1**（从 v4.1 升级），Tkinter GUI
- **v5.1 新增**:
  - 参数管理 API 集成（GET/PUT `/workflows/{id}/params`，含兼容层）
  - `_fetch_workflow_params` 优先使用参数管理 API 端点（策略0）
  - 工作流列表 UI 显示 [SDXL旗舰] 标签
  - 3 个辅助方法：`_server_get_params`, `_server_update_params`, `_server_params_history`
  - **v5.1 修正**: SDXL 工作流参数字段统一为 `prompt`/`negative_prompt`（与 AOM3 相同），服务器内部分发到 text_g/text_l
- `HAS_COMMS_SDK` 由 `from comms_client import CommsClient` 决定
- 配置持久化: `comfyui_client_config.json`
- SDK: `comms_client.py` (v5.1, 512行, 含8个工作流适配)

### 2.4 关键 UI 功能
| 区域 | 说明 |
|------|------|
| 工具栏 | 连接状态/VRAM/队列/详情面板 + 4按钮（测试连接/检查队列/中断/释放显存）|
| Tab 1 (生成) | 资产选择/阶段/提示词/参数/生成按钮 |
| Tab 2 (AI) | LLM 配置 + 优化提示词/生成提示词/推荐工作流/自定义对话 |
| Tab 3 (资源) | 服务器资源浏览器（模型/VAE/LoRA/节点）|
| Tab 4 (分析) | 项目扫描 + AI 需求提取 + 资产缺口分析 |
| Tab 5 (工作流) | 工作流编辑器（新建/加载/保存/验证/AI 生成）|

### 2.5 可用的游戏美术工作流（v5.1）

#### SDXL 旗舰（NoobAI-XL-Vpred）— 推荐：人形角色首选

| 工作流 | 画布 | 步数 | CFG | 参数字段 | 用途 |
|--------|------|------|-----|----------|------|
| `game-character-design-sdxl` | **1024x1536** | 28 | 7-9 | `text_g`, `text_l`, `negative_prompt`, `seed`, `steps`, `cfg` | **角色立绘(旗舰)** — 全身1024x1536, Euler采样, 解决AOM3白发偏见 |
| `game-concept-art-sdxl` | **1024x1024** | 35 | 6 | `text_g`, `text_l`, `negative_prompt`, `seed`, `steps`, `cfg` | **概念图(旗舰)** — 复杂场景理解远超SD1.5 |
| `game-sprite-sheet-sdxl` | **1536x1536** (2x2) | 35 | 5.5 | `text_g`, `text_l`, `negative_prompt`, `seed`, `steps`, `cfg` | **精灵帧动画(旗舰)** — 每帧768x768, 角色一致性最强 |

> **SDXL 双 CLIP 注入机制（v5.1修正）**: SDXL工作流内部使用 `CLIPTextEncodeSDXLNPU` 节点，需要同时注入 `text_g`(全局构图/风格) + `text_l`(细节/发色/服装)。
> 客户端 `_map_params_to_workflow()` 通过 `workflow_id` 包含 "sdxl" 检测 → 自动发送 `text_g` + `text_l`（内容相同，模型自行分配权重）。
> **若不注入 text_l，工作流默认值会覆盖发色 → 蓝发。**

#### AOM3 品质（AbyssOrangeMix3）— 非人形/快速任务

| 工作流 | 画布 | 步数 | CFG | 参数字段 | 用途 |
|--------|------|------|-----|----------|------|
| `game-concept-art` | 1024x1024 | 30 | 7-10 | `prompt`, `negative_prompt`, `seed`, `steps`, `cfg` | 概念图/宣传画 |
| `game-character-design` | 768x1024 | 20 | 5-8 | `prompt`, `negative_prompt`, `seed`, `steps`, `cfg` | 角色立绘（快速版）|
| `game-environment` | 1536x640 | 20 | 7-8 | `prompt`, `negative_prompt`, `seed`, `steps`, `cfg` | 场景/关卡背景/Tile |
| `game-sprite-sheet` | **2048x2048** (2x2) | **25** | 7 | `prompt`, `negative_prompt`, `seed`, `steps`, `cfg` | 精灵帧动画条带（每帧1024x1024）|

#### 极速

| 工作流 | 画布 | 步数 | 参数字段 | 用途 |
|--------|------|------|----------|------|
| `game-item-icon` | 1024x1024 | 4 | `prompt`, `seed` | 道具/图标/UI 元素（仅 prompt+seed）|

#### 图像工具蓝图（4个）
`image-inpainting`(Flux Fill), `image-depth-map`(Lotus), `image-blur`, `image-sharpen`

### 2.6 提示词双阶段设计
每个 ASSET_TEMPLATES 包含两套提示词：
- `prompt`: 概念图阶段（dark fantasy concept art, digital painting, 单对象居中）
- `sprite_prompt`: 精灵帧阶段（simple pixel art, minimalist game sprite, 纯白背景, 居中）
- `_update_prompts()` 根据 stage 自动选择对应字段

### 2.7 资产类型 → 工作流映射表（v5.1 自动策略）

| 资产 | 阶段 | 默认工作流 | 原因 |
|------|------|-----------|------|
| **Boss (含S-SN)** | concept | **`game-character-design-sdxl`** | SDXL解决AOM3白发偏见 |
| **Player** | concept | **`game-concept-art-sdxl`** | SDXL角色一致性更好 |
| **Enemy/NPC** | concept | **`game-concept-art-sdxl`** | SDXL发色控制更准 |
| **非人形** (MapTile等) | concept | `game-concept-art` | AOM3够用 |
| **Boss** | sprite | **`game-character-design-sdxl`** | SDXL立绘质量最高 |
| **Player** | sprite | **`game-character-design-sdxl`** | 同上 |
| **Enemy/NPC** | sprite | `game-character-design` | AOM3快速版够用 |
| Item/Prop/UI | any | `game-item-icon` | 极速 |
| MapTile/Background | any | `game-environment` | AOM3宽幅专用 |
| 精灵帧动画条带 | sprite | `game-sprite-sheet` 或 `-sdxl` | 多帧动态 |

### 2.8 精灵帧 img2img 模式
- 用户从浏览器选择参考底图
- 客户端读取 PNG → base64 编码
- 仅当目标工作流 schema 包含 `image` 字段时附加
- 大部分工作流不支持图片输入 → 自动走纯文生图路径

### 2.9 SDXL 参数自动调整
客户端选择 SDXL 工作流时自动设置：
```
game-character-design-sdxl → steps=28, cfg=8, 1024x1536
game-concept-art-sdxl    → steps=35, cfg=6, 1024x1024
game-sprite-sheet-sdxl   → steps=35, cfg=6, 1536x1536
game-sprite-sheet         → steps=25, cfg=7, 2048x2048 (AOM3更新后)
```


## 3. 当前已知问题

### 3.1 服务器端
- ComfyUI 后端（8188）偶尔挂掉导致 8189 代理返回 502 / 空 prompt_id，需重启 ComfyUI
- 服务器重启后防火墙可能重置，需重新开放 8189 端口
- `/client-sdk` 端点偶尔连接中断（可能走 8188 代理），其他 API 正常
- `game-item-icon` 工作流仅支持 prompt+seed 参数，不含 steps/cfg 控制

### 3.2 客户端
- 精灵帧 img2img 流程尚未完整测试（需概念图生成成功作为参考底图）
- SDK 已更新至 v5.1 (512行)，参数管理方法（http_get_params 等）可能需要后续 SDK 版本包含
- **SDXL 参数注入已验证（2026-06-16）**: CLIPTextEncodeSDXLNPU 需要 text_g + text_l 同时注入，仅发 prompt 会导致 text_l 用默认值（发色/细节失控）


## 4. 近期重要变更

| 日期 | 变更 | 详情 |
|------|------|------|
| 2026-06-16 | **SDXL参数映射根因修复** | SDXL工作流使用CLIPTextEncodeSDXLNPU(text_g+text_l)，客户端之前只发prompt→text_l用默认值→发色失控+双视图; 修复:_map_params_to_workflow检测sdxl→同时注入text_g+text_l; 工作流默认残留character design sheet也是双视图原因 |
| 2026-06-16 | **客户端升级 v5.1** | SDXL双CLIP(text_g/text_l)自动映射; 参数管理API集成(兼容层); 工作流UI显示[SDXL旗舰]标签; _fetch_workflow_params新增策略0; SDK更新至512行 |
| 2026-06-16 | **v5.1 紧急修复** | SDXL参数映射错误修正: text_g/text_l→prompt(服务器API统一用prompt, 内部分发到SDXL双CLIP); 删除2张失败图片(蓝色光斑) |
| 2026-06-16 | **服务器升级 v5.1** | 新增 3 个 SDXL 旗舰工作流 (NoobAI-XL-Vpred); game-sprite-sheet 画布更新为 2048x2048; 总工作流 8 个; 文档 67KB |
| 2026-06-16 | 客户端适配 v5.1 (初版) | _get_logical_workflow_id 加入 SDXL 映射; S-SN 强制用 character-design-sdxl; 新增 _auto_adjust_sdxl_params() |
| 2026-06-16 | S-SN 提示词激进优化 | 全大写发色 + 深红裙 + 多重否定 + DEEP DARK CRIMSON |
| 2026-06-16 | 默认工作流调整 | Boss/Player concept 改用 SDXL; Boss/Player sprite 用 character-design-sdxl |
| 2026-06-16 | 旧版 S-SN 概念图清理 | 删除 123316 (白发双人)、124737 (粉色) |
| 2026-06-16 | __pycache__ 清理 | 删除 Python 字节码缓存 |
| 2026-06-15 | 服务器 v3.2→v5.1 精简重构 | 工作流从 41→8 (+3 SDXL); 新增 NoobAI-XL 模型; NPU 加速 |
| 2026-06-15 | LLM 提示词更新 | 系统提示词从 SD 权重语法改为自然语言格式 |
| 2026-06-15 | 项目分析融合 | `ProjectScanner` 扩展了指导文档扫描 + `get_context_for_asset()` |

## 5. ASSET_TEMPLATES 汇总

所有模板提示词已改为自然语言格式（无 SD 权重语法），并加入解剖学约束（人形角色两臂两腿五指五趾）。

- **Player**: 墨语, 莎娜 (64x64)
- **Enemy 小怪**: 腐化村民, 暗影之刃, 水晶寄生体, 沼泽潜伏者 (32x32)
- **Enemy 精英**: 灵魂吞噬者, 熔岩元素, 机械构造体 (48x48)
- **Boss**: 时空守护者 (64x64), 记忆守护者 (80x80), S-SN (96x96) — S-SN 发色=深红, **强制SDXL**
- **MapTile**: 废墟都市Tile, 遗忘庄园Tile, 古代神殿Tile, 实验室碎片Tile (64x64)
- **Prop**: 营火, 炼药锅, 时空门, 时空裂隙 (32x32)

## 6. 提示词规则

### 6.1 通用规则
- 概念图：自然语言逗号分隔 + `dark fantasy concept art, digital painting`
- 精灵帧：`simple pixel art, minimalist game sprite` + 纯白背景 + 居中
- 统一视觉语言：blue-purple soul glow (#4A3A8C), cyan crystal erosion (#00D4FF)

### 6.2 反向提示词（所有资产通用）
```
photorealistic, 3D render, CGI, smooth vector art, flat design,
bright sunny, low resolution, blurry, watermark, text, signature,
ugly, distorted, bad anatomy, extra limbs, fused fingers, missing fingers,
extra fingers, too many fingers, extra legs, too many legs, multiple legs,
four legs, six legs, low quality, jpeg artifacts, multiple characters,
two people, group of people, duplicate, mirrored copy, extra figure,
second character, more than one person
```

### 6.3 人形角色解剖学约束
每个含人形的 prompt 应包含：
```
anatomically correct human body exactly two arms two legs
five fingers each hand five toes each foot
```

### 6.4 S-SN 特殊处理（针对 AOM3 偏见，使用SDXL后可简化）
- 发色开头全大写强调：`DEEP DARK CRIMSON RED HAIR`
- 多个颜色锚定词：`wine-red`, `dark ruby`, `fresh blood`
- 多重否定词：`not white not silver not gray not platinum not blonde not pale not lavender not pink not pastel not light-colored`
- 服装改为深红：`deep crimson-red ceremonial divine gown` (不用 white/ivory)
- 去除 `goddess` 关键词（触发白发偏见），改用 `ultimate boss`
- **★ 使用 game-character-design-sdxl 后可大幅放宽以上约束（NoobAI无白发偏见）**
