# -*- coding: utf-8 -*-
"""从手绘 UI 底图识别槽位网格几何参数。
输出 JSON: 每个区域 {originX, originY, cellW, cellH, gapX, gapY, cols, rows}（像素, 原图坐标系, y向下）。
另识别炼金阵 4 个菱形槽与中心书槽的中心点。
"""
import json, sys
import numpy as np
from PIL import Image

ROOT = r"d:\Program Files\Unity\U3Dproject\Island-Illusion-Palace"
INV = ROOT + r"\Assets\ArtMaterials\Items\背包\背包界面.png"
ALC = ROOT + r"\Assets\ArtMaterials\Items\炼金\炼金ui\炼金界面.png"


def load_gray(path):
    img = Image.open(path).convert("RGBA")
    arr = np.asarray(img).astype(np.float32)
    # 合成到黑底（素材全不透明, 但保险起见）
    a = arr[..., 3:4] / 255.0
    rgb = arr[..., :3] * a
    gray = rgb.mean(axis=2)
    return gray


def find_runs(proj, thresh, min_len):
    """在投影数组上找超过阈值的连续段, 返回 [(start,end)]。"""
    mask = proj > thresh
    runs = []
    i = 0
    n = len(mask)
    while i < n:
        if mask[i]:
            j = i
            while j < n and mask[j]:
                j += 1
            if j - i >= min_len:
                runs.append((i, j))
            i = j
        else:
            i += 1
    return runs


def detect_grid(gray, region, thresh_ratio=0.5):
    """region = (x0,y0,x1,y1)。阈值=区域均值的某个比例以上且绝对值>30。"""
    x0, y0, x1, y1 = region
    sub = gray[y0:y1, x0:x1]
    # 槽位是中灰实心块; 手绘线描也亮但细。用形态学思路: 对行列投影取"超过阈值的占比"
    thr = max(35.0, sub.mean() + 20)
    binm = (sub > thr).astype(np.float32)
    colproj = binm.mean(axis=0)  # 每列被槽覆盖的比例
    rowproj = binm.mean(axis=1)
    # 槽内列占比高(>0.6), 间隙列占比低
    col_runs = find_runs(colproj, 0.6, 8)
    row_runs = find_runs(rowproj, 0.6, 8)
    if not col_runs or not row_runs:
        return None
    cells_w = [e - s for s, e in col_runs]
    cells_h = [e - s for s, e in row_runs]
    gaps_x = [col_runs[i+1][0] - col_runs[i][1] for i in range(len(col_runs)-1)]
    gaps_y = [row_runs[i+1][0] - row_runs[i][1] for i in range(len(row_runs)-1)]
    return {
        "originX": x0 + col_runs[0][0],
        "originY": y0 + row_runs[0][0],
        "cellW": round(float(np.mean(cells_w)), 2),
        "cellH": round(float(np.mean(cells_h)), 2),
        "gapX": round(float(np.mean(gaps_x)), 2) if gaps_x else 0.0,
        "gapY": round(float(np.mean(gaps_y)), 2) if gaps_y else 0.0,
        "cols": len(col_runs),
        "rows": len(row_runs),
        "cellW_all": cells_w,
        "cellH_all": cells_h,
        "endX": x0 + col_runs[-1][1],
        "endY": y0 + row_runs[-1][1],
    }


def detect_blobs(gray, region, thr=60, min_area=800):
    """简单连通域(4连通) 找实心亮块, 返回质心+面积+bbox。用于菱形槽/书槽。"""
    x0, y0, x1, y1 = region
    sub = gray[y0:y1, x0:x1]
    binm = sub > thr
    h, w = binm.shape
    lab = np.zeros((h, w), dtype=np.int32)
    cur = 0
    blobs = []
    # BFS (iterative, 用栈)
    from collections import deque
    for sy in range(0, h, 1):
        # 跳过已标记/非前景 —— 全图扫描慢, 用 find 加速
        pass
    # 更快: 用 scipy? 不在依赖里。手写扫描 but limit iterations by using while with queue on seed points
    visited = np.zeros_like(binm, dtype=bool)
    coords = np.argwhere(binm)
    for (yy, xx) in coords:
        if visited[yy, xx]:
            continue
        cur += 1
        q = deque([(yy, xx)])
        visited[yy, xx] = True
        pts = []
        while q:
            cy, cx = q.popleft()
            pts.append((cy, cx))
            lab[cy, cx] = cur
            for dy, dx in ((1,0),(-1,0),(0,1),(0,-1)):
                ny, nx = cy+dy, cx+dx
                if 0 <= ny < h and 0 <= nx < w and binm[ny, nx] and not visited[ny, nx]:
                    visited[ny, nx] = True
                    q.append((ny, nx))
        if len(pts) >= min_area:
            arr = np.array(pts)
            blobs.append({
                "cx": round(float(arr[:,1].mean() + x0), 1),
                "cy": round(float(arr[:,0].mean() + y0), 1),
                "area": len(pts),
                "bbox": [int(arr[:,1].min()+x0), int(arr[:,0].min()+y0), int(arr[:,1].max()+x0), int(arr[:,0].max()+y0)],
                "w": int(arr[:,1].max()-arr[:,1].min()+1),
                "h": int(arr[:,0].max()-arr[:,0].min()+1),
            })
    return blobs


def main():
    out = {}

    # ═══ 背包 960×540 ═══
    g = load_gray(INV)
    H, W = g.shape
    print(f"背包: {W}x{H}, mean={g.mean():.1f}", file=sys.stderr)
    inv = {}
    # 三个区域粗略框 (按目检): 左 0-315, 中 320-640, 右 645-960; 垂直 50-500
    inv["weapons"] = detect_grid(g, (0, 50, 315, 500))
    inv["consumables"] = detect_grid(g, (320, 50, 640, 500))
    inv["materials"] = detect_grid(g, (645, 50, 960, 500))
    out["inventory"] = {"imageW": W, "imageH": H, "grids": inv}

    # ═══ 炼金 3840×2160 ═══
    g2 = load_gray(ALC)
    H2, W2 = g2.shape
    print(f"炼金: {W2}x{H2}, mean={g2.mean():.1f}", file=sys.stderr)
    alc = {}
    # 左配方栏 2x7: x 60-640, y 180-2000 (按1024预览比例×3.75)
    alc["recipes"] = detect_grid(g2, (40, 150, 700, 2050))
    # 右材料栏 3x6: x 2850-3800, y 150-2050
    alc["materials"] = detect_grid(g2, (2800, 150, 3830, 2050))
    out["alchemy"] = {"imageW": W2, "imageH": H2, "grids": alc}

    # 中央菱形槽与书槽: 中心区域 x 900-2900, y 100-2100
    blobs = detect_blobs(g2, (900, 100, 2900, 2100), thr=60, min_area=3000)
    out["alchemy"]["centerBlobs"] = blobs

    print(json.dumps(out, ensure_ascii=False, indent=1))


if __name__ == "__main__":
    main()
