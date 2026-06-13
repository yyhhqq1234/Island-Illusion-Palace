#!/usr/bin/env python3
"""
ComfyUI 美术素材生产客户端 v1.0
独立于 Unity 的 ComfyUI 资产生成工具

功能：
- 服务器连接状态监控（WebSocket 8189）
- 资产生成（概念图 + 精灵帧）
- 提示词模板管理
- 批量生成队列
- 历史记录

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
from urllib.parse import urlencode
from collections import OrderedDict

try:
    import requests
except ImportError:
    print("请安装依赖: pip install requests websocket-client")
    sys.exit(1)

# ==========================================
# 配置
# ==========================================
SERVER_URL = "http://10.150.164.64:8188"
WS_URL = "ws://10.150.164.64:8189"
OUTPUT_BASE = r"d:\Program Files\Unity\U3Dproject\Island-Illusion-Palace\Assets\ArtMaterials"

# ==========================================
# 提示词模板 (从 ComfyUIPromptTemplates.cs 提取)
# ==========================================

CONCEPT_BASE_POSITIVE = (
    "(dark fantasy concept art:1.3), (digital painting:1.2), dramatic lighting, "
    "(soul glow: blue-purple luminescence #4A3A8C), (crystal erosion: geometric cyan crystals #00D4FF), "
    "rim light, high contrast, detailed texture, atmospheric perspective"
)

CONCEPT_NEGATIVE = (
    "photorealistic, 3D render, CGI, smooth vector art, flat design, "
    "bright sunny, cartoon, anime, chibi, pixel art, low resolution, "
    "blurry, watermark, text, signature, ugly, distorted, bad anatomy, "
    "extra limbs, fused fingers, low quality, jpeg artifacts"
)

SPRITE_BASE_POSITIVE = (
    "(pixel art:1.4), (2D top-down game sprite:1.3), dark fantasy, "
    "(soul glow: blue-purple luminescence #4A3A8C), (crystal erosion: geometric cyan crystals #00D4FF), "
    "cel-shading, 1px outline, transparent background, "
    "dark atmospheric, high contrast, limited color palette"
)

SPRITE_NEGATIVE = (
    "photorealistic, 3D render, CGI, smooth vector art, flat design, "
    "bright sunny, cartoon, anime, chibi, painterly texture, visible brushstrokes, "
    "blurry, watermark, text, signature, ugly, distorted, bad anatomy, "
    "extra limbs, fused fingers, low quality, jpeg artifacts, anti-aliasing"
)

STATE_PROMPTS = {
    "idle": "standing idle pose, neutral stance, breathing slightly",
    "walk": "walking forward, stepping motion, dynamic movement",
    "attack": "mid-swing attack pose, arm raised, energy flowing, action pose",
    "skill": "casting spell, energy swirling, dramatic power pose",
    "death": "falling down, collapsing, dissolving into particles",
}

# 资产模板 (assetType -> {prompt, targetWidth, targetHeight, category, genWidth, genHeight})
ASSET_TEMPLATES = OrderedDict()

# ========== 玩家 (64x64 target) ==========
ASSET_TEMPLATES["墨语"] =      {"prompt": "(Mo Yu:1.3), white-haired male protagonist, blue-purple soul aura, light armor, determined expression, 2.5-heads-tall", "targetW": 64, "targetH": 64, "cat": "Player", "genW": 512, "genH": 512}
ASSET_TEMPLATES["莎娜"] =      {"prompt": "(Shana:1.3), crimson-haired female, scarlet red energy aura, elegant mage robe, kind smile, 2.5-heads-tall", "targetW": 64, "targetH": 64, "cat": "Player", "genW": 512, "genH": 512}

# ========== 普通敌人 (32x32 target) ==========
ASSET_TEMPLATES["腐化村民"] =  {"prompt": "(corrupted villager:1.3), twisted humanoid, tattered cloth, (blue-purple corrupted energy:1.1), dark veins, hollow eyes, hunched posture, 2-heads-tall", "targetW": 32, "targetH": 32, "cat": "Enemy", "genW": 512, "genH": 512}
ASSET_TEMPLATES["暗影之刃"] =  {"prompt": "(shadow blade assassin:1.3), dark hooded figure, dual daggers, (cyan energy trail:1.1), shadowy cloak, agile stance, 2-heads-tall", "targetW": 32, "targetH": 32, "cat": "Enemy", "genW": 512, "genH": 512}
ASSET_TEMPLATES["水晶寄生体"] = {"prompt": "(crystal parasite monster:1.3), insectoid creature, (cyan crystal growth on body:1.2), scuttling legs, unnatural movement, 2-heads-tall", "targetW": 32, "targetH": 32, "cat": "Enemy", "genW": 512, "genH": 512}
ASSET_TEMPLATES["沼泽潜伏者"] = {"prompt": "(swamp lurker monster:1.3), amphibious creature, half-submerged in mud, slimy skin, (yellow-green toxic mist:1.1), webbed hands, bulging eyes, 2-heads-tall", "targetW": 32, "targetH": 32, "cat": "Enemy", "genW": 512, "genH": 512}

# ========== 精英 (48x48 target) ==========
ASSET_TEMPLATES["灵魂吞噬者"] = {"prompt": "(soul devourer monster:1.3), large floating entity, (swirling souls around it:1.2), dark void body, multiple eyes, tentacle-like appendages, 3-heads-tall", "targetW": 48, "targetH": 48, "cat": "Enemy", "genW": 768, "genH": 768}
ASSET_TEMPLATES["熔岩元素"] =   {"prompt": "(lava elemental:1.3), humanoid magma creature, molten rock body, (orange-yellow lava veins:1.2), heat distortion, volcanic rock texture, 3.5-heads-tall", "targetW": 48, "targetH": 48, "cat": "Enemy", "genW": 768, "genH": 768}
ASSET_TEMPLATES["机械构造体"] = {"prompt": "(mechanical construct:1.3), large humanoid robot, brass and steel body, (cyan energy core in chest:1.2), steam pipes, gear mechanisms visible, 3.5-heads-tall", "targetW": 48, "targetH": 48, "cat": "Enemy", "genW": 768, "genH": 768}

# ========== BOSS ==========
ASSET_TEMPLATES["时空守护者"] = {"prompt": "(time guardian boss:1.4), massive armored entity, clockwork mechanisms embedded in armor, (golden #C8A848 sacred markings:1.2), floating time gears, hourglass core, imposing stance, 4-heads-tall", "targetW": 64, "targetH": 64, "cat": "Boss", "genW": 512, "genH": 512}
ASSET_TEMPLATES["记忆守护者"] = {"prompt": "(memory guardian boss:1.4), ethereal humanoid figure, (fragmented body made of memory shards:1.3), blue-purple soul glow, translucent, floating, 4-heads-tall", "targetW": 80, "targetH": 80, "cat": "Boss", "genW": 640, "genH": 640}
ASSET_TEMPLATES["S-SN"] =       {"prompt": "(Scarlet Soul Shana:1.4), divine female figure, (crimson red #C41E3A and blue-purple #4A3A8C dual energy:1.3), floating, crystalline wings, elegant battle dress, 4-heads-tall", "targetW": 96, "targetH": 96, "cat": "Boss", "genW": 768, "genH": 768}

# ========== 地图 Tile ==========
ASSET_TEMPLATES["废墟都市Tile"] = {"prompt": "(ruined city tileset:1.3), collapsed buildings, broken streets, (cyan neon signs still glowing:1.2), rusted metal, top-down orthographic view, seamless tileable", "targetW": 64, "targetH": 64, "cat": "MapTile", "genW": 512, "genH": 512}
ASSET_TEMPLATES["遗忘庄园Tile"] = {"prompt": "(forgotten manor tileset:1.3), decaying noble mansion interior, (faded gold #C8A848 decorations:1.1), cracked marble floors, Victorian gothic, top-down orthographic view, seamless tileable", "targetW": 64, "targetH": 64, "cat": "MapTile", "genW": 512, "genH": 512}
ASSET_TEMPLATES["古代神殿Tile"] = {"prompt": "(ancient temple tileset:1.3), white stone temple, (golden sacred engravings:1.2), cracked stone pillars, divine rays from ceiling, top-down orthographic view, seamless tileable", "targetW": 64, "targetH": 64, "cat": "MapTile", "genW": 512, "genH": 512}
ASSET_TEMPLATES["实验室碎片Tile"] = {"prompt": "(laboratory tileset:1.3), sterile white sci-fi lab, (cyan glowing specimen tanks:1.2), metal floors, holographic displays, abandoned research facility, top-down orthographic view, seamless tileable", "targetW": 64, "targetH": 64, "cat": "MapTile", "genW": 512, "genH": 512}

# ========== 道具 (32x32 target) ==========
ASSET_TEMPLATES["营火"] =   {"prompt": "(campfire:1.3), (blue-purple soul flame:1.4), stone circle, glowing embers, warm light radius, safe haven, top-down view", "targetW": 32, "targetH": 32, "cat": "Prop", "genW": 512, "genH": 512}
ASSET_TEMPLATES["炼药锅"] = {"prompt": "(alchemy cauldron:1.3), large iron cauldron, (blue-purple bubbling liquid:1.2), crystal decorations on rim, alchemy symbols, glowing runes, top-down view", "targetW": 32, "targetH": 32, "cat": "Prop", "genW": 512, "genH": 512}
ASSET_TEMPLATES["时空门"] = {"prompt": "(time portal:1.3), (cyan and blue-purple swirling vortex:1.4), floating crystal fragments around portal, energy rings, dimensional gateway, top-down view", "targetW": 32, "targetH": 32, "cat": "Prop", "genW": 512, "genH": 512}
ASSET_TEMPLATES["时空裂隙"] = {"prompt": "(time rift:1.3), (unstable dimensional tear:1.3), jagged edges, (cyan and blue-purple energy leaking:1.2), reality distortion, random sparks, top-down view", "targetW": 32, "targetH": 32, "cat": "Prop", "genW": 512, "genH": 512}


# ==========================================
# 主应用类
# ==========================================
class ComfyUIGenerator:
    def __init__(self, root):
        self.root = root
        self.root.title("ComfyUI 美术素材生产客户端 v1.0")
        self.root.geometry("1100x750")
        self.root.minsize(900, 600)

        self.generating = False
        self.prompt_id = None
        self.server_connected = False

        self._build_ui()
        self._refresh_assets()

    # ==========================================
    # UI 构建
    # ==========================================
    def _build_ui(self):
        # 顶部工具栏
        toolbar = ttk.Frame(self.root, padding=5)
        toolbar.pack(fill=tk.X)

        self.status_label = ttk.Label(toolbar, text="服务器: 未连接", foreground="red")
        self.status_label.pack(side=tk.LEFT, padx=5)

        self.queue_label = ttk.Label(toolbar, text="队列: -")
        self.queue_label.pack(side=tk.LEFT, padx=10)

        ttk.Button(toolbar, text="测试连接", command=self._test_connection).pack(side=tk.LEFT, padx=5)
        ttk.Button(toolbar, text="检查队列", command=self._check_queue).pack(side=tk.LEFT, padx=5)

        ttk.Separator(self.root, orient=tk.HORIZONTAL).pack(fill=tk.X)

        # 主内容区
        main = ttk.Frame(self.root, padding=10)
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
        ]
        self.param_vars = {}
        for i, (label, vname, default) in enumerate(params):
            ttk.Label(param_frame, text=label, width=6).grid(row=i, column=0, sticky=tk.W, pady=2)
            var = tk.StringVar(value=default)
            self.param_vars[vname] = var
            ttk.Entry(param_frame, textvariable=var, width=10).grid(row=i, column=1, sticky=tk.W, pady=2)

        ttk.Label(param_frame, text="(-1=随机种子)").grid(row=4, column=2, sticky=tk.W, padx=5)

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
        self.generate_btn = ttk.Button(btn_frame, text="生成", command=self._generate, style="Accent.TButton")
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
        self.log_text = scrolledtext.ScrolledText(right, height=12, wrap=tk.WORD, state=tk.DISABLED)
        self.log_text.pack(fill=tk.BOTH, expand=True)

    # ==========================================
    # 业务逻辑
    # ==========================================
    def _log(self, msg, tag=""):
        self.log_text.configure(state=tk.NORMAL)
        ts = datetime.datetime.now().strftime("%H:%M:%S")
        prefix = f"[{ts}]"
        if tag:
            prefix += f" [{tag}]"
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
            self.state_frame.pack(fill=tk.X, pady=(5, 0), before=self.generate_btn.master)
            self.param_vars["width_var"].set("512")
            self.param_vars["height_var"].set("512")
        else:
            self.state_frame.pack_forget()
            self.param_vars["width_var"].set("1024")
            self.param_vars["height_var"].set("1024")
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

    def _test_connection(self):
        threading.Thread(target=self._do_test_connection, daemon=True).start()

    def _do_test_connection(self):
        try:
            r = requests.get(f"{SERVER_URL}/api/system_stats", timeout=10)
            self.root.after(0, lambda: self.status_label.configure(text=f"服务器: 已连接 (ComfyUI)", foreground="green"))
            self.root.after(0, lambda: self._log("连接成功!"))
            self.server_connected = True
            self._check_queue()
        except Exception as e:
            self.root.after(0, lambda: self.status_label.configure(text="服务器: 连接失败", foreground="red"))
            self.root.after(0, lambda: self._log(f"连接失败: {e}", "ERROR"))
            self.server_connected = False

    def _check_queue(self):
        threading.Thread(target=self._do_check_queue, daemon=True).start()

    def _do_check_queue(self):
        try:
            r = requests.get(f"{SERVER_URL}/api/queue", timeout=5)
            data = r.json()
            running = len(data.get("queue_running", []))
            pending = len(data.get("queue_pending", []))
            self.root.after(0, lambda: self.queue_label.configure(text=f"队列: {running} 运行 / {pending} 等待"))
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

        self.generating = True
        self.generate_btn.configure(state=tk.DISABLED, text="生成中...")
        self.progress.start(10)

        threading.Thread(target=self._do_generate, daemon=True).start()

    def _do_generate(self):
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

            # 确定输出目录
            if stage == "concept":
                output_dir = os.path.join(self.output_var.get(), tmpl["cat"], name, "Concept")
            else:
                output_dir = os.path.join(self.output_var.get(), tmpl["cat"], name, "Frames", state)
            os.makedirs(output_dir, exist_ok=True)

            self.root.after(0, lambda: self._log(f"开始生成: {name} ({stage})"))
            self.root.after(0, lambda: self._log(f"输出: {output_dir}"))
            self.root.after(0, lambda: self._log(f"尺寸: {width}x{height}, 步数: {steps}, CFG: {cfg}, 种子: {seed}"))

            # 构建 workflow
            workflow = {
                "1": {"class_type": "CheckpointLoaderSimple", "inputs": {"ckpt_name": "动漫 primemix_v21.safetensors"}},
                "2": {"class_type": "CLIPTextEncode", "inputs": {"text": positive, "clip": ["1", 1]}},
                "3": {"class_type": "CLIPTextEncode", "inputs": {"text": negative, "clip": ["1", 1]}},
                "4": {"class_type": "EmptyLatentImage", "inputs": {"width": width, "height": height, "batch_size": 1}},
                "5": {"class_type": "KSampler", "inputs": {"seed": seed, "steps": steps, "cfg": cfg, "sampler_name": "euler_ancestral", "scheduler": "normal", "denoise": 1, "model": ["1", 0], "positive": ["2", 0], "negative": ["3", 0], "latent_image": ["4", 0]}},
                "6": {"class_type": "VAEDecode", "inputs": {"samples": ["5", 0], "vae": ["1", 2]}},
                "7": {"class_type": "SaveImage", "inputs": {"images": ["6", 0], "filename_prefix": "ComfyUI"}},
            }

            # 提交
            body = {"prompt": workflow, "client_id": "comfyui-client-gui"}
            r = requests.post(f"{SERVER_URL}/api/prompt", json=body, timeout=30)
            r.raise_for_status()
            prompt_id = r.json().get("prompt_id")
            self.root.after(0, lambda: self._log(f"已提交: {prompt_id}"))

            # 轮询
            history = None
            for i in range(300):
                time.sleep(2)
                try:
                    h = requests.get(f"{SERVER_URL}/api/history/{prompt_id}", timeout=10)
                    h.raise_for_status()
                    history = h.json()
                    if prompt_id in history:
                        break
                except Exception:
                    pass
                if (i + 1) % 15 == 0:
                    self.root.after(0, lambda: self._log(f"等待中... {(i + 1) * 2}s"))
            else:
                self.root.after(0, lambda: self._log("生成超时!", "ERROR"))
                return

            self.root.after(0, lambda: self._log("生成完成!"))

            # 解析输出
            history_data = history.get(prompt_id, {})
            outputs = history_data.get("outputs", {})
            images = []
            for node_id, node_data in outputs.items():
                for img in node_data.get("images", []):
                    images.append(img)

            if not images:
                self.root.after(0, lambda: self._log("无输出图片", "ERROR"))
                return

            self.root.after(0, lambda: self._log(f"获取到 {len(images)} 张图片"))

            # 下载
            ts = datetime.datetime.now().strftime("%Y%m%d_%H%M%S")
            for i, img in enumerate(images):
                params = {"filename": img["filename"], "subfolder": img.get("subfolder", ""), "type": img.get("type", "output")}
                dl_url = f"{SERVER_URL}/api/view?{urlencode(params)}"

                if stage == "concept":
                    save_name = f"{name}_Concept_{ts}.png"
                else:
                    save_name = f"{name}_{state}_{i + 1}.png"

                save_path = os.path.join(output_dir, save_name)
                r = requests.get(dl_url, timeout=60)
                r.raise_for_status()
                with open(save_path, "wb") as f:
                    f.write(r.content)
                self.root.after(0, lambda p=save_path: self._log(f"已保存: {p}"))

            self.root.after(0, lambda: self._log("全部完成!"))

        except Exception as e:
            self.root.after(0, lambda: self._log(f"错误: {e}", "ERROR"))
        finally:
            self.generating = False
            self.root.after(0, lambda: self.generate_btn.configure(state=tk.NORMAL, text="生成"))
            self.root.after(0, lambda: self.progress.stop())


# ==========================================
# 入口
# ==========================================
def main():
    root = tk.Tk()

    # 样式
    style = ttk.Style()
    try:
        style.theme_use("clam")
    except Exception:
        pass

    app = ComfyUIGenerator(root)
    root.mainloop()

if __name__ == "__main__":
    main()