using UnityEngine;
using UnityEngine.UI;

namespace IIPUI
{
    /// <summary>
    /// 共享 UI 构造工厂 —— 统一 HUD/面板的视觉原语。
    /// 所有"代码生成的 UI"通过本工厂创建，保证圆角 9-slice 背景 + 边框 + 雅黑标签 + 悬停反馈一致。
    /// 配色统一引用 IIPUIStyle（样式 Token 单一数据源），字体复用 IIPUIFont。
    /// </summary>
    public static class IIPUIFactory
    {
        // ── 配色（紫调暗夜）—— 色板唯一数据源在 IIPUIStyle，此处仅保留兼容别名 ──
        public static readonly Color PanelBg      = IIPUIStyle.PanelBackground;
        public static readonly Color ContentBg    = IIPUIStyle.ContentBackground;
        public static readonly Color SlotBg       = IIPUIStyle.SlotBackground;
        public static readonly Color SlotBgEmpty  = IIPUIStyle.SlotBackgroundEmpty;
        public static readonly Color ButtonNormal = IIPUIStyle.ButtonNormal;
        public static readonly Color ButtonHover  = IIPUIStyle.ButtonHover;
        public static readonly Color TabActive    = IIPUIStyle.TabActive;
        public static readonly Color TabInactive  = IIPUIStyle.TabInactive;
        public static readonly Color BorderDim    = IIPUIStyle.BorderDim;
        public static readonly Color BorderBright = IIPUIStyle.BorderBright;
        public static readonly Color AccentGold   = IIPUIStyle.AccentGold;
        public static readonly Color AccentPurple = IIPUIStyle.AccentPurple;
        public static readonly Color AccentCyan   = IIPUIStyle.AccentCyan;
        public static readonly Color AccentGreen  = IIPUIStyle.AccentGreen;
        public static readonly Color AccentRed    = IIPUIStyle.AccentRed;

        // ── 文字色（别名）──
        public static readonly Color TextTitle = IIPUIStyle.TextTitle;
        public static readonly Color TextMain  = IIPUIStyle.TextPrimary;
        public static readonly Color TextDim   = IIPUIStyle.TextSecondary;
        public static readonly Color TextKey   = IIPUIStyle.TextKey;

        // 字号约定（别名，唯一数据源在 IIPUIStyle）
        public const int SizeTitle  = IIPUIStyle.FontSizeTitle;
        public const int SizeButton = IIPUIStyle.FontSizeButton;
        public const int SizeLabel  = IIPUIStyle.FontSizeLabel;
        public const int SizeSmall  = IIPUIStyle.FontSizeSmall;
        public const int SizeKey    = IIPUIStyle.FontSizeKey;

        static Sprite _rounded;
        static bool _roundedChecked; // 避免每次 ApplyRounded 都重复打 Warning
        /// <summary>
        /// 圆角 9-slice Sprite。
        /// 优先用 Resources/UI/MenuRoundedCorner，但该资源若为低分辨率（&lt;64px）或未设 9-slice border，
        /// sliced 拉伸时圆角会退化成模糊锯齿，此时改用程序化生成的 128×128 高质量圆角（半径 20px + 1.2px 抗锯齿 + 正确 border）。
        /// </summary>
        public static Sprite RoundedSprite
        {
            get
            {
                if (_rounded == null)
                {
                    _rounded = Resources.Load<Sprite>("UI/MenuRoundedCorner");
                    // 资源缺失或质量不达标（无 border / 太小）时，程序化生成高质量圆角，根治"圆角锯齿"
                    if (_rounded == null || _rounded.border == Vector4.zero || _rounded.rect.width < 64f)
                    {
                        if (_rounded == null && !_roundedChecked)
                        {
                            _roundedChecked = true;
                            Debug.LogWarning("[IIPUIFactory] 未找到 Resources/UI/MenuRoundedCorner 圆角 Sprite，改用程序化生成圆角。");
                        }
                        _rounded = GenerateRoundedRectSprite(128, 20f, 1.2f);
                    }
                }
                return _rounded;
            }
            set { _rounded = value; _roundedChecked = value != null; }
        }

