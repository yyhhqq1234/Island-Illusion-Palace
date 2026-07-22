# -*- coding: utf-8 -*-
"""按 IIPArtLayout 几何 + 真实美术素材(已裁剪留白),在 Python 中预渲染炼金/背包面板,
供多模态审美审查(无需 Unity 运行)。输出到 tools/local-vision/shots/。
坐标系:面板 1440×810,中心原点 y 向上 → 图像左上角原点 y 向下;画布外加 60px 边距看板外元素。
v2: 书本文字移到书上/书下暗底;篮/书辉光改径向椭圆;背包详情面板按 C# 修正几何;新增炼金成功/失败两态。
"""
import os
from PIL import Image, ImageDraw, ImageFont, ImageFilter

ROOT = r"d:\Program Files\Unity\U3Dproject\Island-Illusion-Palace"
ALC_DIR = ROOT + r"\Assets\ArtMaterials\Items\炼金"
RES_ART = ROOT + r"\Assets\Resources\UI\Art"
OUT = ROOT + r"\tools\local-vision\shots"
PW, PH = 1440, 810
PAD = 60  # 画布边距(看面板外元素,如底部信息栏)
CW, CH = PW + PAD * 2, PH + PAD * 2

INV_W, INV_H = 960.0, 540.0
ALC_W, ALC_H = 3840.0, 2160.0

class Grid:
    def __init__(self, ox, oy, cw, ch, px, py, cols, rows):
        self.ox, self.oy, self.cw, self.ch = ox, oy, cw, ch
        self.px, self.py, self.cols, self.rows = px, py, cols, rows

INV_WEAPONS = Grid(19, 70, 61, 57.2, 73.33, 68.97, 4, 6)
INV_CONSUM  = Grid(339, 70, 60.5, 57.2, 73.17, 68.97, 4, 6)
INV_MATS    = Grid(656, 70, 59.5, 57.2, 73.5, 68.97, 4, 6)
ALC_RECIPES = Grid(108, 204, 225, 218, 271, 255.8, 2, 7)
ALC_MATS    = Grid(2891, 216, 260, 248, 309.5, 295, 3, 6)
ALC_BASKETS = [(1681.3, 418.7, 365, 330), (2330.6, 1068.5, 328, 364),
               (1681.4, 1717.7, 365, 328), (1035.1, 1068.3, 305, 365)]
ALC_BOOK = (1681.5, 1065, 500, 500)

PURPLE = (123, 94, 168)
GOLD = (255, 204, 51)
CYAN = (77, 204, 255)
WHITE = (235, 235, 245)
DIM = (166, 166, 191)
FONT = r"C:\Windows\Fonts\msyh.ttc"


def to_px(px_, py_):
    """面板局部坐标 → 图像像素(含 PAD 边距)"""
    return px_ + CW / 2.0, CH / 2.0 - py_


def grid_slot(g, col, row, img_w, img_h):
    cx_img = g.ox + col * g.px + g.cw * 0.5
    cy_img = g.oy + row * g.py + g.ch * 0.5
    return ((cx_img / img_w - 0.5) * PW, (0.5 - cy_img / img_h) * PH,
            g.cw / img_w * PW, g.ch / img_h * PH)


def box_slot(box, img_w, img_h, scale=1.0):
    cx, cy, w, h = box
    return ((cx / img_w - 0.5) * PW, (0.5 - cy / img_h) * PH,
            w * scale / img_w * PW, h * scale / img_h * PH)


def load_icon(path, size):
    img = Image.open(path).convert("RGBA")
    img.thumbnail((int(size[0]), int(size[1])), Image.LANCZOS)
    return img


def paste_center(canvas, icon, cx, cy):
    x, y = to_px(cx, cy)
    canvas.alpha_composite(icon, (int(x - icon.width / 2), int(y - icon.height / 2)))


