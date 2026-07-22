using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Text;

namespace IIPUI
{
    /// <summary>
    /// 一次性菜单 UI 视觉打磨工具。
    /// 菜单项：IIP / 打磨菜单UI视觉
    /// 幂等：重复执行无副作用，所有改动都是 SetValue。
    /// 处理 Forest.unity 与 MainMenu.unity 两个场景里的 PauseMenu 子树（含 SettingsPanel）。
    /// </summary>
    public static class MenuVisualPolishEditor
    {
        // ── 配色（紫调暗夜，呼应"幻宫"主题）—— 统一引用 IIPUIStyle 单一数据源 ──
        static readonly Color COL_PANEL_BG       = IIPUIStyle.PanelBackground;
        static readonly Color COL_BUTTON_NORMAL  = IIPUIStyle.ButtonNormal;
        static readonly Color COL_BUTTON_HOVER   = IIPUIStyle.ButtonHover;
        static readonly Color COL_TAB_ACTIVE     = IIPUIStyle.TabActive;
        static readonly Color COL_TAB_INACTIVE   = IIPUIStyle.TabInactive;
        static readonly Color COL_KEYDISPLAY     = IIPUIStyle.KeyDisplayBackground;
        static readonly Color COL_CONTENT_BG     = IIPUIStyle.ContentBackground;

        // ── 文字色 ──
        static readonly Color COL_TITLE     = IIPUIStyle.TextTitle;
        static readonly Color COL_TEXT_MAIN = IIPUIStyle.TextPrimary;
        static readonly Color COL_TEXT_DIM  = IIPUIStyle.TextSecondary;
        static readonly Color COL_TEXT_VAL  = IIPUIStyle.TextValue;
        static readonly Color COL_TEXT_KEY  = IIPUIStyle.TextKey;

        // ── 字号 ──
        const int SIZE_TITLE_PAUSE = 32;
        const int SIZE_TITLE_SET   = 28;
        const int SIZE_BUTTON      = 22;
        const int SIZE_TAB         = 20;
        const int SIZE_LABEL       = 20;
        const int SIZE_VALUE       = 20;
        const int SIZE_KEY         = 18;

        static Sprite _roundedSprite;

        [MenuItem("IIP/打磨菜单UI视觉")]
        static void PolishAll()
        {
            EnsureRoundedSprite();
            string[] scenes = { "Assets/Scenes/Forest.unity", "Assets/Scenes/MainMenu.unity" };
            foreach (var path in scenes)
            {
                Debug.Log($"[MenuPolish] 处理场景: {path}");
                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

                // 1) 修复历史生成器写入的错误码点（操作→U+64CC, ？→U+FF3F 等）
                FixCorruptedCodePoints(scene);

                // 2) 打磨所有 PauseMenu（Forest 里有）
                var pauseMenus = UnityEngine.Object.FindObjectsOfType<PauseMenu>(true);
                foreach (var pm in pauseMenus)
                {
                    PolishPauseMenu(pm);
                    PolishSettingsPanel(pm);
                    IIPUIFont.ApplyTo(pm.transform);
                }

                // 3) 打磨独立 SettingsPanel（MainMenu 里 SettingsPanel 不挂在 PauseMenu 下）
                var standaloneSPs = UnityEngine.Object.FindObjectsOfType<SettingsPanelController>(true);
                foreach (var spc in standaloneSPs)
                {
                    PolishStandaloneSettingsPanel(spc);
                    IIPUIFont.ApplyTo(spc.transform);
                }

                // 4) 打磨主菜单面板按钮（MainMenu 专属）
                PolishMainMenuPanel();

                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log($"[MenuPolish] 已保存: {path}, PauseMenu={pauseMenus.Length}, 独立SettingsPanel={standaloneSPs.Length}");
            }
            // 重开 Forest 作为活动场景
            EditorSceneManager.OpenScene("Assets/Scenes/Forest.unity", OpenSceneMode.Single);
            AssetDatabase.Refresh();
            Debug.Log("[MenuPolish] 全部完成。");
        }