        /// <summary>
        /// 程序化生成圆角矩形 9-slice Sprite（SDF 签名距离场 + 边缘抗锯齿）。
        /// size: 纹理边长；radius: 圆角半径（像素）；aa: 抗锯齿过渡宽度（像素）。
        /// border 设为 radius 四边，sliced 拉伸时四角不变形。
        /// </summary>
        public static Sprite GenerateRoundedRectSprite(int size, float radius, float aa)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            var px = new Color32[size * size];
            float half = size * 0.5f;
            float r = Mathf.Min(radius, half - 1f);
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                // 点到内部矩形（收缩 r 后的中心区域）的距离，即圆角 SDF
                float dx = Mathf.Max(Mathf.Abs(x + 0.5f - half) - (half - r), 0f);
                float dy = Mathf.Max(Mathf.Abs(y + 0.5f - half) - (half - r), 0f);
                float d = Mathf.Sqrt(dx * dx + dy * dy) - r;
                // d<0 在形内；边缘 aa 宽度线性过渡
                float a = Mathf.Clamp01(0.5f - d / aa);
                px[y * size + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(a * 255f));
            }
            tex.SetPixels32(px);
            tex.Apply(false, false);
            int b = Mathf.CeilToInt(r);
            var sp = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f,
                0, SpriteMeshType.FullRect, new Vector4(b, b, b, b));
            sp.name = "IIP_RoundedRect_Gen";
            return sp;
        }

        static Sprite _checkmark;
        /// <summary>
        /// 程序化"勾形" Sprite（64×64，两笔划线 + 抗锯齿）。
        /// 用于 Toggle 的 Checkmark，替代实心方块，让开关状态一目了然。
        /// </summary>
        public static Sprite CheckmarkSprite
        {
            get
            {
                if (_checkmark == null)
                {
                    const int N = 64;
                    var tex = new Texture2D(N, N, TextureFormat.RGBA32, false);
                    tex.wrapMode = TextureWrapMode.Clamp;
                    tex.filterMode = FilterMode.Bilinear;
                    var px = new Color32[N * N];
                    // 勾形的三个拐点（归一化坐标，左下原点）
                    Vector2 p0 = new Vector2(0.20f, 0.52f);
                    Vector2 p1 = new Vector2(0.42f, 0.28f);
                    Vector2 p2 = new Vector2(0.82f, 0.72f);
                    const float thickness = 0.09f; // 笔划半宽（归一化）
                    for (int y = 0; y < N; y++)
                    for (int x = 0; x < N; x++)
                    {
                        var p = new Vector2((x + 0.5f) / N, (y + 0.5f) / N);
                        float d = Mathf.Min(DistToSegment(p, p0, p1), DistToSegment(p, p1, p2));
                        float a = Mathf.Clamp01((thickness - d) / 0.03f);
                        px[y * N + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(a * 255f));
                    }
                    tex.SetPixels32(px);
                    tex.Apply(false, false);
                    _checkmark = Sprite.Create(tex, new Rect(0, 0, N, N), new Vector2(0.5f, 0.5f), 100f);
                    _checkmark.name = "IIP_Checkmark";
                }
                return _checkmark;
            }
        }

        static float DistToSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude);
            return Vector2.Distance(p, a + ab * t);
        }

        /// <summary>把 Toggle 的勾选图形换成勾形 sprite（Simple 模式，颜色由调用方定）。</summary>
        public static void ApplyCheckmark(Image img)
        {
            if (img == null) return;
            img.sprite = CheckmarkSprite;
            img.type = Image.Type.Simple;
            img.preserveAspect = true;
        }

        /// <summary>应用圆角 9-slice sprite 到 Image（sliced 模式可任意拉伸不变形）。</summary>
        public static void ApplyRounded(Image img, bool sliced = true)
        {
            if (img == null) return;
            var sp = RoundedSprite;
            if (sp != null)
            {
                img.sprite = sp;
                img.type = sliced ? Image.Type.Sliced : Image.Type.Simple;
            }
            // sp==null 时保持 Image 默认白色 Sprite（已在 RoundedSprite getter 中警告过）
        }

        static Sprite _radialGlow;
        /// <summary>
        /// 径向椭圆辉光 Sprite（运行时程序化生成，128×128，中心实、边缘 (1-t)² 衰减）。
        /// 用于菱形/异形交互区的"光晕汇聚"效果（炼金篮、书本金辉），比圆角矩形更贴合异形轮廓。
        /// </summary>
        public static Sprite RadialGlowSprite
        {
            get
            {
                if (_radialGlow == null)
                {
                    const int N = 128;
                    var tex = new Texture2D(N, N, TextureFormat.RGBA32, false);
                    tex.wrapMode = TextureWrapMode.Clamp;
                    tex.filterMode = FilterMode.Bilinear;
                    var px = new Color32[N * N];
                    float half = N * 0.5f;
                    for (int y = 0; y < N; y++)
                    for (int x = 0; x < N; x++)
                    {
                        float dx = (x + 0.5f - half) / half;
                        float dy = (y + 0.5f - half) / half;
                        float d = Mathf.Sqrt(dx * dx + dy * dy);
                        float a = d >= 1f ? 0f : (1f - d) * (1f - d);
                        px[y * N + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(a * 255f));
                    }
                    tex.SetPixels32(px);
                    tex.Apply(false, false);
                    _radialGlow = Sprite.Create(tex, new Rect(0, 0, N, N), new Vector2(0.5f, 0.5f), 100f);
                    _radialGlow.name = "IIP_RadialGlow";
                }
                return _radialGlow;
            }
        }

        /// <summary>应用径向辉光 sprite 到 Image（Simple 模式，随 RectTransform 拉伸成椭圆）。</summary>
        public static void ApplyRadialGlow(Image img)
        {
            if (img == null) return;
            img.sprite = RadialGlowSprite;
            img.type = Image.Type.Simple;
        }

        static Sprite _ring;
        /// <summary>
        /// 程序化空心圆环 Sprite（128×128，环带 r∈[0.80,0.97]，边缘 1.5px 抗锯齿）。
        /// 配合 Image.Type.Filled + Radial360 使用：fillAmount=1 时只显示圆环而非实心方块，
        /// 用于闪避冷却环形指示器等"环形进度"场景。
        /// </summary>
        public static Sprite RingSprite
        {
            get
            {
                if (_ring == null)
                {
                    const int N = 128;
                    var tex = new Texture2D(N, N, TextureFormat.RGBA32, false);
                    tex.wrapMode = TextureWrapMode.Clamp;
                    tex.filterMode = FilterMode.Bilinear;
                    var px = new Color32[N * N];
                    float half = N * 0.5f;
                    const float inner = 0.80f, outer = 0.97f, aa = 0.025f;
                    for (int y = 0; y < N; y++)
                    for (int x = 0; x < N; x++)
                    {
                        float dx = (x + 0.5f - half) / half;
                        float dy = (y + 0.5f - half) / half;
                        float d = Mathf.Sqrt(dx * dx + dy * dy);
                        // 环带内外缘各做一段线性衰减抗锯齿
                        float aIn = Mathf.Clamp01((d - inner) / aa);
                        float aOut = Mathf.Clamp01((outer - d) / aa);
                        float a = Mathf.Min(aIn, aOut);
                        px[y * N + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(a * 255f));
                    }
                    tex.SetPixels32(px);
                    tex.Apply(false, false);
                    _ring = Sprite.Create(tex, new Rect(0, 0, N, N), new Vector2(0.5f, 0.5f), 100f);
                    _ring.name = "IIP_Ring";
                }
                return _ring;
            }
        }

        /// <summary>应用空心圆环 sprite 到 Image（Simple 模式；调用方自行切换 Filled+Radial360 做冷却扫圈）。</summary>
        public static void ApplyRing(Image img)
        {
            if (img == null) return;
            img.sprite = RingSprite;
        }

        /// <summary>创建带圆角背景的 Image 节点（铺满父节点）。</summary>
        public static Image CreateRoundedImage(string name, Transform parent, Color color, bool sliced = true)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = color;
            ApplyRounded(img, sliced);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return img;
        }

        /// <summary>创建一个给定尺寸、居中锚点的圆角 Image。</summary>
        public static Image CreateRoundedBox(string name, Transform parent, Vector2 size, Color color, bool sliced = true)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = color;
            ApplyRounded(img, sliced);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            return img;
        }

        /// <summary>在父节点上叠加一圈描边边框（子 Image，铺满父）。</summary>
        public static Image CreateBorder(Transform parent, Color color, bool rounded = true)
        {
            var go = new GameObject("Border", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = color;
            if (rounded) ApplyRounded(img, true);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return img;
        }

        /// <summary>创建雅黑文本节点（铺满父，居中）。fontSize/alignment 可调。</summary>
        public static Text CreateLabel(string name, Transform parent, string text, int fontSize, Color color,
            TextAnchor alignment = TextAnchor.MiddleCenter, FontStyle style = FontStyle.Normal)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<Text>();
            t.font = IIPUIFont.Get();
            t.text = text;
            t.fontSize = fontSize;
            t.alignment = alignment;
            t.color = color;
            t.fontStyle = style;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return t;
        }

        /// <summary>在指定锚点/偏移创建文本节点（用于键位、数量等角落文字）。</summary>
        public static Text CreateLabelAnchored(string name, Transform parent, string text, int fontSize, Color color,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax,
            TextAnchor alignment = TextAnchor.MiddleCenter)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<Text>();
            t.font = IIPUIFont.Get();
            t.text = text;
            t.fontSize = fontSize;
            t.alignment = alignment;
            t.color = color;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
            return t;
        }

        /// <summary>给 GameObject 挂 HoverButton（悬停高亮+缩放），返回组件。</summary>
        public static HoverButton ApplyHover(GameObject go, Color? hoverColor = null, float scale = 1.06f)
        {
            var hb = go.GetComponent<HoverButton>();
            if (hb == null) hb = go.AddComponent<HoverButton>();
            hb.hoverColor = hoverColor ?? ButtonHover;
            hb.hoverScale = scale;
            return hb;
        }

        /// <summary>
        /// 统一面板根外观（场景序列化的旧面板换肤用）：深底圆角 + 底层低调边框。
        /// 只改视觉，不动布局与事件。边框节点名 IIPBorder，重复调用幂等。
        /// </summary>
        public static void StylePanelRoot(GameObject panel)
        {
            if (panel == null) return;
            var img = panel.GetComponent<Image>();
            if (img == null) return;
            img.color = PanelBg;
            ApplyRounded(img, true);
            if (panel.transform.Find("IIPBorder") == null)
            {
                var border = CreateBorder(panel.transform, BorderDim, true);
                border.gameObject.name = "IIPBorder";
                border.transform.SetAsFirstSibling(); // 渲染在内容之下
                border.raycastTarget = false;
            }
        }

        /// <summary>
        /// 统一按钮外观（场景序列化的旧按钮换肤用）：ButtonNormal 圆角底 + 紫辉光 hover + 雅黑标签。
        /// 只改视觉，不动布局与已绑定的 onClick。
        /// </summary>
        public static void StyleButton(Button btn)
        {
            if (btn == null) return;
            var img = btn.targetGraphic as Image;
            if (img == null) img = btn.GetComponent<Image>();
            if (img != null)
            {
                img.color = ButtonNormal;
                ApplyRounded(img, true);
            }
            btn.transition = Selectable.Transition.None; // 交由 HoverButton 做紫辉光反馈
            ApplyHover(btn.gameObject, ButtonHover);
            var label = btn.GetComponentInChildren<Text>(true);
            if (label != null)
            {
                label.font = IIPUIFont.Get();
                label.color = TextMain;
                label.fontSize = SizeButton;
            }
        }

        /// <summary>
        /// 创建统一外观的槽位节点（圆角底 + 边框 + 可选居中标签 + 可选角落键位）。
        /// 返回根 GameObject，调用方可继续在内部加 Icon/CooldownMask 等子节点。
        /// </summary>
        public static GameObject MakeSlot(string name, Transform parent, Vector2 size, Color bgColor,
            string centerLabel = null, string keyLabel = null, bool border = true)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = bgColor;
            ApplyRounded(img, true);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;

            if (border) CreateBorder(go.transform, BorderDim, true);
            if (centerLabel != null)
                CreateLabel("Label", go.transform, centerLabel, SizeLabel, TextDim);
            if (keyLabel != null)
                CreateLabelAnchored("Key", go.transform, keyLabel, SizeKey, TextKey,
                    new Vector2(0, 0.72f), new Vector2(0.34f, 1f),
                    new Vector2(3, 0), new Vector2(0, -2), TextAnchor.UpperLeft);
            return go;
        }
    }
}
