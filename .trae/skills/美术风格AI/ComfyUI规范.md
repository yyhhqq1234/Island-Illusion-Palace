# ComfyUI 使用规范

> 版本：2.2 | 日期：2026-06-14 | 维护者：美术风格AI

本文档是"幻宫：时空回响"项目中 ComfyUI 美术资产生成的操作规范，供美术风格AI在调用 ComfyUI 时参考。

---

## 0. 强制前置规则

> **每次使用 ComfyUI 之前，必须先打开通信面板。**

| 规则 | 说明 |
|------|------|
| **通信面板必须先开** | 打开 ComfyUI 资产生成器窗口时，会自动弹出通信面板。也可通过 `Tools > ComfyUI > 通信面板` 手动打开。 |
| **连接 8189 端口** | 通信面板通过 WebSocket 连接 `ws://10.150.164.64:8189`，实时显示服务器状态、GPU 显存、队列负载、优化建议。 |
| **状态确认后再操作** | 确认通信面板显示"已连接"且服务器状态正常后，再进行生成操作。若显存使用率 > 85% 或队列积压 > 3，应等待或优化后再提交任务。 |

**通信面板功能**：
- 实时 GPU 显存使用率（进度条 + 数值）
- 任务队列状态（运行中 / 等待中数量）
- 已连接客户端数量
- 服务器运行时间
- 自动推送优化建议（如启用 NPU/iGPU 加速节点）

---

## 1. 服务器配置

### 1.1 基本信息

| 配置项 | 值 |
|--------|-----|
| 服务器地址 | `http://10.150.164.64:8188` |
| WebSocket 地址（任务进度） | `ws://10.150.164.64:8188/ws` |
| WebSocket 地址（实时通信） | `ws://10.150.164.64:8189` |
| 实时通信 HTTP API | `http://10.150.164.64:8189` |
| API 文档地址 | `http://10.150.164.64:8188/extensions/default_workflows/api_docs.html` |
| ComfyUI 版本 | 0.20.1 |
| Python 版本 | 3.13.12 |
| PyTorch 版本 | 2.11.0+cu130 |
| OpenVINO 版本 | 2026.2 |
| GPU | NVIDIA GeForce RTX 3060（12GB GDDR6） |
| CPU | Intel Core Ultra 9 275HX（24 核, 2.7GHz） |
| NPU | Intel AI Boost（AD1D, ~13 TOPS） |
| iGPU | Intel Arc Graphics（Xe 矩阵扩展） |
| RAM | 16GB DDR5 |

### 1.2 算力架构

服务器采用**多算力协同加速**方案：

```
RTX 3060  →  UNet 扩散模型（主力）
Intel NPU →  CLIP 文本编码（OpenVINO 加速）
Intel Arc →  VAE 解码（OpenVINO 加速）
CPU 24核  →  图像预处理 + 调度 + 备用
```

### 1.3 自定义加速节点

| 节点 | 分类 | 功能 | 替代节点 |
|------|------|------|---------|
| `CLIPTextEncodeNPU` | conditioning/npu | NPU 加速文本编码 | `CLIPTextEncode` |
| `CLIPTextEncodeSDXLNPU` | conditioning/npu | NPU 加速 SDXL 编码 | `CLIPTextEncodeSDXL` |
| `VAEDecodeIGPU` | latent/igpu | iGPU 加速 VAE 解码 | `VAEDecode` |
| `NPUStatus` | utils/npu | 全算力状态面板 | — |

> **使用说明**: 在工作流中将对应原生节点替换为加速节点即可。首次使用会自动转换模型为 OpenVINO 格式并缓存（10-30 秒），后续即时响应。失败时自动回退到 GPU。

### 1.4 可用模型

| 类别 | 模型名 | 用途 |
|------|--------|------|
| **Checkpoint** | `动漫 primemix_v21.safetensors` | 文生图主模型（动漫风格，适合生成像素画参考） |
| **LoRA** | `Qwen-Image-Edit-2511-Lightning-4steps-V1.0-bf16.safetensors` | 图生图 LoRA（4 步快速推理） |
| **VAE** | `flux-ae.safetensors` | Flux 系列 VAE |
| **VAE** | `qwen_image_vae.safetensors` | Qwen 系列 VAE（配合 Qwen LoRA 使用） |

### 1.5 启动参数

```
--listen 10.150.164.64 --port 8188 --highvram --fast --force-channels-last
--bf16-unet --cpu-vae --async-offload 4 --cache-lru 100
```

| 参数 | 作用 |
|------|------|
| `--highvram` | 模型常驻 GPU 显存，消除任务间重载 |
| `--fast` | FP16 累积 + 自动调优，推理加速 15-30% |
| `--force-channels-last` | GPU 最优内存布局 |
| `--bf16-unet` | UNet BF16 精度，显存减半 |
| `--cpu-vae` | VAE 卸载到 CPU，释放 GPU 显存 |
| `--async-offload 4` | 4 条异步卸载流，模型切换更快 |
| `--cache-lru 100` | LRU 缓存 100 个中间结果 |

### 1.6 API 端点

| 端点 | 方法 | 用途 |
|------|------|------|
| `/api/prompt` | POST | **提交任务**：传入 workflow JSON |
| `/api/history` | GET | 获取所有执行历史 |
| `/api/history/{prompt_id}` | GET | 获取特定任务执行结果 |
| `/api/queue` | GET | 查看队列（`queue_running` + `queue_pending`） |
| `/api/queue` | POST | 清空队列（`{"clear": true}`）或删除任务（`{"delete": ["id"]}`） |
| `/api/view` | GET | 下载生成图片（参数：`filename`, `subfolder`, `type`） |
| `/api/upload/image` | POST | 上传图片到 input 目录 |
| `/api/interrupt` | POST | 中断当前执行 |
| `/api/free` | POST | 释放显存（卸载模型） |
| `/api/models` | GET | 获取模型文件夹类型列表 |
| `/api/models/{folder}` | GET | 获取特定文件夹模型列表 |
| `/api/embeddings` | GET | 获取已加载的模型信息 |
| `/api/object_info` | GET | 获取所有可用节点定义 |
| `/api/object_info/{node_class}` | GET | 获取特定节点的输入输出定义 |
| `/api/system_stats` | GET | 获取系统状态（GPU、RAM、NPU 等） |
| `/ws` | WebSocket | 实时接收进度和执行状态 |

### 1.7 WebSocket 消息类型（端口 8188）

| 类型 | 说明 |
|------|------|
| `status` | 队列状态 |
| `progress` | 节点执行进度（`value` / `max`） |
| `executing` | 开始执行某节点 |
| `executed` | 节点执行完成（含输出） |
| `execution_success` | 整个工作流完成 |
| `execution_error` | 执行出错 |

### 1.8 实时通信服务（端口 8189）

专用 WebSocket 服务器，支持客户端与服务器实时双向通信。服务器每 **5 秒**自动广播 GPU 状态、队列负载和优化建议，热更新时主动推送通知。

**连接流程**：
1. 客户端通过 WebSocket 连接 `ws://10.150.164.64:8189`
2. 服务器自动分配 `client_id`，发送 `welcome` 消息
3. 客户端可发送 `register` 注册元数据（平台、版本等）
4. 服务器每 5 秒自动广播 `server_status` 给所有客户端
5. 服务器根据 GPU 显存使用率自动生成优化建议
6. 热更新检测到文件变更时，通过 `/notify` 通知所有客户端

**HTTP 端点**：

