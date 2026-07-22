using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace IIPUI
{
    /// <summary>
    /// 背包(InventoryPanel) / 炼金(AlchemyPanel) 面板构建工具 —— 根面板轻量版。
    ///
    /// 架构背景(整图做底重构后):两个面板的全部内部布局(美术底图、槽位、详情、Tooltip)
    /// 均由 InventoryUI / AlchemyUI 在运行时代码重建(BuildArtPanel 会清空旧子节点),
    /// prefab 只需保留一个命名正确的空 RectTransform 根,场景只需回填根引用。
    ///
    /// 【菜单 1】IIP / 构建背包炼金面板Prefab
    ///   确保 Assets/Prefab/UI/ 下两个 prefab 资产存在,且为"空根 + 默认关闭"状态:
    ///   清空全部子节点(运行时重建,旧树已无意义),移除根上旧 Image 残留,SetActive(false)。
    ///   不打开/修改/保存任何场景。
    ///
    /// 【菜单 2】IIP / 回填当前场景背包炼金引用(修复工具)
    ///   场景引用断链时,打开该场景执行:按名查找面板根回填 inventoryPanel / alchemyPanel,
    ///   itemSlotPrefab(图标数据源)为空时从资产库兜底加载,然后保存当前场景。不硬编码场景路径。
    /// </summary>
    public static class InventoryAlchemyPanelBuilder
    {
        const string LOG = "[InventoryAlchemyPanelBuilder]";
        const string INV_PREFAB = "Assets/Prefab/UI/InventoryPanel.prefab";
        const string ALC_PREFAB = "Assets/Prefab/UI/AlchemyPanel.prefab";
        const string SLOT_PREFAB = "Assets/Prefab/Item/ItemSlot/ItemSlotPrefab.prefab";

        // ════════════════════════════════════════════════
        // 菜单 1:确保 Prefab 为空根(不触碰任何场景)
        // ════════════════════════════════════════════════

        [MenuItem("IIP/构建背包炼金面板Prefab")]
        static void BuildAll()
        {
            EnsureBareRootPrefab(INV_PREFAB, "InventoryPanel");
            EnsureBareRootPrefab(ALC_PREFAB, "AlchemyPanel");

            AssetDatabase.Refresh();
            Debug.Log($"{LOG} 构建完成:InventoryPanel / AlchemyPanel 已重置为空根(默认关闭)。" +
                      "面板内部布局由 InventoryUI/AlchemyUI 运行时重建,所有引用场景自动同步。");
        }

        /// <summary>确保 prefab 资产存在且为:空子节点 + 无根 Image + SetActive(false)。缺失则新建。</summary>
        static void EnsureBareRootPrefab(string path, string rootName)
        {
            bool existed = AssetDatabase.LoadAssetAtPath<GameObject>(path) != null;
            GameObject root;
            if (existed)
            {
                root = PrefabUtility.LoadPrefabContents(path);
            }
            else
            {
                root = new GameObject(rootName, typeof(RectTransform));
                Debug.LogWarning($"{LOG} 缺少 Prefab 资产,已新建: {path}");
            }

            try
            {
                root.name = rootName;

                // 清空全部子节点(运行时重建,旧树保留只会增加 prefab 体积与歧义)
                for (int i = root.transform.childCount - 1; i >= 0; i--)
                    Object.DestroyImmediate(root.transform.GetChild(i).gameObject);

                // 移除根上残留 Image(旧构建器加的 PanelBg 底色,运行时不使用)
                var img = root.GetComponent<UnityEngine.UI.Image>();
                if (img != null) Object.DestroyImmediate(img);

                // 根 RectTransform:居中锚点,尺寸由运行时代码设定(1440×810)
                var rt = (RectTransform)root.transform;
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = Vector2.zero;
                rt.localScale = Vector3.one;

                root.SetActive(false); // 面板默认关闭
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                if (existed) PrefabUtility.UnloadPrefabContents(root);
                else Object.DestroyImmediate(root);
            }
        }

        // ════════════════════════════════════════════════
        // 菜单 2:活跃场景引用回填(断链修复工具)
        // ════════════════════════════════════════════════

        [MenuItem("IIP/回填当前场景背包炼金引用")]
        static void BackfillActiveScene()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid() || string.IsNullOrEmpty(scene.path))
            {
                Debug.LogError($"{LOG} 无有效活跃场景,请先打开需要回填的场景再执行。");
                return;
            }

            int invFilled = 0, alcFilled = 0;

            var invUI = Object.FindObjectOfType<InventoryUI>(true);
            if (invUI != null)
            {
                var panel = FindPanelByName("InventoryPanel", invUI.inventoryPanel);
                if (panel != null)
                {
                    invUI.inventoryPanel = panel;
                    panel.SetActive(false);
                    invFilled++;
                }
                else Debug.LogWarning($"{LOG} 场景中未找到 InventoryPanel 节点,跳过 InventoryUI 面板回填。");

                if (invUI.itemSlotPrefab == null)
                {
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(SLOT_PREFAB);
                    if (prefab != null) { invUI.itemSlotPrefab = prefab; invFilled++; }
                }
                if (invFilled > 0) EditorUtility.SetDirty(invUI);
            }
            else Debug.LogWarning($"{LOG} 场景中未找到 InventoryUI,跳过背包回填。");

            var alcUI = Object.FindObjectOfType<AlchemyUI>(true);
            if (alcUI != null)
            {
                var panel = FindPanelByName("AlchemyPanel", alcUI.alchemyPanel);
                if (panel != null)
                {
                    alcUI.alchemyPanel = panel;
                    panel.SetActive(false);
                    alcFilled++;
                }
                else Debug.LogWarning($"{LOG} 场景中未找到 AlchemyPanel 节点,跳过 AlchemyUI 面板回填。");

                if (alcUI.itemSlotPrefab == null)
                {
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(SLOT_PREFAB);
                    if (prefab != null) { alcUI.itemSlotPrefab = prefab; alcFilled++; }
                }
                if (alcFilled > 0) EditorUtility.SetDirty(alcUI);
            }
            else Debug.LogWarning($"{LOG} 场景中未找到 AlchemyUI,跳过炼金回填。");

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"{LOG} 场景[{scene.name}] 回填完成:InventoryUI {invFilled} 项,AlchemyUI {alcFilled} 项,场景已保存。");
        }

        /// <summary>优先复用已有引用;为空时全场景(含未激活)按名查找面板根。</summary>
        static GameObject FindPanelByName(string name, GameObject existing)
        {
            if (existing != null) return existing;
            foreach (var t in Object.FindObjectsOfType<Transform>(true))
                if (t.name == name) return t.gameObject;
            return null;
        }
    }
}
