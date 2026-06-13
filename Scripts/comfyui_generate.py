"""ComfyUI 美术资产生成脚本 v3 (Python)"""
import requests
import json
import time
import sys
import os
import random
from urllib.parse import urlencode, quote

SERVER_URL = "http://10.150.164.64:8188"
OUTPUT_BASE = r"d:\Program Files\Unity\U3Dproject\Island-Illusion-Palace\Assets\ArtMaterials\Boss"

CONCEPT_BASE_POSITIVE = "(dark fantasy concept art:1.3), (digital painting:1.2), dramatic lighting, (soul glow: blue-purple luminescence #4A3A8C), (crystal erosion: geometric cyan crystals #00D4FF), rim light, high contrast, detailed texture, atmospheric perspective"

CONCEPT_NEGATIVE = "photorealistic, 3D render, CGI, smooth vector art, flat design, bright sunny, cartoon, anime, chibi, pixel art, low resolution, blurry, watermark, text, signature, ugly, distorted, bad anatomy, extra limbs, fused fingers, low quality, jpeg artifacts"

SPRITE_BASE_POSITIVE = "(pixel art:1.4), (2D top-down game sprite:1.3), dark fantasy, (soul glow: blue-purple luminescence #4A3A8C), (crystal erosion: geometric cyan crystals #00D4FF), cel-shading, 1px outline, transparent background, dark atmospheric, high contrast, limited color palette"

SPRITE_NEGATIVE = "photorealistic, 3D render, CGI, smooth vector art, flat design, bright sunny, cartoon, anime, chibi, painterly texture, visible brushstrokes, blurry, watermark, text, signature, ugly, distorted, bad anatomy, extra limbs, fused fingers, low quality, jpeg artifacts, anti-aliasing"

GUARDIAN_PROMPT = "(time guardian boss:1.4), massive armored entity, clockwork mechanisms embedded in armor, (golden #C8A848 sacred markings:1.2), floating time gears, hourglass core, imposing stance, ancient guardian, 4-heads-tall"

STATE_PROMPTS = {
    "idle": "standing idle pose, neutral stance, breathing slightly",
    "walk": "walking forward, stepping motion, dynamic movement",
    "attack": "mid-swing attack pose, arm raised, energy flowing, action pose",
    "skill": "casting spell, energy swirling, dramatic power pose",
    "death": "falling down, collapsing, dissolving into particles",
}

def build_workflow(positive, negative, width, height, steps, cfg):
    seed = random.randint(0, 2147483647)
    return {
        "1": {"class_type": "CheckpointLoaderSimple", "inputs": {"ckpt_name": "动漫 primemix_v21.safetensors"}},
        "2": {"class_type": "CLIPTextEncode", "inputs": {"text": positive, "clip": ["1", 1]}},
        "3": {"class_type": "CLIPTextEncode", "inputs": {"text": negative, "clip": ["1", 1]}},
        "4": {"class_type": "EmptyLatentImage", "inputs": {"width": width, "height": height, "batch_size": 1}},
        "5": {"class_type": "KSampler", "inputs": {"seed": seed, "steps": steps, "cfg": cfg, "sampler_name": "euler_ancestral", "scheduler": "normal", "denoise": 1, "model": ["1", 0], "positive": ["2", 0], "negative": ["3", 0], "latent_image": ["4", 0]}},
        "6": {"class_type": "VAEDecode", "inputs": {"samples": ["5", 0], "vae": ["1", 2]}},
        "7": {"class_type": "SaveImage", "inputs": {"images": ["6", 0], "filename_prefix": "ComfyUI"}},
    }

def main():
    asset_type = sys.argv[1] if len(sys.argv) > 1 else "时空守护者"
    stage = sys.argv[2] if len(sys.argv) > 2 else "concept"
    state = sys.argv[3] if len(sys.argv) > 3 else "idle"

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

    print("=" * 50)
    print(f" ComfyUI Generate: {asset_type} / {stage} / {state}")
    print("=" * 50)
    print(f"Output: {output_dir}")
    print(f"Positive: {positive[:120]}...")
    print(f"Negative: {negative[:80]}...")
    print(f"Size: {width}x{height}")

    # Build workflow
    workflow = build_workflow(positive, negative, width, height, 20, 7.0)
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