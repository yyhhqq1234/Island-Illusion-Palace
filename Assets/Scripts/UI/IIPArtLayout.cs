using UnityEngine;

namespace IIPUI
{
    /// <summary>
    /// 手绘美术底图的槽位几何数据（由 tools/ui-grid-detect/detect_slots.py 从 PNG 像素识别生成，请勿手改）。
    /// 图片坐标系：原点在图片左上，y 向下，单位 = 图片像素。
    /// 运行时通过 ToPanel / GetSlot 等比换算到面板局部坐标（面板中心为原点，y 向上），
    /// 面板必须与底图保持相同宽高比（两张底图均为 16:9，面板统一 1440×810）。
    /// </summary>
    public static class IIPArtLayout
    {
        /// <summary>规则网格：原点（首槽左上）+ 单槽尺寸 + 步进 + 行列数。</summary>
        public struct Grid
        {
            public float originX, originY;
            public float cellW, cellH;
            public float pitchX, pitchY;
            public int cols, rows;
        }

        /// <summary>自由槽位：中心点 + 包围盒尺寸（图片像素）。</summary>
        public struct SlotBox
        {
            public float cx, cy, w, h;
            public SlotBox(float cx, float cy, float w, float h) { this.cx = cx; this.cy = cy; this.w = w; this.h = h; }
        }

        // ═══════════════════════════════════════════
        // 背包底图 960×540（面板 1440×810 = 1.5×）
        // ═══════════════════════════════════════════
        public const float InvW = 960f;
        public const float InvH = 540f;

        /// <summary>武器栏（剑图标，左）4×6</summary>
        public static readonly Grid InvWeapons = new Grid
        { originX = 19f, originY = 70f, cellW = 61f, cellH = 57.2f, pitchX = 73.33f, pitchY = 68.97f, cols = 4, rows = 6 };

        /// <summary>消耗品栏（烧瓶图标，中）4×6</summary>
        public static readonly Grid InvConsumables = new Grid
        { originX = 339f, originY = 70f, cellW = 60.5f, cellH = 57.2f, pitchX = 73.17f, pitchY = 68.97f, cols = 4, rows = 6 };

        /// <summary>材料栏（右）4×6</summary>
        public static readonly Grid InvMaterials = new Grid
        { originX = 656f, originY = 70f, cellW = 59.5f, cellH = 57.2f, pitchX = 73.5f, pitchY = 68.97f, cols = 4, rows = 6 };

        // ═══════════════════════════════════════════
        // 炼金底图 3840×2160（面板 1440×810 = 0.375×）
        // ═══════════════════════════════════════════
        public const float AlcW = 3840f;
        public const float AlcH = 2160f;

        /// <summary>配方栏（左）2×7</summary>
        public static readonly Grid AlcRecipes = new Grid
        { originX = 108f, originY = 204f, cellW = 225f, cellH = 218f, pitchX = 271f, pitchY = 255.8f, cols = 2, rows = 7 };

        /// <summary>材料栏（右）3×6</summary>
        public static readonly Grid AlcMaterials = new Grid
        { originX = 2891f, originY = 216f, cellW = 260f, cellH = 248f, pitchX = 309.5f, pitchY = 295f, cols = 3, rows = 6 };

        /// <summary>菱形篮槽-北（上）</summary>
        public static readonly SlotBox AlcBasketN = new SlotBox(1681.3f, 418.7f, 365f, 330f);
        /// <summary>菱形篮槽-东（右）</summary>
        public static readonly SlotBox AlcBasketE = new SlotBox(2330.6f, 1068.5f, 328f, 364f);
        /// <summary>菱形篮槽-南（下）</summary>
        public static readonly SlotBox AlcBasketS = new SlotBox(1681.4f, 1717.7f, 365f, 328f);
        /// <summary>菱形篮槽-西（左）</summary>
        public static readonly SlotBox AlcBasketW = new SlotBox(1035.1f, 1068.3f, 305f, 365f);
        /// <summary>中央书槽（产物/配方书，区域已内缩）</summary>
        public static readonly SlotBox AlcBook = new SlotBox(1681.5f, 1065f, 500f, 500f);

        // ═══════════════════════════════════════════
        // 坐标换算
        // ═══════════════════════════════════════════

        /// <summary>图片像素点 → 面板局部坐标（面板中心原点，y 向上）。panelSize 与图片同宽高比。</summary>
        public static Vector2 ToPanel(float imgX, float imgY, float imgW, float imgH, Vector2 panelSize)
        {
            return new Vector2((imgX / imgW - 0.5f) * panelSize.x, (0.5f - imgY / imgH) * panelSize.y);
        }

        /// <summary>图片像素尺寸 → 面板局部尺寸。</summary>
        public static Vector2 ScaleSize(float w, float h, float imgW, float imgH, Vector2 panelSize)
        {
            return new Vector2(w / imgW * panelSize.x, h / imgH * panelSize.y);
        }

        /// <summary>取网格第 (col,row) 槽的中心（面板局部坐标）与尺寸。</summary>
        public static void GetSlot(Grid g, int col, int row, float imgW, float imgH, Vector2 panelSize,
            out Vector2 center, out Vector2 size)
        {
            float cx = g.originX + col * g.pitchX + g.cellW * 0.5f;
            float cy = g.originY + row * g.pitchY + g.cellH * 0.5f;
            center = ToPanel(cx, cy, imgW, imgH, panelSize);
            size = ScaleSize(g.cellW, g.cellH, imgW, imgH, panelSize);
        }

        /// <summary>取自由槽位的中心（面板局部坐标）与尺寸。scale 可缩放包围盒（如菱形内缩 0.72）。</summary>
        public static void GetSlot(SlotBox box, float imgW, float imgH, Vector2 panelSize, float scale,
            out Vector2 center, out Vector2 size)
        {
            center = ToPanel(box.cx, box.cy, imgW, imgH, panelSize);
            size = ScaleSize(box.w * scale, box.h * scale, imgW, imgH, panelSize);
        }
    }
}
