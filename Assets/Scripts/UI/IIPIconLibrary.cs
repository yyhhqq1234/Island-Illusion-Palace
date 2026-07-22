using System.Collections.Generic;
using UnityEngine;

namespace IIPUI
{
    /// <summary>
    /// 物品图标库 —— 运行时从 ItemSlotPrefab 的 iconDatabase(53 项,Inspector 手工配置)解析 Sprite。
    ///
    /// 背景:InventorySystem 构造的 InventoryItem.icon 恒为 null,旧 UI 靠 ItemSlot 组件
    /// 按物品名称查库显示图标;新一代运行时 UI(背包/炼金整图做底)不再实例化 ItemSlot,
    /// 统一改走本静态库:名称去空格模糊匹配 → Sprite / 稀有度。
    ///
    /// 用法:任一持有 itemSlotPrefab 引用的 UI(InventoryUI / AlchemyUI)在构建面板时
    /// 调用 SeedFromPrefab 播种(幂等);之后各 UI 直接 Resolve / TryGetRarity。
    /// </summary>
    public static class IIPIconLibrary
    {
        static Dictionary<string, ItemSlot.ItemIconData> byName;
        static bool seeded;

        public static bool IsSeeded => seeded;

        /// <summary>从 ItemSlot prefab 播种(幂等,已播种则直接返回)。</summary>
        public static void SeedFromPrefab(GameObject slotPrefab)
        {
            if (seeded || slotPrefab == null) return;
            var slot = slotPrefab.GetComponent<ItemSlot>();
            if (slot == null || slot.iconDatabase == null)
            {
                Debug.LogWarning($"[IIPIconLibrary] prefab '{slotPrefab.name}' 上无 ItemSlot 组件或 iconDatabase 为空,图标库播种失败。");
                return;
            }

            byName = new Dictionary<string, ItemSlot.ItemIconData>();
            foreach (var data in slot.iconDatabase.GetAllIcons())
            {
                if (data == null || string.IsNullOrEmpty(data.itemName)) continue;
                string key = Normalize(data.itemName);
                if (!byName.ContainsKey(key)) byName.Add(key, data);
            }
            seeded = true;

            int withIcon = 0;
            foreach (var kv in byName) if (kv.Value.icon != null) withIcon++;
            Debug.Log($"[IIPIconLibrary] 播种完成:共 {byName.Count} 项,已配置 Sprite {withIcon} 项。");
        }

        /// <summary>按物品名解析 Sprite;未播种或找不到返回 null。</summary>
        public static Sprite Resolve(string itemName)
        {
            var d = ResolveData(itemName);
            return d != null ? d.icon : null;
        }

        /// <summary>按物品名解析稀有度;找不到返回 false(rarity 给 Common)。</summary>
        public static bool TryGetRarity(string itemName, out ItemRarity rarity)
        {
            var d = ResolveData(itemName);
            if (d != null) { rarity = d.rarity; return true; }
            rarity = ItemRarity.Common;
            return false;
        }

        static ItemSlot.ItemIconData ResolveData(string itemName)
        {
            if (!seeded || byName == null || string.IsNullOrEmpty(itemName)) return null;
            ItemSlot.ItemIconData d;
            return byName.TryGetValue(Normalize(itemName), out d) ? d : null;
        }

        static string Normalize(string s)
            => s.Trim().Replace(" ", "").Replace("　", "").ToLowerInvariant();
    }
}
