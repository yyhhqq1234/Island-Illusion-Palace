#!/usr/bin/env python3
"""
ComfyUI 美术素材生产客户端 v4.1
Unity 项目深度集成版 - 支持项目文件分析、需求提取、AI 提示词生成

v4.1 新增功能：
- 模块8: ServerMonitor（服务器实时状态监控）- WebSocket 实时连接 + HTTP 轮询降级
- 增强版顶部工具栏 - VRAM/队列/客户端数/运行时间实时显示
- 可折叠服务器详情面板 - GPU显存条、已加载模型、优化建议

v4.0 功能保留：
- 模块1: AI 大模型集成 (LLM Integration) - 提示词优化/生成、工作流推荐
- 模块2: ComfyUI 服务器资源浏览器 - 模型列表、节点浏览器、自定义节点
- 模块3: 自定义工作流配置器 - 可视化工作流构建与编辑
- 模块4: AI 一键最优工作流生成 - LLM 自动构建最优工作流

功能：
- 服务器连接状态监控（WebSocket 8189）
- 资产生成（概念图 + 精灵帧）
- 提示词模板管理
- 批量生成队列
- 历史记录
- AI 助手（提示词优化/生成）
- 服务器资源浏览
- 工作流编辑器
- 配置持久化
- 项目文件分析与智能资产生成

依赖：pip install requests websocket-client
"""

import tkinter as tk
from tkinter import ttk, messagebox, filedialog, scrolledtext
import threading
import json
import os
import sys
import time
import random
import datetime
import re
from urllib.parse import urlencode
from collections import OrderedDict

try:
    import requests
except ImportError:
    print("请安装依赖: pip install requests")
    sys.exit(1)

# WebSocket 客户端（可选，未安装时 ServerMonitor 自动降级为 HTTP 轮询）
WEBSOCKET_AVAILABLE = False
try:
    import websocket
    WEBSOCKET_AVAILABLE = True
except ImportError:
    pass

# ==========================================
# 配置
# ==========================================
SERVER_URL = "http://10.150.164.64:8189"  # 统一 API 入口（v3.2 服务器）
COMFY_URL = "http://10.150.164.64:8189"   # ComfyUI 通过 8189 代理
WS_URL = "ws://10.150.164.64:8189"        # WebSocket 实时通信
FALLBACK_URL = "http://10.150.164.64:8188"  # 回退：直接访问 ComfyUI 8188

OUTPUT_BASE = r"d:\Program Files\Unity\U3Dproject\Island-Illusion-Palace\Assets\ArtMaterials"
# [已弃用] DEFAULT_CHECKPOINT — 服务器 v3.2 统一管理模型，客户端不再需要手动指定
DEFAULT_CHECKPOINT = "sd_xl_base_1.0.safetensors"
CONFIG_FILE = os.path.join(os.path.dirname(os.path.abspath(__file__)), "comfyui_client_config.json")
WORKFLOW_DIR = os.path.join(os.path.dirname(os.path.abspath(__file__)), "workflows")

# 导入服务器 SDK（v3.2 统一 API 客户端）
try:
    from comms_client import CommsClient
    HAS_COMMS_SDK = True
except ImportError:
    HAS_COMMS_SDK = False
    print("[ComfyUIClient] comms_client.py 未找到，将使用 HTTP 直接调用 8189 API")

# ==========================================
# 提示词模板 (基于项目现有素材风格分析 v3)
# ==========================================
#
# 风格基准：简约像素画 (Simple Pixel Art)
# - 像素颗粒清晰可见，无抗锯齿
# - 简洁的配色方案，色块分明
# - 纯白背景，角色居中
# - 头身比 1:2 ~ 1:2.5（Q版比例）
# - 四方向：正面(Down)/侧面(Left,Right)/背面(Up)
#

CONCEPT_BASE_POSITIVE = (
    "dark fantasy concept art, digital painting, dramatic lighting, "
    "blue-purple soul glow luminescence, geometric cyan crystal erosion, "
    "rim light, high contrast, detailed texture, atmospheric perspective"
)

CONCEPT_NEGATIVE = (
    "photorealistic, 3D render, CGI, smooth vector art, flat design, "
    "bright sunny, low resolution, blurry, watermark, text, signature, ugly, "
    "distorted, bad anatomy, extra limbs, fused fingers, low quality, jpeg artifacts"
)

# ====== 精灵帧提示词 (简约像素画风格) ======
SPRITE_BASE_POSITIVE = (
    "simple pixel art, minimalist game sprite, dark fantasy, "
    "blue-purple soul glow luminescence, geometric cyan crystal erosion, "
    "clean pixel lines, flat color blocks, no anti-aliasing, no gradients, "
    "white background, centered composition, simple shading, limited color palette"
)

SPRITE_NEGATIVE = (
    "photorealistic, 3D render, CGI, smooth vector art, flat design, bright sunny, "
    "hand-drawn lineart, smooth outline, cel-shading, painterly texture, "
    "visible brushstrokes, anti-aliasing, smooth edges, gradient shading, complex lighting, "
    "blurry, watermark, text, signature, ugly, distorted, bad anatomy, extra limbs, "
    "fused fingers, low quality, jpeg artifacts, crowded composition, background detail"
)

STATE_PROMPTS = {
    "idle":   "front-facing view, standing idle pose, neutral stance, arms at sides",
    "walk":   "front-facing view, walking pose, one foot forward mid-step",
    "attack": "front-facing view, attack pose, weapon raised mid-swing, dynamic action stance",
    "skill":  "front-facing view, casting spell pose, hands channeling energy",
    "death":  "front-facing view, defeated pose, collapsing forward, body tilting",
}

# 方向提示词（用于四方向精灵帧生成）
DIRECTION_PROMPTS = {
    "down":  "front-facing view, facing camera directly, symmetrical pose",
    "up":    "back-facing view, seen from behind, back of head and shoulders visible",
    "left":  "left-side profile view, facing left, body turned 90 degrees left",
    "right": "right-side profile view, facing right, body turned 90 degrees right",
}

# 资产模板 (assetType -> {prompt, targetWidth, targetHeight, category, genWidth, genHeight})
ASSET_TEMPLATES = OrderedDict()

# ========== 玩家 (简约像素画, ~2.5头身) ==========
ASSET_TEMPLATES["墨语"] =      {"prompt": "Mo Yu, dark blue-black short hair, purple eyes, neutral expression, dark blue-gray outfit, subtle soul aura glow, pixel art style, 2.5-heads-tall", "targetW": 64, "targetH": 64, "cat": "Player", "genW": 512, "genH": 512}
ASSET_TEMPLATES["莎娜"] =      {"prompt": "Shana, crimson long hair, warm gentle eyes, kind smile, elegant mage robe with scarlet accents, soft red energy aura, pixel art style, 2.5-heads-tall", "targetW": 64, "targetH": 64, "cat": "Player", "genW": 512, "genH": 512}

# ========== 普通敌人 (简约像素画, ~2头身) ==========
ASSET_TEMPLATES["腐化村民"] =  {"prompt": "corrupted villager, gray-green sickly skin, messy dark hair, glowing red eyes, tattered brown clothing, hunched posture, emaciated build, pixel art style, 2-heads-tall", "targetW": 32, "targetH": 32, "cat": "Enemy", "genW": 512, "genH": 512}
ASSET_TEMPLATES["暗影之刃"] =  {"prompt": "shadow blade assassin, dark hooded figure, dual daggers, cyan energy trail, shadowy cloak, agile stance, 2-heads-tall", "targetW": 32, "targetH": 32, "cat": "Enemy", "genW": 512, "genH": 512}
ASSET_TEMPLATES["水晶寄生体"] = {"prompt": "crystal parasite monster, insectoid creature, cyan crystal growth on body, scuttling legs, unnatural movement, 2-heads-tall", "targetW": 32, "targetH": 32, "cat": "Enemy", "genW": 512, "genH": 512}
ASSET_TEMPLATES["沼泽潜伏者"] = {"prompt": "swamp lurker monster, amphibious creature, half-submerged in mud, slimy skin, yellow-green toxic mist, webbed hands, bulging eyes, 2-heads-tall", "targetW": 32, "targetH": 32, "cat": "Enemy", "genW": 512, "genH": 512}

# ========== 精英 (48x48 target) ==========
ASSET_TEMPLATES["灵魂吞噬者"] = {"prompt": "soul devourer monster, large floating entity, swirling souls around it, dark void body, multiple eyes, tentacle-like appendages, 3-heads-tall", "targetW": 48, "targetH": 48, "cat": "Enemy", "genW": 768, "genH": 768}
ASSET_TEMPLATES["熔岩元素"] =   {"prompt": "lava elemental, humanoid magma creature, molten rock body, orange-yellow lava veins, heat distortion, volcanic rock texture, 3.5-heads-tall", "targetW": 48, "targetH": 48, "cat": "Enemy", "genW": 768, "genH": 768}
ASSET_TEMPLATES["机械构造体"] = {"prompt": "mechanical construct, large humanoid robot, brass and steel body, cyan energy core in chest, steam pipes, gear mechanisms visible, 3.5-heads-tall", "targetW": 48, "targetH": 48, "cat": "Enemy", "genW": 768, "genH": 768}

# ========== BOSS (简约像素画, ~2.5-3头身) ==========
ASSET_TEMPLATES["时空守护者"] = {"prompt": "time guardian boss, armored entity, clockwork mechanisms embedded in armor, golden sacred markings, floating time gears, hourglass core, imposing stance, pixel art style, 2.5-heads-tall", "targetW": 64, "targetH": 64, "cat": "Boss", "genW": 512, "genH": 512}
ASSET_TEMPLATES["记忆守护者"] = {"prompt": "memory guardian boss, ethereal humanoid figure, fragmented body made of memory shards, blue-purple soul glow, translucent, floating, 4-heads-tall", "targetW": 80, "targetH": 80, "cat": "Boss", "genW": 640, "genH": 640}
ASSET_TEMPLATES["S-SN"] =       {"prompt": "Scarlet Soul Shana, divine female figure, crimson red and blue-purple dual energy, floating, crystalline wings, elegant battle dress, 4-heads-tall", "targetW": 96, "targetH": 96, "cat": "Boss", "genW": 768, "genH": 768}

# ========== 地图 Tile ==========
ASSET_TEMPLATES["废墟都市Tile"] = {"prompt": "ruined city tileset, collapsed buildings, broken streets, cyan neon signs still glowing, rusted metal, top-down orthographic view, seamless tileable", "targetW": 64, "targetH": 64, "cat": "MapTile", "genW": 512, "genH": 512}
ASSET_TEMPLATES["遗忘庄园Tile"] = {"prompt": "forgotten manor tileset, decaying noble mansion interior, faded gold decorations, cracked marble floors, Victorian gothic, top-down orthographic view, seamless tileable", "targetW": 64, "targetH": 64, "cat": "MapTile", "genW": 512, "genH": 512}
ASSET_TEMPLATES["古代神殿Tile"] = {"prompt": "ancient temple tileset, white stone temple, golden sacred engravings, cracked stone pillars, divine rays from ceiling, top-down orthographic view, seamless tileable", "targetW": 64, "targetH": 64, "cat": "MapTile", "genW": 512, "genH": 512}
ASSET_TEMPLATES["实验室碎片Tile"] = {"prompt": "laboratory tileset, sterile white sci-fi lab, cyan glowing specimen tanks, metal floors, holographic displays, abandoned research facility, top-down orthographic view, seamless tileable", "targetW": 64, "targetH": 64, "cat": "MapTile", "genW": 512, "genH": 512}

# ========== 道具 (32x32 target) ==========
ASSET_TEMPLATES["营火"] =   {"prompt": "campfire, blue-purple soul flame, stone circle, glowing embers, warm light radius, safe haven, top-down view", "targetW": 32, "targetH": 32, "cat": "Prop", "genW": 512, "genH": 512}
ASSET_TEMPLATES["炼药锅"] = {"prompt": "alchemy cauldron, large iron cauldron, blue-purple bubbling liquid, crystal decorations on rim, alchemy symbols, glowing runes, top-down view", "targetW": 32, "targetH": 32, "cat": "Prop", "genW": 512, "genH": 512}
ASSET_TEMPLATES["时空门"] = {"prompt": "time portal, cyan and blue-purple swirling vortex, floating crystal fragments around portal, energy rings, dimensional gateway, top-down view", "targetW": 32, "targetH": 32, "cat": "Prop", "genW": 512, "genH": 512}
ASSET_TEMPLATES["时空裂隙"] = {"prompt": "time rift, unstable dimensional tear, jagged edges, cyan and blue-purple energy leaking, reality distortion, random sparks, top-down view", "targetW": 32, "targetH": 32, "cat": "Prop", "genW": 512, "genH": 512}


# ==========================================
# LLM System Prompts
# ==========================================
LLM_OPTIMIZE_PROMPT = """你是一个专业的游戏美术 AI 提示词工程师。你的任务是将用户的简单描述转换为高质量的 Stable Diffusion / ComfyUI 提示词。

## 项目风格规范（必须遵守）
- 本项目精灵帧采用 **简约像素画 (Simple Pixel Art)** 风格
- 像素颗粒清晰可见，无抗锯齿，无平滑边缘
- 简洁配色方案，色块分明，扁平化上色
- 纯白背景，角色居中
- 头身比 1:2 ~ 1:2.5（Q版比例）
- 四方向：正面(Down)/侧面(Left,Right)/背面(Up)
- 统一视觉语言：dark fantasy, soul glow blue-purple #4A3A8C, crystal erosion cyan #00D4FF

## 规则：
1. 正向提示词使用英文，权重语法 (tag:weight)
2. 精灵帧必须包含：(simple pixel art), (minimalist game sprite), clean pixel lines, flat color blocks, no anti-aliasing
3. 概念图可以保留数字绘画风格标签
4. 反向提示词必须排除手绘/插画风格：hand-drawn lineart, smooth outline, cel-shading, painterly texture

输出格式（严格按此格式输出，不要添加其他文字）：
POSITIVE: [优化后的正向提示词]
NEGATIVE: [优化后的反向提示词]"""

LLM_GENERATE_PROMPT = """你是一个专业的游戏美术 AI 提示词工程师。根据给定的资产类型和生成阶段，自动生成高质量的 Stable Diffusion / ComfyUI 提示词。

## 项目风格规范（必须遵守）
- 本项目精灵帧采用 **简约像素画 (Simple Pixel Art)** 风格
- 像素颗粒清晰可见，无抗锯齿，无平滑边缘
- 简洁配色方案，色块分明，扁平化上色
- 纯白背景，角色居中
- 头身比 1:2 ~ 1:2.5（Q版比例）
- 四方向：正面(Down)/侧面(Left,Right)/背面(Up)
- 统一视觉语言：dark fantasy, soul glow blue-purple #4A3A8C, crystal erosion cyan #00D4FF

## 规则：
1. 正向提示词使用英文，权重语法 (tag:weight)
2. 概念图阶段：注重整体氛围、构图、光影、数字绘画质感
3. 精灵帧阶段：注重简约像素风格、像素线条、扁平色块、纯白背景、单帧动作姿态
4. 根据资产类型选择合适的角色描述、姿态、头身比例
5. 反向提示词包含所有需要排除的质量问题

输出格式（严格按此格式输出，不要添加其他文字）：
POSITIVE: [生成的正向提示词]
NEGATIVE: [生成的反向提示词]"""

LLM_WORKFLOW_PROMPT = """你是一个 ComfyUI 工作流专家。请根据用户需求生成最优的 ComfyUI workflow JSON。

## 可用资源信息
{server_resources_summary}

## 用户需求
{user_requirement}

## Workflow JSON 格式要求
- key 是字符串形式的节点ID（如 "1", "2"...）
- 每个节点包含 class_type 和 inputs
- inputs 中的引用使用 ["目标节点ID", output_index] 格式
- 必须以 SaveImage 或 SaveImageWebsocket 结尾

## 优化原则
1. 选择最强的可用模型（优先使用最新/最大的 checkpoint）
2. 使用最高效的采样器（euler_ancestral 或 dpmpp_2m）
3. 根据任务类型选择最佳步数（概念图 25-30 步，精灵帧 20-25 步）
4. 如果是 img2img，denoise 设为 0.45-0.55
5. 合理利用 ControlNet（如果有）提升姿态控制精度

请仅返回 JSON，不要其他文字。"""

LLM_PROJECT_ANALYSIS_PROMPT = """你是一个游戏项目的智能分析师。根据提供的项目文件内容和上下文，提取美术资产生成需求。

## 项目背景
本项目是「幻宫：时空回响」(Island: Illusion Palace)，一个 Unity 2D top-down Roguelite Action RPG。

## 你的任务
1. **识别文档中描述的所有角色/敌人/Boss/道具/地图元素**
   - 提取每个元素的名称、描述、视觉特征

2. **判断哪些已有美术素材，哪些缺失**
   - 对比现有资产清单和文档描述
   - 标记每个资产的生成状态（已存在/缺失/需更新）

3. **为每个缺失资产生成详细的视觉描述**（用于后续提示词生成）
   - 包含角色外观、配色方案、风格特征
   - 遵循简约像素画 (Simple Pixel Art) 风格规范

4. **按优先级排序**
   - Boss > 玩家 > 精英敌人 > 普通敌人 > 召唤物 > 道具 > 地图Tile

## 输出格式（严格按此格式输出）
```
## 分析结果

### 已有资产
- [资产名称]: [简要说明]

### 缺失资产（按优先级）

#### [优先级] [资产名称]
- **分类**: Player/Enemy/Boss/Summoner/MapTile/Prop
- **描述**: [从文档提取的详细描述]
- **视觉特征**: [颜色、造型、特效等]
- **建议提示词**: [用于 Stable Diffusion 的英文提示词]

[继续列出所有缺失资产...]
```"""

LLM_REQUIREMENT_TO_PROMPT_PROMPT = """你是一个专业的游戏美术 AI 提示词工程师。将项目需求分析结果转换为高质量的 Stable Diffusion / ComfyUI 提示词。

## 项目风格规范（必须遵守）
- 本项目精灵帧采用 **简约像素画 (Simple Pixel Art)** 风格
- 像素颗粒清晰可见，无抗锯齿，无平滑边缘
- 简洁配色方案，色块分明，扁平化上色
- 纯白背景，角色居中
- 头身比 1:2 ~ 1:2.5（Q版比例）
- 四方向：正面(Down)/侧面(Left,Right)/背面(Up)
- 统一视觉语言：dark fantasy, soul glow blue-purple #4A3A8C, crystal erosion cyan #00D4FF

## 规则
1. 正向提示词使用英文，权重语法 (tag:weight)
2. 必须包含：(simple pixel art), (minimalist game sprite), clean pixel lines, flat color blocks, no anti-aliasing
3. 反向提示词排除手绘/插画风格
4. 为每个资产分别生成概念图和精灵帧两套提示词

## 输入数据
{requirements_data}

## 输出格式（严格按此格式，每个资产一组）：
```
### [资产名称]
POSITIVE_CONCEPT: [概念图正向提示词]
NEGATIVE_CONCEPT: [概念图反向提示词]
POSITIVE_SPRITE: [精灵帧正向提示词]
NEGATIVE_SPRITE: [精灵帧反向提示词]
TARGET_SIZE: [目标尺寸如 64x64]
CATEGORY: [资产分类]
```"""


# ==========================================
# 模块1: LLM Client (AI 大模型集成)
# ==========================================
class LLMClient:
    """AI 大模型客户端 - 支持 OpenAI 兼容 API"""

    def __init__(self, base_url="http://localhost:1234/v1", api_key="", model_name=""):
        self.base_url = base_url.rstrip("/")
        self.api_key = api_key or ""
        self.model_name = model_name
        self.available = False

    def _get_headers(self):
        """构建请求头（有 key 才带 Authorization）"""
        headers = {"Content-Type": "application/json"}
        if self.api_key:
            headers["Authorization"] = f"Bearer {self.api_key}"
        return headers

    def test_connection(self):
        """测试 LLM 服务是否可用"""
        try:
            r = requests.get(f"{self.base_url}/models", headers=self._get_headers(), timeout=10)
            self.available = r.status_code == 200
            return self.available
        except Exception:
            self.available = False
            return False

    def chat(self, system_prompt, user_message, temperature=0.7, max_tokens=2048):
        """调用 OpenAI 兼容 API"""
        if not self.model_name:
            raise ValueError("未设置模型名称")

        body = {
            "model": self.model_name,
            "messages": [
                {"role": "system", "content": system_prompt},
                {"role": "user", "content": user_message}
            ],
            "temperature": temperature,
            "max_tokens": max_tokens
        }
        r = requests.post(f"{self.base_url}/chat/completions", json=body, headers=self._get_headers(), timeout=120)
        r.raise_for_status()
        return r.json()["choices"][0]["message"]["content"]

    def get_models(self):
        """获取可用模型列表"""
        try:
            r = requests.get(f"{self.base_url}/models", headers=self._get_headers(), timeout=10)
            data = r.json()
            return [m["id"] for m in data.get("data", [])]
        except Exception:
            return []