def glow_rounded(canvas, cx, cy, w, h, color, alpha):
    """圆角矩形辉光(背包方槽)"""
    ov = Image.new("RGBA", canvas.size, (0, 0, 0, 0))
    d = ImageDraw.Draw(ov)
    x, y = to_px(cx, cy)
    d.rounded_rectangle([x - w / 2, y - h / 2, x + w / 2, y + h / 2],
                        radius=min(w, h) * 0.25, fill=color + (int(alpha * 255),))
    ov = ov.filter(ImageFilter.GaussianBlur(3))
    canvas.alpha_composite(ov)


def glow_radial(canvas, cx, cy, w, h, color, alpha):
    """径向椭圆辉光(菱形篮/书本,模拟 IIPUIFactory.RadialGlowSprite)"""
    gw, gh = max(2, int(w)), max(2, int(h))
    g = Image.new("L", (gw, gh), 0)
    gd = ImageDraw.Draw(g)
    steps = 24
    for i in range(steps, 0, -1):
        t = i / steps
        a = int(255 * (1 - t) ** 2 * 0.9)
        gd.ellipse([gw * (1 - t) / 2, gh * (1 - t) / 2, gw * (1 + t) / 2, gh * (1 + t) / 2],
                   fill=min(255, a + g.getpixel((gw // 2, gh // 2)) if i == steps else a))
    # 简化:直接由内向外叠加椭圆环
    g = Image.new("L", (gw, gh), 0)
    px = g.load()
    cxp, cyp = gw / 2, gh / 2
    for yy in range(gh):
        for xx in range(gw):
            d2 = ((xx - cxp) / (gw / 2)) ** 2 + ((yy - cyp) / (gh / 2)) ** 2
            if d2 < 1:
                px[xx, yy] = int(255 * (1 - d2 ** 0.5) ** 2)
    tint = Image.new("RGBA", (gw, gh), color + (0,))
    tint.putalpha(g.point(lambda v: int(v * alpha)))
    x, y = to_px(cx, cy)
    canvas.alpha_composite(tint, (int(x - gw / 2), int(y - gh / 2)))


def text(canvas, cx, cy, s, size, fill, anchor="mm", shadow=True, font=None):
    d = ImageDraw.Draw(canvas)
    x, y = to_px(cx, cy)
    f = font or ImageFont.truetype(FONT, size)
    if shadow:
        d.text((x + 1, y + 1), s, font=f, fill=(0, 0, 0, 200), anchor=anchor)
    d.text((x, y), s, font=f, fill=fill, anchor=anchor)


def new_canvas(bg_path):
    bg = Image.open(bg_path).convert("RGBA").resize((PW, PH), Image.LANCZOS)
    canvas = Image.new("RGBA", (CW, CH), (8, 8, 14, 255))
    canvas.alpha_composite(bg, (PAD, PAD))
    return canvas


def fill_alchemy_grids(canvas, with_glows=True):
    """配方 2×7 + 材料 3×6(两态共用)"""
    f10 = ImageFont.truetype(FONT, 10)
    recipes = ["治疗药剂", "灵魂稳定剂", "召唤持续时间药剂", "负担缓解熏香", "召唤强化剂", "记忆共鸣药剂"]
    for i in range(14):
        col, row = i % 2, i // 2
        cx, cy, w, h = grid_slot(ALC_RECIPES, col, row, ALC_W, ALC_H)
        if i < len(recipes):
            p = ALC_DIR + "\\炼金产物\\" + recipes[i] + ".png"
            if os.path.exists(p):
                paste_center(canvas, load_icon(p, (w * 0.58, h * 0.58)), cx, cy + h * 0.10)
            text(canvas, cx, cy - h * 0.30, recipes[i], 10, WHITE, font=f10)
        elif i < len(recipes) + 2:
            text(canvas, cx, cy, "???", 13, DIM)
        if with_glows and i == 1:
            glow_rounded(canvas, cx, cy, w * 1.08, h * 1.08, PURPLE, 0.70)
    mats = ["腐朽木屑", "碎骨", "星光草", "腐化组织", "月影花",
            "锈蚀零件", "灵魂微尘", "浑浊露珠", "净化盐晶", "哀嚎藤蔓"]
    counts = [12, 8, 5, 3, 2, 6, 4, 9, 1, 7]
    for i in range(18):
        col, row = i % 3, i // 3
        cx, cy, w, h = grid_slot(ALC_MATS, col, row, ALC_W, ALC_H)
        if i < len(mats):
            p = ALC_DIR + "\\炼金素材(基础)\\" + mats[i] + ".png"
            if os.path.exists(p):
                paste_center(canvas, load_icon(p, (w * 0.66, h * 0.66)), cx, cy)
            if counts[i] > 1:
                text(canvas, cx + w * 0.38, cy - h * 0.34, str(counts[i]), 15, WHITE)
        if with_glows and i == 2:
            glow_rounded(canvas, cx, cy, w * 1.08, h * 1.08, PURPLE, 0.40)


def fill_baskets(canvas, fills):
    for i, box in enumerate(ALC_BASKETS):
        cx, cy, w, h = box_slot(box, ALC_W, ALC_H, 0.72)
        if fills[i]:
            p, cnt = fills[i]
            glow_radial(canvas, cx, cy, w * 1.10, h * 1.10, PURPLE, 0.30)
            paste_center(canvas, load_icon(p, (w * 0.72, h * 0.72)), cx, cy)
            if cnt > 1:
                text(canvas, cx + w * 0.34, cy - h * 0.30, str(cnt), 16, WHITE)


def fill_stats(canvas):
    f16 = ImageFont.truetype(FONT, 16)
    stats = [(ALC_DIR + r"\炼金ui\理智.png", "理智 78%", CYAN, -260),
             (ALC_DIR + r"\炼金ui\记忆碎片.png", "碎片 ×2(+15%)", PURPLE, 0),
             (ALC_DIR + r"\炼金ui\负载.png", "负载 45/100", GOLD, 260)]
    d = None
    for p, s, color, x in stats:
        if os.path.exists(p):
            icon = load_icon(p, (44, 44))
            px_, py_ = to_px(x - 90, -368)
            canvas.alpha_composite(icon, (int(px_), int(py_ - 22)))
        d = d or ImageDraw.Draw(canvas)
        tx, ty = to_px(x - 38, -368)
        d.text((tx + 1, ty + 1), s, font=f16, fill=(0, 0, 0, 200), anchor="lm")
        d.text((tx, ty), s, font=f16, fill=color + (255,), anchor="lm")


BOOK_CX, BOOK_CY = box_slot(ALC_BOOK, ALC_W, ALC_H, 1.0)[0:2]  # ≈(5.6, 5.6)

def build_alchemy_preview():
    canvas = new_canvas(ALC_DIR + r"\炼金ui\炼金界面.png")
    fill_alchemy_grids(canvas)
    fill_baskets(canvas, [(ALC_DIR + r"\炼金素材(基础)\星光草.png", 2),
                          (ALC_DIR + r"\炼金素材(基础)\碎骨.png", 1), None, None])
    # 书本:图标在书上;文字自上而下 名称→成功率→提示(提示贴书缘,指向书本)
    paste_center(canvas, load_icon(ALC_DIR + r"\炼金产物\治疗药剂.png", (62, 62)), BOOK_CX, BOOK_CY + 30)
    text(canvas, BOOK_CX, 172, "治疗药剂", 16, WHITE)
    text(canvas, BOOK_CX, 146, "成功率 92%", 13, CYAN)
    text(canvas, BOOK_CX, 120, "点击书本 或 按 Enter 炼成", 11, DIM)
    text(canvas, BOOK_CX, -140, "星光草 2/3 · 碎骨 1/1", 12, DIM)
    fill_stats(canvas)
    canvas.convert("RGB").save(OUT + r"\mock_alchemy_preview.jpg", quality=88)
    print("mock_alchemy_preview.jpg OK")


def build_alchemy_success():
    canvas = new_canvas(ALC_DIR + r"\炼金ui\炼金界面.png")
    fill_alchemy_grids(canvas, with_glows=False)
    fill_baskets(canvas, [None, None, None, None])  # 炼成后清空
    # 金色径向辉光 + 产物 + NEW! 标记
    glow_radial(canvas, BOOK_CX, BOOK_CY, 215, 215, GOLD, 0.35)
    paste_center(canvas, load_icon(ALC_DIR + r"\炼金产物\弱点洞悉药剂.png", (76, 76)), BOOK_CX, BOOK_CY + 16)
    badge = load_icon(RES_ART + r"\NewRecipe_Text.png", (96, 45))
    paste_center(canvas, badge, BOOK_CX + 70, BOOK_CY + 76)
    fill_stats(canvas)
    canvas.convert("RGB").save(OUT + r"\mock_alchemy_success.jpg", quality=88)
    print("mock_alchemy_success.jpg OK")


def build_alchemy_fail():
    canvas = new_canvas(ALC_DIR + r"\炼金ui\炼金界面.png")
    fill_alchemy_grids(canvas, with_glows=False)
    fill_baskets(canvas, [None, None, None, None])
    # Failure... 手绘文字(书上)
    ft = load_icon(RES_ART + r"\AlchemyFail_Text.png", (200, 83))
    paste_center(canvas, ft, 0, 150)
    # 材料返还面板(书右)
    rp = load_icon(RES_ART + r"\MaterialReturn_Panel.png", (168, 285))
    paste_center(canvas, rp, 196, 40)
    # 返还材料图标 4 个
    ret = [ALC_DIR + r"\炼金素材(基础)\星光草.png", ALC_DIR + r"\炼金素材(基础)\碎骨.png"]
    for i in range(4):
        if i < len(ret):
            paste_center(canvas, load_icon(ret[i], (44, 44)), 196, 90.5 - i * 52)
    fill_stats(canvas)
    canvas.convert("RGB").save(OUT + r"\mock_alchemy_fail.jpg", quality=88)
    print("mock_alchemy_fail.jpg OK")


def build_inventory():
    canvas = new_canvas(ROOT + r"\Assets\ArtMaterials\Items\背包\背包界面.png")
    prods = ["治疗药剂", "灵魂稳定剂", "召唤强化剂", "时空道标", "真理窥视药剂", "重生护符"]
    mats = ["腐朽木屑", "碎骨", "星光草", "腐化组织", "月影花", "锈蚀零件", "灵魂微尘"]
    sections = [(INV_WEAPONS, prods, "炼金产物"), (INV_CONSUM, prods[::-1], "炼金产物"),
                (INV_MATS, mats, "炼金素材(基础)")]
    for g, names, sub in sections:
        for i in range(24):
            col, row = i % 4, i // 4
            cx, cy, w, h = grid_slot(g, col, row, INV_W, INV_H)
            if i < len(names):
                p = ALC_DIR + "\\" + sub + "\\" + names[i] + ".png"
                if os.path.exists(p):
                    paste_center(canvas, load_icon(p, (w * 0.70, h * 0.70)), cx, cy)
            if i == 2:
                glow_rounded(canvas, cx, cy, w, h, (153, 102, 255), 0.30)
            if i == 0:
                glow_rounded(canvas, cx, cy, w * 1.10, h * 1.10, PURPLE, 0.40)
            if i == 4:
                glow_rounded(canvas, cx, cy, w * 1.10, h * 1.10, PURPLE, 0.70)

    # 底部信息栏(面板下缘外侧 y=-431)
    ov = Image.new("RGBA", canvas.size, (0, 0, 0, 0))
    d = ImageDraw.Draw(ov)
    bx, by = to_px(0, -PH / 2 - 26)
    d.rounded_rectangle([bx - 310, by - 19, bx + 310, by + 19], radius=8, fill=(20, 18, 33, 153))
    canvas.alpha_composite(ov)
    text(canvas, -210, -PH / 2 - 26, "灵魂: 1,280", 14, CYAN)
    text(canvas, 0, -PH / 2 - 26, "材料: 45/96", 14, WHITE)
    text(canvas, 210, -PH / 2 - 26, "负重: 32/100", 14, GOLD)

    # ══ 详情面板(修正几何):360×600 @ (250,0),local 系 y∈[-300,+300] ══
    ov = Image.new("RGBA", canvas.size, (0, 0, 0, 0))
    d = ImageDraw.Draw(ov)
    x0, y0 = to_px(250 - 180, 300)   # 左上
    x1, y1 = to_px(250 + 180, -300)  # 右下
    d.rounded_rectangle([x0, y0, x1, y1], radius=12, fill=(26, 23, 41, 247))
    canvas.alpha_composite(ov)
    frame = Image.open(RES_ART + r"\Tooltip_Frame.png").convert("RGBA").resize(
        (int(x1 - x0) + 28, int(y1 - y0) + 28), Image.LANCZOS)
    canvas.alpha_composite(frame, (int(x0) - 14, int(y0) - 14))
    # local:+258 顶 → panel y = +258..+162 (图标 96)
    paste_center(canvas, load_icon(ALC_DIR + r"\炼金产物\治疗药剂.png", (96, 96)), 250, 210)
    text(canvas, 250, 127, "治疗药剂", 18, WHITE)          # name 顶+142
    text(canvas, 250, 98, "精良", 15, GOLD)                # rarity 顶+110
    text(canvas, 250, 73, "消耗品", 12, DIM)               # type 顶+84
    text(canvas, 250, 47, "数量 3 · 炼金价值 8", 12, DIM)  # quantity 顶+58
    text(canvas, 250, 10, "恢复 20% 生命值。饮用后温润的", 13, WHITE)  # desc 顶+20 起
    text(canvas, 250, -14, "药力沿血脉流淌,治愈伤痛。", 13, WHITE)
    # 使用按钮(底部,中心 local -262)
    ov = Image.new("RGBA", canvas.size, (0, 0, 0, 0))
    d = ImageDraw.Draw(ov)
    bx, by = to_px(250, -262)
    d.rounded_rectangle([bx - 120, by - 26, bx + 120, by + 26], radius=10, fill=(74, 58, 140, 255))
    canvas.alpha_composite(ov)
    text(canvas, 250, -262, "使 用", 16, WHITE)

    # ══ Tooltip(跟随鼠标示例:武器区第2槽右下) ══
    scx, scy, sw, sh = grid_slot(INV_WEAPONS, 1, 0, INV_W, INV_H)
    ov = Image.new("RGBA", canvas.size, (0, 0, 0, 0))
    d = ImageDraw.Draw(ov)
    tx0, ty0 = to_px(scx + 20, scy - 20)  # pivot 左上
    d.rounded_rectangle([tx0, ty0, tx0 + 340, ty0 + 150], radius=8, fill=(26, 23, 41, 247),
                        outline=(140, 128, 191, 255), width=2)
    canvas.alpha_composite(ov)
    d = ImageDraw.Draw(canvas)
    f16 = ImageFont.truetype(FONT, 16); f13 = ImageFont.truetype(FONT, 13); f12 = ImageFont.truetype(FONT, 12)
    d.text((tx0 + 12, ty0 + 10), "星光草", font=f16, fill=WHITE + (255,))
    d.text((tx0 + 12, ty0 + 36), "普通 · 炼金价值 3", font=f13, fill=GOLD + (255,))
    d.text((tx0 + 12, ty0 + 62), "星光凝结的草药,在月夜下微微发光。", font=f12, fill=DIM + (255,))

    canvas.convert("RGB").save(OUT + r"\mock_inventory_detail.jpg", quality=88)
    print("mock_inventory_detail.jpg OK")


if __name__ == "__main__":
    build_alchemy_preview()
    build_alchemy_success()
    build_alchemy_fail()
    build_inventory()
