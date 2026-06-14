"""ComfyUI 美术资产生成脚本 v4 (Python) - 支持 txt2img 和 img2img"""
import requests
import json
import time
import sys
import os
import random
import argparse
from urllib.parse import urlencode, quote

SERVER_URL = "http://10.150.164.64:8188"
OUTPUT_BASE = r"d:\Program Files\Unity\U3Dproject\Island-Illusion-Palace\Assets\ArtMaterials"
DEFAULT_CHECKPOINT = "sd_xl_base_1.0.safetensors"  # 文生图默认使用 SDXL

CONCEPT_BASE_POSITIVE = "(dark fantasy concept art:1.3), (digital painting:1.2), dramatic lighting, (soul glow: blue-purple luminescence #4A3A8C), (crystal erosion: geometric cyan crystals #00D4FF), rim light, high contrast, detailed texture, atmospheric perspective"

CONCEPT_NEGATIVE = "photorealistic, 3D render, CGI, smooth vector art, flat design, bright sunny, cartoon, anime, chibi, pixel art, low resolution, blurry, watermark, text, signature, ugly, distorted, bad anatomy, extra limbs, fused fingers, low quality, jpeg artifacts"

SPRITE_BASE_POSITIVE = "(simple pixel art:1.4), (minimalist game sprite:1.3), dark fantasy, (soul glow: blue-purple luminescence #4A3A8C), (crystal erosion: geometric cyan crystals #00D4FF), clean pixel lines, flat color blocks, no anti-aliasing, no gradients, white background, centered composition, simple shading, limited color palette"

SPRITE_NEGATIVE = "photorealistic, 3D render, CGI, smooth vector art, flat design, bright sunny, hand-drawn lineart, smooth outline, cel-shading, painterly texture, visible brushstrokes, anti-aliasing, smooth edges, gradient shading, complex lighting, blurry, watermark, text, signature, ugly, distorted, bad anatomy, extra limbs, fused fingers, low quality, jpeg artifacts, crowded composition, background detail"

GUARDIAN_PROMPT = "(time guardian boss:1.4), (armored entity:1.3), (clockwork mechanisms embedded in armor:1.2), (golden #C8A848 sacred markings:1.2), floating time gears, hourglass core, imposing stance, pixel art style, 2.5-heads-tall"

STATE_PROMPTS = {
    "idle":   "(front-facing view:1.2), standing idle pose, neutral stance, arms at sides, slight breathing posture",
    "walk":   "(front-facing view:1.2), walking pose, one foot forward mid-step, natural arm swing",
    "attack": "(front-facing view:1.2), attack pose, weapon raised mid-swing, dynamic action stance",
    "skill":  "(front-facing view:1.2), casting spell pose, hands channeling energy, power-up stance",
    "death":  "(front-facing view:1.2), defeated pose, collapsing forward, body tilting, fading effect",
}

def build_workflow(positive, negative, width, height, steps, cfg, batch_size=1, checkpoint=None):
    """构建 txt2img 工作流（纯文本生图）"""
    ckpt = checkpoint or DEFAULT_CHECKPOINT
    seed = random.randint(0, 2147483647)
    return {
        "1": {"class_type": "CheckpointLoaderSimple", "inputs": {"ckpt_name": ckpt}},
        "2": {"class_type": "CLIPTextEncode", "inputs": {"text": positive, "clip": ["1", 1]}},
        "3": {"class_type": "CLIPTextEncode", "inputs": {"text": negative, "clip": ["1", 1]}},
        "4": {"class_type": "EmptyLatentImage", "inputs": {"width": width, "height": height, "batch_size": batch_size}},
        "5": {"class_type": "KSampler", "inputs": {"seed": seed, "steps": steps, "cfg": cfg, "sampler_name": "euler_ancestral", "scheduler": "normal", "denoise": 1, "model": ["1", 0], "positive": ["2", 0], "negative": ["3", 0], "latent_image": ["4", 0]}},
        "6": {"class_type": "VAEDecode", "inputs": {"samples": ["5", 0], "vae": ["1", 2]}},
        "7": {"class_type": "SaveImage", "inputs": {"images": ["6", 0], "filename_prefix": "ComfyUI"}},
    }