        // 修复历史生成器写入的错误 Unicode 码点。
        // 根因：旧版代码生成器把"操作"写成了 U+64CC U+4F5A（应为 U+64CD U+4F5C），
        // "？"写成了 U+FF3F（应为 U+FF1F）。字符级替换修所有出现处。
        static void FixCorruptedCodePoints(UnityEngine.SceneManagement.Scene scene)
        {
            // 损坏码点 → 正确字符
            var fixMap = new System.Collections.Generic.Dictionary<char, char>
            {
                { '擌', '操' },  // U+64CC → U+64CD 操
                { '佚', '作' },  // U+4F5A → U+4F5C 作
                { '＿', '？' },  // U+FF3F → U+FF1F ？
            };
            int fixedCount = 0;
            foreach (var go in scene.GetRootGameObjects())
            {
                foreach (var t in go.GetComponentsInChildren<Text>(true))
                {
                    if (t == null || string.IsNullOrEmpty(t.text)) continue;
                    var sb = new StringBuilder(t.text.Length);
                    bool changed = false;
                    foreach (var ch in t.text)
                    {
                        if (fixMap.TryGetValue(ch, out var fixedCh))
                        {
                            sb.Append(fixedCh);
                            changed = true;
                        }
                        else sb.Append(ch);
                    }
                    if (changed)
                    {
                        Debug.Log($"[MenuPolish] 修复码点: '{t.text}' -> '{sb}' @ {GetPath(t.transform)}");
                        t.text = sb.ToString();
                        EditorUtility.SetDirty(t);
                        fixedCount++;
                    }
                }
            }
            if (fixedCount > 0) Debug.Log($"[MenuPolish] 共修复 {fixedCount} 处错误码点文本");
        }

        static string GetPath(Transform t)
        {
            if (t.parent == null) return t.name;
            return GetPath(t.parent) + "/" + t.name;
        }

        // ══════════════════════════════════════════
        // 暂停菜单
        // ═══════════════════════════════════════════