| 端点 | 方法 | 用途 |
|------|------|------|
| `ws://10.150.164.64:8189` | WS | 实时通信 |
| `http://10.150.164.64:8189/status` | GET | 获取服务器当前状态（GPU、队列、客户端数、优化建议） |
| `http://10.150.164.64:8189/health` | GET | 健康检查（`{"status": "ok", "uptime": ...}`） |
| `http://10.150.164.64:8189/clients` | GET | 已连接客户端列表和数量 |
| `http://10.150.164.64:8189/history` | GET | 获取最近 100 条消息历史 |
| `http://10.150.164.64:8189/notify` | POST | 热更新通知接口（由热更新服务调用，广播给所有客户端） |

**消息类型**：

| 类型 | 方向 | 触发条件 | 说明 |
|------|------|---------|------|
| `welcome` | 服务器→客户端 | 连接建立 | 欢迎消息，含 client_id、协议版本、初始状态 |
| `ping` | 客户端→服务器 | 手动 | 心跳检测 |
| `pong` | 服务器→客户端 | 响应 ping | 心跳响应，含服务器时间 |
| `request_status` | 客户端→服务器 | 手动 | 拉取最新服务器状态 |
| `server_status` | 服务器→客户端 | 5秒定时/响应请求 | GPU 显存、队列、客户端数、优化建议 |
| `optimization_tip` | 服务器→客户端 | 状态变化 | 显存高/队列积压时自动推送优化建议 |
| `hot_reload` | 服务器→客户端 | 文件变更 | 热更新通知，含变更内容、等级、时间 |
| `chat` | 双向 | 手动 | 客户端间实时文本消息互通 |
| `register` | 客户端→服务器 | 连接后 | 注册客户端元数据 |
| `get_history` | 客户端→服务器 | 手动 | 获取最近 N 条消息历史 |
| `message_history` | 服务器→客户端 | 响应 get_history | 消息历史列表 |
| `submit_priority` | 客户端→服务器 | 手动 | 请求优先处理当前任务 |
| `ack` | 服务器→客户端 | 响应操作 | 操作确认回执 |

**`server_status` 数据结构**：

```json
{
    "type": "server_status",
    "data": {
        "timestamp": 1718123456.789,
        "uptime": 3600.0,
        "gpu": {
            "name": "NVIDIA GeForce RTX 3060",
            "vram_used": 8192,
            "vram_total": 12288,
            "vram_free": 4096
        },
        "queue": {"running": [], "pending": []},
        "connected_clients": 3,
        "optimization_tips": [{
            "level": "warning",
            "category": "vram",
            "message": "GPU显存使用率超过85%...",
            "action": "将 CLIPTextEncode 替换为 CLIPTextEncodeNPU"
        }]
    },
    "timestamp": 1718123456.789
}
```

**优化建议规则**：

| 条件 | 等级 | 建议 |
|------|------|------|
| GPU 显存 > 85% | `warning` | 启用 NPU/iGPU 加速节点释放显存 |
| GPU 显存 60-85% | `info` | 运行正常，可考虑降低 batch_size |
| 队列积压 > 3 | `info` | 提示队列等待数量 |
| 无异常 | `success` | 服务器运行状态良好 |

### 1.9 运维信息

| 配置项 | 值 |
|--------|-----|
| 日志目录 | `d:\ComfyUI-P\logs\` |
| 日志文件 | `service.log` |
| 日志格式 | `[YYYY-MM-DD HH:MM:SS] [LEVEL] 消息` |
| 日志轮转 | 超过 10MB 自动备份，保留 7 天 |
| 工作流目录 | `d:\ComfyUI-P\workflows\` |
| 启动脚本 | `run_comfyui.bat`（主启动）/ `startup.bat`（开机自启）/ `monitor_service.bat`（监控+崩溃重连） |
| 崩溃恢复 | 每 60 秒检查，崩溃自动重启（最多 5 次），每次重启自动清理缓存 |

### 1.10 连接测试

```powershell
# 测试 ComfyUI 连接
Invoke-RestMethod -Uri "http://10.150.164.64:8188/api/system_stats" -Method Get

