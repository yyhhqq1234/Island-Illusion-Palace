using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace IIPUI
{
    /// <summary>
    /// 美术网格滚动区：ScrollRect + RectMask2D 包裹手绘网格区域（仅垂直滚动，Clamped）。
    ///
    /// 用途：背包/炼金面板的物品栏槽位对齐手绘底图，物品数量超出可见行时，
    /// 内容行数动态增长，玩家用鼠标滚轮上下滚动即可看到并选中所有物品。
    ///
    /// 结构：Viewport（透明底 + RectMask2D 裁剪 + ScrollRect）
    ///         └─ Content（顶锚定，高度 = 总行数自适应）
    ///              └─ 槽位由持有方创建，锚点固定为顶中 (0.5,1)、轴心中心，
    ///                 位置通过 GetCellAnchoredPos(col,row) 获取。
    ///
    /// 视口透明 Image 接收滚轮（滚动事件沿层级冒泡到 ScrollRect），
    /// 格子间隙的点击通过 onGapClick 回调转发（保持原有"点击空白关闭详情"语义）。
    /// </summary>
    public class ArtScrollGrid
    {
        /// <summary>视口外扩像素（避免裁掉槽位 1.10x 辉光的描边）</summary>
        const float ViewportPadding = 8f;

        readonly IIPArtLayout.Grid grid;
        readonly Vector2 cellSize;   // 单槽尺寸（面板单位）
        readonly Vector2 pitch;      // 槽位步进（面板单位）
        readonly Vector2 gridSize;   // 可见网格总尺寸（面板单位）

        /// <summary>视口（带 RectMask2D / ScrollRect）</summary>
        public RectTransform Viewport { get; private set; }
        /// <summary>滚动内容容器（槽位父节点）</summary>
        public RectTransform Content { get; private set; }
        /// <summary>列数</summary>
        public int Cols { get { return grid.cols; } }
        /// <summary>可见行数</summary>
        public int VisibleRows { get { return grid.rows; } }
        /// <summary>单槽尺寸（面板单位）</summary>
        public Vector2 CellSize { get { return cellSize; } }

        /// <summary>
        /// 在 parent 下创建滚动网格区，几何与 IIPArtLayout.Grid 手绘网格逐格对齐。
        /// onGapClick：格子间隙点击的回调（通常与面板背景点击一致），可为 null。
        /// </summary>
        public ArtScrollGrid(Transform parent, string name, IIPArtLayout.Grid g,
            float imgW, float imgH, Vector2 panelSize, UnityAction onGapClick)
        {
            grid = g;
            cellSize = IIPArtLayout.ScaleSize(g.cellW, g.cellH, imgW, imgH, panelSize);
            pitch = IIPArtLayout.ScaleSize(g.pitchX, g.pitchY, imgW, imgH, panelSize);
            gridSize = new Vector2((g.cols - 1) * pitch.x + cellSize.x,
                                   (g.rows - 1) * pitch.y + cellSize.y);
            Vector2 topLeft = IIPArtLayout.ToPanel(g.originX, g.originY, imgW, imgH, panelSize);
            Vector2 center = topLeft + new Vector2(gridSize.x * 0.5f, -gridSize.y * 0.5f);

            // 视口：透明底接收滚轮/间隙点击 + RectMask2D 裁剪溢出槽位
            var vpGo = new GameObject($"Scroll_{name}",
                typeof(RectTransform), typeof(Image), typeof(RectMask2D), typeof(ScrollRect));
            vpGo.transform.SetParent(parent, false);
            Viewport = (RectTransform)vpGo.transform;
            Viewport.anchorMin = new Vector2(0.5f, 0.5f);
            Viewport.anchorMax = new Vector2(0.5f, 0.5f);
            Viewport.pivot = new Vector2(0.5f, 0.5f);
            Viewport.anchoredPosition = center;
            Viewport.sizeDelta = gridSize + new Vector2(ViewportPadding * 2f, ViewportPadding * 2f);
            var vpImg = vpGo.GetComponent<Image>();
            vpImg.color = new Color(1f, 1f, 1f, 0f);
            vpImg.raycastTarget = true;
            if (onGapClick != null)
            {
                var gapBtn = vpGo.AddComponent<Button>();
                gapBtn.transition = Selectable.Transition.None;
                gapBtn.targetGraphic = vpImg;
                gapBtn.onClick.AddListener(onGapClick);
            }

            // 内容：顶锚定，高度随行数增长（初始 = 视口高，不可滚动）
            var ctGo = new GameObject("Content", typeof(RectTransform));
            ctGo.transform.SetParent(Viewport, false);
            Content = (RectTransform)ctGo.transform;
            Content.anchorMin = new Vector2(0.5f, 1f);
            Content.anchorMax = new Vector2(0.5f, 1f);
            Content.pivot = new Vector2(0.5f, 1f);
            Content.anchoredPosition = Vector2.zero;
            Content.sizeDelta = Viewport.sizeDelta;

            var scrollRect = vpGo.GetComponent<ScrollRect>();
            scrollRect.viewport = Viewport;
            scrollRect.content = Content;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.inertia = false;      // 直接跟随滚轮，无惯性漂移
            scrollRect.scrollSensitivity = 30f; // 每格滚轮 ≈ 30px

            Debug.Log($"[ArtScrollGrid] 创建滚动网格 {name}：{g.cols}列×{g.rows}可见行，视口 {Viewport.sizeDelta}");
        }

        /// <summary>槽位 (col,row) 在 Content 下的锚定位置（槽位锚点须为顶中 (0.5,1)、轴心中心）。</summary>
        public Vector2 GetCellAnchoredPos(int col, int row)
        {
            float x = col * pitch.x + cellSize.x * 0.5f - gridSize.x * 0.5f;
            float y = -(ViewportPadding + row * pitch.y + cellSize.y * 0.5f);
            return new Vector2(x, y);
        }

        /// <summary>设置内容总行数（不少于可见行数），高度自适应并立即钳制滚动位置。</summary>
        public void SetTotalRows(int totalRows)
        {
            int rows = Mathf.Max(totalRows, grid.rows);
            float h = (rows - 1) * pitch.y + cellSize.y + ViewportPadding * 2f;
            Content.sizeDelta = new Vector2(Content.sizeDelta.x, h);

            // 内容缩短后立即钳制回有效范围，避免一帧越界（Clamped 下一帧也会兜底）
            float maxOffset = Mathf.Max(0f, h - Viewport.sizeDelta.y);
            Vector2 pos = Content.anchoredPosition;
            pos.y = Mathf.Clamp(pos.y, 0f, maxOffset);
            Content.anchoredPosition = pos;
        }

        /// <summary>滚动回顶部（面板打开时可调用）。</summary>
        public void ResetToTop()
        {
            Content.anchoredPosition = Vector2.zero;
        }
    }
}