        static void PolishPauseMenu(PauseMenu pm)
        {
            var panelT = typeof(PauseMenu).GetField("pausePanel",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var pausePanel = panelT?.GetValue(pm) as GameObject;
            if (pausePanel != null)
            {
                // 面板背景
                SetImageColor(pausePanel, COL_PANEL_BG);
                SetImageSprite(pausePanel, _roundedSprite, sliced: true);
                SetPaddingAndSize(pausePanel, new RectOffset(28, 28, 28, 28));

                // 标题
                var title = pausePanel.transform.Find("Content/Title");
                if (title != null) StyleText(title, SIZE_TITLE_PAUSE, COL_TITLE, FontStyle.Bold);

                // 按钮
                foreach (var btnName in new[] { "ResumeButton", "SettingsButton", "MainMenuButton", "QuitButton" })
                {
                    var btn = pausePanel.transform.Find("Content/" + btnName);
                    if (btn == null) continue;
                    SetImageColor(btn.gameObject, COL_BUTTON_NORMAL);
                    SetImageSprite(btn.gameObject, _roundedSprite, sliced: true);
                    var hover = btn.GetComponent<HoverButton>();
                    if (hover != null) hover.hoverColor = COL_BUTTON_HOVER;
                    var txt = btn.Find("Text");
                    if (txt != null) StyleText(txt, SIZE_BUTTON, COL_TEXT_MAIN, FontStyle.Normal);
                }
            }

            // 确认对话框
            var confirmT = typeof(PauseMenu).GetField("confirmDialogPanel",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var confirm = confirmT?.GetValue(pm) as GameObject;
            if (confirm != null)
            {
                SetImageColor(confirm, COL_PANEL_BG);
                SetImageSprite(confirm, _roundedSprite, sliced: true);
                var dt = confirm.transform.Find("DialogText");
                if (dt != null) StyleText(dt, SIZE_LABEL, COL_TEXT_MAIN, FontStyle.Normal);
                foreach (var bn in new[] { "ButtonGroup/ConfirmButton", "ButtonGroup/CancelButton" })
                {
                    var b = confirm.transform.Find(bn);
                    if (b == null) continue;
                    SetImageColor(b.gameObject, COL_BUTTON_NORMAL);
                    SetImageSprite(b.gameObject, _roundedSprite, sliced: true);
                    var hov = b.GetComponent<HoverButton>();
                    if (hov != null) hov.hoverColor = COL_BUTTON_HOVER;
                    var t = b.Find("Text");
                    if (t != null) StyleText(t, SIZE_BUTTON, COL_TEXT_MAIN, FontStyle.Normal);
                }
            }
        }

        // ══════════════════════════════════════════
        // 设置面板
        // ═══════════════════════════════════════════

        static void PolishSettingsPanel(PauseMenu pm)
        {
            var spT = typeof(PauseMenu).GetField("settingsPanel",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var sp = spT?.GetValue(pm) as GameObject;
            if (sp == null) return;

            // 面板背景
            SetImageColor(sp, COL_PANEL_BG);
            SetImageSprite(sp, _roundedSprite, sliced: true);

            // 标题
            var tt = sp.transform.Find("TitleText");
            if (tt != null) StyleText(tt, SIZE_TITLE_SET, COL_TITLE, FontStyle.Bold);

            // Tab 容器与按钮
            var tc = sp.transform.Find("TabContainer");
            if (tc != null)
            {
                var hlg = EnsureComponent<HorizontalLayoutGroup>(tc.gameObject);
                hlg.spacing = 8;
                hlg.padding = new RectOffset(0, 0, 0, 0);
                hlg.childAlignment = TextAnchor.UpperLeft;
                hlg.childControlWidth = true;
                hlg.childControlHeight = true;
                hlg.childForceExpandWidth = true;
                hlg.childForceExpandHeight = false;
                foreach (Transform tab in tc)
                {
                    SetImageSprite(tab.gameObject, _roundedSprite, sliced: true);
                    // 颜色交给 SettingsPanelController 运行时按激活态设；这里先给未激活色作初值
                    SetImageColor(tab.gameObject, COL_TAB_INACTIVE);
                    var le = EnsureComponent<LayoutElement>(tab.gameObject);
                    le.preferredHeight = 44;
                    le.minHeight = 40;
                    var txt = tab.Find("Text");
                    if (txt != null) StyleText(txt, SIZE_TAB, COL_TEXT_DIM, FontStyle.Normal);
                }
            }

            // 内容区
            var ca = sp.transform.Find("ContentArea");
            if (ca != null)
            {
                SetImageColor(ca.gameObject, COL_CONTENT_BG);
                SetImageSprite(ca.gameObject, _roundedSprite, sliced: true);
                PolishContentTab(ca, "AudioVideoContent");
                PolishContentTab(ca, "ControlsContent");
                PolishContentTab(ca, "AssistContent");
            }

            // 返回按钮
            var bb = sp.transform.Find("BackButton");
            if (bb != null)
            {
                SetImageColor(bb.gameObject, COL_BUTTON_NORMAL);
                SetImageSprite(bb.gameObject, _roundedSprite, sliced: true);
                var hov = bb.GetComponent<HoverButton>();
                if (hov != null) hov.hoverColor = COL_BUTTON_HOVER;
                var txt = bb.Find("Text");
                if (txt != null) StyleText(txt, SIZE_BUTTON, COL_TEXT_MAIN, FontStyle.Normal);
            }
        }

        // MainMenu 场景里独立的 SettingsPanel（挂在 Canvas/SettingsPanel/SettingsCard 上，不在 PauseMenu 下）
        static void PolishStandaloneSettingsPanel(SettingsPanelController spc)
        {
            var card = spc.transform; // SettingsPanel 根上挂 Image + Controller，但实际内容在 SettingsCard 子节点？
            // 探测结构：SettingsPanel 根 / SettingsCard / {TitleText, TabContainer, ContentArea, BackButton}
            Transform root = spc.transform;
            Transform cardT = root.Find("SettingsCard");
            Transform container = cardT != null ? cardT : root;

            SetImageColor(root.gameObject, COL_PANEL_BG);
            SetImageSprite(root.gameObject, _roundedSprite, sliced: true);

            var tt = container.Find("TitleText");
            if (tt != null) StyleText(tt, SIZE_TITLE_SET, COL_TITLE, FontStyle.Bold);

            var tc = container.Find("TabContainer");
            if (tc != null)
            {
                var hlg = EnsureComponent<HorizontalLayoutGroup>(tc.gameObject);
                hlg.spacing = 8;
                hlg.padding = new RectOffset(0, 0, 0, 0);
                hlg.childAlignment = TextAnchor.UpperLeft;
                hlg.childControlWidth = true;
                hlg.childControlHeight = true;
                hlg.childForceExpandWidth = true;
                hlg.childForceExpandHeight = false;
                foreach (Transform tab in tc)
                {
                    SetImageSprite(tab.gameObject, _roundedSprite, sliced: true);
                    SetImageColor(tab.gameObject, COL_TAB_INACTIVE);
                    var le = EnsureComponent<LayoutElement>(tab.gameObject);
                    le.preferredHeight = 44;
                    le.minHeight = 40;
                    var txt = tab.Find("Text");
                    if (txt != null) StyleText(txt, SIZE_TAB, COL_TEXT_DIM, FontStyle.Normal);
                }
            }

            var ca = container.Find("ContentArea");
            if (ca != null)
            {
                SetImageColor(ca.gameObject, COL_CONTENT_BG);
                SetImageSprite(ca.gameObject, _roundedSprite, sliced: true);
                PolishContentTab(ca, "AudioVideoContent");
                PolishContentTab(ca, "ControlsContent");
                PolishContentTab(ca, "AssistContent");
            }

            var bb = container.Find("BackButton");
            if (bb != null)
            {
                SetImageColor(bb.gameObject, COL_BUTTON_NORMAL);
                SetImageSprite(bb.gameObject, _roundedSprite, sliced: true);
                var hov = bb.GetComponent<HoverButton>();
                if (hov != null) hov.hoverColor = COL_BUTTON_HOVER;
                var txt = bb.Find("Text");
                if (txt != null) StyleText(txt, SIZE_BUTTON, COL_TEXT_MAIN, FontStyle.Normal);
            }
        }

        // MainMenu 主菜单面板按钮（Button_NewGame/Continue/Settings/Quit）+ 标题
        static void PolishMainMenuPanel()
        {
            var mmp = GameObject.Find("MainMenuPanel");
            if (mmp == null) return;
            var mmpImg = mmp.GetComponent<Image>();
            if (mmpImg != null)
            {
                mmpImg.color = COL_PANEL_BG;
                SetImageSprite(mmp, _roundedSprite, sliced: true);
            }
            // 主菜单标题
            foreach (Transform c in mmp.transform)
            {
                var t = c.GetComponent<Text>();
                if (t != null && c.name.ToLowerInvariant().Contains("title"))
                    StyleText(c, SIZE_TITLE_PAUSE, COL_TITLE, FontStyle.Bold);
            }
            // 按钮容器
            var bc = mmp.transform.Find("ButtonContainer");
            var buttonRoot = bc != null ? bc : mmp.transform;
            foreach (Transform btn in buttonRoot)
            {
                var btnImg = btn.GetComponent<Image>();
                if (btnImg != null)
                {
                    btnImg.color = COL_BUTTON_NORMAL;
                    SetImageSprite(btn.gameObject, _roundedSprite, sliced: true);
                }
                var hov = btn.GetComponent<HoverButton>();
                if (hov != null) hov.hoverColor = COL_BUTTON_HOVER;
                var txt = btn.GetComponentInChildren<Text>(true);
                if (txt != null) StyleText(txt.transform, SIZE_BUTTON, COL_TEXT_MAIN, FontStyle.Normal);
            }

            // MainMenu 的退出确认面板
            var qcp = GameObject.Find("QuitConfirmPanel");
            if (qcp != null)
            {
                SetImageColor(qcp, COL_PANEL_BG);
                SetImageSprite(qcp, _roundedSprite, sliced: true);
                foreach (Transform c in qcp.transform)
                {
                    var t = c.GetComponent<Text>();
                    if (t != null) StyleText(c, SIZE_LABEL, COL_TEXT_MAIN, FontStyle.Normal);
                    var btn = c.GetComponent<Button>();
                    if (btn != null)
                    {
                        SetImageColor(c.gameObject, COL_BUTTON_NORMAL);
                        SetImageSprite(c.gameObject, _roundedSprite, sliced: true);
                        var hov = c.GetComponent<HoverButton>();
                        if (hov != null) hov.hoverColor = COL_BUTTON_HOVER;
                        var btxt = c.GetComponentInChildren<Text>(true);
                        if (btxt != null) StyleText(btxt.transform, SIZE_BUTTON, COL_TEXT_MAIN, FontStyle.Normal);
                    }
                }
            }
        }

        static void PolishContentTab(Transform contentArea, string tabName)
        {
            var content = contentArea.Find(tabName);
            if (content == null) return;

            // 用 VerticalLayoutGroup 统一排列行
            var vlg = EnsureComponent<VerticalLayoutGroup>(content.gameObject);
            vlg.spacing = 10;
            vlg.padding = new RectOffset(16, 16, 16, 16);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var csf = EnsureComponent<ContentSizeFitter>(content.gameObject);
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            foreach (Transform row in content)
            {
                var le = EnsureComponent<LayoutElement>(row.gameObject);
                le.preferredHeight = 40;
                le.minHeight = 36;

                var hlg = EnsureComponent<HorizontalLayoutGroup>(row.gameObject);
                hlg.spacing = 12;
                hlg.padding = new RectOffset(0, 0, 0, 0);
                hlg.childAlignment = TextAnchor.MiddleLeft;
                hlg.childControlWidth = false;
                hlg.childControlHeight = true;
                hlg.childForceExpandWidth = false;
                hlg.childForceExpandHeight = true;

                foreach (Transform child in row)
                {
                    var t = child.GetComponent<Text>();
                    var s = child.GetComponent<Slider>();
                    var tg = child.GetComponent<Toggle>();
                    var img = child.GetComponent<Image>();
                    if (t != null)
                    {
                        // Label 或 Value
                        if (child.name == "Label")
                            StyleText(child, SIZE_LABEL, COL_TEXT_MAIN, FontStyle.Normal);
                        else if (child.name == "Value")
                        {
                            StyleText(child, SIZE_VALUE, COL_TEXT_VAL, FontStyle.Normal);
                            // 填初始占位百分比，避免空白
                            if (string.IsNullOrEmpty(t.text)) t.text = "80%";
                        }
                        else
                            StyleText(child, SIZE_LABEL, COL_TEXT_MAIN, FontStyle.Normal);
                    }
                    else if (s != null)
                    {
                        var sle = EnsureComponent<LayoutElement>(child.gameObject);
                        sle.preferredWidth = 220;
                        sle.flexibleWidth = 1;
                    }
                    else if (tg != null)
                    {
                        // 给 Toggle 一个最小宽度，避免挤压
                        var sle = EnsureComponent<LayoutElement>(child.gameObject);
                        sle.minWidth = 32;
                    }
                    else if (img != null && child.name == "KeyDisplay")
                    {
                        // 控制页键位显示框
                        img.color = COL_KEYDISPLAY;
                        SetImageSprite(child.gameObject, _roundedSprite, sliced: true);
                        var kle = EnsureComponent<LayoutElement>(child.gameObject);
                        kle.preferredWidth = 90;
                        kle.minWidth = 60;
                        var kt = child.GetComponentInChildren<Text>();
                        if (kt != null) StyleText(kt.transform, SIZE_KEY, COL_TEXT_KEY, FontStyle.Bold);
                    }
                }
            }
        }

        // ══════════════════════════════════════════
        // 工具方法
        // ═══════════════════════════════════════════

        static void StyleText(Transform tr, int size, Color color, FontStyle style)
        {
            var t = tr.GetComponent<Text>();
            if (t == null) return;
            t.font = IIPUIFont.Get();
            t.fontSize = size;
            t.color = color;
            t.fontStyle = style;
            t.alignment = TextAnchor.MiddleCenter;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            EditorUtility.SetDirty(t);
        }

        static void SetImageColor(GameObject go, Color color)
        {
            var img = go.GetComponent<Image>();
            if (img == null) return;
            img.color = color;
            EditorUtility.SetDirty(img);
        }

        static void SetImageSprite(GameObject go, Sprite sprite, bool sliced)
        {
            if (sprite == null) return;
            var img = go.GetComponent<Image>();
            if (img == null) return;
            img.sprite = sprite;
            if (sliced)
            {
                img.type = Image.Type.Sliced;
                img.pixelsPerUnitMultiplier = 2f;
            }
            EditorUtility.SetDirty(img);
        }

        static void SetPaddingAndSize(GameObject go, RectOffset padding)
        {
            // 给面板加/刷新 VerticalLayoutGroup 做内边距
            var vlg = EnsureComponent<VerticalLayoutGroup>(go);
            vlg.padding = padding;
            vlg.spacing = 12;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
        }

        static T EnsureComponent<T>(GameObject go) where T : Component
        {
            var c = go.GetComponent<T>();
            if (c == null) c = go.AddComponent<T>();
            return c;
        }

        // ── 生成共享圆角 Sprite ──
        static void EnsureRoundedSprite()
        {
            const string path = "Assets/ArtMaterials/UI/MenuRoundedCorner.png";
            const string metaColor = "#FFFFFFFF";
            _roundedSprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (_roundedSprite != null) return;

            // 生成 32x32 圆角白底（圆角半径 8），9-slice 友好
            int size = 32;
            int radius = 8;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            var px = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // 到四角的距离
                    float dx = Mathf.Min(x, size - 1 - x);
                    float dy = Mathf.Min(y, size - 1 - y);
                    bool corner = (x < radius || x >= size - radius) && (y < radius || y >= size - radius);
                    bool inside = true;
                    if (corner)
                    {
                        float cx = x < radius ? radius : size - 1 - radius;
                        float cy = y < radius ? radius : size - 1 - radius;
                        float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                        inside = dist <= radius;
                    }
                    px[y * size + x] = inside ? new Color32(255, 255, 255, 255) : new Color32(0, 0, 0, 0);
                }
            }
            tex.SetPixels32(px);
            tex.Apply();

            // 确保目录
            if (!AssetDatabase.IsValidFolder("Assets/ArtMaterials/UI"))
                AssetDatabase.CreateFolder("Assets/ArtMaterials", "UI");
            System.IO.File.WriteAllBytes(path, tex.EncodeToPNG());
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            // 设 9-slice border
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled = false;
                importer.spriteBorder = new Vector4(radius, radius, radius, radius);
                importer.SaveAndReimport();
            }
            _roundedSprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            Debug.Log($"[MenuPolish] 生成圆角 Sprite: {path} (metaColor={metaColor}, sprite={_roundedSprite != null})");
        }
    }
}