# 测试实时通信服务
Invoke-RestMethod -Uri "http://10.150.164.64:8189/health" -Method Get
```

---

## 2. 工作流说明

### 2.1 核心原则：动态创建，不依赖固定文件

> **重要变更（v2.0）**：不再局限于两个预设工作流 JSON 文件。应根据每次生成需求**动态构建 workflow JSON**，直接提交给 ComfyUI。这样可以利用服务器上 693 个节点的任意组合。

**旧方式（已废弃）**：
- 修改本地 `workflows/*.json`
- 受限的两条固定管线（zimage / qwen-edit）

**新方式（推荐）**：
- 调用 `/api/object_info` 获取当前可用节点
- 根据需求动态组装 workflow JSON
- 直接 POST 到 `/api/prompt`

### 2.2 通用文生图工作流模板（Text-to-Image）

使用 `动漫 primemix_v21` checkpoint 进行文生图，适合生成像素画风格的概念参考图或直接生成精灵。

```json
{
  "1": {
    "class_type": "CheckpointLoaderSimple",
    "inputs": { "ckpt_name": "动漫 primemix_v21.safetensors" }
  },
  "2": {
    "class_type": "CLIPTextEncode",
    "inputs": { "text": "正向提示词", "clip": ["1", 1] }
  },
  "3": {
    "class_type": "CLIPTextEncode",
    "inputs": { "text": "反向提示词", "clip": ["1", 1] }
  },
  "4": {
    "class_type": "EmptyLatentImage",
    "inputs": { "width": 512, "height": 512, "batch_size": 1 }
  },
  "5": {
    "class_type": "KSampler",
    "inputs": {
      "seed": 42,
      "steps": 20,
      "cfg": 7,
      "sampler_name": "euler_ancestral",
      "scheduler": "normal",
      "denoise": 1,
      "model": ["1", 0],
      "positive": ["2", 0],
      "negative": ["3", 0],
      "latent_image": ["4", 0]
    }
  },
  "6": {
    "class_type": "VAEDecode",
    "inputs": { "samples": ["5", 0], "vae": ["1", 2] }
  },
  "7": {
    "class_type": "SaveImageWebsocket",
    "inputs": { "images": ["6", 0] }
  }
}
```

**关键节点说明**：

| 节点 | 类型 | 用途 | 关键参数 |
|------|------|------|---------|
| `CheckpointLoaderSimple` | 模型加载 | 加载 `动漫 primemix_v21` | `ckpt_name` |
| `CLIPTextEncode` ×2 | 提示词编码 | 分别编码正/反向提示词 | `text` |
| `EmptyLatentImage` | 画布 | 控制输出尺寸 | `width`, `height` |
| `KSampler` | 采样 | 控制图片质量 | `seed`, `steps`, `cfg`, `sampler_name`, `scheduler` |
| `VAEDecode` | 解码 | 将潜空间转图片 | — |
| `SaveImageWebsocket` | 输出 | 通过 WebSocket 回传图片 | — |

### 2.3 KSampler 参数调优

| 参数 | 默认值 | 说明 | 建议范围 |
|------|--------|------|---------|
| `seed` | 随机 | 随机种子，相同 seed 产生相同结果 | 0 ~ 2^63 |
| `steps` | 20 | 采样步数，越多越精细但越慢 | 15-30（概念图）/ 10-15（精灵） |
| `cfg` | 7 | CFG 引导强度，越高越遵从提示词 | 5-10 |
| `sampler_name` | `euler_ancestral` | 采样器类型 | `euler_ancestral`（创意）/ `euler`（稳定）/ `dpmpp_2m`（高质量） |
| `scheduler` | `normal` | 调度器 | `normal` / `karras` / `exponential` |
| `denoise` | 1.0 | 去噪强度 | 1.0（文生图）/ 0.5-0.8（图生图） |

### 2.4 图生图工作流（Image-to-Image via LoRA）

使用 `Qwen-Image-Edit-2511-Lightning-4steps` LoRA 进行图生图，从概念图中提取精灵帧变体。

```json
{
  "1": {
    "class_type": "CheckpointLoaderSimple",
    "inputs": { "ckpt_name": "动漫 primemix_v21.safetensors" }
  },
  "2": {
    "class_type": "LoraLoader",
    "inputs": {
      "model": ["1", 0],
      "clip": ["1", 1],
      "lora_name": "Qwen-Image-Edit-2511-Lightning-4steps-V1.0-bf16.safetensors",
      "strength_model": 1,
      "strength_clip": 1
    }
  },
  "3": {
    "class_type": "LoadImage",
    "inputs": { "image": "参考图片.png" }
  },
  "4": {
    "class_type": "CLIPTextEncode",
    "inputs": { "text": "正向提示词", "clip": ["2", 1] }
  },
  "5": {
    "class_type": "CLIPTextEncode",
    "inputs": { "text": "反向提示词", "clip": ["2", 1] }
  },
  "6": {
    "class_type": "VAEEncode",
    "inputs": { "pixels": ["3", 0], "vae": ["1", 2] }
  },
  "7": {
    "class_type": "KSampler",
    "inputs": {
      "seed": 42, "steps": 4, "cfg": 1,
      "sampler_name": "euler", "scheduler": "normal",
      "denoise": 0.6,
      "model": ["2", 0],
      "positive": ["4", 0],
      "negative": ["5", 0],
      "latent_image": ["6", 0]
    }
  },
  "8": {
    "class_type": "VAEDecode",
    "inputs": { "samples": ["7", 0], "vae": ["1", 2] }
  },
  "9": {
    "class_type": "SaveImageWebsocket",
    "inputs": { "images": ["8", 0] }
  }
}
```

**图生图特殊参数**：
- `steps: 4` — Lightning LoRA 只用 4 步即可
- `cfg: 1` — 图生图建议低 CFG
- `denoise: 0.6` — 保留 40% 原图特征，变化 60%
- 需提前通过 `/api/upload/image` 上传参考图

### 2.5 工作流构建流程

```
1. 确定需求（概念图 / 精灵帧 / 图生图变体）
     │
2. GET /api/object_info  →  确认所需节点可用
     │
3. 选择模板（文生图 2.2 / 图生图 2.4 / 自定义组合）
     │
4. 填入参数（提示词、尺寸、seed、steps、cfg）
     │
5. POST /api/prompt  →  { "prompt": workflow_json }
     │
6. WebSocket 接收实时进度  →  execution_complete
     │
7. GET /api/history/{prompt_id}  →  获取输出文件列表
     │
8. GET /api/view?filename=...  →  下载图片
```

### 2.6 常用节点速查

以下节点确认可用，可在动态工作流中组合使用：

| 类别 | 节点 | 功能 |
|------|------|------|
| 模型 | `CheckpointLoaderSimple` | 加载 checkpoint |
| 模型 | `LoraLoader` / `LoraLoaderModelOnly` | 加载 LoRA |
| 模型 | `CLIPLoader` / `DualCLIPLoader` / `TripleCLIPLoader` | 加载 CLIP 模型 |
| 提示词 | `CLIPTextEncode` / `CLIPTextEncodeSDXL` / `CLIPTextEncodeSD3` | 文本编码 |
| 采样 | `KSampler` / `KSamplerAdvanced` / `KSamplerSelect` | 噪声采样 |
| 潜空间 | `EmptyLatentImage` / `VAEEncode` / `VAEDecode` / `VAEDecodeTiled` | 潜空间操作 |
| 图像 | `LoadImage` / `SaveImage` / `PreviewImage` | 图像 IO |
| 图像 | `SaveImageWebsocket` | 通过 WebSocket 返回图片（推荐） |
| 图像 | `AdjustBrightness` / `AdjustContrast` / `CenterCropImages` | 图像后处理 |
| 批处理 | `BatchImagesNode` / `BatchLatentsNode` | 批量生成 |
| 控制 | `PrimitiveInt` / `PrimitiveFloat` / `PrimitiveString` | 动态参数注入 |

---

## 3. 提示词规范

### 3.1 基础风格词（所有资产共用）

> **重要修正**：经实际资产分析，游戏内精灵为**像素画 (Pixel Art)** 风格，而非手绘笔触风格。概念图可用高细节数字绘画，但游戏内精灵必须匹配像素画特征。

#### 3.1.1 游戏内精灵基础风格词（像素画）

**正向：**
```
(pixel art:1.4), (2D top-down game sprite:1.3), dark fantasy,
(soul glow: blue-purple luminescence #4A3A8C), (crystal erosion: geometric cyan crystals #00D4FF),
cel-shading, 1px outline, transparent background,
dark atmospheric, high contrast, limited color palette
```

**反向：**
```
photorealistic, 3D render, CGI, smooth vector art, flat design,
bright sunny, cartoon, anime, chibi, painterly texture, visible brushstrokes,
blurry, watermark, text, signature, ugly, distorted, bad anatomy,
extra limbs, fused fingers, low quality, jpeg artifacts, anti-aliasing
```

#### 3.1.2 概念设定图基础风格词（数字绘画）

**正向：**
```
(dark fantasy concept art:1.3), (digital painting:1.2), dramatic lighting,
(soul glow: blue-purple luminescence #4A3A8C), (crystal erosion: geometric cyan crystals #00D4FF),
rim light, high contrast, detailed texture, atmospheric perspective
```

**反向：**
```
photorealistic, 3D render, CGI, smooth vector art, flat design,
bright sunny, cartoon, anime, chibi, pixel art, low resolution,
blurry, watermark, text, signature, ugly, distorted, bad anatomy,
extra limbs, fused fingers, low quality, jpeg artifacts
```

### 3.2 提示词模板系统

模板定义在 `Assets/Scripts/Editor/ComfyUI/ComfyUIPromptTemplates.cs`，通过 `PromptTemplate` 类管理。

**模板结构：**
```csharp
public class PromptTemplate
{
    public string assetType;        // 资产类型名称（如 "时空守护者"）
    public string positivePrompt;   // 资产专属正向提示词
    public string negativePrompt;   // 资产专属反向提示词（null = 使用默认）
    public int width;               // 生成宽度
    public int height;              // 生成高度
    public AssetCategory category;  // 资产分类
}
```

**资产分类：** `Player`, `Enemy`, `Boss`, `MapTile`, `Prop`, `Weapon`, `UI`, `Effect`

### 3.3 提示词编写规范

1. **权重标记**：使用 `(关键词:权重)` 格式，如 `(pixel art:1.4)`
2. **色值标注**：使用 `#HEX` 格式，如 `#4A3A8C`（蓝紫色灵魂光）
3. **比例标注**：使用 `X-heads-tall`，如 `4-heads-tall proportion`
4. **完整提示词 = 基础风格词 + 资产专属词**（由 `GetFullPositivePrompt()` 自动拼接）

#### 像素画专用提示词技巧

| 技巧 | 说明 | 示例 |
|------|------|------|
| **强调像素画** | 必须显式声明 `pixel art` 且权重 ≥1.3 | `(pixel art:1.4)` |
| **Cel-shading** | 像素画使用极简明暗（2-3个色阶） | `cel-shading, 2-tone shading` |
| **1px 轮廓线** | 像素精灵使用 1 像素清晰轮廓 | `1px outline, crisp pixel edges` |
| **限制色板** | 避免复杂渐变，使用有限颜色 | `limited color palette, 8-bit colors` |
| **透明背景** | 精灵必须透明背景 | `transparent background` |
| **禁止抗锯齿** | 像素画边缘必须锐利 | 在反向提示词中加入 `anti-aliasing` |
| **俯视角度** | 所有游戏内资产为顶视角 | `top-down view, 45 degree angle` |
| **Q版比例** | 角色为 2-4 头身 | `chibi proportion, 3-heads-tall` |

### 3.4 精灵帧提示词差异

同一资产的各帧精灵使用相同的风格词，仅通过姿态描述区分：

```
示例（时空守护者 Idle 帧1）：
基础风格词 + (time guardian boss:1.4), ... , standing idle pose, neutral stance

示例（时空守护者 Attack 帧3）：
基础风格词 + (time guardian boss:1.4), ... , mid-swing attack pose, arm raised, energy flowing
```

### 3.5 实际资产风格分析结果

基于对 `Assets/ArtMaterials/` 中 8 类代表性资产的直接图像分析，得出以下关键结论，用于指导 ComfyUI 提示词优化：

#### 3.5.1 核心美术风格定位

| 维度 | 实际特征 | 对 ComfyUI 的启示 |
|------|---------|------------------|
| **风格** | **暗色手绘像素画 (Dark Hand-Drawn Pixel Art)** | 提示词必须显式声明 `pixel art`，而非 `hand-drawn` 或 `painterly` |
| **视角** | 顶视角 (Top-Down, ~45° 俯视) | 所有角色/地图精灵需包含 `top-down view` |
| **着色** | 极简 Cel-Shading（2-3 个色阶） | 提示词加入 `cel-shading, 2-tone shading` |
| **轮廓** | 1px 清晰像素轮廓线，无抗锯齿 | 提示词加入 `1px outline, crisp pixel edges, no anti-aliasing` |
| **纹理** | 平滑像素块面，无单笔刷纹理 | 避免 `visible brushstrokes, painterly texture` |
| **比例** | Q 版比例（玩家 3 头身，敌人 2-2.5 头身，Boss 2.5-4 头身） | 提示词加入 `3-heads-tall, chibi proportion` |
| **背景** | 纯透明背景（精灵标准） | 必须包含 `transparent background` |
| **渐变** | 极少使用渐变，以色块拼接为主 | 加入 `flat colors, limited color palette` |

#### 3.5.2 概念图 vs 游戏内精灵的风格差异

| 对比项 | 概念设定图 | 游戏内精灵 |
|--------|-----------|-----------|
| **用途** | 视觉参考、氛围标杆 | 游戏内可游玩资产 |
| **风格** | 数字绘画 / AI 生成高细节 | 像素画 (Pixel Art) |
| **分辨率** | 1024×1024 或更高 | 32×32 ~ 80×80（见 4.3 节） |
| **光影** | 戏剧化布光、体积感、材质纹理 | 极简 2-3 色阶 Cel-Shading |
| **轮廓** | 无显式轮廓线，靠光影塑造 | 1px 清晰像素轮廓 |
| **示例** | Boss 概念图 `Concept_00001.png` | 腐化村民 `FHCM_idle_1.png` |

**关键结论**：ComfyUI 生成时需明确区分两阶段输出：
- **阶段1（概念图）**：可使用 `digital painting` 风格，高细节
- **阶段2（精灵帧）**：必须使用 `pixel art` 风格，匹配游戏内实际规格

#### 3.5.3 现有资产色彩特征

| 资产 | 主色调 | 点缀色 | 特殊说明 |
|------|--------|--------|---------|
| 玩家（墨语） | 深灰蓝 `#2A2A3A` | 紫罗兰/暗紫 | 安静内敛的少年气质 |
| 敌人（腐化村民） | 灰紫色 `#7A6A8A` | 暗红眼睛 | 病态、腐朽感 |
| 地图树木 | 深绿 `#2D5A3D` / 苔绿 `#4A7A5A` | — | 奇幻森林氛围 |
| 草地 Tile | 明亮草绿 `#4A7A3A` | — | 偏明亮，需后期压暗 |
| 传送门 | 青蓝 `#C8E8E8` ~ `#00D4FF` | 灰蓝阴影 | 碎裂玻璃/裂隙感 |
| 治疗药剂 | 红 `#C41E3A` | 银白瓶盖 | 经典 RPG 红药瓶 |
| 篝火 | 棕褐色 `#8A6A5A` | — | 安全、休憩感 |
| Boss 概念图 | 深灰蓝金属 + 暗金 `#C8A848` | 蓝紫 `#4A3A8C` 能量 | 威严、史诗感 |

### 3.6 全局色彩体系（来自视觉风格指导手册）

以下色彩定义来自《幻宫_视觉风格指导手册》第 2.1 节，所有 ComfyUI 提示词中的色值引用必须与此一致。

#### 3.6.1 核心色彩定义

| 角色 | 色名 | Hex | RGB | 用途 |
|------|------|-----|-----|------|
| 主色 | 灵魂蓝紫 | `#4A3A8C` | 74, 58, 140 | 灵魂能量、辉光、UI 主色 |
| 主色亮 | 灵魂蓝紫亮 | `#7B5EA8` | 123, 94, 168 | 灵魂高亮、UI 悬停态 |
| 主色暗 | 灵魂蓝紫暗 | `#2D1F5E` | 45, 31, 94 | 灵魂暗面、UI 按下态 |
| 强调色 | 绯红 | `#C41E3A` | 196, 30, 58 | 叙事强调、S-SN 相关、伤害数字 |
| 强调色暗 | 绯红暗 | `#8B0020` | 139, 0, 32 | 绯红暗面、血液效果 |
| 高亮色 | 晶化青蓝 | `#00D4FF` | 0, 212, 255 | 晶化高亮、UI 选中态、技能特效 |
| 高亮色暗 | 晶化青蓝暗 | `#0088AA` | 0, 136, 170 | 晶化暗面、冷却中 UI |
| 辅助色1 | 暗金 | `#C8A848` | 200, 168, 72 | UI 边框、稀有度标识 |
| 辅助色2 | 深灰 | `#2A2A3A` | 42, 42, 58 | UI 面板背景 |
| 辅助色3 | 墨绿 | `#1A3A2A` | 26, 58, 42 | 森林场景暗部、毒雾 |
| 辅助色4 | 暖灰 | `#8A8A7A` | 138, 138, 122 | UI 次要文字、非激活态 |

#### 3.6.2 色彩使用禁忌

| 禁忌 | 说明 |
|------|------|
| 蓝紫 `#4A3A8C` ~ `#7B5EA8` 是"灵魂"专属色 | 任何非灵魂元素不得使用此色系作为主色 |
| 绯红 `#C41E3A` 是"S-SN/叙事高潮"专属色 | 仅在 S-SN 相关、关键剧情、致命伤害、结局 UI 中使用 |
| 青蓝 `#00D4FF` 是"晶(HYC)"专属色 | 晶化矿脉、结晶病纹理、晶化粒子、晶能科技 UI 均使用此色 |
| 暗金 `#C8A848` 是"神圣/高等"标识色 | 用于史诗级物品、神圣神殿、传说级 UI 元素 |
| 最深色必须使用 `#1A1A2E` | 禁止使用纯黑 `#000000`，纯黑仅用于 UI 最深遮罩层 |

#### 3.6.3 森林地图色彩板示例

```
底色:  #1A3A2A (墨绿暗部)
中层:  #2D5A3D (森林绿)
亮部:  #4A7A5A (苔绿)
强调:  #4A3A8C (蓝紫灵魂光点)
光照: 低角度暖黄环境光 + 从树冠缝隙洒落的蓝紫灵魂光束
```

**氛围**: 神秘、幽深、生机与侵蚀并存。树木呈现蓝紫色灵魂光晕，空气中漂浮细微灵魂微尘。

---

## 4. 输出规范

### 4.1 目录结构（与实际项目一致）

```
Assets/ArtMaterials/
├── Boss/                              # Boss 资产
│   └── {Boss名称}/
│       ├── Concept/                   # 概念设定图
│       └── Frames/                    # 精灵帧（按状态分子文件夹）
│           ├── Idle/
│           ├── Walk/
│           ├── Attack/
│           ├── Skill/
│           └── Death/
├── Enemies/                           # 敌人资产
│   └── {敌人名称}/
│       ├── {拼音缩写}_idle/           # 如 FHCM_idle/
│       ├── {拼音缩写}_down/           # 如 FHCM_down/
│       ├── {拼音缩写}_up/
│       ├── {拼音缩写}_left/
│       └── {拼音缩写}_right/
├── Player/                            # 玩家角色
│   └── {角色名称}_{动作}/
│       ├── Down/                      # 如 墨语_待机/Down/
│       ├── Up/
│       ├── Left/
│       └── Right/
├── Summoner/                          # 召唤物资产
│   └── {召唤物名称}/
│       ├── {缩写}1_idle/              # 如 FHCM1_idle/
│       ├── {缩写}1_down/
│       ├── {缩写}1_up/
│       ├── {缩写}1_left/
│       └── {缩写}1_right/
│   └── 灵魂之核/
│       └── LHZH_1.png ~ LHZH_10.png
├── Weapons/                           # 武器
│   └── Sword/
├── SafeArea/                          # 安全区物件
│   ├── 篝火/
│   └── 炼药锅/
├── Map/                               # 地图资产
│   ├── 时空传送门/
│   ├── 时空裂缝/
│   └── {地图名称}/                    # 如 森林 1/
│       ├── 地面/
│       │   ├── 空绿草地.png
│       │   └── 土地/
│       │       ├── 土地（1）/
│       │       │   ├── 土地.png
│       │       │   └── 土地边缘/
│       │       │       ├── 四边.png
│       │       │       ├── 单边/      # 上/下/左/右
│       │       │       ├── 对边/      # 上下/左右
│       │       │       ├── 角/        # 左上/右上/左下/右下
│       │       │       └── 三边/      # 4种
│       ├── 树木/
│       ├── 水面/
│       │   ├── 空水面/
│       │   └── 水面边缘/
│       └── 装饰物/
├── Items/                             # 物品与UI
│   ├── 炼金/
│   │   ├── 炼金ui/
│   │   ├── 炼金产物/
│   │   ├── 炼金素材(基础)/
│   │   ├── 炼金素材(稀有)/
│   │   ├── 炼金素材(史诗)/
│   │   └── 炼金素材(传奇)/
│   └── 背包/
├── Effects/                           # 特效
│   ├── 战斗特效/
│   └── {名称}（召唤物特效/
│       ├── {缩写}1_summon/
│       └── {缩写}1_exit/
└── GameMainInterface/                 # 主界面
    └── 主界面/
```

### 4.2 命名规范（基于实际资产分析）

项目采用**混合命名策略**：角色类使用拼音首字母缩写，道具/地图类使用中文描述，方向统一使用英文。

| 资产类型 | 命名格式 | 示例 |
|---------|---------|------|
| Boss 概念图 | `{名称}_Concept_{序号}.png` | `Concept_00001.png` |
| 玩家角色 | `{名称缩写}_{方向}{动作}_{帧}.png` | `MY_downwaiting_1.png` |
| 敌人 | `{拼音缩写}_{方向}_{帧}.png` | `FHCM_idle_1.png`, `FHCM_down_2.png` |
| 召唤物 | `{缩写}1_{方向}_{帧}.png` | `FHCM1_idle_1.png`, `FHCM1_right_5.png` |
| 召唤特效 | `{缩写}1_{动作}_{帧}.png` | `FHCM1_summon_1.png`, `FHCM1_exit_4.png` |
| 掉落物 | `{缩写}_{帧}.png` | `LHZH_1.png` ~ `LHZH_10.png`（灵魂之核） |
| 安全区物件 | `{缩写}_{状态}_{帧}.png` | `GH_BURN_1.png`, `LYG_NONE_1.png` |
| 武器 | 英文单词 | `Singer.png`, `Writer.png` |
| 地图地块 | 中文描述 + 方向 | `土地.png`, `右上下.png`, `四边.png` |
| 地图装饰 | 中文描述 + 序号 | `树1.png`, `发光蘑菇3.png`, `落叶堆.png` |
| 物品图标 | 中文描述 | `星光草.png`, `召唤强化剂.png` |
| UI 面板 | 中文描述 | `炼金界面.png`, `背包界面.png` |
| 主界面元素 | 英文或编号 | `Start(Idle).png`, `Q1.png`, `S4.png` |

**命名规则总结：**
1. **方向统一英文**：`Up` / `Down` / `Left` / `Right`
2. **动作状态显式标注**：`idle` / `down` / `up` / `left` / `right` / `BURN` / `BURNING` / `NONE`
3. **召唤物与敌人区分**：通过数字后缀 `1` 区分（`FHCM` = 敌人，`FHCM1` = 召唤物）
4. **地图边缘系统**：`单边/对边/角/三边/四边`，直接对应 Tilemap 的 47 块自动瓦片逻辑
5. **避免特殊字符**：文件名不要包含空格、横杠等（已有 `FHCM1_summon_6 - .png` 异常案例）

### 4.3 尺寸标准

> **重要发现**：游戏内实际精灵尺寸**远小于 1024×1024**。根据《视觉风格指导手册》4.1 节和 4.4 节，精灵以 **32px/tile** 为基准计算：

#### 4.3.1 游戏内精灵目标尺寸（来自视觉风格指导手册）

| 资产类型 | 目标像素尺寸 | 头身比例 | 说明 |
|---------|-------------|---------|------|
| 普通敌人 | **32×32** | 2-2.5 头身 | 如腐化村民、结晶蜥蜴 |
| 精英敌人 | **48×48** | 2-2.5 头身 | 如石像鬼 |
| 地图 Boss | **64×64 ~ 80×80** | 2.5-4 头身 | 如时空守护者 |
| 记忆守护者 | **80×80** | — | 记忆碎片区域 Boss |
| S-SN (最终) | **96×96** | — | 最终 Boss |
| 玩家（墨语） | **~48×48** | 3-3.5 头身 | 以玩家为基准 1.0 缩放 |
| 召唤物 | **~32×32** | — | 与敌人类似尺寸 |
| 地图 Tile | **32×32** 或 **64×64** | — | 取决于 Tilemap 配置 |
| 道具图标 | **16×16 ~ 32×32** | — | UI 用的小尺寸 |
| 武器 | **~32×32** | — | 俯视视角 |

#### 4.3.2 ComfyUI 生成尺寸策略

由于 ComfyUI 工作流（Z-Image-Turbo）默认输出 **1024×1024**，与目标精灵尺寸存在巨大差距，采用以下策略：

| 策略 | 说明 | 适用场景 |
|------|------|---------|
| **高倍像素画生成** | 生成 1024×1024 像素画风格图，在 Unity 中用 **Nearest Neighbor** 降采样至目标尺寸（如 64×64） | 概念图、参考图 |
| **整数倍生成** | 修改工作流尺寸为精灵目标尺寸的整数倍（如 64×64 精灵 → 生成 512×512 = 8x），再用 Nearest Neighbor 降至 64×64 | 直接生成精灵帧 |
| **直接小尺寸生成** | 修改工作流 `EmptySD3LatentImage` 节点为目标尺寸（如 64×64），但 SD3 模型在小尺寸下可能效果不佳 | 不推荐 |

**推荐做法**：
1. **概念图**：保持 1024×1024，`digital painting` 风格
2. **精灵帧**：先生成 512×512（目标尺寸 8x），再降采样至目标尺寸
3. 降采样时必须使用 **Nearest Neighbor**（最近邻）算法，以保持像素画的锐利边缘
4. 生成时提示词必须强调 `pixel art` 和 `crisp pixel edges`，避免产生中间色调

#### 4.3.3 生成尺寸速查表

| 目标精灵尺寸 | 建议生成尺寸 | 缩放倍数 | 备注 |
|-------------|-------------|---------|------|
| 32×32 | 512×512 | 16x | 普通敌人、道具 |
| 48×48 | 512×512 | ~10.7x | 精英、玩家 |
| 64×64 | 512×512 | 8x | 小型 Boss |
| 80×80 | 640×640 | 8x | 地图 Boss |
| 96×96 | 768×768 | 8x | 最终 Boss |
| 概念图 | 1024×1024 | — | 参考用，不直接用于游戏 |

### 4.4 帧数标准（基于视觉风格指导手册）

> **来源**：《幻宫_视觉风格指导手册》第 5.1 节（玩家动画）、第 5.4 节（普通敌人动画帧规格表）。
> **注意**：当前实际资产帧数**低于**手册标准，ComfyUI 生成时应以手册标准为目标，而非现有资产。

#### 4.4.1 玩家角色动画标准

| 动画状态 | 最少帧数 | 推荐帧数 | 播放速度 | 循环方式 | 备注 |
|---------|---------|---------|---------|---------|------|
| Idle（待机） | **6帧** | 8-12帧 | 8-10 FPS | ping-pong | 含呼吸感微动，非完全静止 |
| Walk（行走） | **6帧** | 8帧 | 10-12 FPS | 顺序循环 | 需与移动速度匹配，避免滑步 |
| Run（奔跑） | **6帧** | 8帧 | 12-15 FPS | 顺序循环 | Shift 加速时切换 |
| Attack（攻击） | **8帧** | 10-14帧 | 12-15 FPS | 单次播放 | 快起慢收：蓄力→挥砍→收招 |
| Dash（闪避） | **4帧** | 6帧 | 15-20 FPS | 单次播放 | 快速模糊效果 |
| Hurt（受击） | **3帧** | 4帧 | 12 FPS | 单次播放 | 短暂闪白 + 击退 |
| Death（死亡） | **8帧** | 10-12帧 | 8-10 FPS | 单次播放 | 崩解为粒子效果衔接 |
| Summon（召唤） | **6帧** | 8帧 | 10 FPS | 单次播放 | 从地面法阵中升起 |

**方向要求**：玩家需要 **4 方向**（Down/Up/Left/Right）全部状态。

**当前资产差距**：
- 墨语 Idle 实际仅 3 帧（手册要求 6 帧）
- 墨语 Walk 实际仅 4 帧（手册要求 6 帧）
- 墨语 Idle Up 仅 1 帧（严重不平衡）

#### 4.4.2 普通敌人动画标准

| 敌人名称 | 分类 | Idle | Walk | Attack | Hurt | Death | Aggro | 特殊状态 |
|---------|------|------|------|--------|------|-------|-------|---------|
| 腐化村民 | 灵魂残留 | 6帧@8FPS | 6帧@10FPS | 6帧@12FPS | 3帧@12FPS | 6帧@8FPS | 4帧@10FPS | — |
| 结晶蜥蜴 | 晶化生物 | 4帧@8FPS | 6帧@10FPS | 5帧@12FPS | 3帧@12FPS | 5帧@8FPS | 3帧@10FPS | — |
| 沼泽潜伏者 | 灵魂残留 | 5帧@8FPS | 6帧@10FPS | 6帧@12FPS | 3帧@12FPS | 6帧@8FPS | 4帧@10FPS | Stealth 4帧@8FPS |
| 冰原狼 | 晶化生物 | 5帧@8FPS | 6帧@12FPS | 6帧@12FPS | 3帧@12FPS | 6帧@8FPS | 4帧@10FPS | — |
| 机械残骸 | 科技造物 | 4帧@6FPS | 5帧@8FPS | 5帧@10FPS | 3帧@12FPS | 6帧@8FPS | 3帧@8FPS | — |
| 骷髅战士 | 灵魂残留 | 6帧@8FPS | 6帧@10FPS | 6帧@12FPS | 3帧@12FPS | 5帧@8FPS | 4帧@10FPS | — |
| 怨魂 | 灵魂残留 | 6帧@8FPS | — | 5帧@10FPS | 3帧@12FPS | 5帧@8FPS | 4帧@10FPS | 漂浮型，无 Walk |
| 石像鬼 | 科技造物 | 6帧@8FPS | — | 6帧@12FPS | 4帧@12FPS | 6帧@8FPS | 5帧@10FPS | Summon 4帧, Flying 6帧 |

**方向要求**：
- 人形敌人（腐化村民、骷髅战士）：**4 方向**全部状态
- 四足敌人（结晶蜥蜴、冰原狼）：**4 方向**，Left/Right 可镜像复用
- 漂浮型（怨魂）：**4 方向**，Idle 帧可复用
- 机械体（机械残骸、石像鬼）：**4 方向**，Left/Right 可镜像复用

#### 4.4.3 Boss 动画标准

| Boss 类型 | 尺寸 | Idle | Walk | Attack | Skill | Death | 方向 |
|----------|------|------|------|--------|-------|-------|------|
| 地图 Boss（时空守护者） | 64×64 ~ 80×80 | 6帧 | 6帧 | 8帧 | 6帧 | 8帧 | 单方向 |
| 记忆守护者 | 80×80 | 6帧 | 6帧 | 8帧 | 6帧 | 8帧 | 单方向 |
| S-SN（最终） | 96×96 | 8帧 | 8帧 | 10帧 | 8帧 | 10帧 | 单方向 |

**Boss 特殊说明**：
- Boss **不需要 4 方向动画**，仅需单方向（默认朝下/朝向玩家）
- 地图 Boss 规格高于普通敌人，低于最终 Boss
- 最终 Boss S-SN 拥有最丰富的动画状态和帧数

#### 4.4.4 环境/特效动画

| 资产类型 | 动画状态 | 帧数 | 循环方式 | 备注 |
|---------|---------|------|---------|------|
| 灵魂之核 | Idle | 10帧 | 顺序循环 | 掉落物旋转/浮动 |
| 篝火 | BURN / BURNING | 多状态 | 状态切换 | NONE → BURN → BURNING |
| 炼药锅 | 工作状态 | 多状态 | 状态切换 | NONE + 工作状态 |
| 水面（空水面） | 流动 | 8帧 | 顺序循环 | — |
| 水面边缘 | 流动 | 8帧 | 顺序循环 | 左右/上下各 8 帧 |
| 水面角落 | 流动 | 8帧 | 顺序循环 | 4 个方向各 8 帧 |
| 召唤特效 | summon | 6帧 | 单次播放 | 召唤物出场 |
| 召唤特效 | exit | 6帧 | 单次播放 | 召唤物退场 |

### 4.5 图片格式与 Unity 导入设置

#### 4.5.1 图片格式

- **格式**：PNG（带透明通道）
- **色彩空间**：sRGB

#### 4.5.2 Unity TextureImporter 设置（基于实际 `.meta` 分析）

所有现有资产使用**高度统一的导入模板**，生成新资产时应保持一致：

| 参数 | 值 | 说明 |
|------|-----|------|
| `textureType` | `8` → Sprite (2D and UI) | 所有资产统一为 Sprite 类型 |
| `spriteMode` | `1` → Single | 单图模式，非图集/Sheet |
| `spritePixelsToUnits` | `100` | 100 像素 = 1 世界单位 |
| `filterMode` | `1` → Bilinear | 双线性过滤 |
| `wrapU / wrapV / wrapW` | `1` → Repeat | 纹理重复模式 |
| `maxTextureSize` | `2048` | 最大纹理尺寸 |
| `textureFormat` | `1` → Automatic | 自动格式 |
| `alphaIsTransparency` | `1` | 启用 Alpha 透明 |
| `alphaUsage` | `1` | 使用 Alpha 通道 |
| `spriteGenerateFallbackPhysicsShape` | `1` | 自动生成物理轮廓 |
| `enableMipMap` | `0` | **关闭 Mipmap**（2D 手绘/精灵不需要） |
| `sRGBTexture` | `1` | sRGB 颜色空间 |
| `nPOTScale` | `0` → None | 非 2 的幂不缩放 |
| `spritePivot` | `{x: 0.5, y: 0.5}` | 中心点居中 |
| `spriteMeshType` | `1` → Full Rect | 完整矩形网格 |

**注意**：
- 未使用 Sprite Sheet / Atlas（所有 `spriteSheet.sprites: []`）
- 未使用 Flipbook 多行列配置（`flipbookRows/Columns: 1`）
- 部分 `.meta` 包含 `Windows Store Apps` 平台设置，部分没有，属历史不一致，不影响功能

---

## 5. 故障排查

### 5.1 连接问题

| 症状 | 可能原因 | 排查步骤 |
|------|---------|---------|
| 连接超时 | 服务器未启动 | 1. 检查服务器PC是否开机 2. 运行 ComfyUI 服务 3. 确认端口 8188 未被占用 |
| 连接被拒绝 | 防火墙/网络问题 | 1. `ping 10.150.164.64` 2. 检查防火墙规则 3. 确认两台PC在同一网络 |
| HTTP 405 | 端点方法不匹配 | 正常现象（GET /api/prompt 返回405表示服务器在线） |

### 5.2 生成问题

| 症状 | 可能原因 | 排查步骤 |
|------|---------|---------|
| 返回空结果 `{}` | 任务仍在队列中 | 等待并轮询，默认超时10分钟 |
| 生成超时 | 任务卡住 | 1. 调用 `/api/interrupt` 中断 2. 减少 steps 参数 3. 检查服务器GPU状态 |
| 图片质量差 | 提示词问题 | 1. 检查提示词权重 2. 增加 steps（最大20） 3. 调整 cfg 参数 |
| 风格不一致 | 提示词缺少风格词 | 确保所有生成使用 `GetFullPositivePrompt()` 拼接基础风格词 |
| qwen-edit 失败 | 参考图片不存在 | 1. 先上传参考图片到服务器 2. 确认 `LoadImage` 节点引用正确文件名 |

### 5.3 WebSocket 问题

| 症状 | 可能原因 | 排查步骤 |
|------|---------|---------|
| 连接失败 | 协议不匹配 | 确认使用 `ws://` 而非 `http://` |
| 连接断开 | 网络不稳定 | 自动重连（最多3次，间隔2秒） |
| 收不到进度 | clientId 不匹配 | 确保 `QueuePrompt` 和 `Connect` 使用相同 clientId |

### 5.4 Unity 侧问题

| 症状 | 可能原因 | 排查步骤 |
|------|---------|---------|
| 编译错误 | 缺少 Newtonsoft.Json | 确认 `Newtonsoft.Json` 包已安装 |
| 编辑器无响应 | 异步操作阻塞 | 使用 `await` 而非 `.Result` 或 `.Wait()` |
| 保存路径错误 | 目录不存在 | 调用 `EnsureDirectoryExists()` 创建目录 |

### 5.5 紧急恢复

1. **中断所有生成**：`POST /api/interrupt`
2. **清除队列**：重启 ComfyUI 服务器
3. **重置 WebSocket**：`Disconnect()` → `ClearAllHandlers()` → `Connect()`

---

## 6. 已知问题与注意事项

基于对 `Assets/ArtMaterials/` 的全面分析，以下问题在生成新资产时需特别注意：

### 6.1 现有资产缺陷

| 问题 | 位置 | 风险等级 | 说明与建议 |
|------|------|---------|-----------|
| **缺失 `.meta` 文件** | `Boss/时空守护者/Concept/Concept_00001.png` | 🔴 高 | 该图片无对应的 `.meta`，Unity 尚未正式导入。重新导入会导致 GUID 变化，可能破坏引用。生成新 Boss 资产后需确保 `.meta` 正确生成。 |
| **帧数严重不对称** | `Player/墨语_待机/Up/` 仅 1 帧 | 🟡 中 | 其他方向待机有 3~5 帧，Up 方向仅 1 帧。运行时切换到此方向可能出现明显卡顿。新资产应避免此类不平衡。 |
| **孤儿 `.meta` 文件** | 根目录 `Test.meta` | 🟢 低 | 无对应文件夹，Unity 可能报黄警告。不影响功能。 |
| **空文件夹残留** | `Map/森林.meta` | 🟢 低 | 存在 meta 但文件夹内无文件，可能是已废弃的旧版本。 |
| **文件名含特殊字符** | `Effects/.../FHCM1_summon_6 - .png` | 🟡 中 | 文件名包含空格+横杠（`6 - `），代码中加载时若未做 URL/路径编码可能出错。新资产命名应避免空格和特殊符号。 |

### 6.2 生成新资产时的注意事项

1. **`.meta` 文件必须存在**：生成图片后需确保 Unity 正确生成 `.meta`，否则 GUID 引用会断裂。
2. **帧数平衡**：同一角色的各方向动画帧数应尽量保持一致，避免出现 1 帧 vs 5 帧的极端差异。
3. **命名规范一致性**：
   - 角色/敌人/召唤物：拼音缩写 + 英文方向 + 序号
   - 道具/地图/UI：中文描述
   - 禁止空格、横杠等特殊字符
4. **Boss 为单方向**：Boss 不需要 4 方向动画，仅需单方向（默认朝下/朝向玩家）。
5. **召唤物应比敌人更精细**：现有召唤物 Idle 13 帧 vs 敌人 Idle 5 帧，生成召唤物时应保持此标准。
6. **特效分两种形式**：
   - 帧动画 PNG 序列（如召唤/退场特效）
   - Shader/材质驱动（如战斗特效 `GitHit Material.mat`）
   需根据特效类型选择合适的生成方式。
7. **地图 Tile 边缘系统**：生成地图地块时需同时生成完整的 47 块自动瓦片集（单边 4 + 对边 2 + 角 4 + 三边 4 + 四边 1 + 基础 1 + 空绿地 1）。

### 6.3 当前资产完成度概览

| 类别 | 完成度 | 说明 |
|------|--------|------|
| 玩家（墨语） | ⚠️ 部分完成 | 待机 Up 仅 1 帧，其他方向完整 |
| 敌人（腐化村民） | ✅ 完整 | Idle + 四方向 Move 共 23 帧 |
| 召唤物（腐化村民） | ✅ 完整 | 比敌人更精细，Idle 13 帧 |
| Boss（时空守护者） | 🔴 仅概念图 | 1 张概念图，无精灵帧，无 `.meta` |
| 地图（森林 1） | ✅ 完整 | 完整的 Autotile 边缘瓦片 + 动态水面 + 装饰物 |
| 物品/炼金 | ✅ 完整 | 按稀有度分级，47 个图标 |
| 武器 | ⚠️ 部分完成 | 仅 Sword 类别 2 个图标 |
| 主界面 UI | ✅ 完整 | 背景、按钮、装饰元素 |
| 安全区 | ✅ 完整 | 篝火 + 炼药锅多状态 |
| 特效 | ⚠️ 部分完成 | 召唤物特效完整，战斗特效依赖材质 |

---

## 7. 关联代码文件更新提示

以下代码文件需根据本文档更新进行同步修改（v2.2 关键变更）：

| 文件 | 位置 | 需修改内容 |
|------|------|-----------|
| `ComfyUIPromptTemplates.cs` | `Assets/Scripts/Editor/ComfyUI/` | ✅ 1. 基础风格词已拆分为概念/精灵两套（v1.2）<br>✅ 2. 模板尺寸已更新为目标生成尺寸（v1.2）<br>✅ 3. 已移除对旧 `EmptySD3LatentImage` 节点的引用（v2.0） |
| `ComfyUIAssetGenerator.cs` | `Assets/Scripts/Editor/ComfyUI/` | ✅ 1. Nearest Neighbor 降采样已添加（v1.2）<br>✅ 2. 使用 `client.BuildTextToImageWorkflow()` 动态构建（v2.1）<br>✅ 3. 使用 `client.BuildImageEditWorkflow()` 动态构建（v2.1）<br>✅ 4. 已移除对静态 `workflows/*.json` 文件的依赖（v2.1） |
| `ComfyUIClient.cs` | `Assets/Scripts/Editor/ComfyUI/` | ✅ 1. 已新增 `GetObjectInfo()` / `GetObjectInfo(nodeClass)` 方法（v2.1）<br>✅ 2. `BuildTextToImageWorkflow()` 已改为动态 JObject 构建（v2.1）<br>✅ 3. `BuildImageEditWorkflow()` 已改为动态 JObject 构建（v2.1）<br>✅ 4. 已删除旧 `LoadWorkflowTemplate()`、`InjectXxx()` 静态方法（v2.1） |
| `ComfyUIWindow.cs` | `Assets/Scripts/Editor/ComfyUI/` | ✅ 1. 已修复废弃方法调用，改用 `GetConceptXxx` / `GetSpriteXxx`（v2.1）<br>✅ 2. `OnEnable` 时自动打开通信面板（v2.2）<br>✅ 3. 新增"通信面板"按钮（v2.2） |
| `ComfyUICommunicationPanel.cs` | `Assets/Scripts/Editor/ComfyUI/` | ✅ **新增**（v2.2）：通信面板 EditorWindow，连接 `ws://10.150.164.64:8189`，实时显示 GPU 状态、队列、优化建议 |
| `ComfyUIWebSocket.cs` | `Assets/Scripts/Editor/ComfyUI/` | `SaveImageWebsocket` 节点会将图片直接通过 WebSocket 推送，需确认 `OnImageReady` 事件正确处理此通道 |
| `workflows/*.json` | `Assets/Scripts/Editor/ComfyUI/workflows/` | 保留作为参考模板，代码不再硬依赖这些文件（可手动清理） |

---

## 8. 版本记录

| 版本 | 日期 | 修改人 | 变更摘要 |
|------|------|--------|---------|
| 1.0 | 2026-06-13 | 主策划AI | 初始版本，涵盖服务器配置、工作流、提示词、输出规范、故障排查 |
| 1.1 | 2026-06-13 | 主策划AI | 基于 `Assets/ArtMaterials/` 全面分析更新：补充实际目录结构、命名规范、帧数标准、Unity 导入设置、已知问题与资产完成度概览 |
| 1.2 | 2026-06-13 | 主策划AI | 基于实际资产图像分析 + 《幻宫_视觉风格指导手册》重大更新：核心修正为像素画风格、新增色彩体系、重写尺寸/帧数标准 |
| **2.0** | **2026-06-13** | **主策划AI** | **服务器模型切换 + 动态工作流重构**：<br>1. **服务器模型变更**：Checkpoint 从 Z-Image-Turbo 替换为 `动漫 primemix_v21`<br>2. **重写第 1 章**：更新服务器信息（ComfyUI 0.20.1、PyTorch 2.11.0、RTX 3060、4 个模型）<br>3. **重写第 2 章**：废弃固定 workflow 文件方案，改为动态构建 workflow JSON<br>4. **新增 2.2**：通用文生图工作流模板（CheckpointLoaderSimple + KSampler + SaveImageWebsocket）<br>5. **新增 2.3**：KSampler 参数调优指南<br>6. **新增 2.4**：图生图工作流模板（LoraLoader + Qwen-Image-Edit LoRA）<br>7. **新增 2.5**：工作流构建完整流程（8 步）<br>8. **新增 2.6**：常用节点速查表（11 类）<br>9. **更新第 7 章**：新增 `GetObjectInfo()`、`BuildTextToImageWorkflow()` 等代码同步需求 |
| **2.1** | **2026-06-14** | **主策划AI** | **代码同步 + 服务器文档更新**：<br>1. **ComfyUIClient.cs**：重写 `BuildTextToImageWorkflow()` / `BuildImageEditWorkflow()` 为动态 JObject 构建，新增 `GetObjectInfo()` 方法，删除旧静态方法<br>2. **ComfyUIWindow.cs**：修复废弃方法警告，改用 `GetConceptXxx` / `GetSpriteXxx`<br>3. **第 1.8 节**：更新实时通信服务文档，新增 `welcome`/`ping`/`pong`/`hot_reload`/`message_history`/`ack` 消息类型<br>4. **第 1.8 节**：新增 `server_status` 数据结构、优化建议规则表、连接流程说明<br>5. **第 1.8 节**：新增 `/history`、`/notify` HTTP 端点<br>6. **第 7 章**：更新代码同步清单，标注已完成项 |
| **2.2** | **2026-06-14** | **主策划AI** | **通信面板强制规则**：<br>1. **新增第 0 章**：每次使用 ComfyUI 前必须先打开通信面板的强制前置规则<br>2. **新增 `ComfyUICommunicationPanel.cs`**：通信面板 EditorWindow，连接 `ws://10.150.164.64:8189`，实时显示 GPU 状态、队列、优化建议<br>3. **修改 `ComfyUIWindow.cs`**：`OnEnable` 时自动打开通信面板，新增"通信面板"按钮<br>4. **第 7 章**：更新代码同步清单，新增通信面板文件 |