# ==========================================
# 模块5: ProjectScanner（项目扫描器）
# ==========================================
class ProjectScanner:
    """项目文件扫描与分析引擎 - 深度集成 Unity 项目"""

    # 支持读取的文件类型
    SUPPORTED_EXTENSIONS = {'.md', '.cs', '.py', '.json', '.txt', '.yaml', '.yml'}
    MAX_FILE_SIZE = 50 * 1024  # 50KB 截断限制
    MAX_KB_SIZE = 8000  # 知识库最大 token 数

    def __init__(self, project_root):
        self.project_root = project_root
        self.design_docs = {}      # {filename: content} 策划文档缓存
        self.script_files = []     # 脚本文件列表
        self.asset_inventory = {}  # 现有资产清单 {category: [files]}
        self.knowledge_base = ""   # 构建好的项目知识库文本
        self.scan_complete = False

    def detect_project_root(self):
        """自动检测项目根目录"""
        if self.project_root and os.path.exists(self.project_root):
            return self.project_root

        # 从脚本所在位置向上查找
        script_dir = os.path.dirname(os.path.abspath(__file__))
        current = script_dir
        while current != os.path.dirname(current):  # 直到根目录
            if os.path.exists(os.path.join(current, "Assets")):
                return current
            current = os.path.dirname(current)

        return script_dir

    def scan_project(self):
        """全量扫描项目，构建知识库"""
        try:
            root = self.detect_project_root()
            if not root or not os.path.exists(root):
                return False, "无法检测到有效的项目根目录"

            self.project_root = root
            self._scan_design_docs()
            self._scan_script_files()
            self._scan_asset_inventory()
            self.build_knowledge_base()
            self.scan_complete = True
            return True, f"扫描完成：{len(self.design_docs)} 个策划文档，{len(self.script_files)} 个脚本文件"
        except Exception as e:
            return False, f"扫描失败: {e}"

    def _scan_design_docs(self):
        """扫描策划文档目录"""
        self.design_docs = {}
        docs_dir = os.path.join(self.project_root, "Assets", "I-IP markdown")
        if not os.path.exists(docs_dir):
            return

        for root, dirs, files in os.walk(docs_dir):
            for file in files:
                if any(file.lower().endswith(ext) for ext in self.SUPPORTED_EXTENSIONS):
                    filepath = os.path.join(root, file)
                    rel_path = os.path.relpath(filepath, docs_dir)
                    try:
                        content = self._read_file_safe(filepath)
                        if content:
                            self.design_docs[rel_path] = {
                                "path": filepath,
                                "content": content,
                                "size": len(content)
                            }
                    except Exception:
                        pass

    def _scan_script_files(self):
        """扫描脚本文件"""
        self.script_files = []
        scripts_dir = os.path.join(self.project_root, "Assets", "Scripts")
        if not os.path.exists(scripts_dir):
            return

        for root, dirs, files in os.walk(scripts_dir):
            for file in files:
                if file.endswith('.cs') or file.endswith('.py'):
                    filepath = os.path.join(root, file)
                    rel_path = os.path.relpath(filepath, scripts_dir)
                    self.script_files.append({
                        "path": filepath,
                        "rel_path": rel_path,
                        "size": os.path.getsize(filepath)
                    })

    def _scan_asset_inventory(self):
        """扫描现有美术素材，返回分类清单"""
        self.asset_inventory = {}
        assets_dir = os.path.join(self.project_root, "Assets", "ArtMaterials")
        if not os.path.exists(assets_dir):
            return

        categories = ["Player", "Boss", "Enemies", "Summoner", "MapTile", "Prop", "Effect", "UI"]
        for cat in categories:
            cat_dir = os.path.join(assets_dir, cat)
            if os.path.exists(cat_dir):
                files = []
                for root, dirs, fs in os.walk(cat_dir):
                    for f in fs:
                        if f.lower().endswith(('.png', '.jpg', '.jpeg', '.bmp')):
                            files.append(os.path.join(root, f))
                if files:
                    self.asset_inventory[cat] = files

    def _read_file_safe(self, filepath, max_size=None):
        """安全读取文件内容（带大小限制）"""
        if max_size is None:
            max_size = self.MAX_FILE_SIZE
        try:
            with open(filepath, 'r', encoding='utf-8', errors='ignore') as f:
                content = f.read(max_size)
                if len(content) >= max_size:
                    content = content[:max_size] + f"\n\n[⚠️ 文件已截断，原始大小超过 {max_size//1024}KB]"
                return content
        except Exception:
            return None

    def get_design_doc(self, filename):
        """读取指定策划文档内容"""
        if filename in self.design_docs:
            return self.design_docs[filename]
        return None

    def list_design_docs(self):
        """列出所有策划文档"""
        return list(self.design_docs.keys())

    def get_asset_inventory(self):
        """获取现有资产清单"""
        return self.asset_inventory

    def analyze_asset_gaps(self):
        """对比策划文档需求和现有资产，返回缺口分析"""
        gaps = []
        # 分析 ASSET_TEMPLATES 中定义的资产
        for asset_name, tmpl in ASSET_TEMPLATES.items():
            category = tmpl["cat"]
            existing_assets = self.asset_inventory.get(category, [])

            # 简单检查：是否有该资产的文件夹或文件
            has_asset = False
            for asset_path in existing_assets:
                if asset_name in asset_path:
                    has_asset = True
                    break

            if not has_asset:
                gaps.append({
                    "name": asset_name,
                    "category": category,
                    "priority": self._get_asset_priority(category),
                    "template_prompt": tmpl["prompt"],
                    "target_size": f"{tmpl['targetW']}x{tmpl['targetH']}",
                    "status": "缺失"
                })
            else:
                gaps.append({
                    "name": asset_name,
                    "category": category,
                    "priority": self._get_asset_priority(category),
                    "template_prompt": tmpl["prompt"],
                    "target_size": f"{tmpl['targetW']}x{tmpl['targetH']}",
                    "status": "已存在"
                })

        # 按优先级排序
        gaps.sort(key=lambda x: x["priority"], reverse=True)
        return gaps

    def _get_asset_priority(self, category):
        """获取资产优先级"""
        priority_map = {"Boss": 5, "Player": 4, "Enemy": 3, "Summoner": 2, "MapTile": 1, "Prop": 0}
        return priority_map.get(category, 0)

    def build_knowledge_base(self):
        """构建供 LLM 使用的项目上下文摘要（控制在 MAX_KB_SIZE token 以内）"""
        parts = []
        parts.append(f"=== 项目名称: 幻宫：时空回响 (Island: Illusion Palace) ===\n")
        parts.append(f"项目根目录: {self.project_root}\n")

        # 添加策划文档摘要
        parts.append("\n=== 策划文档列表 ===\n")
        for doc_name, doc_info in self.design_docs.items():
            preview = doc_info["content"][:200].replace('\n', ' ')
            parts.append(f"• [{doc_name}] ({doc_info['size']} 字符) - 预览: {preview}...")

        # 添加资产模板信息
        parts.append("\n=== 已定义的美术资产模板 ===\n")
        for asset_name, tmpl in ASSET_TEMPLATES.items():
            parts.append(f"• [{asset_name}] 分类:{tmpl['cat']} 目标尺寸:{tmpl['targetW']}x{tmpl['targetH']}")
            parts.append(f"  提示词: {tmpl['prompt'][:100]}...")

        # 添加现有资产清单
        parts.append("\n=== 现有美术素材 ===\n")
        total_assets = sum(len(files) for files in self.asset_inventory.values())
        parts.append(f"共发现 {total_assets} 个素材文件\n")
        for cat, files in self.asset_inventory.items():
            parts.append(f"• {cat}: {len(files)} 个文件")

        # 组合并控制大小
        full_text = "\n".join(parts)
        if len(full_text) > self.MAX_KB_SIZE * 4:  # 粗略估算 token
            full_text = full_text[:self.MAX_KB_SIZE * 4] + "\n\n[⚠️ 知识库已截断以控制大小]"

        self.knowledge_base = full_text
        return self.knowledge_base

    def extract_requirements_from_doc(self, doc_name):
        """从指定策划文档中提取美术资产生成需求的原始文本"""
        if doc_name not in self.design_docs:
            return None, "文档不存在"
        doc_info = self.design_docs[doc_name]
        return doc_info["content"], doc_info["path"]

    def get_scan_summary(self):
        """获取扫描结果摘要"""
        if not self.scan_complete:
            return "尚未完成项目扫描"

        summary_parts = [
            f"📊 项目扫描报告",
            f"=" * 50,
            f"",
            f"📁 项目根目录: {self.project_root}",
            f"",
            f"📝 策划文档: {len(self.design_docs)} 个",
        ]

        # 列出主要策划文档
        key_docs = [d for d in self.design_docs.keys() if d.endswith('.md')]
        for doc in sorted(key_docs)[:10]:
            summary_parts.append(f"   • {doc}")
        if len(key_docs) > 10:
            summary_parts.append(f"   ... 还有 {len(key_docs) - 10} 个文档")

        summary_parts.extend([
            f"",
            f"💻 脚本文件: {len(self.script_files)} 个",
            f"",
            f"🎨 美术素材:",
        ])

        total_assets = sum(len(files) for files in self.asset_inventory.values())
        summary_parts.append(f"   共计 {total_assets} 个文件")
        for cat, files in sorted(self.asset_inventory.items()):
            summary_parts.append(f"   • {cat}: {len(files)} 个")

        # 资产缺口分析
        gaps = self.analyze_asset_gaps()
        missing = [g for g in gaps if g["status"] == "缺失"]
        existing = [g for g in gaps if g["status"] == "已存在"]

        summary_parts.extend([
            f"",
            f"⚠️  资产缺口分析:",
            f"   缺失: {len(missing)} 个 | 已存在: {len(existing)} / {len(gaps)}",
        ])

        if missing:
            summary_parts.append(f"\n   【缺失资产】(按优先级排序):")
            for gap in missing[:15]:
                priority_stars = "⭐" * gap["priority"]
                summary_parts.append(f"   {priority_stars} [{gap['category']}] {gap['name']} - {gap['target_size']}")
            if len(missing) > 15:
                summary_parts.append(f"   ... 还有 {len(missing) - 15} 个缺失资产")

        return "\n".join(summary_parts)