def build_img2img_workflow(image_filename, positive, negative, width, height, steps, cfg, denoise, seed, batch_size=1, checkpoint=None):
    """
    构建 img2img 工作流（以参考图为基础进行变换）

    节点流程:
    CheckpointLoaderSimple → CLIPTextEncode(x2) → LoadImage → VAEEncode → KSampler(denoise) → VAEDecode → SaveImage
    """
    ckpt = checkpoint or DEFAULT_CHECKPOINT
    return {
        "1": {"class_type": "CheckpointLoaderSimple", "inputs": {"ckpt_name": ckpt}},
        "2": {"class_type": "CLIPTextEncode", "inputs": {"text": positive, "clip": ["1", 1]}},
        "3": {"class_type": "CLIPTextEncode", "inputs": {"text": negative, "clip": ["1", 1]}},
        "4": {"class_type": "LoadImage", "inputs": {"image": image_filename}},
        "5": {"class_type": "VAEEncode", "inputs": {"pixels": ["4", 0], "vae": ["1", 2]}},
        "6": {"class_type": "KSampler", "inputs": {
            "seed": seed,
            "steps": steps,
            "cfg": cfg,
            "sampler_name": "euler_ancestral",
            "scheduler": "normal",
            "denoise": denoise,
            "model": ["1", 0],
            "positive": ["2", 0],
            "negative": ["3", 0],
            "latent_image": ["5", 0]
        }},
        "7": {"class_type": "VAEDecode", "inputs": {"samples": ["6", 0], "vae": ["1", 2]}},
        "8": {"class_type": "SaveImage", "inputs": {"images": ["7", 0], "filename_prefix": "ComfyUI_img2img"}},
    }

def upload_image_to_comfyui(image_path):
    """
    上传图片到 ComfyUI 服务器的 input 目录
    
    ComfyUI 的 img2img 需要图片先上传到服务器的 input 目录，
    然后通过 LoadImage 节点按文件名加载。
    """
    try:
        filename = os.path.basename(image_path)
        print(f"正在上传参考图: {filename}...")

        with open(image_path, 'rb') as f:
            files = {'image': (filename, f, 'image/png')}
            data = {'overwrite': 'true'}
            r = requests.post(f"{SERVER_URL}/api/upload/image", files=files, data=data, timeout=60)
            r.raise_for_status()

        result = r.json()
        uploaded_name = result.get("name", filename)
        print(f"参考图上传成功: {uploaded_name}")
        return uploaded_name

    except Exception as e:
        print(f"ERROR: 上传参考图失败: {e}")
        return None

