# -*- coding: utf-8 -*-
"""批量裁剪 UI 图标 PNG 的透明留白(原地,保留 3px 内边距)。
背景整图(炼金界面/背包界面/材料信息界面)不碰。
git 已跟踪原文件,可直接还原。用法: python trim_icons.py [--dry-run]
"""
import sys, os, glob
from PIL import Image

sys.stdout.reconfigure(encoding="utf-8")
ROOT = r"d:\Program Files\Unity\U3Dproject\Island-Illusion-Palace"
PAD = 3
SKIP = {"炼金界面.png", "背包界面.png", "材料信息界面.png"}

TARGETS = (
    glob.glob(ROOT + r"\Assets\ArtMaterials\Items\炼金\炼金素材(基础)\*.png") +
    glob.glob(ROOT + r"\Assets\ArtMaterials\Items\炼金\炼金素材(稀有)\*.png") +
    glob.glob(ROOT + r"\Assets\ArtMaterials\Items\炼金\炼金素材(史诗)\*.png") +
    glob.glob(ROOT + r"\Assets\ArtMaterials\Items\炼金\炼金素材(传奇)\*.png") +
    glob.glob(ROOT + r"\Assets\ArtMaterials\Items\炼金\炼金产物\*.png") +
    glob.glob(ROOT + r"\Assets\ArtMaterials\Items\炼金\炼金ui\*.png")
)

def main(dry_run):
    trimmed = skipped = 0
    for p in TARGETS:
        name = os.path.basename(p)
        if name in SKIP:
            print(f"跳过(整图): {name}")
            skipped += 1
            continue
        img = Image.open(p).convert("RGBA")
        bb = img.getchannel("A").getbbox()
        if bb is None:
            print(f"跳过(全透明): {name}")
            skipped += 1
            continue
        w, h = img.size
        l = max(0, bb[0] - PAD); t = max(0, bb[1] - PAD)
        r = min(w, bb[2] + PAD); b = min(h, bb[3] + PAD)
        cw, ch = r - l, b - t
        if cw >= w * 0.97 and ch >= h * 0.97:
            print(f"跳过(已紧凑): {name} {w}x{h}")
            skipped += 1
            continue
        print(f"裁剪: {name}  {w}x{h} -> {cw}x{ch}  (内容占比 {cw*100//w}%x{ch*100//h}%, 宽高比 {cw/ch:.3f})")
        if not dry_run:
            img.crop((l, t, r, b)).save(p, optimize=True)
        trimmed += 1
    print(f"\n合计: 裁剪 {trimmed}, 跳过 {skipped}" + ("(DRY-RUN,未写盘)" if dry_run else ",已原地写盘"))

if __name__ == "__main__":
    main("--dry-run" in sys.argv)