# ==========================================
# 模块8: ServerMonitor（服务器实时状态监控）
# ==========================================
class ServerMonitor:
    """ComfyUI 服务器实时状态监控 - WebSocket 连接 + HTTP 轮询降级

    服务器地址统一引用配置区常量 SERVER_URL / WS_URL，
    修改服务器地址只需修改文件顶部的配置区即可。
    """

    # 重连配置
    RECONNECT_INTERVAL = 5  # 秒

    def __init__(self, on_status_update_callback=None):
        self.ws = None
        self.connected = False
        self.server_data = {}  # 最新服务器状态数据
        self.on_status_update = on_status_update_callback
        self._running = False
        self._polling = False  # HTTP 轮询模式标志

    def start(self):
        """启动监控"""
        self._running = True
        if WEBSOCKET_AVAILABLE:
            self._connect_ws()
        else:
            # websocket-client 未安装，降级为 HTTP 轮询
            print("[ServerMonitor] websocket-client 未安装，使用 HTTP 轮询模式")
            self._start_http_polling()

    def stop(self):
        """停止监控"""
        self._running = False
        self._polling = False
        self._close_ws()

    def _connect_ws(self):
        """建立 WebSocket 连接"""
        try:
            import websocket
            self.ws = websocket.WebSocketApp(
                WS_URL,
                on_open=self._on_open,
                on_message=self._on_message,
                on_error=self._on_error,
                on_close=self._on_close
            )
            # 在后台线程运行
            t = threading.Thread(target=self.ws.run_forever, daemon=True)
            t.start()
        except Exception as e:
            print(f"[ServerMonitor] WebSocket 连接失败: {e}，降级为 HTTP 轮询")
            self._start_http_polling()

    def _close_ws(self):
        """关闭 WebSocket"""
        if self.ws:
            try:
                self.ws.close()
            except Exception:
                pass
            self.ws = None
        self.connected = False

    def _on_open(self, ws):
        """连接建立回调"""
        self.connected = True
        print("[ServerMonitor] WebSocket 已连接")
        self._send({"type": "ping"})

    def _on_message(self, ws, message):
        """收到消息回调"""
        try:
            data = json.loads(message)
            msg_type = data.get("type", "")

            if msg_type == "pong":
                pass  # 心跳响应
            elif msg_type == "server_status":
                self.server_data = data.get("data", {})
                if self.on_status_update:
                    self.on_status_update(self.server_data)
            elif msg_type == "welcome":
                # 服务器欢迎消息，可能包含初始状态
                if "data" in data and isinstance(data["data"], dict):
                    self.server_data.update(data["data"])
                    if self.on_status_update:
                        self.on_status_update(self.server_data)
            elif msg_type == "notify":
                # 通知消息（如任务完成等）
                if self.on_status_update:
                    self.on_status_update({"notify": data.get("data", {})})

        except json.JSONDecodeError:
            pass

    def _on_error(self, ws, error):
        """错误回调"""
        print(f"[ServerMonitor] WebSocket 错误: {error}")

    def _on_close(self, ws, close_status_code, close_msg):
        """连接关闭回调"""
        self.connected = False
        print(f"[ServerMonitor] WebSocket 已关闭: {close_status_code} {close_msg}")
        if self._running:
            self._schedule_reconnect()

    def _send(self, data):
        """发送消息"""
        if self.ws and self.connected:
            try:
                self.ws.send(json.dumps(data))
            except Exception:
                pass

    def _schedule_reconnect(self):
        """安排重连"""
        if self._running:
            t = threading.Timer(self.RECONNECT_INTERVAL, self._connect_ws)
            t.daemon = True
            t.start()

    def _start_http_polling(self):
        """HTTP 轮询降级方案（当 websocket-client 不可用时）"""
        self._polling = True

        def poll():
            while self._polling and self._running:
                try:
                    # v3.2: 使用 8189 /status 获取服务器状态
                    r = requests.get(f"{SERVER_URL}/status", timeout=5)
                    if r.status_code == 200:
                        http_data = r.json()
                        gpu_info = http_data.get("gpu", {})
                        queue_info = http_data.get("queue", {})
                        self.server_data = {
                            "comfyui_version": http_data.get("comfyui_version", http_data.get("version", "?")),
                            "pytorch_version": http_data.get("pytorch_version", "?"),
                            "gpu": {
                                "name": gpu_info.get("name", "?"),
                                "vram_total": gpu_info.get("vram_total", 0),
                                "vram_free": gpu_info.get("vram_free", 0),
                                "vram_used": gpu_info.get("vram_used", 0),
                            },
                            "queue": {
                                "current": len(queue_info.get("queue_running", [])),
                                "pending": len(queue_info.get("queue_pending", [])),
                            },
                            "clients": http_data.get("clients", 1),
                            "uptime_seconds": http_data.get("uptime_seconds", 0),
                            "models_loaded": http_data.get("models_loaded", []),
                            "system_ram": {
                                "total_gb": http_data.get("system_ram", {}).get("total_gb", 0),
                                "free_gb": http_data.get("system_ram", {}).get("free_gb", 0),
                            }
                        }
                        if self.on_status_update:
                            self.on_status_update(self.server_data)
                except Exception:
                    pass
                time.sleep(5)  # 每5秒轮询一次

        t = threading.Thread(target=poll, daemon=True)
        t.start()

    def get_status_text(self):
        """获取格式化的状态文本"""
        d = self.server_data
        if not d:
            return "未连接"

        gpu = d.get("gpu", {})
        vram_free = gpu.get("vram_free", "?")
        queue = d.get("queue", {})
        q_current = queue.get("current", "?")
        clients = d.get("clients", "?")
        uptime = d.get("uptime_seconds", 0)

        hours = int(uptime // 3600)
        mins = int((uptime % 3600) // 60)

        return f"VRAM:{vram_free}GB | 队列:{q_current} | 客户端:{clients} | 运行:{hours}h{mins}m"


# ==========================================
# 模块2+3+4: 工作流编辑器 & 服务器资源
# ==========================================
class WorkflowEditor:
    """自定义工作流配置器 - 简化版（树形结构 + 表单编辑）"""

    def __init__(self):
        self.nodes = {}           # {node_id: {"class_type": ..., "inputs": {...}}}
        self.connections = []     # [(src_node_id, src_port, dst_node_id, dst_port)]
        self.next_node_id = 1
        self.object_info = {}     # 服务器节点信息缓存

    def add_node(self, class_type, inputs=None):
        """添加新节点"""
        node_id = str(self.next_node_id)
        self.nodes[node_id] = {
            "class_type": class_type,
            "inputs": inputs or {}
        }
        self.next_node_id += 1
        return node_id

    def remove_node(self, node_id):
        """删除节点及其相关连接"""
        if node_id in self.nodes:
            del self.nodes[node_id]
            self.connections = [
                c for c in self.connections
                if c[0] != node_id and c[2] != node_id
            ]

    def add_connection(self, src_node, src_port, dst_node, dst_port):
        """添加连接"""
        conn = (src_node, src_port, dst_node, dst_port)
        if conn not in self.connections:
            self.connections.append(conn)

    def remove_connection(self, index):
        """删除连接"""
        if 0 <= index < len(self.connections):
            del self.connections[index]

    def to_workflow_json(self):
        """导出为 ComfyUI workflow JSON 格式"""
        workflow = {}
        for node_id, node_data in self.nodes.items():
            inputs = dict(node_data["inputs"])
            # 将连接引用转换为 ComfyUI 格式
            for conn in self.connections:
                if conn[2] == node_id and conn[3] in inputs:
                    inputs[conn[3]] = [conn[0], int(conn[1])]
            workflow[node_id] = {
                "class_type": node_data["class_type"],
                "inputs": inputs
            }
        return workflow

    def from_workflow_json(self, workflow_json):
        """从 JSON 导入工作流"""
        self.nodes = {}
        self.connections = []
        self.next_node_id = 1

        # 先添加所有节点
        for node_id, node_data in workflow_json.items():
            self.nodes[node_id] = {
                "class_type": node_data["class_type"],
                "inputs": {}
            }
            # 分离字面值和连接引用
            for key, value in node_data.get("inputs", {}).items():
                if isinstance(value, list) and len(value) == 2:
                    # 这是一个连接引用，稍后处理
                    pass
                else:
                    self.nodes[node_id]["inputs"][key] = value

        # 再提取连接关系
        for node_id, node_data in workflow_json.items():
            for key, value in node_data.get("inputs", {}).items():
                if isinstance(value, list) and len(value) == 2:
                    self.connections.append((value[0], value[1], node_id, key))

        # 计算 next_node_id
        if self.nodes:
            max_id = max(int(nid) for nid in self.nodes.keys() if nid.isdigit())
            self.next_node_id = max_id + 1

    def validate(self):
        """验证工作流完整性"""
        errors = []
        warnings = []

        # 检查是否有节点
        if not self.nodes:
            errors.append("工作流为空，没有节点")
            return errors, warnings

        # 检查每个节点的必填参数
        for node_id, node_data in self.nodes.items():
            class_type = node_data["class_type"]
            if class_type in self.object_info:
                node_info = self.object_info[class_type]
                required_inputs = node_info.get("input", {}).get("required", {})
                for param_name in required_inputs:
                    if param_name not in node_data["inputs"]:
                        # 检查是否通过连接提供
                        has_conn = any(
                            c[2] == node_id and c[3] == param_name
                            for c in self.connections
                        )
                        if not has_conn:
                            warnings.append(f"节点 {node_id} ({class_type}) 缺少必填参数: {param_name}")

        # 检查是否有 SaveImage 结尾节点
        has_save = any(
            nd["class_type"] in ("SaveImage", "SaveImageWebsocket")
            for nd in self.nodes.values()
        )
        if not has_save:
            warnings.append("工作流缺少 SaveImage 或 SaveImageWebsocket 节点")

        # 检查断开的连接
        for conn in self.connections:
            src_node, src_port, dst_node, dst_port = conn
            if src_node not in self.nodes:
                errors.append(f"连接源节点 {src_node} 不存在")
            if dst_node not in self.nodes:
                errors.append(f"连接目标节点 {dst_node} 不存在")

        return errors, warnings

    def clear(self):
        """清空工作流"""
        self.nodes = {}
        self.connections = []
        self.next_node_id = 1


# ==========================================
# 配置管理
# ==========================================
class ConfigManager:
    """配置持久化管理"""

    DEFAULT_CONFIG = {
        "llm": {
            "provider": "lm_studio",
            "base_url": "http://localhost:1234/v1",
            "api_key": "",
            "model_name": ""
        },
        "server": {
            "url": SERVER_URL,
            "output_base": r"d:\Program Files\Unity\U3Dproject\Island-Illusion-Palace\Assets\ArtMaterials"
        },
        "project": {
            "root_path": "",  # 自动检测或手动设置
            "auto_scan": True  # 启动时自动扫描项目
        }
    }

    @classmethod
    def load(cls):
        """加载配置"""
        if os.path.exists(CONFIG_FILE):
            try:
                with open(CONFIG_FILE, "r", encoding="utf-8") as f:
                    config = json.load(f)
                # 合并默认配置，确保新字段存在
                merged = cls.DEFAULT_CONFIG.copy()
                for key in merged:
                    if key in config:
                        if isinstance(merged[key], dict):
                            merged[key].update(config[key])
                        else:
                            merged[key] = config[key]
                return merged
            except Exception:
                return cls.DEFAULT_CONFIG.copy()
        return cls.DEFAULT_CONFIG.copy()

    @classmethod
    def save(cls, config):
        """保存配置"""
        try:
            os.makedirs(os.path.dirname(CONFIG_FILE), exist_ok=True)
            with open(CONFIG_FILE, "w", encoding="utf-8") as f:
                json.dump(config, f, indent=2, ensure_ascii=False)
            return True
        except Exception as e:
            print(f"保存配置失败: {e}")
            return False


# ==========================================
# 工作流文件管理
# ==========================================
def ensure_workflow_dir():
    """确保工作流目录存在"""
    os.makedirs(WORKFLOW_DIR, exist_ok=True)

def save_workflow_file(name, workflow_json):
    """保存工作流到文件"""
    ensure_workflow_dir()
    path = os.path.join(WORKFLOW_DIR, f"{name}.json")
    with open(path, "w", encoding="utf-8") as f:
        json.dump(workflow_json, f, indent=2, ensure_ascii=False)
    return path

def load_workflow_file(name):
    """加载工作流文件"""
    path = os.path.join(WORKFLOW_DIR, f"{name}.json")
    with open(path, "r", encoding="utf-8") as f:
        return json.load(f)

def list_workflows():
    """列出已保存的工作流"""
    ensure_workflow_dir()
    return sorted(
        f.replace(".json", "")
        for f in os.listdir(WORKFLOW_DIR)
        if f.endswith(".json")
    )

def delete_workflow_file(name):
    """删除工作流文件"""
    path = os.path.join(WORKFLOW_DIR, f"{name}.json")
    if os.path.exists(path):
        os.remove(path)
        return True
    return False


# ==========================================
# 主应用类 (v3.0 - Notebook 架构)
# ==========================================
class ComfyUIGenerator:
    def __init__(self, root):
        self.root = root
        self.root.title("ComfyUI 美术素材生产客户端 v4.0 - 项目深度集成版")
        self.root.geometry("1300x850")
        self.root.minsize(1100, 750)

        # 状态变量
        self.generating = False
        self.prompt_id = None
        self.server_connected = False
        self.ref_image_path = None

        # 模块实例
        self.llm_client = None
        self.workflow_editor = WorkflowEditor()
        self.server_object_info = {}  # 服务器节点信息缓存
        self.server_checkpoints = []  # 服务器 checkpoint 列表
        self.server_vae = []
        self.server_loras = []

        # 模块5: ProjectScanner 实例
        project_root = self.config.get("project", {}).get("root_path", "") if hasattr(self, 'config') else ""
        if not project_root:
            project_root = OUTPUT_BASE.replace("\\Assets\\ArtMaterials", "")
        self.project_scanner = ProjectScanner(project_root)

        # 模块8: ServerMonitor 实例（服务器实时监控）
        self.server_monitor = ServerMonitor(on_status_update_callback=self._on_server_status_update)

        # 加载配置
        self.config = ConfigManager.load()

        # [已弃用] checkpoint 变量保留用于 8188 回退兼容
        self.checkpoint_var = tk.StringVar(value=DEFAULT_CHECKPOINT)
        self.ckpt_combo = None  # 不再渲染，仅保留属性避免回退代码崩溃

        # 构建 UI
        self._build_ui()
        self._load_config_to_ui()
        self._refresh_assets()

        # 初始化 LLM 客户端
        self._init_llm_client()

        # 测试服务器连接
        self._test_connection()

        # 启动服务器实时监控（WebSocket / HTTP 轮询）
        self.server_monitor.start()

    # ==========================================
    # UI 构建 - Notebook 架构
    # ==========================================
    def _build_ui(self):
        # ===== 顶部工具栏（增强版 - 含实时服务器状态监控）=====
        toolbar = ttk.Frame(self.root, padding=5)
        toolbar.pack(fill=tk.X)

        # 左侧：连接控制区
        conn_frame = ttk.Frame(toolbar)
        conn_frame.pack(side=tk.LEFT, fill=tk.X, expand=True)

        self.status_label = ttk.Label(conn_frame, text="● 服务器: 未连接", foreground="red", font=("", 10))
        self.status_label.pack(side=tk.LEFT, padx=5)

        # 分隔线
        ttk.Separator(conn_frame, orient=tk.VERTICAL).pack(side=tk.LEFT, fill=tk.Y, padx=8)

        # 实时状态显示区（由 ServerMonitor 更新）
        self.monitor_vram_label = ttk.Label(conn_frame, text="VRAM: -- GB", font=("Consolas", 9), foreground="gray")
        self.monitor_vram_label.pack(side=tk.LEFT, padx=5)

        self.monitor_queue_label = ttk.Label(conn_frame, text="队列: --", font=("Consolas", 9), foreground="gray")
        self.monitor_queue_label.pack(side=tk.LEFT, padx=5)

        self.monitor_clients_label = ttk.Label(conn_frame, text="客户端: --", font=("Consolas", 9), foreground="gray")
        self.monitor_clients_label.pack(side=tk.LEFT, padx=5)

        self.monitor_uptime_label = ttk.Label(conn_frame, text="运行: --", font=("Consolas", 9), foreground="gray")
        self.monitor_uptime_label.pack(side=tk.LEFT, padx=5)

        # 右侧：操作按钮
        btn_frame = ttk.Frame(toolbar)
        btn_frame.pack(side=tk.RIGHT)
        ttk.Button(btn_frame, text="🔄 测试连接", command=self._test_connection, width=12).pack(side=tk.LEFT, padx=2)
        ttk.Button(btn_frame, text="📋 检查队列", command=self._check_queue, width=12).pack(side=tk.LEFT, padx=2)

        # 可展开的服务器详情按钮
        self.monitor_detail_btn = ttk.Button(btn_frame, text="📊 详情", command=self._toggle_monitor_detail, width=6)
        self.monitor_detail_btn.pack(side=tk.LEFT, padx=2)

        # ===== 可折叠的服务器详情面板（默认隐藏）=====
        self.monitor_detail_frame = ttk.Frame(self.root, padding=5)
        # 注意：不 pack，通过 toggle 显示/隐藏

        # 详情面板内容
        detail_grid = ttk.Frame(self.monitor_detail_frame)
        detail_grid.pack(fill=tk.X)

        # 第一行：GPU 信息
        row1 = ttk.Frame(detail_grid)
        row1.pack(fill=tk.X, pady=2)
        ttk.Label(row1, text="GPU:", font=("", 9, "bold"), width=8).pack(side=tk.LEFT)
        self.detail_gpu_name = ttk.Label(row1, text="--", foreground="dodgerblue")
        self.detail_gpu_name.pack(side=tk.LEFT, padx=5)
        ttk.Label(row1, text="显存:", width=6).pack(side=tk.LEFT)
        self.detail_vram_bar = ttk.Progressbar(row1, length=200, mode='determinate')
        self.detail_vram_bar.pack(side=tk.LEFT, padx=5)
        self.detail_vram_text = ttk.Label(row1, text="-- / -- GB")
        self.detail_vram_text.pack(side=tk.LEFT)

        # 第二行：系统信息
        row2 = ttk.Frame(detail_grid)
        row2.pack(fill=tk.X, pady=2)
        ttk.Label(row2, text="ComfyUI:", font=("", 9, "bold"), width=8).pack(side=tk.LEFT)
        self.detail_version = ttk.Label(row2, text="--")
        self.detail_version.pack(side=tk.LEFT, padx=5)
        ttk.Label(row2, text="PyTorch:", width=8).pack(side=tk.LEFT)
        self.detail_pytorch = ttk.Label(row2, text="--")
        self.detail_pytorch.pack(side=tk.LEFT, padx=5)
        ttk.Label(row2, text="RAM:", width=6).pack(side=tk.LEFT)
        self.detail_ram = ttk.Label(row2, text="--")
        self.detail_ram.pack(side=tk.LEFT, padx=5)

        # 第三行：模型和队列
        row3 = ttk.Frame(detail_grid)
        row3.pack(fill=tk.X, pady=2)
        ttk.Label(row3, text="已加载模型:", font=("", 9, "bold"), width=8).pack(side=tk.LEFT)
        self.detail_models = ttk.Label(row3, text="--", wraplength=500)
        self.detail_models.pack(side=tk.LEFT, padx=5, fill=tk.X, expand=True)

        # 第四行：优化建议
        row4 = ttk.Frame(detail_grid)
        row4.pack(fill=tk.X, pady=2)
        ttk.Label(row4, text="💡 建议:", font=("", 9, "bold"), width=8).pack(side=tk.LEFT)
        self.detail_suggestion = ttk.Label(row4, text="--", foreground="orange", wraplength=700)
        self.detail_suggestion.pack(side=tk.LEFT, padx=5, fill=tk.X, expand=True)

        self.monitor_detail_visible = False

        ttk.Separator(self.root, orient=tk.HORIZONTAL).pack(fill=tk.X)

        # Notebook (选项卡)
        self.notebook = ttk.Notebook(self.root)
        self.notebook.pack(fill=tk.BOTH, expand=True, padx=5, pady=5)

        # Tab 1: 资产生成（原有功能）
        self.tab_generate = ttk.Frame(self.notebook)
        self.notebook.add(self.tab_generate, text="资产生成")
        self._build_tab_generate()

        # Tab 2: AI 助手（新）
        self.tab_ai = ttk.Frame(self.notebook)
        self.notebook.add(self.tab_ai, text="AI 助手")
        self._build_tab_ai()
        self._on_llm_provider_changed()  # 初始化提供商提示文字

        # Tab 3: 服务器资源（新）
        self.tab_resources = ttk.Frame(self.notebook)
        self.notebook.add(self.tab_resources, text="服务器资源")
        self._build_tab_resources()

        # Tab 4: 工作流编辑器（新）
        self.tab_workflow = ttk.Frame(self.notebook)
        self.notebook.add(self.tab_workflow, text="工作流编辑器")
        self._build_tab_workflow()

        # Tab 5: 项目分析（新增 - 模块5+6+7）
        self.tab_project = ttk.Frame(self.notebook)
        self.notebook.add(self.tab_project, text="📊 项目分析")
        self._build_tab_project_analysis()

        # 初始化项目根目录显示
        if hasattr(self, 'project_root_label'):
            self.project_root_label.configure(text=self.project_scanner.project_root or "未检测到")

        # 底部日志栏
        log_frame = ttk.Frame(self.root, padding=5)
        log_frame.pack(fill=tk.X)
        ttk.Label(log_frame, text="系统日志:", font=("", 9, "bold")).pack(side=tk.LEFT)
        self.global_log_text = scrolledtext.ScrolledText(log_frame, height=4, wrap=tk.WORD, state=tk.DISABLED)
        self.global_log_text.pack(fill=tk.X, side=tk.LEFT, expand=True, padx=(5, 0))

    # ------------------------------------------
    # Tab 1: 资产生成（保留原有功能）
    # ------------------------------------------
    def _build_tab_generate(self):
        parent = self.tab_generate
        main = ttk.Frame(parent, padding=10)
        main.pack(fill=tk.BOTH, expand=True)

        # 左侧控制面板
        left = ttk.Frame(main, width=400)
        left.pack(side=tk.LEFT, fill=tk.Y, padx=(0, 10))
        left.pack_propagate(False)

        # --- 资产选择 ---
        ttk.Label(left, text="资产类型", font=("", 10, "bold")).pack(anchor=tk.W, pady=(0, 2))
        sel_frame = ttk.Frame(left)
        sel_frame.pack(fill=tk.X)

        self.cat_var = tk.StringVar(value="全部")
        cat_combo = ttk.Combobox(sel_frame, textvariable=self.cat_var, values=["全部", "Player", "Enemy", "Boss", "MapTile", "Prop"], width=10, state="readonly")
        cat_combo.pack(side=tk.LEFT, padx=(0, 5))
        cat_combo.bind("<<ComboboxSelected>>", lambda e: self._refresh_assets())

        self.asset_var = tk.StringVar()
        self.asset_combo = ttk.Combobox(sel_frame, textvariable=self.asset_var, width=22, state="readonly")
        self.asset_combo.pack(side=tk.LEFT, fill=tk.X, expand=True)
        self.asset_combo.bind("<<ComboboxSelected>>", lambda e: self._on_asset_changed())

        # --- 生成阶段 ---
        ttk.Label(left, text="生成阶段", font=("", 10, "bold")).pack(anchor=tk.W, pady=(10, 2))
        stage_frame = ttk.Frame(left)
        stage_frame.pack(fill=tk.X)

        self.stage_var = tk.StringVar(value="concept")
        ttk.Radiobutton(stage_frame, text="概念图", variable=self.stage_var, value="concept", command=self._on_stage_changed).pack(side=tk.LEFT, padx=5)
        ttk.Radiobutton(stage_frame, text="精灵帧", variable=self.stage_var, value="sprite", command=self._on_stage_changed).pack(side=tk.LEFT, padx=5)

        # 动画状态（精灵帧时显示）
        self.state_frame = ttk.Frame(left)
        self.state_frame.pack(fill=tk.X, pady=(5, 0))
        ttk.Label(self.state_frame, text="动画状态:").pack(side=tk.LEFT)
        self.state_var = tk.StringVar(value="idle")
        self.state_combo = ttk.Combobox(self.state_frame, textvariable=self.state_var, values=list(STATE_PROMPTS.keys()), width=10, state="readonly")
        self.state_combo.pack(side=tk.LEFT, padx=5)

        # --- 概念图选择区 ---
        self.concept_frame = ttk.LabelFrame(left, text="参考底图 (img2img)", padding=5)
        self.concept_frame.pack(fill=tk.X, pady=(10, 0))

        concept_btn_frame = ttk.Frame(self.concept_frame)
        concept_btn_frame.pack(fill=tk.X)
        ttk.Button(concept_btn_frame, text="浏览概念图", command=self._browse_concept_image).pack(side=tk.LEFT, padx=(0, 5))
        ttk.Button(concept_btn_frame, text="清除", command=self._clear_concept_image).pack(side=tk.LEFT)

        self.concept_preview_label = ttk.Label(self.concept_frame, text="未选择概念图", foreground="gray")
        self.concept_preview_label.pack(anchor=tk.W, pady=(5, 0))

        # --- 服务器工作流选择（v3.2 统一 API）---
        ttk.Label(left, text="工作流选择", font=("", 10, "bold")).pack(anchor=tk.W, pady=(10, 2))
        wf_select_frame = ttk.Frame(left)
        wf_select_frame.pack(fill=tk.X)

        self.server_workflow_var = tk.StringVar()
        self.server_workflow_combo = ttk.Combobox(wf_select_frame, textvariable=self.server_workflow_var, width=26, state="readonly")
        self.server_workflow_combo.pack(side=tk.LEFT, fill=tk.X, expand=True)
        self.server_workflow_combo.bind("<<ComboboxSelected>>", self._on_server_workflow_selected)
        ttk.Button(wf_select_frame, text="刷新", width=5, command=self._refresh_server_workflows_v2).pack(side=tk.LEFT, padx=(5, 0))

        self.server_wf_info_label = ttk.Label(wf_select_frame, text="点击「刷新」加载服务器工作流", font=("", 8), foreground="gray")
        self.server_wf_info_label.pack(fill=tk.X)

        # [已弃用] 工作流模式选择 — v3.2 服务器统一管理，保留用于向后兼容
        # self.wf_mode_var 和 _on_workflow_mode_changed 仍可用，但默认不再显示
        self.wf_mode_var = tk.StringVar(value="server")  # 默认使用服务器模式
        # 保留旧的 _server_wf_map 兼容性
        self._server_wf_map = {}

        # --- 参数设置 ---
        ttk.Label(left, text="生成参数", font=("", 10, "bold")).pack(anchor=tk.W, pady=(10, 2))
        param_frame = ttk.Frame(left)
        param_frame.pack(fill=tk.X)

        params = [
            ("宽度:", "width_var", "1024"),
            ("高度:", "height_var", "1024"),
            ("步数:", "steps_var", "20"),
            ("CFG:", "cfg_var", "7.0"),
            ("种子:", "seed_var", "-1"),
            ("数量:", "count_var", "1"),
            ("去噪:", "denoise_var", "0.5"),
        ]
        self.param_vars = {}
        for i, (label, vname, default) in enumerate(params):
            ttk.Label(param_frame, text=label, width=6).grid(row=i, column=0, sticky=tk.W, pady=2)
            var = tk.StringVar(value=default)
            self.param_vars[vname] = var
            ttk.Entry(param_frame, textvariable=var, width=10).grid(row=i, column=1, sticky=tk.W, pady=2)

        ttk.Label(param_frame, text="(-1=随机种子)").grid(row=4, column=2, sticky=tk.W, padx=5)
        ttk.Label(param_frame, text="(精灵帧推荐 4-8)").grid(row=5, column=2, sticky=tk.W, padx=5)
        ttk.Label(param_frame, text="(0.4-0.6 推荐)").grid(row=6, column=2, sticky=tk.W, padx=5)

        # --- 输出目录 ---
        ttk.Label(left, text="输出目录", font=("", 10, "bold")).pack(anchor=tk.W, pady=(10, 2))
        out_frame = ttk.Frame(left)
        out_frame.pack(fill=tk.X)
        self.output_var = tk.StringVar(value=OUTPUT_BASE)
        ttk.Entry(out_frame, textvariable=self.output_var, width=35).pack(side=tk.LEFT, fill=tk.X, expand=True)
        ttk.Button(out_frame, text="...", width=3, command=self._browse_output).pack(side=tk.LEFT)

        # --- 生成按钮 ---
        btn_frame = ttk.Frame(left)
        btn_frame.pack(fill=tk.X, pady=15)
        self.generate_btn = ttk.Button(btn_frame, text="生成", command=self._generate)
        self.generate_btn.pack(fill=tk.X, ipady=5)

        self.progress = ttk.Progressbar(btn_frame, mode="indeterminate")
        self.progress.pack(fill=tk.X, pady=5)

        # 右侧：提示词 + 日志
        right = ttk.Frame(main)
        right.pack(side=tk.LEFT, fill=tk.BOTH, expand=True)

        # 提示词预览
        ttk.Label(right, text="正向提示词（可编辑）", font=("", 10, "bold")).pack(anchor=tk.W)
        self.positive_text = scrolledtext.ScrolledText(right, height=5, wrap=tk.WORD)
        self.positive_text.pack(fill=tk.X, pady=(0, 5))

        ttk.Label(right, text="反向提示词", font=("", 10, "bold")).pack(anchor=tk.W)
        self.negative_text = scrolledtext.ScrolledText(right, height=3, wrap=tk.WORD)
        self.negative_text.pack(fill=tk.X, pady=(0, 5))

        # 信息标签
        self.info_label = ttk.Label(right, text="", foreground="gray")
        self.info_label.pack(anchor=tk.W, pady=(0, 5))

        # 日志
        ttk.Label(right, text="生成日志", font=("", 10, "bold")).pack(anchor=tk.W)
        self.log_text = scrolledtext.ScrolledText(right, height=6, wrap=tk.WORD, state=tk.DISABLED)
        self.log_text.pack(fill=tk.X, pady=(0, 5))

        # 生成结果预览区
        ttk.Label(right, text="最近生成的图片", font=("", 10, "bold")).pack(anchor=tk.W)
        self.result_frame = ttk.Frame(right)
        self.result_frame.pack(fill=tk.X)
        self.result_label = ttk.Label(self.result_frame, text="暂无生成结果", foreground="gray")
        self.result_label.pack(anchor=tk.W)

    # ------------------------------------------
    # Tab 2: AI 助手（模块1: LLM 集成）
    # ------------------------------------------
    def _build_tab_ai(self):
        parent = self.tab_ai
        main = ttk.Frame(parent, padding=10)
        main.pack(fill=tk.BOTH, expand=True)

        # 左侧：LLM 配置
        left = ttk.LabelFrame(main, text="LLM 配置", padding=10, width=350)
        left.pack(side=tk.LEFT, fill=tk.Y, padx=(0, 10))
        left.pack_propagate(False)

        # 提供商选择
        ttk.Label(left, text="提供商:").pack(anchor=tk.W, pady=(0, 2))
        self.llm_provider_var = tk.StringVar(value="lm_studio")
        provider_frame = ttk.Frame(left)
        provider_frame.pack(fill=tk.X, pady=(0, 8))
        for val, label in [("lm_studio", "LM Studio (本地)"), ("openai", "OpenAI"), ("custom", "自定义")]:
            rb = ttk.Radiobutton(provider_frame, text=label, variable=self.llm_provider_var, value=val,
                           command=self._on_llm_provider_changed)
            rb.pack(anchor=tk.W)

        # 提供商提示（动态更新）
        self.llm_provider_hint = ttk.Label(left, text="", foreground="dodgerblue", wraplength=330)
        self.llm_provider_hint.pack(anchor=tk.W, pady=(0, 8))

        # API 地址
        ttk.Label(left, text="API 地址:").pack(anchor=tk.W, pady=(5, 2))
        self.llm_url_var = tk.StringVar(value="http://localhost:1234/v1")
        url_frame = ttk.Frame(left)
        url_frame.pack(fill=tk.X, pady=(0, 8))
        ttk.Entry(url_frame, textvariable=self.llm_url_var, width=35).pack(side=tk.LEFT, fill=tk.X, expand=True)

        # API 密钥
        ttk.Label(left, text="API 密钥:").pack(anchor=tk.W, pady=(5, 2))
        self.llm_key_var = tk.StringVar(value="")
        key_frame = ttk.Frame(left)
        key_frame.pack(fill=tk.X, pady=(0, 8))
        self.llm_key_entry = ttk.Entry(key_frame, textvariable=self.llm_key_var, width=33, show="*")
        self.llm_key_entry.pack(side=tk.LEFT, fill=tk.X, expand=True)
        ttk.Button(key_frame, text="👁", width=3, command=self._toggle_llm_key_visibility).pack(side=tk.LEFT, padx=(5, 0))

        # 模型名称
        ttk.Label(left, text="模型名称:").pack(anchor=tk.W, pady=(5, 2))
        model_frame = ttk.Frame(left)
        model_frame.pack(fill=tk.X, pady=(0, 8))
        self.llm_model_var = tk.StringVar(value="")
        self.llm_model_combo = ttk.Combobox(model_frame, textvariable=self.llm_model_var, width=32)
        self.llm_model_combo.pack(side=tk.LEFT, fill=tk.X, expand=True)
        ttk.Button(model_frame, text="刷新", width=5, command=self._refresh_llm_models).pack(side=tk.LEFT, padx=(5, 0))

        # 连接测试
        self.llm_status_label = ttk.Label(left, text="状态: 未测试", foreground="gray")
        self.llm_status_label.pack(anchor=tk.W, pady=(5, 8))
        ttk.Button(left, text="测试 LLM 连接", command=self._test_llm_connection).pack(fill=tk.X, pady=(0, 8))
        ttk.Button(left, text="保存配置", command=self._save_llm_config).pack(fill=tk.X)

        ttk.Separator(left, orient=tk.HORIZONTAL).pack(fill=tk.X, pady=10)

        # 快捷操作说明
        ttk.Label(left, text="快捷操作", font=("", 9, "bold")).pack(anchor=tk.W)
        help_text = """• 优化提示词: 将当前资产生成的正/反向提示词发送给 LLM 优化
• 生成提示词: 基于资产类型+阶段自动生成全新提示词
• 推荐工作流: 让 AI 分析并推荐最优工作流配置"""
        ttk.Label(left, text=help_text, justify=tk.LEFT, foreground="gray", wraplength=320).pack(anchor=tk.W)

        # 右侧：操作区
        right = ttk.Frame(main)
        right.pack(side=tk.LEFT, fill=tk.BOTH, expand=True)

        # 操作按钮区
        btn_frame = ttk.LabelFrame(right, text="操作", padding=10)
        btn_frame.pack(fill=tk.X, pady=(0, 10))

        btn_row1 = ttk.Frame(btn_frame)
        btn_row1.pack(fill=tk.X, pady=2)
        ttk.Button(btn_row1, text="✨ 优化当前提示词", command=self._ai_optimize_prompts, width=25).pack(side=tk.LEFT, padx=2)
        ttk.Button(btn_row1, text="🎨 AI 生成提示词", command=self._ai_generate_prompts, width=25).pack(side=tk.LEFT, padx=2)

        btn_row2 = ttk.Frame(btn_frame)
        btn_row2.pack(fill=tk.X, pady=2)
        ttk.Button(btn_row2, text="🔧 推荐工作流", command=self._ai_recommend_workflow, width=25).pack(side=tk.LEFT, padx=2)
        ttk.Button(btn_row2, text="📋 应用到资产生成", command=self._apply_ai_result_to_generate, width=25).pack(side=tk.LEFT, padx=2)

        # 对话输入区
        input_frame = ttk.LabelFrame(right, text="自由对话（自定义需求）", padding=10)
        input_frame.pack(fill=tk.X, pady=(0, 10))

        ttk.Label(input_frame, text="输入您的需求描述:").pack(anchor=tk.W)
        self.ai_input_text = scrolledtext.ScrolledText(input_frame, height=3, wrap=tk.WORD)
        self.ai_input_text.pack(fill=tk.X, pady=(5, 5))
        send_frame = ttk.Frame(input_frame)
        send_frame.pack(fill=tk.X)
        ttk.Button(send_frame, text="发送给 AI", command=self._ai_custom_chat).pack(side=tk.LEFT)
        self.ai_busy_label = ttk.Label(send_frame, text="", foreground="blue")
        self.ai_busy_label.pack(side=tk.LEFT, padx=10)

        # 结果展示区
        result_frame = ttk.LabelFrame(right, text="AI 返回结果", padding=10)
        result_frame.pack(fill=tk.BOTH, expand=True)

        self.ai_result_text = scrolledtext.ScrolledText(result_frame, height=12, wrap=tk.WORD)
        self.ai_result_text.pack(fill=tk.BOTH, expand=True)

        result_btn_frame = ttk.Frame(result_frame)
        result_btn_frame.pack(fill=tk.X, pady=(5, 0))
        ttk.Button(result_btn_frame, text="复制结果", command=self._copy_ai_result).pack(side=tk.LEFT, padx=2)
        ttk.Button(result_btn_frame, text="清空", command=lambda: self.ai_result_text.delete("1.0", tk.END)).pack(side=tk.LEFT, padx=2)

        # 存储最后一次 AI 结果（用于跨 Tab 应用）
        self.last_ai_positive = ""
        self.last_ai_negative = ""
        self.last_ai_workflow = None

    # ------------------------------------------
    # Tab 3: 服务器资源浏览器（模块2）
    # ------------------------------------------
    def _build_tab_resources(self):
        parent = self.tab_resources

        # 子 Notebook
        sub_nb = ttk.Notebook(parent)
        sub_nb.pack(fill=tk.BOTH, expand=True, padx=5, pady=5)

        # 子 Tab: 扩散模型
        tab_ckpt = ttk.Frame(sub_nb)
        sub_nb.add(tab_ckpt, text="扩散模型")
        self._build_resource_checkpoint_tab(tab_ckpt)

        # 子 Tab: VAE
        tab_vae = ttk.Frame(sub_nb)
        sub_nb.add(tab_vae, text="VAE")
        self._build_resource_vae_tab(tab_vae)

        # 子 Tab: LoRA
        tab_lora = ttk.Frame(sub_nb)
        sub_nb.add(tab_lora, text="LoRA")
        self._build_resource_lora_tab(tab_lora)

        # 子 Tab: 节点浏览器
        tab_nodes = ttk.Frame(sub_nb)
        sub_nb.add(tab_nodes, text="节点浏览器")
        self._build_resource_nodes_tab(tab_nodes)

        # 子 Tab: 自定义节点
        tab_custom = ttk.Frame(sub_nb)
        sub_nb.add(tab_custom, text="自定义节点")
        self._build_resource_custom_tab(tab_custom)

        # 子 Tab: 我的工作流
        tab_mywf = ttk.Frame(sub_nb)
        sub_nb.add(tab_mywf, text="我的工作流")
        self._build_resource_workflows_tab(tab_mywf)

        # 刷新按钮栏
        toolbar = ttk.Frame(parent, padding=5)
        toolbar.pack(fill=tk.X)
        ttk.Button(toolbar, text="🔄 刷新所有资源", command=self._refresh_all_resources).pack(side=tk.LEFT, padx=5)
        self.resource_status_label = ttk.Label(toolbar, text="", foreground="gray")
        self.resource_status_label.pack(side=tk.LEFT, padx=10)

    def _build_resource_checkpoint_tab(self, parent):
        """扩散模型列表"""
        toolbar = ttk.Frame(parent, padding=5)
        toolbar.pack(fill=tk.X)
        ttk.Button(toolbar, text="刷新", command=lambda: self._fetch_checkpoints()).pack(side=tk.LEFT)

        columns = ("filename", "size", "sha256")
        self.ckpt_tree = ttk.Treeview(parent, columns=columns, show="headings", height=12)
        self.ckpt_tree.heading("filename", text="文件名")
        self.ckpt_tree.heading("size", text="大小")
        self.ckpt_tree.heading("sha256", text="SHA256 (前16位)")
        self.ckpt_tree.column("filename", width=300)
        self.ckpt_tree.column("size", width=100)
        self.ckpt_tree.column("sha256", width=180)
        self.ckpt_tree.pack(fill=tk.BOTH, expand=True, padx=5, pady=5)

        # 绑定双击事件：选中后可应用到工作流
        self.ckpt_tree.bind("<Double-1>", lambda e: self._on_checkpoint_selected())

        self.ckpt_status = ttk.Label(parent, text="", foreground="gray")
        self.ckpt_status.pack(anchor=tk.W, padx=5)

    def _build_resource_vae_tab(self, parent):
        """VAE 模型列表"""
        toolbar = ttk.Frame(parent, padding=5)
        toolbar.pack(fill=tk.X)
        ttk.Button(toolbar, text="刷新", command=lambda: self._fetch_vae()).pack(side=tk.LEFT)

        self.vae_tree = ttk.Treeview(parent, columns=("filename",), show="headings", height=12)
        self.vae_tree.heading("filename", text="文件名")
        self.vae_tree.column("filename", width=500)
        self.vae_tree.pack(fill=tk.BOTH, expand=True, padx=5, pady=5)

        self.vae_status = ttk.Label(parent, text="", foreground="gray")
        self.vae_status.pack(anchor=tk.W, padx=5)

    def _build_resource_lora_tab(self, parent):
        """LoRA 模型列表"""
        toolbar = ttk.Frame(parent, padding=5)
        toolbar.pack(fill=tk.X)
        ttk.Button(toolbar, text="刷新", command=lambda: self._fetch_loras()).pack(side=tk.LEFT)

        self.lora_tree = ttk.Treeview(parent, columns=("filename",), show="headings", height=12)
        self.lora_tree.heading("filename", text="文件名")
        self.lora_tree.column("filename", width=500)
        self.lora_tree.pack(fill=tk.BOTH, expand=True, padx=5, pady=5)

        self.lora_status = ttk.Label(parent, text="", foreground="gray")
        self.lora_status.pack(anchor=tk.W, padx=5)

    def _build_resource_nodes_tab(self, parent):
        """节点浏览器 - 左侧分类树 + 右侧详情"""
        paned = ttk.PanedWindow(parent, orient=tk.HORIZONTAL)
        paned.pack(fill=tk.BOTH, expand=True, padx=5, pady=5)

        # 左侧：分类树
        left_frame = ttk.Frame(paned, width=280)
        paned.add(left_frame, weight=1)

        search_frame = ttk.Frame(left_frame)
        search_frame.pack(fill=tk.X, padx=5, pady=5)
        self.node_search_var = tk.StringVar()
        ttk.Entry(search_frame, textvariable=self.node_search_var, width=25).pack(side=tk.LEFT, fill=tk.X, expand=True)
        ttk.Button(search_frame, text="搜索", width=5, command=self._search_nodes).pack(side=tk.LEFT, padx=(5, 0))

        self.node_category_tree = ttk.Treeview(left_frame, columns=("count",), show="tree headings", height=20)
        self.node_category_tree.heading("#0", text="分类 / 节点")
        self.node_category_tree.heading("count", text="数量")
        self.node_category_tree.column("#0", width=200)
        self.node_category_tree.column("count", width=50)
        self.node_category_tree.pack(fill=tk.BOTH, expand=True, padx=5, pady=5)
        self.node_category_tree.bind("<<TreeviewSelect>>", self._on_node_selected)

        # 右侧：节点详情
        right_frame = ttk.Frame(paned)
        paned.add(right_frame, weight=2)

        ttk.Label(right_frame, text="节点详情", font=("", 10, "bold")).pack(anchor=tk.W, padx=5, pady=(5, 2))

        self.node_detail_text = scrolledtext.ScrolledText(right_frame, height=22, wrap=tk.WORD, state=tk.DISABLED)
        self.node_detail_text.pack(fill=tk.BOTH, expand=True, padx=5, pady=5)

        detail_btn_frame = ttk.Frame(right_frame)
        detail_btn_frame.pack(fill=tk.X, padx=5, pady=5)
        ttk.Button(detail_btn_frame, text="添加到工作流", command=self._add_selected_node_to_workflow).pack(side=tk.LEFT, padx=2)

        self.node_status = ttk.Label(parent, text="", foreground="gray")
        self.node_status.pack(anchor=tk.W, padx=5)

    def _build_resource_custom_tab(self, parent):
        """自定义节点列表"""
        toolbar = ttk.Frame(parent, padding=5)
        toolbar.pack(fill=tk.X)
        ttk.Button(toolbar, text="刷新", command=lambda: self._fetch_and_show_custom_nodes()).pack(side=tk.LEFT)

        self.custom_node_tree = ttk.Treeview(parent, columns=("category", "display_name"), show="headings", height=18)
        self.custom_node_tree.heading("category", text="来源分类")
        self.custom_node_tree.heading("display_name", text="节点名称")
        self.custom_node_tree.column("category", width=250)
        self.custom_node_tree.column("display_name", width=350)
        self.custom_node_tree.pack(fill=tk.BOTH, expand=True, padx=5, pady=5)

        self.custom_node_status = ttk.Label(parent, text="显示 category 含 'api'/'advanced'/'external' 的外部节点", foreground="gray")
        self.custom_node_status.pack(anchor=tk.W, padx=5)

    def _build_resource_workflows_tab(self, parent):
        """已保存的工作流列表"""
        toolbar = ttk.Frame(parent, padding=5)
        toolbar.pack(fill=tk.X)
        ttk.Button(toolbar, text="刷新列表", command=self._refresh_workflow_list).pack(side=tk.LEFT, padx=2)
        ttk.Button(toolbar, text="加载到编辑器", command=self._load_selected_workflow).pack(side=tk.LEFT, padx=2)
        ttk.Button(toolbar, text="删除", command=self._delete_selected_workflow).pack(side=tk.LEFT, padx=2)

        self.workflow_list_tree = ttk.Treeview(parent, columns=("name", "modified"), show="headings", height=15)
        self.workflow_list_tree.heading("name", text="工作流名称")
        self.workflow_list_tree.heading("modified", text="修改时间")
        self.workflow_list_tree.column("name", width=300)
        self.workflow_list_tree.column("modified", width=200)
        self.workflow_list_tree.pack(fill=tk.BOTH, expand=True, padx=5, pady=5)

        self.wf_list_status = ttk.Label(parent, text=f"工作流存储路径: {WORKFLOW_DIR}", foreground="gray")
        self.wf_list_status.pack(anchor=tk.W, padx=5)

    # ------------------------------------------
    # Tab 4: 工作流编辑器（模块3+4）
    # ------------------------------------------
    def _build_tab_workflow(self):
        parent = self.tab_workflow

        # 工具栏
        toolbar = ttk.Frame(parent, padding=5)
        toolbar.pack(fill=tk.X)
        ttk.Button(toolbar, text="新建", command=self._workflow_new).pack(side=tk.LEFT, padx=2)
        ttk.Button(toolbar, text="加载JSON", command=self._workflow_load_json).pack(side=tk.LEFT, padx=2)
        ttk.Button(toolbar, text="保存", command=self._workflow_save).pack(side=tk.LEFT, padx=2)
        ttk.Button(toolbar, text="另存为...", command=self._workflow_save_as).pack(side=tk.LEFT, padx=2)
        ttk.Separator(toolbar, orient=tk.VERTICAL).pack(side=tk.LEFT, fill=tk.Y, padx=5)
        ttk.Button(toolbar, text="验证", command=self._workflow_validate).pack(side=tk.LEFT, padx=2)
        ttk.Button(toolbar, text="应用到生成", command=self._workflow_apply_to_generate).pack(side=tk.LEFT, padx=2)
        ttk.Separator(toolbar, orient=tk.VERTICAL).pack(side=tk.LEFT, fill=tk.Y, padx=5)
        ttk.Button(toolbar, text="🤖 AI 生成工作流", command=self._ai_generate_workflow_editor).pack(side=tk.LEFT, padx=2)

        # 主区域：三列布局
        main = ttk.Frame(parent)
        main.pack(fill=tk.BOTH, expand=True, padx=5, pady=5)

        # 左侧：节点库 + 已添加节点
        left = ttk.Frame(main, width=280)
        left.pack(side=tk.LEFT, fill=tk.Y, padx=(0, 5))
        left.pack_propagate(False)

        # 节点库搜索
        lib_frame = ttk.LabelFrame(left, text="节点库", padding=5)
        lib_frame.pack(fill=tk.X, pady=(0, 5))

        self.wf_search_var = tk.StringVar()
        ttk.Entry(lib_frame, textvariable=self.wf_search_var, width=28).pack(fill=tk.X, pady=(0, 5))
        self.wf_node_lib_tree = ttk.Treeview(lib_frame, columns=("type",), show="headings", height=8)
        self.wf_node_lib_tree.heading("type", text="节点类型")
        self.wf_node_lib_tree.column("type", width=250)
        self.wf_node_lib_tree.pack(fill=tk.X)
        self.wf_node_lib_tree.bind("<Double-1>", self._on_wf_lib_node_double_click)

        # 已添加的节点列表
        nodes_frame = ttk.LabelFrame(left, text="已添加节点", padding=5)
        nodes_frame.pack(fill=tk.BOTH, expand=True, pady=(0, 5))

        self.wf_nodes_tree = ttk.Treeview(nodes_frame, columns=("id", "class_type"), show="headings", height=10)
        self.wf_nodes_tree.heading("id", text="ID")
        self.wf_nodes_tree.heading("class_type", text="类型")
        self.wf_nodes_tree.column("id", width=40)
        self.wf_nodes_tree.column("class_type", width=210)
        self.wf_nodes_tree.pack(fill=tk.BOTH, expand=True)
        self.wf_nodes_tree.bind("<<TreeviewSelect>>", self._on_wf_node_selected)
        self.wf_nodes_tree.bind("<Delete>", lambda e: self._wf_remove_selected_node())

        ttk.Button(nodes_frame, text="删除选中节点 (Del)", command=self._wf_remove_selected_node).pack(fill=tk.X, pady=(5, 0))

        # 中间：连接管理
        middle = ttk.Frame(main, width=300)
        middle.pack(side=tk.LEFT, fill=tk.Y, padx=5)
        middle.pack_propagate(False)

        conn_frame = ttk.LabelFrame(middle, text="连接管理", padding=5)
        conn_frame.pack(fill=tk.BOTH, expand=True)

        # 添加连接
        ttk.Label(conn_frame, text="源节点:").pack(anchor=tk.W)
        self.conn_src_node_var = tk.StringVar()
        self.conn_src_node_combo = ttk.Combobox(conn_frame, textvariable=self.conn_src_node_var, width=20, state="readonly")
        self.conn_src_node_combo.pack(fill=tk.X, pady=(0, 5))

        ttk.Label(conn_frame, text="源端口:").pack(anchor=tk.W)
        self.conn_src_port_var = tk.StringVar()
        self.conn_src_port_combo = ttk.Combobox(conn_frame, textvariable=self.conn_src_port_var, width=20, state="readonly")
        self.conn_src_port_combo.pack(fill=tk.X, pady=(0, 5))

        ttk.Label(conn_frame, text="目标节点:").pack(anchor=tk.W)
        self.conn_dst_node_var = tk.StringVar()
        self.conn_dst_node_combo = ttk.Combobox(conn_frame, textvariable=self.conn_dst_node_var, width=20, state="readonly")
        self.conn_dst_node_combo.pack(fill=tk.X, pady=(0, 5))

        ttk.Label(conn_frame, text="目标端口:").pack(anchor=tk.W)
        self.conn_dst_port_var = tk.StringVar()
        self.conn_dst_port_combo = ttk.Combobox(conn_frame, textvariable=self.conn_dst_port_var, width=20, state="readonly")
        self.conn_dst_port_combo.pack(fill=tk.X, pady=(0, 5))

        ttk.Button(conn_frame, text="➕ 添加连接", command=self._wf_add_connection).pack(fill=tk.X, pady=5)

        # 连接列表
        ttk.Label(conn_frame, text="已有连接:").pack(anchor=tk.W, pady=(10, 2))
        self.wf_conn_tree = ttk.Treeview(conn_frame, columns=("connection",), show="headings", height=8)
        self.wf_conn_tree.heading("connection", text="连接 (源→目标)")
        self.wf_conn_tree.column("connection", width=270)
        self.wf_conn_tree.pack(fill=tk.BOTH, expand=True)
        self.wf_conn_tree.bind("<<TreeviewSelect>>", self._on_wf_conn_selected)

        conn_btn_frame = ttk.Frame(conn_frame)
        conn_btn_frame.pack(fill=tk.X, pady=(5, 0))
        ttk.Button(conn_btn_frame, text="删除连接", command=self._wf_remove_selected_connection).pack(side=tk.LEFT)

        # 右侧：参数编辑 + JSON 预览
        right = ttk.Frame(main)
        right.pack(side=tk.LEFT, fill=tk.BOTH, expand=True, padx=(5, 0))

        # 参数编辑区
        param_frame = ttk.LabelFrame(right, text="节点参数编辑", padding=5)
        param_frame.pack(fill=tk.X, pady=(0, 5))

        self.wf_param_frame_inner = ttk.Frame(param_frame)
        self.wf_param_frame_inner.pack(fill=tk.X)
        self.wf_param_widgets = {}  # {param_name: widget}

        ttk.Button(param_frame, text="应用参数修改", command=self._wf_apply_params).pack(fill=tk.X, pady=(5, 0))

        # JSON 预览
        json_frame = ttk.LabelFrame(right, text="Workflow JSON 预览", padding=5)
        json_frame.pack(fill=tk.BOTH, expand=True)

        self.wf_json_text = scrolledtext.ScrolledText(json_frame, height=15, wrap=tk.NONE, font=("Consolas", 9))
        self.wf_json_text.pack(fill=tk.BOTH, expand=True)

        json_btn_frame = ttk.Frame(json_frame)
        json_btn_frame.pack(fill=tk.X, pady=(5, 0))
        ttk.Button(json_btn_frame, text="刷新预览", command=self._wf_refresh_json_preview).pack(side=tk.LEFT, padx=2)
        ttk.Button(json_btn_frame, text="复制JSON", command=self._wf_copy_json).pack(side=tk.LEFT, padx=2)

        # 状态栏
        self.wf_status_label = ttk.Label(parent, text="就绪", foreground="gray")
        self.wf_status_label.pack(anchor=tk.W, padx=5)

    # ------------------------------------------
    # Tab 5: 项目分析（模块5+6+7 - 新增）
    # ------------------------------------------
    def _build_tab_project_analysis(self):
        """构建项目分析 Tab - 文件浏览器、资产分析、AI 需求提取"""
        parent = self.tab_project

        # 主布局：左右分栏
        main_paned = ttk.PanedWindow(parent, orient=tk.HORIZONTAL)
        main_paned.pack(fill=tk.BOTH, expand=True, padx=5, pady=5)

        # ===== 左侧面板：项目文件浏览器 =====
        left_frame = ttk.LabelFrame(main_paned, text="📁 项目文件浏览器", padding=5, width=320)
        main_paned.add(left_frame, weight=1)

        # 项目根目录显示
        root_info_frame = ttk.Frame(left_frame)
        root_info_frame.pack(fill=tk.X, pady=(0, 5))
        ttk.Label(root_info_frame, text="项目根目录:", font=("", 8)).pack(side=tk.LEFT)
        self.project_root_label = ttk.Label(root_info_frame, text="", foreground="blue", wraplength=280)
        self.project_root_label.pack(side=tk.LEFT, fill=tk.X, expand=True, padx=(5, 0))

        # 扫描按钮
        scan_btn_frame = ttk.Frame(left_frame)
        scan_btn_frame.pack(fill=tk.X, pady=(0, 5))
        ttk.Button(scan_btn_frame, text="🔄 扫描项目", command=self._do_scan_project).pack(side=tk.LEFT, padx=2)
        self.scan_status_label = ttk.Label(scan_btn_frame, text="未扫描", foreground="gray")
        self.scan_status_label.pack(side=tk.LEFT, padx=10)

        # 文件树（Treeview）
        tree_scroll_frame = ttk.Frame(left_frame)
        tree_scroll_frame.pack(fill=tk.BOTH, expand=True)

        self.project_file_tree = ttk.Treeview(tree_scroll_frame, show="tree headings", height=18)
        self.project_file_tree.heading("#0", text="文件/文件夹")
        self.project_file_tree.column("#0", width=290)
        
        tree_scroll_y = ttk.Scrollbar(tree_scroll_frame, orient=tk.VERTICAL, command=self.project_file_tree.yview)
        tree_scroll_x = ttk.Scrollbar(tree_scroll_frame, orient=tk.HORIZONTAL, command=self.project_file_tree.xview)
        self.project_file_tree.configure(yscrollcommand=tree_scroll_y.set, xscrollcommand=tree_scroll_x.set)

        self.project_file_tree.grid(row=0, column=0, sticky="nsew")
        tree_scroll_y.grid(row=0, column=1, sticky="ns")
        tree_scroll_x.grid(row=1, column=0, sticky="ew")

        tree_scroll_frame.grid_rowconfigure(0, weight=1)
        tree_scroll_frame.grid_columnconfigure(0, weight=1)

        # 绑定文件选择事件
        self.project_file_tree.bind("<<TreeviewSelect>>", self._on_project_file_selected)

        # 右侧面板：上下分栏
        right_panel = ttk.Frame(main_paned)
        main_paned.add(right_panel, weight=3)

        # ===== 右上：文件内容查看器 =====
        content_frame = ttk.LabelFrame(right_panel, text="📄 文件内容预览", padding=5)
        content_frame.pack(fill=tk.BOTH, expand=True, pady=(0, 5))

        # 文件信息栏
        file_info_bar = ttk.Frame(content_frame)
        file_info_bar.pack(fill=tk.X, pady=(0, 3))
        self.preview_file_label = ttk.Label(file_info_bar, text="未选择文件", foreground="gray")
        self.preview_file_label.pack(side=tk.LEFT)
        self.preview_size_label = ttk.Label(file_info_bar, text="", foreground="gray")
        self.preview_size_label.pack(side=tk.RIGHT)

        # 内容显示区
        preview_scroll_frame = ttk.Frame(content_frame)
        preview_scroll_frame.pack(fill=tk.BOTH, expand=True)

        self.file_preview_text = scrolledtext.ScrolledText(preview_scroll_frame, height=12, wrap=tk.WORD,
                                                            font=("Consolas", 9), state=tk.DISABLED)
        self.file_preview_text.pack(fill=tk.BOTH, expand=True)

        # ===== 右下：分析操作区 + 结果展示 =====
        bottom_panel = ttk.Frame(right_panel)
        bottom_panel.pack(fill=tk.BOTH, expand=True)

        # 操作按钮区
        action_frame = ttk.LabelFrame(bottom_panel, text="🔧 分析操作", padding=5)
        action_frame.pack(fill=tk.X, pady=(0, 5))

        btn_row1 = ttk.Frame(action_frame)
        btn_row1.pack(fill=tk.X, pady=2)
        ttk.Button(btn_row1, text="📊 分析项目现状", command=self._analyze_project_full, width=22).pack(side=tk.LEFT, padx=2)
        ttk.Button(btn_row1, text="🔍 资产缺口分析", command=self._analyze_asset_gaps, width=22).pack(side=tk.LEFT, padx=2)

        btn_row2 = ttk.Frame(action_frame)
        btn_row2.pack(fill=tk.X, pady=2)
        ttk.Button(btn_row2, text="🤖 AI提取需求", command=self._ai_extract_requirements_from_doc, width=22).pack(side=tk.LEFT, padx=2)
        ttk.Button(btn_row2, text="🚀 一键生成队列", command=self._ai_generate_queue_from_requirements, width=22).pack(side=tk.LEFT, padx=2)

        # AI 忙碌状态
        self.project_ai_busy_label = ttk.Label(action_frame, text="", foreground="blue")
        self.project_ai_busy_label.pack(anchor=tk.W, pady=(5, 0))

        # 结果展示区
        result_frame = ttk.LabelFrame(bottom_panel, text="📋 分析报告 / AI 结果", padding=5)
        result_frame.pack(fill=tk.BOTH, expand=True)

        self.analysis_result_text = scrolledtext.ScrolledText(result_frame, height=10, wrap=tk.WORD,
                                                               font=("Consolas", 9), state=tk.DISABLED)
        self.analysis_result_text.pack(fill=tk.BOTH, expand=True)

        # 结果操作按钮
        result_btn_frame = ttk.Frame(result_frame)
        result_btn_frame.pack(fill=tk.X, pady=(5, 0))
        ttk.Button(result_btn_frame, text="复制报告", command=self._copy_analysis_result).pack(side=tk.LEFT, padx=2)
        ttk.Button(result_btn_frame, text="清空", command=lambda: self._clear_analysis_result()).pack(side=tk.LEFT, padx=2)
        ttk.Button(result_btn_frame, text="应用到资产生成", command=self._apply_analysis_to_generate).pack(side=tk.LEFT, padx=2)

        # 存储最后一次分析结果
        self.last_analysis_requirements = []  # 存储提取的需求列表

    # ==========================================
    # 业务逻辑 - 原有功能（保持不变）
    # ==========================================
    def _log(self, msg, tag=""):
        """记录日志到全局日志栏"""
        self.global_log_text.configure(state=tk.NORMAL)
        ts = datetime.datetime.now().strftime("%H:%M:%S")
        prefix = f"[{ts}]"
        if tag:
            prefix += f" [{tag}]"
        self.global_log_text.insert(tk.END, f"{prefix} {msg}\n")
        self.global_log_text.see(tk.END)
        self.global_log_text.configure(state=tk.DISABLED)

        # 同时记录到 Tab 1 的日志
        if hasattr(self, 'log_text'):
            self.log_text.configure(state=tk.NORMAL)
            self.log_text.insert(tk.END, f"{prefix} {msg}\n")
            self.log_text.see(tk.END)
            self.log_text.configure(state=tk.DISABLED)

    def _refresh_assets(self):
        cat = self.cat_var.get()
        if cat == "全部":
            names = list(ASSET_TEMPLATES.keys())
        else:
            names = [k for k, v in ASSET_TEMPLATES.items() if v["cat"] == cat]
        self.asset_combo["values"] = names
        if names:
            self.asset_combo.current(0)
            self._on_asset_changed()

    # ==========================================
    # v3.2: 服务器工作流管理（8189 统一 API）
    # ==========================================
    def _refresh_server_workflows_v2(self):
        """从服务器 8189 API 加载工作流列表（41 个工作流：11 注册 + 30 蓝图）"""
        try:
            if HAS_COMMS_SDK:
                data = CommsClient.http_list_workflows(SERVER_URL)
            else:
                r = requests.get(f"{SERVER_URL}/workflows", timeout=10)
                r.raise_for_status()
                data = r.json()

            if "error" in data:
                raise Exception(data["error"])

            # 构建 (显示名, workflow_id) 列表
            items = []
            # 注册工作流
            for wf_id, wf in data.get("workflows", {}).items():
                items.append((f"[注册] {wf.get('name','?')} ({wf.get('speed','?')})", wf_id))
            # 蓝图
            for bp_id, bp in data.get("blueprints", {}).items():
                items.append((f"[蓝图] {bp.get('name','?')} [{bp.get('category','?')}]", bp_id))

            display_names = [item[0] for item in items]
            self.server_workflow_combo['values'] = display_names
            self._server_workflow_map = {item[0]: item[1] for item in items}

            # 默认选中 "text-to-image-z-image-turbo"（蓝图，参数可控）
            default_idx = 0
            for i, (name, wid) in enumerate(items):
                if wid == "text-to-image-z-image-turbo":
                    default_idx = i
                    break
            if display_names:
                self.server_workflow_var.set(display_names[default_idx])

            self.server_wf_info_label.configure(text=f"共 {len(items)} 个工作流可用")
            self._log("info", f"加载了 {len(items)} 个服务器工作流")
        except Exception as e:
            self._log("error", f"加载工作流失败: {e}")
            self.server_wf_info_label.configure(text=f"连接失败: {e}，将尝试回退到 8188")

    def _on_server_workflow_selected(self, event=None):
        """服务器工作流选择变更回调"""
        display_name = self.server_workflow_var.get()
        wf_id = self._server_workflow_map.get(display_name, "")
        if wf_id:
            self._log("info", f"已选择工作流: {display_name} → {wf_id}")

    def _get_logical_workflow_id(self):
        """根据资产类型和阶段自动选择最合适的服务器工作流"""
        # 先检查用户是否手动选择了工作流
        display_name = self.server_workflow_var.get()
        manual_wf_id = self._server_workflow_map.get(display_name, "")
        if manual_wf_id and not display_name.startswith("[蓝图] text-to-image-z-image-turbo"):
            # 用户手动选择了非默认工作流，尊重用户选择
            return manual_wf_id

        asset_type = self.asset_type_var.get().lower()
        is_concept = self.stage_var.get() == "concept"

        if is_concept:
            # 概念图使用 Z-Image 蓝图（轻量模型，适配12GB显存，提示词用自然语言）
            return "text-to-image-z-image-turbo"

        # 精灵帧映射
        mapping = {
            "boss": "unity-2d-character-sprite",
            "enemy": "unity-2d-character-sprite",
            "player": "unity-2d-character-sprite",
            "npc": "unity-2d-character-sprite",
            "item": "game-item-fast",
            "map_tile": "unity-2d-tilemap",
            "background": "game-env-background",
            "ui": "unity-2d-ui-element",
        }
        return mapping.get(asset_type, "zimage-text-to-image")

    def _fetch_workflow_params(self, workflow_id):
        """获取服务器工作流的参数 schema（字段名和类型）

        查询顺序：
        1. /blueprints/{id} — 蓝图有明确的 params schema
        2. /workflows 返回的 registered workflows 列表 — 注册工作流的基本信息

        Args:
            workflow_id: 工作流 ID（如 text-to-image-z-image-turbo, game-character-design）

        Returns:
            dict 参数 schema，如 {"text": {"type": "string"}, "width": {"type": "int", ...}}
        """
        # 策略1: 尝试蓝图端点（有完整 params schema）
        try:
            r = requests.get(f"{SERVER_URL}/blueprints/{workflow_id}", timeout=10)
            if r.status_code == 200:
                data = r.json()
                params = data.get("params", {})
                if params:
                    return params
        except Exception:
            pass

        # 策略2: 从 /workflows 全量列表中查找注册工作流信息
        try:
            r = requests.get(f"{SERVER_URL}/workflows", timeout=10)
            if r.status_code == 200:
                data = r.json()
                reg_wfs = data.get("workflows", {})
                if workflow_id in reg_wfs:
                    wf_info = reg_wfs[workflow_id]
                    # 注册工作流通常没有显式 params schema，
                    # 返回包含 name/speed 等基本信息的字典作为标识
                    return {"_type": "registered", "_info": wf_info}
        except Exception:
            pass

        # 均失败
        self._log("warning", f"未找到 {workflow_id} 的参数 schema，使用默认映射")
        return {}

    def _map_params_to_workflow(self, raw_params, wf_schema):
        """将客户端内部参数映射为服务器工作流期望的字段名

        映射规则：
        - positive → 查找 type=string 且 description 含 "prompt" 的字段（通常是 "text"）
        - negative → 同上但优先级低
        - width/height/steps/cfg/seed → 直接匹配同名或同义字段

        Args:
            raw_params: 客户端内部标准参数 {"positive": ..., "width": ...}
            wf_schema: 工作流参数 schema

        Returns:
            适配后的参数字典
        """
        if not wf_schema or wf_schema.get("_type") == "registered":
            # 无 schema 或注册工作流 → 使用默认映射
            return {
                "text": raw_params["positive"],
                "width": raw_params["width"],
                "height": raw_params["height"],
            }

        result = {}
        # 建立反向映射：从 schema 字段名推断用途
        string_fields = []  # 可能是 prompt 的文本字段
        for field_name, field_info in wf_schema.items():
            ftype = field_info.get("type", "")
            desc = (field_info.get("description") or "").lower()

            if ftype == "string":
                string_fields.append(field_name)

        # 将 positive 提示词填入第一个 string 字段（通常是 "text" 或 "prompt"）
        if string_fields:
            result[string_fields[0]] = raw_params["positive"]
            # 如果有第二个 string 字段且工作流支持 negative，填入
            if len(string_fields) > 1 and raw_params["negative"]:
                result[string_fields[1]] = raw_params["negative"]

        # 数值字段直接匹配
        numeric_map = {
            "width": "width",
            "height": "height",
            "steps": "steps",
            "cfg": ("cfg", "cfg_scale", "guidance_scale"),
            "seed": "seed",
        }
        for client_key, server_keys in numeric_map.items():
            val = raw_params.get(client_key)
            if val is None:
                continue
            if isinstance(server_keys, str):
                server_keys = [server_keys]
            for sk in server_keys:
                if sk in wf_schema:
                    result[sk] = val
                    break

        return result

    # [已弃用] v3.2 服务器统一管理模型，以下方法保留用于向后兼容
    def _refresh_checkpoints(self):
        """[已弃用] 从服务器获取可用 checkpoint 列表 — v3.2 不再使用"""
        try:
            r = requests.get(f"{FALLBACK_URL}/api/object_info/CheckpointLoaderSimple", timeout=10)
            r.raise_for_status()
            data = r.json()
            node_info = data.get("CheckpointLoaderSimple", {})
            ckpt_list = node_info.get("input", {}).get("required", {}).get("ckpt_name", [[]])[0]
            if ckpt_list:
                if self.ckpt_combo is not None:
                    self.ckpt_combo["values"] = ckpt_list
                current = self.checkpoint_var.get()
                if current not in ckpt_list:
                    self.checkpoint_var.set(ckpt_list[0])
                self._log(f"获取到 {len(ckpt_list)} 个 Checkpoint (8188回退)")
            else:
                self._log("未获取到 Checkpoint 列表，使用默认值")
        except Exception as e:
            self._log(f"获取 Checkpoint 列表失败: {e}", "ERROR")

    def _on_checkpoint_changed(self, event=None):
        """Checkpoint 变更时自动调整 SDXL 相关参数

        SDXL 模型与 SD 1.5 模型的最佳参数不同：
        - SDXL 推荐：steps=30, cfg=7, 尺寸=1024×1024（训练分辨率更高）
        - SD 1.5 默认：steps=20, cfg=7, 尺寸=512×512
        """
        ckpt = self.checkpoint_var.get()
        is_sdxl = 'sdxl' in ckpt.lower() or 'xl' in ckpt.lower()

        if is_sdxl:
            # SDXL 推荐参数：更高步数、更大分辨率
            self.param_vars["steps_var"].set("30")
            self.param_vars["cfg_var"].set("7")
            self.param_vars["width_var"].set("1024")
            self.param_vars["height_var"].set("1024")
            self._log(f"已切换到 SDXL 模型，自动调整参数: steps=30, 尺寸=1024×1024")
        else:
            # SD 1.5 默认参数
            self.param_vars["steps_var"].set("20")
            self.param_vars["cfg_var"].set("7")
            self.param_vars["width_var"].set("512")
            self.param_vars["height_var"].set("512")
            self._log(f"已切换到 SD 1.5 模型，自动调整参数: steps=20, 尺寸=512×512")

    def _on_workflow_mode_changed(self):
        """工作流模式切换回调：显示/隐藏工作流选择器"""
        mode = self.wf_mode_var.get()
        if mode == "builtin":
            # 内置模式：隐藏选择器
            self.wf_select_frame.pack_forget()
            self._log("工作流模式: 内置（客户端自动构建）")
        elif mode == "server":
            # 服务器模式：显示选择器并刷新列表
            self.wf_select_frame.pack(fill=tk.X, pady=(5, 0))
            self._refresh_server_workflows()
        elif mode == "local":
            # 本地模式：显示选择器并加载本地列表
            self.wf_select_frame.pack(fill=tk.X, pady=(5, 0))
            self._load_local_workflows()

    def _refresh_server_workflows(self):
        """刷新服务器工作流列表

        ComfyUI REST API 不提供列出已保存工作流的功能。
        策略：
        1. 从生成历史记录中提取可用的工作流（作为可复用选项）
        2. 已知文件名探测（/api/workflow?filename=xxx）
        3. 若均无结果，提示用户手动输入文件名或使用内置模式

        用户也可直接在输入框中手动键入工作流文件名（如 my-workflow.json）
        """
        try:
            server_url = self.config.get("server", {}).get("url", SERVER_URL)
            workflow_names = []

            # 方法A：从历史记录中提取最近用过的工作流
            try:
                r = requests.get(f"{server_url}/api/history?max_items=10", timeout=10)
                if r.status_code == 200:
                    history_data = r.json()
                    if isinstance(history_data, dict) and history_data:
                        # 从历史记录的 prompt 中提取节点信息，标记为 "历史: {id[:8]}"
                        for hid, hval in list(history_data.items())[:5]:
                            prompt = hval.get("prompt", {})
                            if prompt and isinstance(prompt, dict):
                                node_count = len(prompt)
                                has_save = any(
                                    nd.get("class_type") in ("SaveImage", "SaveImageWebsocket")
                                    for nd in prompt.values() if isinstance(nd, dict)
                                )
                                label = f"📋 历史 {hid[:8]} ({node_count}节点)"
                                if not has_save:
                                    label += " ⚠"
                                workflow_names.append((label, hid))
            except Exception:
                pass

            # 方法B：已知工作流文件名探测
            known_workflows = [
                "zimage-text-to-image.json",
                "qwen-edit-img-to-img.json",
                "default_workflow.json",
            ]
            for name in known_workflows:
                try:
                    tr = requests.get(f"{server_url}/api/workflow?filename={name}", timeout=5)
                    if tr.status_code == 200:
                        workflow_names.append((name, name))
                except Exception:
                    pass

            # 更新 UI
            if workflow_names:
                display_names = [item[0] for item in workflow_names]
                self.server_wf_combo["values"] = display_names
                self.server_wf_var.set(display_names[0])
                # 存储映射：显示名 -> 实际值
                self._server_wf_map = {item[0]: item[1] for item in workflow_names}
                count_hist = sum(1 for _, v in workflow_names if len(v) > 20)  # 历史记录ID较长
                count_file = len(workflow_names) - count_hist
                hints = []
                if count_hist > 0:
                    hints.append(f"{count_hist}个历史工作流")
                if count_file > 0:
                    hints.append(f"{count_file}个已保存文件")
                self.wf_hint_label.configure(text=" | ".join(hints))
                self._log("info", f"获取到 {len(workflow_names)} 个服务器工作流（{', '.join(hints)}）")
            else:
                self.server_wf_combo["values"] = []
                self.server_wf_var.set("")
                self._server_wf_map = {}
                self.wf_hint_label.configure(text="可直接输入文件名 (如 my-workflow.json)")
                self._log("info", "无已保存工作流 — 可在上方输入框中手动输入文件名，或切换到「内置」模式")

        except Exception as e:
            self._log("error", f"获取服务器工作流失败: {e}")
            self.server_wf_combo["values"] = []
            self.server_wf_var.set("")
            self.wf_hint_label.configure(text=f"连接失败: {e}")

    def _load_local_workflows(self):
        """读取本地 workflows/ 目录中的 JSON 文件列表"""
        try:
            if not os.path.isdir(WORKFLOW_DIR):
                os.makedirs(WORKFLOW_DIR, exist_ok=True)
                self.server_wf_combo["values"] = ["(目录为空)"]
                self.server_wf_var.set("(目录为空)")
                self._log(f"info", f"本地工作流目录已创建: {WORKFLOW_DIR}")
                return

            json_files = sorted(
                f for f in os.listdir(WORKFLOW_DIR)
                if f.endswith(".json") and os.path.isfile(os.path.join(WORKFLOW_DIR, f))
            )

            if json_files:
                self.server_wf_combo["values"] = json_files
                current = self.server_wf_var.get()
                if not current or current not in json_files:
                    self.server_wf_var.set(json_files[0])
                self._log(f"info", f"加载到 {len(json_files)} 个本地工作流")
            else:
                self.server_wf_combo["values"] = ["(无文件)"]
                self.server_wf_var.set("(无文件)")
                self._log("warning", "本地工作流目录中没有 JSON 文件")

        except Exception as e:
            self._log("error", f"加载本地工作流列表失败: {e}")
            self.server_wf_combo["values"] = ["(加载失败)"]
            self.server_wf_var.set("(加载失败)")

    def _load_server_workflow(self, display_name):
        """从 ComfyUI 服务器加载工作流

        支持三种来源：
        1. 历史记录（显示名以 "📋 历史" 开头）→ 从 /api/history/{id} 加载
        2. 已保存文件（文件名）→ 从 /api/workflow?filename= 加载
        3. 手动输入的文件名 → 同上

        Args:
            display_name: 用户选择的显示名或手动输入的文本

        Returns:
            工作流字典，失败返回 None
        """
        if not display_name or display_name.startswith("("):
            return None

        try:
            server_url = self.config.get("server", {}).get("url", SERVER_URL)

            # 通过映射解析实际值（历史记录ID 或 文件名）
            actual = getattr(self, '_server_wf_map', {}).get(display_name, display_name)

            # 判断来源：display_name 前缀 "📋" 表示历史记录
            # 兼容：无前缀但实际值为长字符串（UUID格式历史ID）时也走历史分支
            if display_name.startswith("📋") or (len(actual) > 20 and not actual.endswith(".json")):
                # 历史记录：从 /api/history/{id} 获取 prompt
                r = requests.get(f"{server_url}/api/history/{actual}", timeout=15)
                if r.status_code == 200:
                    data = r.json()
                    wf = data.get("prompt", {})
                    if wf:
                        self._log("info", f"已加载历史工作流: {actual[:8]}...")
                        return wf
                raise Exception(f"历史记录 {actual[:8]} 不存在")
            else:
                # 文件名：从 /api/workflow?filename= 加载
                r = requests.get(f"{server_url}/api/workflow?filename={actual}", timeout=15)
                r.raise_for_status()
                wf = r.json()
                self._log("info", f"已加载服务器工作流: {actual}")
                return wf

        except Exception as e:
            self._log("error", f"加载服务器工作流 '{display_name}' 失败: {e}")
            return None

    def _load_local_workflow(self, filename):
        """从本地 workflows/ 目录加载指定工作流 JSON

        Args:
            filename: 文件名

        Returns:
            工作流字典，失败返回 None
        """
        if not filename or filename.startswith("("):
            return None
        try:
            filepath = os.path.join(WORKFLOW_DIR, filename)
            with open(filepath, "r", encoding="utf-8") as f:
                wf = json.load(f)
            self._log(f"info", f"已加载本地工作流: {filename}")
            return wf
        except Exception as e:
            self._log("error", f"加载本地工作流 '{filename}' 失败: {e}")
            return None

    def _on_asset_changed(self):
        name = self.asset_var.get()
        if name not in ASSET_TEMPLATES:
            return
        tmpl = ASSET_TEMPLATES[name]
        self.info_label.configure(text=f"分类: {tmpl['cat']} | 目标: {tmpl['targetW']}x{tmpl['targetH']} | 生成: {tmpl['genW']}x{tmpl['genH']}")
        self._update_prompts()

    def _on_stage_changed(self):
        stage = self.stage_var.get()
        if stage == "sprite":
            self.state_frame.pack(fill=tk.X, pady=(5, 0), before=self.concept_frame)
            self.concept_frame.pack(fill=tk.X, pady=(10, 0), before=self.generate_btn.master)
            self.param_vars["width_var"].set("512")
            self.param_vars["height_var"].set("512")
            self.param_vars["count_var"].set("6")
        else:
            self.state_frame.pack_forget()
            self.concept_frame.pack_forget()
            self.param_vars["width_var"].set("1024")
            self.param_vars["height_var"].set("1024")
            self.param_vars["count_var"].set("1")
        self._update_prompts()

    def _update_prompts(self):
        name = self.asset_var.get()
        if name not in ASSET_TEMPLATES:
            return
        tmpl = ASSET_TEMPLATES[name]
        stage = self.stage_var.get()

        if stage == "concept":
            positive = f"{CONCEPT_BASE_POSITIVE}, {tmpl['prompt']}"
            negative = CONCEPT_NEGATIVE
        else:
            state = self.state_var.get()
            state_p = STATE_PROMPTS.get(state, "idle pose")
            positive = f"{SPRITE_BASE_POSITIVE}, {tmpl['prompt']}, {state_p}, single frame, pixel art character"
            negative = SPRITE_NEGATIVE

        self.positive_text.delete("1.0", tk.END)
        self.positive_text.insert("1.0", positive)
        self.negative_text.delete("1.0", tk.END)
        self.negative_text.insert("1.0", negative)

    def _browse_output(self):
        path = filedialog.askdirectory(initialdir=self.output_var.get())
        if path:
            self.output_var.set(path)

    def _browse_concept_image(self):
        name = self.asset_var.get()
        if name and name in ASSET_TEMPLATES:
            tmpl = ASSET_TEMPLATES[name]
            initial_dir = os.path.join(self.output_var.get(), tmpl["cat"], name, "Concept")
            if not os.path.exists(initial_dir):
                initial_dir = self.output_var.get()
        else:
            initial_dir = self.output_var.get()

        path = filedialog.askopenfilename(
            title="选择概念图（参考底图）",
            initialdir=initial_dir,
            filetypes=[("PNG 图片", "*.png"), ("所有文件", "*.*")]
        )
        if path:
            self._set_concept_image(path)

    def _set_concept_image(self, path):
        self.ref_image_path = path
        filename = os.path.basename(path)
        self.concept_preview_label.configure(text=f"✓ 已选择: {filename}", foreground="green")
        self._log(f"已选择参考底图: {path}")

    def _clear_concept_image(self):
        self.ref_image_path = None
        self.concept_preview_label.configure(text="未选择概念图", foreground="gray")
        self._log("已清除参考底图")

    # ------------------------------------------
    # 模块8: ServerMonitor 回调方法
    # ------------------------------------------
    def _on_server_status_update(self, data):
        """服务器状态更新回调（从 WebSocket 接收，在主线程更新UI）"""
        def update_ui():
            gpu = data.get("gpu", {})
            vram_total = gpu.get("vram_total", 0)
            vram_free = gpu.get("vram_free", 0)
            vram_used = gpu.get("vram_used", 0)

            queue = data.get("queue", {})
            q_current = queue.get("current", 0)
            q_pending = queue.get("pending", 0)

            clients = data.get("clients", 0)
            uptime = data.get("uptime_seconds", 0)

            hours = int(uptime // 3600)
            mins = int((uptime % 3600) // 60)

            # 更新工具栏简明状态
            self.monitor_vram_label.configure(text=f"VRAM: {vram_free:.1f}GB")
            self.monitor_queue_label.configure(text=f"队列: {q_current}/{q_pending}")
            self.monitor_clients_label.configure(text=f"客户端: {clients}")
            self.monitor_uptime_label.configure(text=f"运行: {hours}h{mins}m")

            # VRAM 颜色提示
            if vram_total > 0:
                usage_pct = vram_used / vram_total * 100
                if usage_pct > 90:
                    color = "red"
                elif usage_pct > 70:
                    color = "orange"
                else:
                    color = "green"
                self.monitor_vram_label.configure(foreground=color)

            # 更新连接状态
            if not self.server_connected:
                self.server_connected = True
                self.status_label.configure(text="● 服务器: 已连接 (实时)", foreground="green")

            # 更新详情面板（如果可见）
            if self.monitor_detail_visible:
                self._update_monitor_detail(data)

        # 使用 root.after 确保 UI 更新在主线程
        self.root.after(0, update_ui)

    def _update_monitor_detail(self, data):
        """更新详情面板数据"""
        gpu = data.get("gpu", {})
        vram_total = gpu.get("vram_total", 0)
        vram_free = gpu.get("vram_free", 0)
        vram_used = gpu.get("vram_used", 0)

        self.detail_gpu_name.configure(text=gpu.get("name", "--"))

        if vram_total > 0:
            self.detail_vram_bar['maximum'] = 100
            self.detail_vram_bar['value'] = (vram_used / vram_total) * 100
            self.detail_vram_text.configure(text=f"{vram_used:.1f} / {vram_total:.1f} GB (空闲 {vram_free:.1f})")

        self.detail_version.configure(text=data.get("comfyui_version", "--"))
        self.detail_pytorch.configure(text=data.get("pytorch_version", "--"))

        ram = data.get("system_ram", {})
        ram_total = ram.get("total_gb", 0)
        ram_free = ram.get("free_gb", 0)
        self.detail_ram.configure(text=f"{ram_free:.1f} / {ram_total:.1f} GB")

        models = data.get("models_loaded", [])
        self.detail_models.configure(text=", ".join(models) if models else "--")

        # 根据VRAM使用率给建议
        suggestion = ""
        if vram_total > 0:
            usage = vram_used / vram_total * 100
            if usage > 95:
                suggestion = "⚠️ 显存几乎耗尽！请等待当前任务完成或考虑减少 batch_size"
            elif usage > 80:
                suggestion = "显存使用较高，建议避免同时生成大尺寸图片"
            elif usage < 30:
                suggestion = "显存充裕，可以放心进行批量生成任务"
            else:
                suggestion = "显存使用正常"
        self.detail_suggestion.configure(text=suggestion)

    def _toggle_monitor_detail(self):
        """切换详情面板显示/隐藏"""
        if self.monitor_detail_visible:
            self.monitor_detail_frame.forget()
            self.monitor_detail_visible = False
            self.monitor_detail_btn.configure(text="📊 详情")
        else:
            # 在工具栏后插入详情面板
            self.monitor_detail_frame.pack(fill=tk.X)
            self.monitor_detail_visible = True
            self.monitor_detail_btn.configure(text="▲ 收起")
            # 如果有数据，立即刷新
            if self.server_monitor.server_data:
                self._update_monitor_detail(self.server_monitor.server_data)

    def _test_connection(self):
        threading.Thread(target=self._do_test_connection, daemon=True).start()

    def _do_test_connection(self):
        """v3.2: 优先测试 8189 连接，失败回退到 8188"""
        try:
            # 先尝试 8189 /status
            r = requests.get(f"{SERVER_URL}/status", timeout=10)
            if r.status_code == 200:
                http_data = r.json()
                if http_data.get("server") or http_data.get("gpu"):
                    self.root.after(0, lambda: self.status_label.configure(
                        text="服务器: 已连接 (v3.2)", foreground="green"))
                    self.root.after(0, lambda: self._log("8189 v3.2 API 连接成功!"))
                    self.server_connected = True
                    self._check_queue()

                    # 更新监控数据
                    gpu_info = http_data.get("gpu", {})
                    queue_info = http_data.get("queue", {})
                    monitor_data = {
                        "comfyui_version": http_data.get("comfyui_version", "?"),
                        "pytorch_version": http_data.get("pytorch_version", "?"),
                        "gpu": {
                            "name": gpu_info.get("name", "?"),
                            "vram_total": gpu_info.get("vram_total", 0),
                            "vram_free": gpu_info.get("vram_free", 0),
                            "vram_used": gpu_info.get("vram_used", 0),
                        },
                        "queue": {
                            "current": len(queue_info.get("queue_running", [])),
                            "pending": len(queue_info.get("queue_pending", [])),
                        },
                        "clients": http_data.get("clients", 1),
                        "uptime_seconds": http_data.get("uptime_seconds", 0),
                        "models_loaded": http_data.get("models_loaded", []),
                        "system_ram": {
                            "total_gb": http_data.get("system_ram", {}).get("total_gb", 0),
                            "free_gb": http_data.get("system_ram", {}).get("free_gb", 0),
                        }
                    }
                    self.root.after(0, lambda d=monitor_data: self._on_server_status_update(d))
                    # 加载工作流列表
                    self.root.after(0, self._refresh_server_workflows_v2)
                    return

            # 8189 无响应或无有效数据，尝试 8188
            raise Exception("8189 无有效响应")
        except Exception as e1:
            try:
                r = requests.get(f"{FALLBACK_URL}/api/system_stats", timeout=10)
                self.root.after(0, lambda: self.status_label.configure(
                    text="服务器: 已连接 (8188回退)", foreground="orange"))
                self.root.after(0, lambda e1=str(e1): self._log(f"8189 不可用 ({e1})，已回退到 8188 直连模式"))
                self.server_connected = True
            except Exception as e2:
                self.root.after(0, lambda: self.status_label.configure(
                    text="服务器: 连接失败", foreground="red"))
                self.root.after(0, lambda e1=str(e1), e2=str(e2): self._log(f"连接失败: 8189={e1}, 8188={e2}", "ERROR"))
                self.server_connected = False

    def _check_queue(self):
        threading.Thread(target=self._do_check_queue, daemon=True).start()

    def _do_check_queue(self):
        """v3.2: 通过 8189 /comfy/queue 检查队列"""
        try:
            r = requests.get(f"{COMFY_URL}/comfy/queue", timeout=5)
            data = r.json()
            running = len(data.get("queue_running", []))
            pending = len(data.get("queue_pending", []))
            self.root.after(0, lambda: self.monitor_queue_label.configure(text=f"队列: {running}/{pending}"))
        except Exception as e:
            self.root.after(0, lambda: self._log(f"队列检查失败: {e}", "ERROR"))

    def _generate(self):
        if self.generating:
            messagebox.showwarning("提示", "正在生成中，请等待完成")
            return

        name = self.asset_var.get()
        if not name or name not in ASSET_TEMPLATES:
            messagebox.showwarning("提示", "请选择资产类型")
            return

        stage = self.stage_var.get()
        if stage == "sprite" and not self.ref_image_path:
            messagebox.showwarning("提示", "精灵帧模式需要先选择参考底图（概念图）\n请点击「浏览概念图」按钮选择一张概念图")
            return

        self.generating = True
        self.generate_btn.configure(state=tk.DISABLED, text="生成中...")
        self.progress.start(10)

        threading.Thread(target=self._do_generate, daemon=True).start()

    def _do_generate(self):
        """v3.2: 通过 8189 统一 API 提交流程，自动回退到 8188 旧模式"""
        try:
            name = self.asset_var.get()
            tmpl = ASSET_TEMPLATES[name]
            stage = self.stage_var.get()
            state = self.state_var.get()

            positive = self.positive_text.get("1.0", tk.END).strip()
            negative = self.negative_text.get("1.0", tk.END).strip()

            width = int(self.param_vars["width_var"].get())
            height = int(self.param_vars["height_var"].get())
            steps = int(self.param_vars["steps_var"].get())
            cfg = float(self.param_vars["cfg_var"].get())
            seed = int(self.param_vars["seed_var"].get())
            if seed < 0:
                seed = random.randint(0, 2147483647)
            count = int(self.param_vars["count_var"].get())
            denoise = float(self.param_vars["denoise_var"].get())

            output_dir = os.path.join(self.output_var.get(), tmpl["cat"], name, "Concept" if stage == "concept" else f"Frames/{state}")
            os.makedirs(output_dir, exist_ok=True)

            self.root.after(0, lambda: self._log(f"开始生成: {name} ({stage})"))
            self.root.after(0, lambda: self._log(f"输出: {output_dir}"))
            self.root.after(0, lambda: self._log(f"尺寸: {width}x{height}, 步数: {steps}, CFG: {cfg}, 种子: {seed}"))
            self.root.after(0, lambda: self._log(f"数量: {count}, 去噪强度: {denoise}"))

            # 优先尝试 8189 v3.2 API
            workflow_id = self._get_logical_workflow_id()
            self.root.after(0, lambda: self._log(f"工作流: {workflow_id}"))

            # 获取工作流参数 schema，用于字段名映射
            wf_schema = self._fetch_workflow_params(workflow_id)

            # 构建内部参数（客户端标准字段）
            raw_params = {
                "positive": positive,
                "negative": negative,
                "seed": seed,
                "steps": steps,
                "cfg": cfg,
                "width": width,
                "height": height,
            }

            # 根据服务器工作流 schema 映射为实际参数
            params = self._map_params_to_workflow(raw_params, wf_schema)

            # 如果有参考底图（精灵帧模式），尝试上传后使用 img2img
            has_concept_ref = (stage == "sprite" and self.ref_image_path)
            if has_concept_ref:
                # v3.2: 若服务器支持 base64 图片参数，传递概念图
                try:
                    import base64
                    with open(self.ref_image_path, "rb") as imgf:
                        concept_base64 = base64.b64encode(imgf.read()).decode()
                    params["image"] = concept_base64
                    # img2img 场景使用对应的编辑工作流
                    if workflow_id in ("zimage-text-to-image", "game-character-design"):
                        workflow_id = "qwen-edit-img-to-img"
                    self.root.after(0, lambda: self._log("模式: img2img (含概念底图)"))
                except Exception as e:
                    self.root.after(0, lambda: self._log(f"warning: 概念图编码失败: {e}，使用纯文生图"))

            # 通过 8189 统一入口提交
            result = None
            use_fallback = False
            try:
                if HAS_COMMS_SDK:
                    result = CommsClient.http_execute_workflow(workflow_id, params, SERVER_URL)
                else:
                    payload = {
                        "workflow_id": workflow_id,
                        "params": params
                    }
                    # 调试：记录完整请求
                    self.root.after(0, lambda p=payload: self._log(f"[DEBUG] 请求: {json.dumps(p, ensure_ascii=False)[:500]}"))
                    r = requests.post(f"{SERVER_URL}/workflows/execute", json=payload, timeout=30)
                    # 调试：记录状态码和原始响应
                    self.root.after(0, lambda s=r.status_code, t=r.text[:500]: self._log(f"[DEBUG] 响应 status={s}, body={t}"))
                    r.raise_for_status()
                    result = r.json()

                # 调试：记录 API 解析后返回
                self.root.after(0, lambda res=result: self._log(f"[DEBUG] API 返回: {json.dumps(res, ensure_ascii=False)[:300]}, type={type(result).__name__}"))

                if "error" in result:
                    raise Exception(result["error"])
            except Exception as e:
                self.root.after(0, lambda err=e: self._log(f"warning: 8189 API 失败 ({err})，回退到 8188 直接模式"))
                use_fallback = True

            # 8189 回退：使用旧的 8188 直连模式
            if use_fallback:
                self._do_generate_fallback(name, tmpl, stage, state, positive, negative,
                                           width, height, steps, cfg, seed, count, denoise, output_dir)
                return

            prompt_id = result.get("prompt_id", "")
            wf_name = result.get("workflow_name", workflow_id)
            self.root.after(0, lambda: self._log(f"已提交: {wf_name} (prompt_id: {prompt_id})"))

            if not prompt_id:
                self.root.after(0, lambda: self._log("未获取到 prompt_id", "ERROR"))
                return

            # 轮询等待结果
            self._current_prompt_id = prompt_id
            self._current_output_dir = output_dir
            self._current_name = name
            self._current_stage = stage
            self._current_state = state
            self._current_ts = datetime.datetime.now().strftime("%Y%m%d_%H%M%S")
            self._poll_attempt = 0
            self._poll_result()

        except Exception as e:
            self.root.after(0, lambda err=e: self._log(f"错误: {err}", "ERROR"))
        finally:
            # note: generating flag + button restore handled in _handle_completed / _do_generate_fallback
            pass

    def _poll_result(self):
        """v3.2: 通过 8189 轮询等待任务完成"""
        pid = getattr(self, '_current_prompt_id', None)
        if not pid:
            self._finish_generate()
            return

        self._poll_attempt += 1
        try:
            # 检查队列
            if HAS_COMMS_SDK:
                queue = CommsClient.http_queue(COMFY_URL)
            else:
                r = requests.get(f"{COMFY_URL}/comfy/queue", timeout=5)
                queue = r.json() if r.status_code == 200 else {}

            in_queue = False
            running_list = queue.get("queue_running", [])
            pending_list = queue.get("queue_pending", [])
            for item in running_list:
                if isinstance(item, list) and len(item) > 1 and item[1] == pid:
                    in_queue = True; break
            if not in_queue:
                for item in pending_list:
                    if isinstance(item, list) and len(item) > 1 and item[1] == pid:
                        in_queue = True; break

            if not in_queue:
                # 任务可能已完成，查历史
                if HAS_COMMS_SDK:
                    hist = CommsClient.http_history(pid, COMFY_URL)
                else:
                    r = requests.get(f"{COMFY_URL}/comfy/history/{pid}", timeout=5)
                    hist = r.json() if r.status_code == 200 else {}

                if pid in hist:
                    self._handle_completed(hist[pid])
                    return

            # 输出进度日志
            if self._poll_attempt % 5 == 0:
                running_desc = f"运行中({len(running_list)})" if running_list else ""
                pending_desc = f"等待({len(pending_list)})" if pending_list else ""
                qdesc = " | ".join(filter(None, [running_desc, pending_desc])) or "队列空"
                self.root.after(0, lambda: self._log(f"轮询 #{self._poll_attempt}: {qdesc}"))

            # 超时检查 (最多 10 分钟)
            if self._poll_attempt >= 200:
                self.root.after(0, lambda: self._log("生成超时!", "ERROR"))
                self._finish_generate()
                return

            self.root.after(3000, self._poll_result)

        except Exception as e:
            self.root.after(0, lambda err=e: self._log(f"轮询失败: {err}", "ERROR"))
            if self._poll_attempt >= 200:
                self._finish_generate()
                return
            self.root.after(5000, self._poll_result)

    def _handle_completed(self, history_entry):
        """v3.2: 处理已完成的任务（下载文件并保存）"""
        # 检查执行状态
        status_info = history_entry.get("status", {})
        status_str = status_info.get("status_str", "")

        if status_str == "error":
            # 提取错误信息
            msgs = status_info.get("messages", [])
            err_msg = "未知错误"
            for msg_type, msg_data in reversed(msgs):
                if msg_type == "execution_error":
                    node_type = msg_data.get("node_type", "?")
                    exc_msg = msg_data.get("exception_message", "?")
                    err_msg = f"节点 [{node_type}] 错误: {exc_msg}"
                    break
            self.root.after(0, lambda em=err_msg: self._log(f"执行失败: {em}", "ERROR"))
            self._finish_generate()
            return

        self.root.after(0, lambda: self._log("生成完成!"))
        outputs = history_entry.get("outputs", {})
        images = []
        for node_id, node_data in outputs.items():
            for img in node_data.get("images", []):
                images.append(img)

        if not images:
            self.root.after(0, lambda: self._log("无输出图片", "ERROR"))
            self._finish_generate()
            return

        self.root.after(0, lambda: self._log(f"获取到 {len(images)} 张图片"))
        name = getattr(self, '_current_name', 'unknown')
        stage = getattr(self, '_current_stage', 'concept')
        state = getattr(self, '_current_state', 'idle')
        output_dir = getattr(self, '_current_output_dir', OUTPUT_BASE)
        ts = getattr(self, '_current_ts', datetime.datetime.now().strftime("%Y%m%d_%H%M%S"))

        saved_paths = []
        for i, img in enumerate(images):
            params_str = {"filename": img["filename"], "subfolder": img.get("subfolder", ""),
                          "type": img.get("type", "output")}
            # v3.2: 通过 8189 代理下载
            dl_url = f"{COMFY_URL}/comfy/view?{urlencode(params_str)}"

            if stage == "concept":
                save_name = f"{name}_Concept_{ts}.png"
            else:
                save_name = f"{name}_{state}_{i + 1}.png"

            save_path = os.path.join(output_dir, save_name)
            try:
                r = requests.get(dl_url, timeout=60)
                r.raise_for_status()
                with open(save_path, "wb") as f:
                    f.write(r.content)
                saved_paths.append(save_path)
                self.root.after(0, lambda p=save_path: self._log(f"已保存: {p}"))
            except Exception as e:
                self.root.after(0, lambda err=e: self._log(f"下载失败: {err}", "ERROR"))

        self.root.after(0, lambda: self._update_result_preview(saved_paths))
        self.root.after(0, lambda: self._log("全部完成!"))
        self._finish_generate()

    def _finish_generate(self):
        """清理生成状态"""
        self.generating = False
        self._current_prompt_id = None
        self.root.after(0, lambda: self.generate_btn.configure(state=tk.NORMAL, text="生成"))
        self.root.after(0, lambda: self.progress.stop())

    def _do_generate_fallback(self, name, tmpl, stage, state, positive, negative,
                               width, height, steps, cfg, seed, count, denoise, output_dir):
        """回退模式：直接使用 8188 ComfyUI API（保留向后兼容）"""
        try:
            server_url = FALLBACK_URL

            # 上传参考图（如果需要）
            if stage == "sprite" and self.ref_image_path:
                uploaded_filename = self._upload_image_to_comfyui(self.ref_image_path, server_url)
                if not uploaded_filename:
                    self._finish_generate()
                    return
                workflow = self._build_img2img_workflow(uploaded_filename, positive, negative, width, height, steps, cfg, denoise, seed, count)
            else:
                workflow = self._build_txt2img_workflow(positive, negative, width, height, steps, cfg, seed, count)

            body = {"prompt": workflow, "client_id": "comfyui-client-gui"}
            r = requests.post(f"{server_url}/api/prompt", json=body, timeout=30)
            r.raise_for_status()
            prompt_id = r.json().get("prompt_id")
            self.root.after(0, lambda: self._log(f"已提交(回退): {prompt_id}"))

            history = None
            for i in range(300):
                time.sleep(2)
                try:
                    h = requests.get(f"{server_url}/api/history/{prompt_id}", timeout=10)
                    h.raise_for_status()
                    history = h.json()
                    if prompt_id in history:
                        break
                except Exception:
                    pass
                if (i + 1) % 15 == 0:
                    self.root.after(0, lambda cnt=i: self._log(f"等待中... {(cnt + 1) * 2}s"))
            else:
                self.root.after(0, lambda: self._log("生成超时!", "ERROR"))
                self._finish_generate()
                return

            self.root.after(0, lambda: self._log("生成完成! (回退模式)"))
            history_data = history.get(prompt_id, {})
            outputs = history_data.get("outputs", {})
            images = []
            for node_id, node_data in outputs.items():
                for img in node_data.get("images", []):
                    images.append(img)

            if not images:
                self.root.after(0, lambda: self._log("无输出图片", "ERROR"))
                self._finish_generate()
                return

            ts = datetime.datetime.now().strftime("%Y%m%d_%H%M%S")
            saved_paths = []
            for i, img in enumerate(images):
                params_str = {"filename": img["filename"], "subfolder": img.get("subfolder", ""), "type": img.get("type", "output")}
                dl_url = f"{server_url}/api/view?{urlencode(params_str)}"
                if stage == "concept":
                    save_name = f"{name}_Concept_{ts}.png"
                else:
                    save_name = f"{name}_{state}_{i + 1}.png"
                save_path = os.path.join(output_dir, save_name)
                r = requests.get(dl_url, timeout=60)
                r.raise_for_status()
                with open(save_path, "wb") as f:
                    f.write(r.content)
                saved_paths.append(save_path)
                self.root.after(0, lambda p=save_path: self._log(f"已保存: {p}"))

            self.root.after(0, lambda: self._update_result_preview(saved_paths))
            self.root.after(0, lambda: self._log("全部完成!"))

        except Exception as e:
            self.root.after(0, lambda err=e: self._log(f"回退模式错误: {err}", "ERROR"))
        finally:
            self._finish_generate()

    # [已弃用] _build_txt2img_workflow — v3.2 服务器统一管理模型，保留用于 8188 回退
    def _build_txt2img_workflow(self, positive, negative, width, height, steps, cfg, seed, batch_size=1):
        # 根据模型名自动判断是否使用 SDXL 编码节点（SDXL 必须使用 CLIPTextEncodeSDXL）
        ckpt = self.checkpoint_var.get() or DEFAULT_CHECKPOINT
        is_sdxl = 'sdxl' in ckpt.lower() or 'xl' in ckpt.lower()
        encode_class = "CLIPTextEncodeSDXL" if is_sdxl else "CLIPTextEncode"
        return {
            "1": {"class_type": "CheckpointLoaderSimple", "inputs": {"ckpt_name": ckpt}},
            "2": {"class_type": encode_class, "inputs": {"text": positive, "clip": ["1", 1]}},
            "3": {"class_type": encode_class, "inputs": {"text": negative, "clip": ["1", 1]}},
            "4": {"class_type": "EmptyLatentImage", "inputs": {"width": width, "height": height, "batch_size": batch_size}},
            "5": {"class_type": "KSampler", "inputs": {"seed": seed, "steps": steps, "cfg": cfg, "sampler_name": "euler_ancestral", "scheduler": "normal", "denoise": 1, "model": ["1", 0], "positive": ["2", 0], "negative": ["3", 0], "latent_image": ["4", 0]}},
            "6": {"class_type": "VAEDecode", "inputs": {"samples": ["5", 0], "vae": ["1", 2]}},
            # 注意：使用 SaveImage 而非 SaveImageWebsocket，因为当前客户端通过 /api/history + /api/view 下载流程已可正常工作
            # 如需改用 SaveImageWebsocket（通过 WebSocket 回传二进制图片），需额外实现 WebSocket 监听逻辑
            "7": {"class_type": "SaveImage", "inputs": {"images": ["6", 0], "filename_prefix": "ComfyUI"}},
        }

    # [已弃用] _build_img2img_workflow — v3.2 服务器统一管理模型，保留用于 8188 回退
    def _build_img2img_workflow(self, image_filename, positive, negative, width, height, steps, cfg, denoise, seed, batch_size=1):
        # 根据模型名自动判断是否使用 SDXL 编码节点（SDXL 必须使用 CLIPTextEncodeSDXL）
        ckpt = self.checkpoint_var.get() or DEFAULT_CHECKPOINT
        is_sdxl = 'sdxl' in ckpt.lower() or 'xl' in ckpt.lower()
        encode_class = "CLIPTextEncodeSDXL" if is_sdxl else "CLIPTextEncode"
        return {
            "1": {"class_type": "CheckpointLoaderSimple", "inputs": {"ckpt_name": ckpt}},
            "2": {"class_type": encode_class, "inputs": {"text": positive, "clip": ["1", 1]}},
            "3": {"class_type": encode_class, "inputs": {"text": negative, "clip": ["1", 1]}},
            "4": {"class_type": "LoadImage", "inputs": {"image": image_filename}},
            "5": {"class_type": "VAEEncode", "inputs": {"pixels": ["4", 0], "vae": ["1", 2]}},
            "6": {"class_type": "KSampler", "inputs": {"seed": seed, "steps": steps, "cfg": cfg, "sampler_name": "euler_ancestral", "scheduler": "normal", "denoise": denoise, "model": ["1", 0], "positive": ["2", 0], "negative": ["3", 0], "latent_image": ["5", 0]}},
            "7": {"class_type": "VAEDecode", "inputs": {"samples": ["6", 0], "vae": ["1", 2]}},
            # 注意：使用 SaveImage 而非 SaveImageWebsocket，因为当前客户端通过 /api/history + /api/view 下载流程已可正常工作
            "8": {"class_type": "SaveImage", "inputs": {"images": ["7", 0], "filename_prefix": "ComfyUI_img2img"}},
        }

    def _upload_image_to_comfyui(self, image_path, server_url=None):
        if server_url is None:
            server_url = self.config.get("server", {}).get("url", SERVER_URL)
        try:
            filename = os.path.basename(image_path)
            self.root.after(0, lambda: self._log(f"正在上传参考图: {filename}..."))
            with open(image_path, 'rb') as f:
                files = {'image': (filename, f, 'image/png')}
                data = {'overwrite': 'true'}
                r = requests.post(f"{server_url}/api/upload/image", files=files, data=data, timeout=60)
                r.raise_for_status()
            result = r.json()
            uploaded_name = result.get("name", filename)
            self.root.after(0, lambda: self._log(f"参考图上传成功: {uploaded_name}"))
            return uploaded_name
        except Exception as e:
            self.root.after(0, lambda err=e: self._log(f"上传参考图失败: {err}", "ERROR"))
            return None

    def _update_result_preview(self, saved_paths):
        for widget in self.result_frame.winfo_children():
            widget.destroy()
        if not saved_paths:
            ttk.Label(self.result_frame, text="暂无生成结果", foreground="gray").pack(anchor=tk.W)
            return
        ttk.Label(self.result_frame, text=f"最近生成 {len(saved_paths)} 张图片:", font=("", 9, "bold")).pack(anchor=tk.W)
        for path in saved_paths[:8]:
            filename = os.path.basename(path)
            size_kb = os.path.getsize(path) // 1024
            label = ttk.Label(self.result_frame, text=f"📷 {filename} ({size_kb} KB)")
            label.pack(anchor=tk.W, padx=(10, 0))
        if len(saved_paths) > 8:
            ttk.Label(self.result_frame, text=f"... 还有 {len(saved_paths) - 8} 张", foreground="gray").pack(anchor=tk.W)

    # ==========================================
    # 模块1: LLM 功能实现
    # ==========================================
    def _init_llm_client(self):
        """初始化 LLM 客户端"""
        llm_cfg = self.config.get("llm", {})
        self.llm_client = LLMClient(
            base_url=llm_cfg.get("base_url", "http://localhost:1234/v1"),
            api_key=llm_cfg.get("api_key", ""),
            model_name=llm_cfg.get("model_name", "")
        )

    def _load_config_to_ui(self):
        """将配置加载到 UI"""
        llm_cfg = self.config.get("llm", {})
        self.llm_provider_var.set(llm_cfg.get("provider", "lm_studio"))
        self.llm_url_var.set(llm_cfg.get("base_url", "http://localhost:1234/v1"))
        self.llm_key_var.set(llm_cfg.get("api_key", ""))
        self.llm_model_var.set(llm_cfg.get("model_name", ""))

        srv_cfg = self.config.get("server", {})
        if srv_cfg.get("output_base"):
            self.output_var.set(srv_cfg["output_base"])

    def _on_llm_provider_changed(self):
        """提供商切换时更新默认值和提示"""
        provider = self.llm_provider_var.get()
        if provider == "lm_studio":
            self.llm_url_var.set("http://localhost:1234/v1")
            self.llm_key_var.set("")
            self.llm_provider_hint.configure(text="需在 LM Studio 中开启 Local Server（左侧菜单 → Developer → Local Server），加载模型后自动暴露 API，无需 API Key")
        elif provider == "openai":
            self.llm_url_var.set("https://api.openai.com/v1")
            self.llm_key_var.set("")
            self.llm_provider_hint.configure(text="需要有效的 OpenAI API Key，支持 GPT-4o / GPT-4-turbo 等模型")
        elif provider == "custom":
            self.llm_provider_hint.configure(text="填写兼容 OpenAI 格式的 API 地址和密钥（如 Ollama、vLLM、DeepSeek 等）")

    def _toggle_llm_key_visibility(self):
        """切换 API 密钥可见性"""
        if self.llm_key_entry["show"] == "*":
            self.llm_key_entry.configure(show="")
        else:
            self.llm_key_entry.configure(show="*")

    def _test_llm_connection(self):
        """测试 LLM 连接"""
        self._update_llm_client_from_ui()
        threading.Thread(target=self._do_test_llm, daemon=True).start()

    def _do_test_llm(self):
        try:
            self.root.after(0, lambda: self.llm_status_label.configure(text="状态: 测试中...", foreground="blue"))
            ok = self.llm_client.test_connection()
            if ok:
                models = self.llm_client.get_models()
                self.root.after(0, lambda: self.llm_status_label.configure(text=f"状态: ✓ 已连接 ({len(models)} 个模型)", foreground="green"))
                if models:
                    self.root.after(0, lambda m=models: self._update_llm_model_list(m))
            else:
                self.root.after(0, lambda: self.llm_status_label.configure(text="状态: ✗ 连接失败", foreground="red"))
        except Exception as e:
            err_str = str(e)
            # 针对常见错误给出更友好的提示
            if "ConnectionRefusedError" in err_str or "refused" in err_str.lower() or "10061" in err_str:
                hint = "\n→ 请确认 LM Studio 已启动并开启了 Local Server（Developer → Local Server → Start)"
                if self.llm_provider_var.get() == "lm_studio":
                    err_str += hint
            elif "404" in err_str or "Not Found" in err_str:
                err_str += "\n→ API 地址可能不正确，请检查路径是否包含 /v1 后缀"
            elif "401" in err_str or "Unauthorized" in err_str or "api_key" in err_str.lower():
                err_str += "\n→ API 密钥无效，请检查密钥设置"
            self.root.after(0, lambda err=err_str: self.llm_status_label.configure(text=f"状态: 错误 - {err}", foreground="red"))

    def _update_llm_model_list(self, models):
        """更新模型下拉列表"""
        self.llm_model_combo["values"] = models
        if models and not self.llm_model_var.get():
            self.llm_model_var.set(models[0])

    def _refresh_llm_models(self):
        """刷新模型列表"""
        self._update_llm_client_from_ui()
        threading.Thread(target=self._do_refresh_models, daemon=True).start()

    def _do_refresh_models(self):
        try:
            models = self.llm_client.get_models()
            self.root.after(0, lambda m=models: self._update_llm_model_list(m))
            self.root.after(0, lambda: self.llm_status_label.configure(text=f"获取到 {len(models)} 个模型", foreground="green"))
        except Exception as e:
            self.root.after(0, lambda err=e: self._log(f"刷新模型失败: {err}", "ERROR"))

    def _update_llm_client_from_ui(self):
        """从 UI 更新 LLM 客户端配置"""
        if self.llm_client:
            self.llm_client.base_url = self.llm_url_var.get().rstrip("/")
            self.llm_client.api_key = self.llm_key_var.get()
            self.llm_client.model_name = self.llm_model_var.get()

    def _save_llm_config(self):
        """保存 LLM 配置到文件"""
        self.config["llm"] = {
            "provider": self.llm_provider_var.get(),
            "base_url": self.llm_url_var.get(),
            "api_key": self.llm_key_var.get(),
            "model_name": self.llm_model_var.get()
        }
        self.config["server"]["output_base"] = self.output_var.get()
        if ConfigManager.save(self.config):
            messagebox.showinfo("成功", "配置已保存")
            self._log("LLM 配置已保存")
        else:
            messagebox.showerror("错误", "保存配置失败")

    def _set_ai_busy(self, busy):
        """设置 AI 操作忙碌状态"""
        if busy:
            self.ai_busy_label.configure(text="⏳ AI 思考中...")
        else:
            self.ai_busy_label.configure(text="")

    def _ai_optimize_prompts(self):
        """AI 优化当前提示词"""
        self._update_llm_client_from_ui()
        if not self.llm_client.model_name:
            messagebox.showwarning("提示", "请先配置并选择 LLM 模型名称")
            return

        positive = self.positive_text.get("1.0", tk.END).strip()
        negative = self.negative_text.get("1.0", tk.END).strip()
        if not positive:
            messagebox.showwarning("提示", "正向提示词为空")
            return

        user_msg = f"""请优化以下提示词，使其更适合高质量的游戏美术素材生成：

当前正向提示词:
{positive}

当前反向提示词:
{negative}

资产类型: {self.asset_var.get()}
生成阶段: {self.stage_var.get()}"""

        self._set_ai_busy(True)
        threading.Thread(target=self._do_ai_chat, args=(LLM_OPTIMIZE_PROMPT, user_msg, "optimize"), daemon=True).start()

    def _ai_generate_prompts(self):
        """AI 生成提示词"""
        self._update_llm_client_from_ui()
        if not self.llm_client.model_name:
            messagebox.showwarning("提示", "请先配置并选择 LLM 模型名称")
            return

        name = self.asset_var.get()
        stage = self.stage_var.get()
        state = self.state_var.get() if stage == "sprite" else ""

        user_msg = f"""请为以下资产生成完整的 Stable Diffusion 提示词：

资产名称: {name}
资产类别: {ASSET_TEMPLATES.get(name, {}).get('cat', '未知')}
生成阶段: {stage}
动画状态: {state if state else 'N/A'}"""

        self._set_ai_busy(True)
        threading.Thread(target=self._do_ai_chat, args=(LLM_GENERATE_PROMPT, user_msg, "generate"), daemon=True).start()

    def _ai_recommend_workflow(self):
        """AI 推荐工作流"""
        self._update_llm_client_from_ui()
        if not self.llm_client.model_name:
            messagebox.showwarning("提示", "请先配置并选择 LLM 模型名称")
            return

        requirement = self.ai_input_text.get("1.0", tk.END).strip()
        if not requirement:
            requirement = f"生成 {self.asset_var.get()} 的{'概念图' if self.stage_var.get() == 'concept' else '精灵帧'}"

        self._set_ai_busy(True)
        threading.Thread(target=self._do_ai_recommend_workflow, args=(requirement,), daemon=True).start()

    def _ai_custom_chat(self):
        """自定义对话"""
        self._update_llm_client_from_ui()
        if not self.llm_client.model_name:
            messagebox.showwarning("提示", "请先配置并选择 LLM 模型名称")
            return

        msg = self.ai_input_text.get("1.0", tk.END).strip()
        if not msg:
            messagebox.showwarning("提示", "请输入内容")
            return

        system_prompt = """你是一个专业的 ComfyUI 和 Stable Diffusion 助手，专门帮助游戏美术资产生成。
你可以帮助用户优化提示词、设计工作流、解决技术问题等。请用中文回答。"""

        self._set_ai_busy(True)
        threading.Thread(target=self._do_ai_chat, args=(system_prompt, msg, "chat"), daemon=True).start()

    def _do_ai_chat(self, system_prompt, user_msg, mode):
        """执行 AI 对话（后台线程）"""
        try:
            response = self.llm_client.chat(system_prompt, user_msg)
            self.root.after(0, lambda: self._display_ai_result(response, mode))
        except Exception as e:
            self.root.after(0, lambda err=e: self._display_ai_error(err))
        finally:
            self.root.after(0, lambda: self._set_ai_busy(False))

    def _do_ai_recommend_workflow(self, requirement):
        """执行 AI 工作流推荐（后台线程）"""
        try:
            # 收集服务器资源信息
            checkpoints = self._get_checkpoints_cached()
            object_info_summary = self._summarize_nodes_for_llm()

            prompt = LLM_WORKFLOW_PROMPT.format(
                server_resources_summary=f"Checkpoints: {checkpoints[:5]}\n\nNode categories summary:\n{object_info_summary}",
                user_requirement=requirement
            )

            response = self.llm_client.chat(prompt, "")
            workflow = self._extract_json_from_response(response)

            self.root.after(0, lambda w=workflow: self._display_ai_workflow(w))
        except Exception as e:
            self.root.after(0, lambda err=e: self._display_ai_error(err))
        finally:
            self.root.after(0, lambda: self._set_ai_busy(False))

    def _display_ai_result(self, response, mode):
        """显示 AI 结果"""
        self.ai_result_text.configure(state=tk.NORMAL)
        self.ai_result_text.delete("1.0", tk.END)
        self.ai_result_text.insert("1.0", response)
        self.ai_result_text.configure(state=tk.DISABLED)

        # 解析 POSITIVE/NEGATIVE
        if mode in ("optimize", "generate"):
            pos_match = re.search(r'POSITIVE:\s*(.+?)(?=NEGATIVE:|$)', response, re.DOTALL | re.IGNORECASE)
            neg_match = re.search(r'NEGATIVE:\s*(.+?)$', response, re.DOTALL | re.IGNORECASE)
            if pos_match:
                self.last_ai_positive = pos_match.group(1).strip()
            if neg_match:
                self.last_ai_negative = neg_match.group(1).strip()

        self._log(f"AI 返回结果 (mode={mode})")

    def _display_ai_workflow(self, workflow):
        """显示 AI 生成的工作流"""
        self.last_ai_workflow = workflow
        self.ai_result_text.configure(state=tk.NORMAL)
        self.ai_result_text.delete("1.0", tk.END)
        self.ai_result_text.insert("1.0", json.dumps(workflow, indent=2, ensure_ascii=False))
        self.ai_result_text.configure(state=tk.DISABLED)
        self._log("AI 生成了工作流 JSON")

    def _display_ai_error(self, error):
        """显示 AI 错误"""
        self.ai_result_text.configure(state=tk.NORMAL)
        self.ai_result_text.insert(tk.END, f"\n❌ 错误: {error}\n")
        self.ai_result_text.configure(state=tk.DISABLED)
        self._log(f"AI 错误: {error}", "ERROR")

    def _copy_ai_result(self):
        """复制 AI 结果到剪贴板"""
        result = self.ai_result_text.get("1.0", tk.END).strip()
        self.root.clipboard_clear()
        self.root.clipboard_append(result)
        self._log("已复制 AI 结果到剪贴板")

    def _apply_ai_result_to_generate(self):
        """将 AI 结果应用到资产生成"""
        if self.last_ai_positive:
            self.positive_text.configure(state=tk.NORMAL)
            self.positive_text.delete("1.0", tk.END)
            self.positive_text.insert("1.0", self.last_ai_positive)
            self.positive_text.configure(state=tk.DISABLED)
        if self.last_ai_negative:
            self.negative_text.configure(state=tk.NORMAL)
            self.negative_text.delete("1.0", tk.END)
            self.negative_text.insert("1.0", self.last_ai_negative)
            self.negative_text.configure(state=tk.DISABLED)
        if self.last_ai_positive or self.last_ai_negative:
            self._log("已应用 AI 优化的提示词到资产生成")
            # 切换到资产生成 Tab
            self.notebook.select(0)
            messagebox.showinfo("成功", "已将 AI 优化的提示词应用到资产生成 Tab")
        elif self.last_ai_workflow:
            # 如果只有工作流没有提示词，切换到工作流编辑器
            self.workflow_editor.from_workflow_json(self.last_ai_workflow)
            self._refresh_workflow_editor_ui()
            self.notebook.select(3)
            messagebox.showinfo("成功", "已将 AI 生成的工作流应用到工作流编辑器")
        else:
            messagebox.showwarning("提示", "没有可应用的 AI 结果，请先运行 AI 操作")

    # ==========================================
    # 模块2: 服务器资源功能
    # ==========================================
    def _refresh_all_resources(self):
        """刷新所有服务器资源"""
        self.resource_status_label.configure(text="⏳ 正在刷新...")
        threading.Thread(target=self._do_refresh_all_resources, daemon=True).start()

    def _do_refresh_all_resources(self):
        """在后台线程刷新所有资源"""
        try:
            self._fetch_checkpoints()
            self._fetch_vae()
            self._fetch_loras()
            self._fetch_object_info()
            self._fetch_and_show_custom_nodes()
            self._refresh_workflow_list()
            self.root.after(0, lambda: self.resource_status_label.configure(text="✓ 所有资源已刷新", foreground="green"))
        except Exception as e:
            self.root.after(0, lambda err=e: self.resource_status_label.configure(text=f"✗ 刷新失败: {err}", foreground="red"))

    def _fetch_checkpoints(self):
        """获取 Checkpoint 模型列表"""
        def do_fetch():
            try:
                server_url = self.config.get("server", {}).get("url", SERVER_URL)
                r = requests.get(f"{server_url}/api/models/checkpoints", timeout=15)
                data = r.json()
                self.server_checkpoints = data if isinstance(data, list) else list(data.values()) if isinstance(data, dict) else []

                # 清空并填充 Treeview
                self.root.after(0, lambda: self.ckpt_tree.delete(*self.ckpt_tree.get_children()))
                for ckpt in self.server_checkpoints:
                    if isinstance(ckpt, dict):
                        fname = ckpt.get("filename", ckpt.get("name", "?"))
                        size = ckpt.get("size", "?")
                        sha256 = ckpt.get("sha256", "")[:16] if ckpt.get("sha256") else "-"
                        self.root.after(0, lambda f=fname, s=size, sh=sha256: self.ckpt_tree.insert("", tk.END, values=(f, s, sh)))
                    elif isinstance(ckpt, str):
                        self.root.after(0, lambda f=ckpt: self.ckpt_tree.insert("", tk.END, values=(f, "-", "-")))

                self.root.after(0, lambda: self.ckpt_status.configure(text=f"共 {len(self.server_checkpoints)} 个模型"))
                self.root.after(0, lambda: self._log(f"获取到 {len(self.server_checkpoints)} 个 Checkpoint 模型"))
            except Exception as e:
                self.root.after(0, lambda err=e: self.ckpt_status.configure(text=f"错误: {err}", foreground="red"))

        threading.Thread(target=do_fetch, daemon=True).start()

    def _fetch_vae(self):
        """获取 VAE 模型列表"""
        def do_fetch():
            try:
                server_url = self.config.get("server", {}).get("url", SERVER_URL)
                r = requests.get(f"{server_url}/api/models/vae", timeout=10)
                data = r.json()
                self.server_vae = data if isinstance(data, list) else list(data.values()) if isinstance(data, dict) else []

                self.root.after(0, lambda: self.vae_tree.delete(*self.vae_tree.get_children()))
                for v in self.server_vae:
                    fname = v.get("filename", v.get("name", v)) if isinstance(v, dict) else str(v)
                    self.root.after(0, lambda f=fname: self.vae_tree.insert("", tk.END, values=(f,)))
                self.root.after(0, lambda: self.vae_status.configure(text=f"共 {len(self.server_vae)} 个 VAE"))
            except Exception as e:
                self.root.after(0, lambda err=e: self.vae_status.configure(text=f"错误: {err}", foreground="red"))

        threading.Thread(target=do_fetch, daemon=True).start()

    def _fetch_loras(self):
        """获取 LoRA 模型列表"""
        def do_fetch():
            try:
                server_url = self.config.get("server", {}).get("url", SERVER_URL)
                r = requests.get(f"{server_url}/api/models/loras", timeout=10)
                data = r.json()
                self.server_loras = data if isinstance(data, list) else list(data.values()) if isinstance(data, dict) else []

                self.root.after(0, lambda: self.lora_tree.delete(*self.lora_tree.get_children()))
                for l in self.server_loras:
                    fname = l.get("filename", l.get("name", l)) if isinstance(l, dict) else str(l)
                    self.root.after(0, lambda f=fname: self.lora_tree.insert("", tk.END, values=(f,)))
                self.root.after(0, lambda: self.lora_status.configure(text=f"共 {len(self.server_loras)} 个 LoRA"))
            except Exception as e:
                self.root.after(0, lambda err=e: self.lora_status.configure(text=f"错误: {err}", foreground="red"))

        threading.Thread(target=do_fetch, daemon=True).start()

    def _fetch_object_info(self):
        """获取服务器节点信息"""
        def do_fetch():
            try:
                server_url = self.config.get("server", {}).get("url", SERVER_URL)
                r = requests.get(f"{server_url}/api/object_info", timeout=20)
                self.server_object_info = r.json()
                self.workflow_editor.object_info = self.server_object_info
                self.root.after(0, lambda: self._populate_node_category_tree())
                self.root.after(0, lambda: self._populate_wf_node_lib())
                self.root.after(0, lambda: self.node_status.configure(text=f"共 {len(self.server_object_info)} 个节点", foreground="green"))
                self.root.after(0, lambda: self._log(f"获取到 {len(self.server_object_info)} 个服务器节点"))
            except Exception as e:
                self.root.after(0, lambda err=e: self.node_status.configure(text=f"错误: {err}", foreground="red"))

        threading.Thread(target=do_fetch, daemon=True).start()

    def _populate_node_category_tree(self):
        """填充节点分类树"""
        tree = self.node_category_tree
        tree.delete(*tree.get_children())

        cats = self._get_node_categories()
        for cat_name, nodes in sorted(cats.items()):
            cat_id = tree.insert("", tk.END, text=f"{cat_name}", values=(len(nodes),))
            for node in sorted(nodes, key=lambda x: x["display_name"]):
                tree.insert(cat_id, tk.END, text=node["display_name"], values=("",), tags=(node["id"],))

        tree.tag_configure("selected", background="#e0e0ff")

    def _populate_wf_node_lib(self):
        """填充工作流编辑器的节点库"""
        tree = self.wf_node_lib_tree
        tree.delete(*tree.get_children())

        cats = self._get_node_categories()
        for cat_name, nodes in sorted(cats.items()):
            for node in sorted(nodes, key=lambda x: x["display_name"]):
                display = f"[{cat_name}] {node['display_name']}"
                tree.insert("", tk.END, values=(display,), tags=(node["id"], node["class_type"] if "class_type" in node else node["id"]))

    def _get_node_categories(self):
        """将节点按 category 分组"""
        cats = {}
        for node_id, node_data in self.server_object_info.items():
            cat = node_data.get("category") or "_uncategorized"
            if cat not in cats:
                cats[cat] = []
            cats[cat].append({
                "id": node_id,
                "display_name": node_data.get("display_name") or node_id,
                "description": node_data.get("description") or "",
                "inputs": node_data.get("input", {}),
                "outputs": node_data.get("output") or [],
                "class_type": node_id,
            })
        return cats

    def _search_nodes(self):
        """搜索节点"""
        keyword = self.node_search_var.get().lower().strip()
        if not keyword:
            self._populate_node_category_tree()
            return

        tree = self.node_category_tree
        tree.delete(*tree.get_children())

        cats = self._get_node_categories()
        matched = []
        for cat_name, nodes in cats.items():
            for node in nodes:
                if keyword in node["id"].lower() or keyword in node["display_name"].lower() or keyword in cat_name.lower():
                    matched.append((cat_name, node))

        for cat_name, node in matched:
            tree.insert("", tk.END, text=f"[{cat_name}] {node['display_name']}", values=("",), tags=(node["id"],))

    def _on_node_selected(self, event):
        """节点被选中时显示详情"""
        selection = self.node_category_tree.selection()
        if not selection:
            return

        item = selection[0]
        tags = self.node_category_tree.item(item, "tags")
        if not tags:
            return

        node_id = tags[0]
        if node_id not in self.server_object_info:
            return

        node_data = self.server_object_info[node_id]
        self._show_node_detail(node_id, node_data)

    def _show_node_detail(self, node_id, node_data):
        """显示节点详细信息"""
        detail = self.node_detail_text
        detail.configure(state=tk.NORMAL)
        detail.delete("1.0", tk.END)

        lines = [
            f"节点 ID: {node_id}",
            f"显示名称: {node_data.get('display_name', node_id)}",
            f"分类: {node_data.get('category', 'N/A')}",
            f"描述: {node_data.get('description', 'N/A')}",
            "",
            "=== 输入参数 ===",
        ]

        inputs = node_data.get("input", {})
        required = inputs.get("required", {})
        optional = inputs.get("optional", {})

        if required:
            lines.append("[必填参数]")
            for pname, pinfo in required.items():
                if isinstance(pinfo, list):
                    ptype = pinfo[0] if pinfo else "unknown"
                    extra = f" (默认: {pinfo[1]})" if len(pinfo) > 1 else ""
                else:
                    ptype = str(pinfo)
                    extra = ""
                lines.append(f"  • {pname}: {ptype}{extra}")

        if optional:
            lines.append("\n[可选参数]")
            for pname, pinfo in optional.items():
                if isinstance(pinfo, list):
                    ptype = pinfo[0] if pinfo else "unknown"
                else:
                    ptype = str(pinfo)
                lines.append(f"  • {pname}: {ptype} (可选)")

        lines.extend([
            "",
            "=== 输出 ===",
        ])
        outputs = node_data.get("output", [])
        if outputs:
            for i, out in enumerate(outputs):
                lines.append(f"  [{i}] {out}")
        else:
            lines.append("  (无)")

        detail.insert("1.0", "\n".join(lines))
        detail.configure(state=tk.DISABLED)

    def _on_checkpoint_selected(self, event):
        """Checkpoint 双击事件"""
        selection = self.ckpt_tree.selection()
        if selection:
            item = self.ckpt_tree.item(selection[0])
            filename = item["values"][0]
            self._log(f"选中的 Checkpoint: {filename}")

    def _add_selected_node_to_workflow(self):
        """将选中的节点添加到工作流"""
        selection = self.node_category_tree.selection()
        if not selection:
            return

        item = selection[0]
        tags = self.node_category_tree.item(item, "tags")
        if not tags:
            return

        node_id = tags[0]
        if node_id in self.server_object_info:
            new_id = self.workflow_editor.add_node(node_id)
            self._refresh_workflow_editor_ui()
            self._log(f"已添加节点: {new_id} ({node_id})")
            # 切换到工作流编辑器 Tab
            self.notebook.select(3)

    def _fetch_and_show_custom_nodes(self):
        """获取并显示自定义节点"""
        def do_fetch():
            try:
                if not self.server_object_info:
                    self._fetch_object_info()
                    time.sleep(2)

                custom_keywords = ["api", "advanced", "external", "custom"]
                custom_nodes = []
                for node_id, node_data in self.server_object_info.items():
                    cat = (node_data.get("category") or "").lower()
                    if any(kw in cat for kw in custom_keywords):
                        custom_nodes.append({
                            "category": node_data.get("category") or "",
                            "display_name": node_data.get("display_name") or node_id,
                            "id": node_id,
                        })

                self.root.after(0, lambda: self.custom_node_tree.delete(*self.custom_node_tree.get_children()))
                for cn in sorted(custom_nodes, key=lambda x: (x["category"], x["display_name"])):
                    self.root.after(0, lambda c=cn: self.custom_node_tree.insert("", tk.END, values=(c["category"], c["display_name"])))

                self.root.after(0, lambda: self._log(f"找到 {len(custom_nodes)} 个自定义/外部节点"))
            except Exception as e:
                self.root.after(0, lambda err=e: self._log(f"获取自定义节点失败: {err}", "ERROR"))

        threading.Thread(target=do_fetch, daemon=True).start()

    def _refresh_workflow_list(self):
        """刷新工作流列表"""
        try:
            workflows = list_workflows()
            self.workflow_list_tree.delete(*self.workflow_list_tree.get_children())
            for wf_name in workflows:
                wf_path = os.path.join(WORKFLOW_DIR, f"{wf_name}.json")
                mtime = datetime.datetime.fromtimestamp(os.path.getmtime(wf_path)).strftime("%Y-%m-%d %H:%M:%S")
                self.workflow_list_tree.insert("", tk.END, values=(wf_name, mtime))
            self.wf_list_status.configure(text=f"工作流存储路径: {WORKFLOW_DIR} (共 {len(workflows)} 个)")
        except Exception as e:
            self.wf_list_status.configure(text=f"错误: {e}", foreground="red")

    def _load_selected_workflow(self):
        """加载选中的工作流到编辑器"""
        selection = self.workflow_list_tree.selection()
        if not selection:
            messagebox.showwarning("提示", "请先选择一个工作流")
            return
        wf_name = self.workflow_list_tree.item(selection[0])["values"][0]
        try:
            wf_data = load_workflow_file(wf_name)
            self.workflow_editor.from_workflow_json(wf_data)
            self._refresh_workflow_editor_ui()
            self.notebook.select(3)
            self._log(f"已加载工作流: {wf_name}")
        except Exception as e:
            messagebox.showerror("错误", f"加载工作流失败: {e}")

    def _delete_selected_workflow(self):
        """删除选中的工作流"""
        selection = self.workflow_list_tree.selection()
        if not selection:
            messagebox.showwarning("提示", "请先选择一个工作流")
            return
        wf_name = self.workflow_list_tree.item(selection[0])["values"][0]
        if messagebox.askyesno("确认", f"确定要删除工作流 '{wf_name}' 吗？"):
            if delete_workflow_file(wf_name):
                self._refresh_workflow_list()
                self._log(f"已删除工作流: {wf_name}")

    def _get_checkpoints_cached(self):
        """获取缓存的 checkpoint 列表（用于 LLM prompt）"""
        if self.server_checkpoints:
            return [c.get("filename", c.get("name", c)) if isinstance(c, dict) else str(c) for c in self.server_checkpoints]
        return [DEFAULT_CHECKPOINT]

    def _summarize_nodes_for_llm(self):
        """为 LLM 生成节点摘要"""
        cats = self._get_node_categories()
        summary_parts = []
        for cat_name, nodes in sorted(cats.items()):
            node_names = [str(n["display_name"] or n["id"]) for n in nodes[:10]]  # 每个分类最多 10 个
            summary_parts.append(f"- {cat_name} ({len(nodes)}): {', '.join(node_names)}")
        return "\n".join(summary_parts[:30])  # 最多 30 个分类

    # ==========================================
    # 模块3: 工作流编辑器功能
    # ==========================================
    def _workflow_new(self):
        """新建工作流"""
        self.workflow_editor.clear()
        self._refresh_workflow_editor_ui()
        self.wf_status_label.configure(text="已新建空白工作流")
        self._log("新建工作流")

    def _workflow_load_json(self):
        """从 JSON 文件加载工作流"""
        path = filedialog.askopenfilename(
            title="加载工作流 JSON",
            filetypes=[("JSON 文件", "*.json"), ("所有文件", "*.*")],
            initialdir=WORKFLOW_DIR
        )
        if path:
            try:
                with open(path, "r", encoding="utf-8") as f:
                    wf_data = json.load(f)
                self.workflow_editor.from_workflow_json(wf_data)
                self._refresh_workflow_editor_ui()
                self.wf_status_label.configure(text=f"已加载: {os.path.basename(path)}")
                self._log(f"加载工作流: {path}")
            except Exception as e:
                messagebox.showerror("错误", f"加载失败: {e}")

    def _workflow_save(self):
        """保存工作流"""
        name = simpledialog.askstring("保存工作流", "请输入工作流名称:")
        if name:
            try:
                wf_json = self.workflow_editor.to_workflow_json()
                save_workflow_file(name, wf_json)
                self.wf_status_label.configure(text=f"已保存: {name}.json")
                self._log(f"保存工作流: {name}")
                self._refresh_workflow_list()
            except Exception as e:
                messagebox.showerror("错误", f"保存失败: {e}")

    def _workflow_save_as(self):
        """另存为"""
        self._workflow_save()

    def _workflow_validate(self):
        """验证工作流"""
        errors, warnings = self.workflow_editor.validate()
        msg = []
        if errors:
            msg.append("❌ 错误:")
            msg.extend(f"  • {e}" for e in errors)
        if warnings:
            msg.append("\n⚠️ 警告:")
            msg.extend(f"  • {w}" for w in warnings)
        if not errors and not warnings:
            msg.append("✅ 工作流验证通过！结构完整。")

        self.wf_status_label.configure(text="✓ 验证完成" if not errors else "✗ 发现错误")
        messagebox.showinfo("验证结果", "\n".join(msg))

    def _workflow_apply_to_generate(self):
        """将工作流应用到生成流程（高级功能 - 直接使用自定义 workflow）"""
        wf = self.workflow_editor.to_workflow_json()
        if not wf:
            messagebox.showwarning("提示", "工作流为空")
            return
        if not messagebox.askyesno("确认", "确定要使用当前工作流进行生成吗？\n这将跳过内置的 txt2img/img2img 流程。"):
            return
        self._log("准备使用自定义工作流生成...")
        # TODO: 可以扩展为直接提交自定义 workflow 到服务器
        messagebox.showinfo("提示", "自定义工作流应用功能将在生成时使用该 workflow。\n实际生成逻辑可根据需要进一步扩展。")

    def _refresh_workflow_editor_ui(self):
        """刷新工作流编辑器 UI"""
        # 刷新节点列表
        self.wf_nodes_tree.delete(*self.wf_nodes_tree.get_children())
        for nid, ndata in self.workflow_editor.nodes.items():
            self.wf_nodes_tree.insert("", tk.END, values=(nid, ndata["class_type"]))

        # 刷新连接列表
        self.wf_conn_tree.delete(*self.wf_conn_tree.get_children())
        for i, conn in enumerate(self.workflow_editor.connections):
            display = f"{conn[0]}:{conn[1]} → {conn[2]}:{conn[3]}"
            self.wf_conn_tree.insert("", tk.END, values=(display,), tags=(str(i),))

        # 刷新连接 Combobox 选项
        node_ids = list(self.workflow_editor.nodes.keys())
        for combo in [self.conn_src_node_combo, self.conn_dst_node_combo]:
            combo["values"] = node_ids

        # 刷新 JSON 预览
        self._wf_refresh_json_preview()

        # 清空参数编辑区
        self._clear_param_editors()

    def _clear_param_editors(self):
        """清除参数编辑控件"""
        for widget in self.wf_param_frame_inner.winfo_children():
            widget.destroy()
        self.wf_param_widgets = {}

    def _on_wf_node_selected(self, event):
        """工作流节点被选中"""
        selection = self.wf_nodes_tree.selection()
        if not selection:
            return

        item = selection[0]
        node_id = self.wf_nodes_tree.item(item)["values"][0]

        if node_id in self.workflow_editor.nodes:
            self._show_node_params(node_id)

    def _show_node_params(self, node_id):
        """显示节点的参数编辑界面"""
        self._clear_param_editors()

        ndata = self.workflow_editor.nodes[node_id]
        class_type = ndata["class_type"]

        # 从服务器获取节点定义
        node_def = self.server_object_info.get(class_type, {})
        inputs_def = node_def.get("input", {})
        required = inputs_def.get("required", {})
        optional = inputs_def.get("optional", {})

        row = 0
        ttk.Label(self.wf_param_frame_inner, text=f"节点 {node_id} ({class_type})", font=("", 9, "bold")).grid(row=row, column=0, columnspan=2, sticky=tk.W, pady=(0, 5))
        row += 1

        all_params = {**required, **optional}
        for pname, pinfo in all_params.items():
            current_val = ndata["inputs"].get(pname, "")

            ttk.Label(self.wf_param_frame_inner, text=pname + ":").grid(row=row, column=0, sticky=tk.W, pady=1)

            # 判断是否是连接引用（如果是则不可编辑）
            is_connected = any(
                c[2] == node_id and c[3] == pname
                for c in self.workflow_editor.connections
            )

            if is_connected:
                lbl = ttk.Label(self.wf_param_frame_internal, text="[已连接]", foreground="blue")
                lbl.grid(row=row, column=1, sticky=tk.W, pady=1)
            elif isinstance(pinfo, list) and pinfo and isinstance(pinfo[0], str):
                ptype = pinfo[0].upper()
                if "COMBO" in ptype and len(pinfo) > 1:
                    # 下拉选择
                    var = tk.StringVar(value=str(current_val) if current_val != "" else "")
                    combo = ttk.Combobox(self.wf_param_frame_inner, textvariable=var, values=pinfo[1] if len(pinfo) > 1 else [], width=25)
                    combo.grid(row=row, column=1, sticky=tk.W, pady=1)
                    self.wf_param_widgets[pname] = ("combo", var)
                elif "INT" in ptype:
                    default = pinfo[1] if len(pinfo) > 1 else "0"
                    var = tk.IntVar(value=int(current_val) if current_val != "" else int(default))
                    entry = ttk.Entry(self.wf_param_frame_inner, textvariable=var, width=27)
                    entry.grid(row=row, column=1, sticky=tk.W, pady=1)
                    self.wf_param_widgets[pname] = ("int", var)
                elif "FLOAT" in ptype:
                    default = pinfo[1] if len(pinfo) > 1 else "0.0"
                    var = tk.DoubleVar(value=float(current_val) if current_val != "" else float(default))
                    entry = ttk.Entry(self.wf_param_frame_inner, textvariable=var, width=27)
                    entry.grid(row=row, column=1, sticky=tk.W, pady=1)
                    self.wf_param_widgets[pname] = ("float", var)
                else:
                    var = tk.StringVar(value=str(current_val))
                    entry = ttk.Entry(self.wf_param_frame_inner, textvariable=var, width=27)
                    entry.grid(row=row, column=1, sticky=tk.W, pady=1)
                    self.wf_param_widgets[pname] = ("str", var)
            else:
                var = tk.StringVar(value=str(current_val))
                entry = ttk.Entry(self.wf_param_frame_inner, textvariable=var, width=27)
                entry.grid(row=row, column=1, sticky=tk.W, pady=1)
                self.wf_param_widgets[pname] = ("str", var)

            row += 1

    def _wf_apply_params(self):
        """应用参数修改到工作流编辑器"""
        selection = self.wf_nodes_tree.selection()
        if not selection:
            messagebox.showwarning("提示", "请先选择一个节点")
            return

        node_id = self.wf_nodes_tree.item(selection[0])["values"][0]
        if node_id not in self.workflow_editor.nodes:
            return

        for pname, (ptype, var) in self.wf_param_widgets.items():
            try:
                if ptype == "int":
                    self.workflow_editor.nodes[node_id]["inputs"][pname] = var.get()
                elif ptype == "float":
                    self.workflow_editor.nodes[node_id]["inputs"][pname] = var.get()
                else:
                    val = var.get()
                    if val:
                        self.workflow_editor.nodes[node_id]["inputs"][pname] = val
                    elif pname in self.workflow_editor.nodes[node_id]["inputs"]:
                        del self.workflow_editor.nodes[node_id]["inputs"][pname]
            except Exception as e:
                self._log(f"参数 {pname} 应用失败: {e}", "ERROR")

        self._wf_refresh_json_preview()
        self.wf_status_label.configure(text=f"✓ 节点 {node_id} 参数已更新")

    def _wf_add_connection(self):
        """添加连接"""
        src = self.conn_src_node_var.get()
        src_port = self.conn_src_port_var.get()
        dst = self.conn_dst_node_var.get()
        dst_port = self.conn_dst_port_var.get()

        if not all([src, src_port, dst, dst_port]):
            messagebox.showwarning("提示", "请填写完整的连接信息")
            return

        self.workflow_editor.add_connection(src, src_port, dst, dst_port)
        self._refresh_workflow_editor_ui()
        self._log(f"添加连接: {src}:{src_port} → {dst}:{dst_port}")

    def _wf_remove_selected_node(self):
        """删除选中的节点"""
        selection = self.wf_nodes_tree.selection()
        if not selection:
            return
        node_id = self.wf_nodes_tree.item(selection[0])["values"][0]
        self.workflow_editor.remove_node(node_id)
        self._refresh_workflow_editor_ui()
        self._log(f"删除节点: {node_id}")

    def _wf_remove_selected_connection(self):
        """删除选中的连接"""
        selection = self.wf_conn_tree.selection()
        if not selection:
            return
        idx = int(self.wf_conn_tree.item(selection[0]).get("tags")[0])
        self.workflow_editor.remove_connection(idx)
        self._refresh_workflow_editor_ui()
        self._log("删除连接")

    def _on_wf_conn_selected(self, event):
        """连接被选中"""
        pass

    def _on_wf_lib_node_double_click(self, event):
        """节点库双击添加到工作流"""
        selection = self.wf_node_lib_tree.selection()
        if not selection:
            return

        item = selection[0]
        tags = self.wf_node_lib_tree.item(item, "tags")
        if len(tags) >= 2:
            class_type = tags[1]
            new_id = self.workflow_editor.add_node(class_type)
            self._refresh_workflow_editor_ui()
            self._log(f"从库添加节点: {new_id} ({class_type})")

    def _wf_refresh_json_preview(self):
        """刷新 JSON 预览"""
        self.wf_json_text.configure(state=tk.NORMAL)
        self.wf_json_text.delete("1.0", tk.END)
        wf_json = self.workflow_editor.to_workflow_json()
        self.wf_json_text.insert("1.0", json.dumps(wf_json, indent=2, ensure_ascii=False))
        self.wf_json_text.configure(state=tk.DISABLED)

    def _wf_copy_json(self):
        """复制工作流 JSON"""
        wf_json = self.workflow_editor.to_workflow_json()
        self.root.clipboard_clear()
        self.root.clipboard_append(json.dumps(wf_json, indent=2, ensure_ascii=False))
        self._log("已复制工作流 JSON")

    # ==========================================
    # 模块4: AI 工作流生成
    # ==========================================
    def _ai_generate_workflow_editor(self):
        """在工作流编辑器中使用 AI 生成工作流"""
        self._update_llm_client_from_ui()
        if not self.llm_client.model_name:
            messagebox.showwarning("提示", "请先在「AI 助手」Tab 配置 LLM 模型")
            self.notebook.select(1)  # 切换到 AI 助手 Tab
            return

        requirement = simpledialog.askstring("AI 生成工作流", "请描述您想要生成的内容:\n例如: 生成时空守护者的像素画精灵帧")
        if not requirement:
            return

        self.wf_status_label.configure(text="🤖 AI 正在生成工作流...")
        threading.Thread(target=self._do_ai_generate_workflow, args=(requirement,), daemon=True).start()

    def _do_ai_generate_workflow(self, requirement):
        """后台执行 AI 工作流生成"""
        try:
            checkpoints = self._get_checkpoints_cached()
            object_info_summary = self._summarize_nodes_for_llm()

            prompt = LLM_WORKFLOW_PROMPT.format(
                server_resources_summary=f"Checkpoints: {checkpoints[:5]}\n\nNode categories summary:\n{object_info_summary}",
                user_requirement=requirement
            )

            response = self.llm_client.chat(prompt, "")
            workflow = self._extract_json_from_response(response)

            self.root.after(0, lambda: self._on_ai_workflow_generated(workflow, requirement))
        except Exception as e:
            self.root.after(0, lambda err=e: self._on_ai_workflow_error(err))
        finally:
            self.root.after(0, lambda: self.wf_status_label.configure(text="就绪"))

    def _on_ai_workflow_generated(self, workflow, requirement):
        """AI 工作流生成完成"""
        self.workflow_editor.from_workflow_json(workflow)
        self._refresh_workflow_editor_ui()
        self.wf_status_label.configure(text=f"✓ AI 已生成工作流: {requirement}")
        self._log(f"AI 生成工作流成功: {requirement}")

    def _on_ai_workflow_error(self, error):
        """AI 工作流生成出错"""
        self.wf_status_label.configure(text=f"✗ AI 生成失败: {error}")
        messagebox.showerror("错误", f"AI 工作流生成失败: {error}")

    def _extract_json_from_response(self, response):
        """从 LLM 响应中提取 JSON（处理 markdown code block 包装）"""
        # 尝试直接解析
        try:
            return json.loads(response.strip())
        except json.JSONDecodeError:
            pass

        # 尝试提取 markdown code block 中的 JSON
        match = re.search(r'```(?:json)?\s*\n?(.*?)\n?\s*```', response, re.DOTALL)
        if match:
            try:
                return json.loads(match.group(1).strip())
            except json.JSONDecodeError:
                pass

        # 尝试查找 JSON 对象
        match = re.search(r'\{[\s\S]*\}', response)
        if match:
            try:
                return json.loads(match.group(0))
            except json.JSONDecodeError:
                pass

        raise ValueError("无法从响应中提取有效的 JSON")

    # ==========================================
    # 模块5+6+7: 项目分析功能（新增）
    # ==========================================
    def _do_scan_project(self):
        """执行项目扫描（后台线程）"""
        self.scan_status_label.configure(text="⏳ 扫描中...", foreground="blue")
        threading.Thread(target=self._do_scan_project_thread, daemon=True).start()

    def _do_scan_project_thread(self):
        """后台执行扫描"""
        try:
            success, message = self.project_scanner.scan_project()
            self.root.after(0, lambda: self._on_scan_complete(success, message))
        except Exception as e:
            self.root.after(0, lambda err=e: self._on_scan_complete(False, f"扫描异常: {err}"))

    def _on_scan_complete(self, success, message):
        """扫描完成回调"""
        if success:
            self.scan_status_label.configure(text="✓ 已完成", foreground="green")
            self.project_root_label.configure(text=self.project_scanner.project_root)
            self._log(f"项目扫描完成: {message}")
            # 刷新文件树
            self._refresh_project_file_tree()
        else:
            self.scan_status_label.configure(text="✗ 失败", foreground="red")
            self._log(f"项目扫描失败: {message}", "ERROR")
            messagebox.showerror("扫描失败", message)

    def _refresh_project_file_tree(self):
        """刷新项目文件树"""
        tree = self.project_file_tree
        tree.delete(*tree.get_children())

        if not self.project_scanner.scan_complete:
            return

        # 添加根节点
        root_id = tree.insert("", tk.END, text="📁 项目根目录", open=True)

        # 策划文档节点
        docs = self.project_scanner.list_design_docs()
        if docs:
            doc_node = tree.insert(root_id, tk.END, text=f"📝 策划文档 ({len(docs)})", open=True)
            for doc in sorted(docs):
                tree.insert(doc_node, tk.END, text=doc, values=(doc, "doc"))

        # 脚本文件节点
        scripts = self.project_scanner.script_files
        if scripts:
            script_node = tree.insert(root_id, tk.END, text=f"💻 脚本 ({len(scripts)})")
            # 只显示一级目录分类
            script_categories = {}
            for sf in scripts:
                cat = sf["rel_path"].split(os.sep)[0] if os.sep in sf["rel_path"] else sf["rel_path"]
                if cat not in script_categories:
                    script_categories[cat] = []
                script_categories[cat].append(sf)

            for cat, files in sorted(script_categories.items()):
                cat_node = tree.insert(script_node, tk.END, text=f"📂 {cat} ({len(files)})")
                for f in sorted(files[:20]):  # 限制显示数量
                    tree.insert(cat_node, tk.END, text=f["rel_path"], values=(f["path"], "script"))
                if len(files) > 20:
                    tree.insert(cat_node, tk.END, text=f"... 还有 {len(files)-20} 个文件", values=("", ""))

        # 美术素材节点
        assets = self.project_scanner.get_asset_inventory()
        if assets:
            asset_node = tree.insert(root_id, tk.END, text=f"🎨 美术素材")
            total = sum(len(files) for files in assets.values())
            for cat, files in sorted(assets.items()):
                tree.insert(asset_node, tk.END, text=f"📂 {cat}: {len(files)} 个文件")

    def _on_project_file_selected(self, event):
        """文件树选择事件 - 显示文件内容"""
        selection = self.project_file_tree.selection()
        if not selection:
            return

        item = selection[0]
        values = self.project_file_tree.item(item, "values")
        if not values or len(values) < 2:
            return

        file_path = values[0]
        file_type = values[1] if len(values) > 1 else ""

        if not file_path or not os.path.exists(file_path):
            return

        # 显示文件信息
        filename = os.path.basename(file_path)
        filesize = os.path.getsize(file_path)
        self.preview_file_label.configure(text=file_path)
        self.preview_size_label.configure(text=f"{filesize:,} 字节")

        # 读取并显示内容
        content = self.project_scanner._read_file_safe(file_path)
        if content:
            self.file_preview_text.configure(state=tk.NORMAL)
            self.file_preview_text.delete("1.0", tk.END)
            self.file_preview_text.insert("1.0", content)
            self.file_preview_text.configure(state=tk.DISABLED)
        else:
            self.file_preview_text.configure(state=tk.NORMAL)
            self.file_preview_text.delete("1.0", tk.END)
            self.file_preview_text.insert("1.0", "[无法读取文件或文件为空]")
            self.file_preview_text.configure(state=tk.DISABLED)

    def _analyze_project_full(self):
        """全量分析项目现状"""
        if not self.project_scanner.scan_complete:
            messagebox.showwarning("提示", "请先点击「扫描项目」按钮进行项目扫描")
            return

        summary = self.project_scanner.get_scan_summary()
        self._display_analysis_result(summary)
        self._log("已生成项目全量分析报告")

    def _analyze_asset_gaps(self):
        """资产缺口分析"""
        if not self.project_scanner.scan_complete:
            messagebox.showwarning("提示", "请先点击「扫描项目」按钮进行项目扫描")
            return

        gaps = self.project_scanner.analyze_asset_gaps()
        missing = [g for g in gaps if g["status"] == "缺失"]
        existing = [g for g in gaps if g["status"] == "已存在"]

        report_parts = [
            "🔍 资产缺口分析报告",
            "=" * 50,
            "",
            f"总资产定义数: {len(gaps)}",
            f"已存在: {len(existing)} | 缺失: {len(missing)}",
            "",
        ]

        if missing:
            report_parts.append("【缺失资产】（按优先级排序）:\n")
            for i, gap in enumerate(missing, 1):
                stars = "⭐" * gap["priority"]
                report_parts.append(
                    f"{i}. {stars} [{gap['category']}] {gap['name']}\n"
                    f"   目标尺寸: {gap['target_size']}\n"
                    f"   模板提示词: {gap['template_prompt'][:80]}...\n"
                )
        else:
            report_parts.append("✅ 所有资产均已存在，无缺口！")

        report_parts.append("\n" + "=" * 50)
        report_parts.append("\n【已存在的资产】:")
        for gap in existing:
            report_parts.append(f"  ✓ [{gap['category']}] {gap['name']}")

        self._display_analysis_result("\n".join(report_parts))
        self._log(f"资产缺口分析完成: {len(missing)} 个缺失")

    def _ai_extract_requirements_from_doc(self):
        """AI 从选中文档提取需求"""
        self._update_llm_client_from_ui()
        if not self.llm_client.model_name:
            messagebox.showwarning("提示", "请先在「AI 助手」Tab 配置 LLM 模型")
            self.notebook.select(1)
            return

        if not self.project_scanner.scan_complete:
            messagebox.showwarning("提示", "请先扫描项目")
            return

        # 获取选中的文档
        selection = self.project_file_tree.selection()
        if not selection:
            # 如果没有选中，使用全部策划文档
            docs = self.project_scanner.list_design_docs()
            if not docs:
                messagebox.showwarning("提示", "没有找到策划文档")
                return
            doc_name = docs[0]  # 默认使用第一个
        else:
            item = selection[0]
            values = self.project_file_tree.item(item, "values")
            if not values:
                messagebox.showwarning("提示", "请选择一个策划文档文件")
                return
            doc_name = values[0]

        # 验证是文档类型
        doc_info = self.project_scanner.get_design_doc(doc_name)
        if not doc_info:
            messagebox.showwarning("提示", f"未找到文档: {doc_name}")
            return

        self._set_project_ai_busy(True)
        threading.Thread(target=self._do_ai_extract_requirements,
                         args=(doc_name, doc_info), daemon=True).start()

    def _do_ai_extract_requirements(self, doc_name, doc_info):
        """后台执行 AI 需求提取"""
        try:
            # 构建用户消息
            user_message = f"""请分析以下策划文档，提取美术资产生成需求：

## 文档名称
{doc_name}

## 文档内容
{doc_info['content'][:10000]}  # 限制长度避免超长

## 现有资产清单
{json.dumps({k: len(v) for k, v in self.project_scanner.asset_inventory.items()}, ensure_ascii=False, indent=2)}

## 已定义的资产模板
{json.dumps({k: {'cat': v['cat'], 'size': f"{v['targetW']}x{v['targetH']}"} for k, v in ASSET_TEMPLATES.items()}, ensure_ascii=False, indent=2)}
"""

            # 附带知识库上下文
            knowledge_context = f"\n\n## 项目背景知识\n{self.project_scanner.knowledge_base[:3000]}"
            user_message += knowledge_context

            response = self.llm_client.chat(LLM_PROJECT_ANALYSIS_PROMPT, user_message, temperature=0.6, max_tokens=4096)

            self.root.after(0, lambda r=response: self._display_analysis_result(r))
            self.root.after(0, lambda: self._log(f"AI 需求提取完成 (文档: {doc_name})"))

            # 存储结果用于后续生成队列
            self.last_ai_analysis_result = response

        except Exception as e:
            self.root.after(0, lambda err=e: self._display_analysis_error(err))
        finally:
            self.root.after(0, lambda: self._set_project_ai_busy(False))

    def _ai_generate_queue_from_requirements(self):
        """一键生成队列 - 将需求转换为提示词"""
        self._update_llm_client_from_ui()
        if not self.llm_client.model_name:
            messagebox.showwarning("提示", "请先配置 LLM 模型")
            return

        if not hasattr(self, 'last_ai_analysis_result') or not self.last_ai_analysis_result:
            # 如果还没有 AI 分析结果，先生成分析
            if messagebox.askyesno("提示", "尚未执行 AI 需求提取。\n是否现在执行完整流程（分析 + 生成队列）？"):
                self._ai_extract_requirements_from_doc()
            return

        self._set_project_ai_busy(True)
        threading.Thread(target=self._do_ai_generate_queue, daemon=True).start()

    def _do_ai_generate_queue(self):
        """后台执行 AI 提示词生成"""
        try:
            requirements_data = self.last_ai_analysis_result

            prompt = LLM_REQUIREMENT_TO_PROMPT_PROMPT.format(requirements_data=requirements_data[:8000])
            response = self.llm_client.chat(prompt, "", temperature=0.7, max_tokens=4096)

            self.root.after(0, lambda r=response: self._display_analysis_result(r))
            self.root.after(0, lambda: self._log("AI 已生成完整的提示词队列"))

            # 解析并存储生成的提示词
            self.last_generated_prompts = self._parse_generated_prompts(response)

        except Exception as e:
            self.root.after(0, lambda err=e: self._display_analysis_error(err))
        finally:
            self.root.after(0, lambda: self._set_project_ai_busy(False))

    def _parse_generated_prompts(self, response):
        """解析 AI 生成的提示词"""
        prompts = []
        # 使用正则表达式提取每个资产的提示词组
        pattern = r'###\s*(.+?)\n.*?POSITIVE_CONCEPT:\s*(.+?)\n.*?NEGATIVE_CONCEPT:\s*(.+?)\n.*?POSITIVE_SPRITE:\s*(.+?)\n.*?NEGATIVE_SPRITE:\s*(.+?)\n.*?TARGET_SIZE:\s*(.+?)\n.*?CATEGORY:\s*(.+?)(?:\n###|\Z)'
        matches = re.findall(pattern, response, re.DOTALL | re.IGNORECASE)

        for match in matches:
            prompts.append({
                "name": match[0].strip(),
                "positive_concept": match[1].strip(),
                "negative_concept": match[2].strip(),
                "positive_sprite": match[3].strip(),
                "negative_sprite": match[4].strip(),
                "target_size": match[5].strip(),
                "category": match[6].strip()
            })

        return prompts

    def _set_project_ai_busy(self, busy):
        """设置项目分析 Tab 的 AI 忙碌状态"""
        if busy:
            self.project_ai_busy_label.configure(text="⏳ AI 思考中...")
        else:
            self.project_ai_busy_label.configure(text="")

    def _display_analysis_result(self, result):
        """显示分析结果到文本框"""
        self.analysis_result_text.configure(state=tk.NORMAL)
        self.analysis_result_text.delete("1.0", tk.END)
        self.analysis_result_text.insert("1.0", result)
        self.analysis_result_text.configure(state=tk.DISABLED)

    def _display_analysis_error(self, error):
        """显示分析错误"""
        self.analysis_result_text.configure(state=tk.NORMAL)
        self.analysis_result_text.insert(tk.END, f"\n❌ 错误: {error}\n")
        self.analysis_result_text.configure(state=tk.DISABLED)
        self._log(f"项目分析错误: {error}", "ERROR")

    def _copy_analysis_result(self):
        """复制分析报告到剪贴板"""
        result = self.analysis_result_text.get("1.0", tk.END).strip()
        self.root.clipboard_clear()
        self.root.clipboard_append(result)
        self._log("已复制分析报告到剪贴板")

    def _clear_analysis_result(self):
        """清空分析结果"""
        self.analysis_result_text.configure(state=tk.NORMAL)
        self.analysis_result_text.delete("1.0", tk.END)
        self.analysis_result_text.configure(state=tk.DISABLED)

    def _apply_analysis_to_generate(self):
        """将分析结果应用到资产生成 Tab"""
        if hasattr(self, 'last_generated_prompts') and self.last_generated_prompts:
            # 如果有生成的提示词，让用户选择一个应用
            if self.last_generated_prompts:
                prompt_names = [p["name"] for p in self.last_generated_prompts]
                choice = messagebox.askyesno("应用到资产生成",
                    f"发现 {len(prompt_names)} 个生成的提示词。\n"
                    f"是否将第一个「{prompt_names[0]}」的提示词应用到资产生成 Tab？\n"
                    f"(其他提示词可手动查看并复制)")
                if choice and self.last_generated_prompts:
                    p = self.last_generated_prompts[0]
                    self.positive_text.configure(state=tk.NORMAL)
                    self.positive_text.delete("1.0", tk.END)
                    self.positive_text.insert("1.0", p["positive_concept"])
                    self.positive_text.configure(state=tk.DISABLED)

                    self.negative_text.configure(state=tk.NORMAL)
                    self.negative_text.delete("1.0", tk.END)
                    self.negative_text.insert("1.0", p["negative_concept"])
                    self.negative_text.configure(state=tk.DISABLED)

                    self.notebook.select(0)  # 切换到资产生成 Tab
                    self._log(f"已应用 {p['name']} 的概念图提示词到资产生成 Tab")
                    messagebox.showinfo("成功", f"已将 {p['name']} 的提示词应用到资产生成 Tab")
        else:
            messagebox.showinfo("提示", "暂无可应用的提示词。\n请先执行「AI提取需求」和「一键生成队列」操作。")


# ==========================================
# 入口
# ==========================================
def main():
    root = tk.Tk()

    # 导入 simpledialog（用于对话框）
    global simpledialog
    from tkinter import simpledialog

    # 样式
    style = ttk.Style()
    try:
        style.theme_use("clam")
    except Exception:
        pass

    app = ComfyUIGenerator(root)

    # 优雅关闭：在窗口关闭时停止 ServerMonitor
    def on_closing():
        if hasattr(app, 'server_monitor') and app.server_monitor:
            app.server_monitor.stop()
        root.destroy()

    root.protocol("WM_DELETE_WINDOW", on_closing)
    root.mainloop()


if __name__ == "__main__":
    main()