def main():
    # 命令行参数解析
    parser = argparse.ArgumentParser(
        description='ComfyUI 美术资产生成工具 v4 - 支持 txt2img 和 img2img',
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
使用示例:
  # txt2img 概念图生成（默认模式）
  python comfyui_generate.py "时空守护者" concept

  # img2img 精灵帧生成（以概念图为参考）
  python comfyui_generate.py "时空守护者" sprite idle --ref "Concept/时空守护者_Concept_xxx.png" --count 6 --denoise 0.5

  # 自定义参数
  python comfyui_generate.py "时空守护者" concept --count 4 --steps 30 --cfg 8.0
        """
    )

    parser.add_argument("asset_type", type=str, help="资产类型名称（如：时空守护者）")
    parser.add_argument("stage", type=str, choices=["concept", "sprite"], help="生成阶段: concept(概念图) 或 sprite(精灵帧)")
    parser.add_argument("state", type=str, nargs="?", default="idle", help="动画状态 (仅精灵帧模式需要): idle/walk/attack/skill/death")
    parser.add_argument("--ref", type=str, default=None, help="参考底图路径 (仅精灵帧/img2img 模式)")
    parser.add_argument("--count", type=int, default=1, help="生成数量 (默认: 1, 精灵帧推荐 6)")
    parser.add_argument("--denoise", type=float, default=0.5, help="去噪强度 (默认: 0.5, 范围 0.1-0.9)")
    parser.add_argument("--steps", type=int, default=20, help="采样步数 (默认: 20)")
    parser.add_argument("--cfg", type=float, default=7.0, help="CFG 强度 (默认: 7.0)")
    parser.add_argument("--ckpt", type=str, default=None, help=f"指定 Checkpoint 模型 (默认: {DEFAULT_CHECKPOINT})")

    args = parser.parse_args()

    asset_type = args.asset_type
    stage = args.stage
    state = args.state
    ref_image_path = args.ref
    count = args.count
    denoise = args.denoise
    steps = args.steps
    cfg = args.cfg

    # Build prompts
    state_prompt = STATE_PROMPTS.get(state, "idle pose")
    if stage == "concept":
        positive = f"{CONCEPT_BASE_POSITIVE}, {GUARDIAN_PROMPT}"
        negative = CONCEPT_NEGATIVE
        width, height = 1024, 1024
        output_dir = os.path.join(OUTPUT_BASE, asset_type, "Concept")
    else:
        positive = f"{SPRITE_BASE_POSITIVE}, {GUARDIAN_PROMPT}, {state_prompt}, single frame, pixel art character"
        negative = SPRITE_NEGATIVE
        width, height = 512, 512
        output_dir = os.path.join(OUTPUT_BASE, asset_type, "Frames", state)

    os.makedirs(output_dir, exist_ok=True)

    print("=" * 60)
    print(f" ComfyUI Generate v4: {asset_type} / {stage} / {state}")
    print("=" * 60)
    print(f"Output: {output_dir}")
    print(f"Positive: {positive[:120]}...")
    print(f"Negative: {negative[:80]}...")
    print(f"Size: {width}x{height}, Steps: {steps}, CFG: {cfg}")
    print(f"Count: {count}, Denoise: {denoise}")

    # 根据模式选择工作流类型
    if stage == "sprite" and ref_image_path:
        # img2img 模式
        print(f"\n模式: img2img (参考图: {ref_image_path})")

        # 检查参考图是否存在
        if not os.path.exists(ref_image_path):
            print(f"ERROR: 参考底图不存在: {ref_image_path}")
            sys.exit(1)

        # 上传参考图到 ComfyUI 服务器
        uploaded_filename = upload_image_to_comfyui(ref_image_path)
        if not uploaded_filename:
            sys.exit(1)

        # 构建 img2img workflow
        seed = random.randint(0, 2147483647)
        workflow = build_img2img_workflow(
            uploaded_filename, positive, negative,
            width, height, steps, cfg, denoise, seed, count, checkpoint=args.ckpt
        )
        print(f"使用 img2img 工作流 (denoise={denoise}, batch_size={count})")
    else:
        # txt2img 模式（默认）
        if stage == "sprite":
            print("\nWARNING: 精灵帧模式但未指定 --ref 参数，将使用 txt2img 模式")
            print("提示: 使用 --ref 参数指定参考底图可保持角色外观一致性")

        print("\n模式: txt2img (概念图生成)")
        workflow = build_workflow(positive, negative, width, height, steps, cfg, count, checkpoint=args.ckpt)
        print(f"使用 txt2img 工作流 (batch_size={count})")
    body = {"prompt": workflow, "client_id": "python-generator"}

    # Submit
    print(f"\nSubmitting to {SERVER_URL}/api/prompt...")
    r = requests.post(f"{SERVER_URL}/api/prompt", json=body, timeout=30)
    r.raise_for_status()
    prompt_id = r.json().get("prompt_id")
    if not prompt_id:
        print("ERROR: No prompt_id returned")
        sys.exit(1)
    print(f"Submitted! prompt_id: {prompt_id}")

    # Poll
    print("Waiting for completion...")
    max_wait = 600
    history = None
    for i in range(max_wait):
        time.sleep(2)
        try:
            h = requests.get(f"{SERVER_URL}/api/history/{prompt_id}", timeout=10)
            h.raise_for_status()
            history = h.json()
            if history and prompt_id in history:
                break
        except Exception:
            pass
        if (i + 1) % 15 == 0:
            print(f"  Waiting {(i + 1) * 2}s...")
    else:
        print("ERROR: Timeout (20 min)")
        sys.exit(1)

    print("Generation complete!")

    # Parse outputs
    history_data = history.get(prompt_id, {})
    outputs = history_data.get("outputs", {})
    images = []
    for node_id, node_data in outputs.items():
        for img in node_data.get("images", []):
            images.append(img)

    if not images:
        print("ERROR: No output images")
        sys.exit(1)

    print(f"Got {len(images)} image(s)")

    # Download
    import datetime
    ts = datetime.datetime.now().strftime("%Y%m%d_%H%M%S")
    saved = []

    for i, img in enumerate(images):
        params = {
            "filename": img["filename"],
            "subfolder": img.get("subfolder", ""),
            "type": img.get("type", "output"),
        }
        dl_url = f"{SERVER_URL}/api/view?{urlencode(params)}"

        if stage == "concept":
            save_name = f"{asset_type}_Concept_{ts}.png"
        else:
            save_name = f"{asset_type}_{state}_{i + 1}.png"

        save_path = os.path.join(output_dir, save_name)
        print(f"Download: {img['filename']} -> {save_path}")

        r = requests.get(dl_url, timeout=60)
        r.raise_for_status()
        with open(save_path, "wb") as f:
            f.write(r.content)
        saved.append(save_path)
        print(f"  Saved!")

    print("\n" + "=" * 50)
    print(" Done!")
    print("=" * 50)
    print(f"Output: {output_dir}")
    for f in saved:
        print(f"  {f}")

if __name__ == "__main__":
    main()