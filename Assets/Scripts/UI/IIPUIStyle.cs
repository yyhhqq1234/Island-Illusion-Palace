using UnityEngine;

namespace IIPUI
{
    /// <summary>
    /// 全局 UI 样式 Token 单一数据源（设计系统）。
    /// 所有颜色 / 字号 / 间距在此定义并赋予语义化名称，
    /// UI 组件与 IIPUIFactory 一律从这里引用，禁止裸写 new Color。
    /// 色值与 P2 重构前的现状逐一对应，仅收敛来源，不改变任何渲染结果。
    /// </summary>
    public static class IIPUIStyle
    {
        // ═══════════════════════════════════════════
        // 面板 / 槽位背景（紫调暗夜色板）
        // ═══════════════════════════════════════════

        /// <summary>主面板背景（最深，97% 不透明）</summary>
        public static readonly Color PanelBackground = new Color(0.10f, 0.09f, 0.16f, 0.97f);
        /// <summary>面板内嵌内容区背景（半透明）</summary>
        public static readonly Color ContentBackground = new Color(0.08f, 0.07f, 0.13f, 0.60f);
        /// <summary>通用槽位背景（有内容）</summary>
        public static readonly Color SlotBackground = new Color(0.15f, 0.14f, 0.22f, 0.90f);
        /// <summary>通用槽位背景（空）</summary>
        public static readonly Color SlotBackgroundEmpty = new Color(0.11f, 0.10f, 0.17f, 0.80f);
        /// <summary>进度条深色底（HP/MP 条背景，比 ContentBackground 更不透明）</summary>
        public static readonly Color BarBackgroundDark = new Color(0.08f, 0.07f, 0.13f, 0.80f);
        /// <summary>MP 条专属背景（比 HP 更暗，与青色填充拉开对比，0 MP 时不呈"满条"观感）</summary>
        public static readonly Color ManaBarBackground = new Color(0.04f, 0.04f, 0.07f, 0.92f);
        /// <summary>进度条中性灰底（经验条/召唤物 HP/负担条背景）</summary>
        public static readonly Color BarBackgroundNeutral = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        /// <summary>设置面板键位展示框背景</summary>
        public static readonly Color KeyDisplayBackground = new Color(0.22f, 0.20f, 0.34f, 1.00f);

        // ═══════════════════════════════════════════
        // 按钮 / 页签
        // ═══════════════════════════════════════════

        /// <summary>按钮常态</summary>
        public static readonly Color ButtonNormal = new Color(0.18f, 0.16f, 0.26f, 1.00f);
        /// <summary>按钮悬停高亮</summary>
        public static readonly Color ButtonHover = new Color(0.32f, 0.28f, 0.50f, 1.00f);
        /// <summary>页签激活态</summary>
        public static readonly Color TabActive = new Color(0.28f, 0.24f, 0.42f, 1.00f);
        /// <summary>页签未激活态</summary>
        public static readonly Color TabInactive = new Color(0.15f, 0.14f, 0.22f, 1.00f);
        /// <summary>设置面板页签激活态（比全局 TabActive 略灰）</summary>
        public static readonly Color SettingsTabActive = new Color(0.25f, 0.25f, 0.4f, 1f);
        /// <summary>设置面板页签未激活态</summary>
        public static readonly Color SettingsTabInactive = new Color(0.18f, 0.18f, 0.25f, 1f);
        /// <summary>背包过滤按钮选中态（蓝色 30% 透明）</summary>
        public static readonly Color FilterButtonHighlight = new Color(0.3f, 0.5f, 1f, 0.3f);
        /// <summary>背包过滤按钮常态（白色 10% 透明）</summary>
        public static readonly Color FilterButtonNormal = new Color(1f, 1f, 1f, 0.1f);

        // ═══════════════════════════════════════════
        // 边框 / 选中
        // ═══════════════════════════════════════════

        /// <summary>常规描边（暗紫）</summary>
        public static readonly Color BorderDim = new Color(0.40f, 0.38f, 0.55f, 0.80f);
        /// <summary>高亮描边（亮紫）</summary>
        public static readonly Color BorderBright = new Color(0.55f, 0.50f, 0.75f, 1.00f);
        /// <summary>背包格子选中边框（#448AFF）</summary>
        public static readonly Color SelectionBlue = new Color(0.267f, 0.541f, 1f);
        /// <summary>背包格子悬停边框（60% 透明）</summary>
        public static readonly Color SelectionBlueHover = new Color(0.4f, 0.6f, 1f, 0.6f);

        // ═══════════════════════════════════════════
        // 主题强调色
        // ═══════════════════════════════════════════

        /// <summary>金色强调（稀有度传说/等级徽章/标题点缀）</summary>
        public static readonly Color AccentGold = new Color(1.00f, 0.80f, 0.20f, 1.00f);
        /// <summary>主题紫强调（传送点/幻宫主题色）</summary>
        public static readonly Color AccentPurple = new Color(0.55f, 0.40f, 0.85f, 1.00f);
        /// <summary>青色强调（MP 条/经验条/小地图玩家点）</summary>
        public static readonly Color AccentCyan = new Color(0.30f, 0.80f, 1.00f, 1.00f);
        /// <summary>绿色强调（就绪状态/正向提示）</summary>
        public static readonly Color AccentGreen = new Color(0.40f, 0.80f, 0.45f, 1.00f);
        /// <summary>红色强调（HP 条/小地图敌人点/危险）</summary>
        public static readonly Color AccentRed = new Color(0.90f, 0.35f, 0.35f, 1.00f);

        // ═══════════════════════════════════════════
        // 文字色
        // ═══════════════════════════════════════════

        /// <summary>标题文字（淡紫白）</summary>
        public static readonly Color TextTitle = new Color(0.95f, 0.90f, 1.00f, 1f);
        /// <summary>正文文字（主文本）</summary>
        public static readonly Color TextPrimary = new Color(0.92f, 0.92f, 0.96f, 1f);
        /// <summary>次要文字（弱化说明/占位）</summary>
        public static readonly Color TextSecondary = new Color(0.65f, 0.65f, 0.75f, 1f);
        /// <summary>键位提示文字（纯白）</summary>
        public static readonly Color TextKey = new Color(1.00f, 1.00f, 1.00f, 1f);
        /// <summary>设置面板数值文字（淡蓝白）</summary>
        public static readonly Color TextValue = new Color(0.75f, 0.80f, 0.95f, 1f);
        /// <summary>角落键位小字（75% 透明白，如 Space 提示）</summary>
        public static readonly Color TextKeyHint = new Color(1f, 1f, 1f, 0.75f);

        // ═══════════════════════════════════════════
        // 状态语义色（血条 / 警告等级）
        // ═══════════════════════════════════════════

        /// <summary>生命填充绿（召唤物 HP/负担正常态/小地图安全区）</summary>
        public static readonly Color HealthFill = new Color(0.4f, 0.8f, 0.4f, 1f);
        /// <summary>低血量警告红（召唤物 HP 低血量）</summary>
        public static readonly Color HealthFillLow = new Color(0.9f, 0.3f, 0.3f, 1f);
        /// <summary>召唤物状态条 HP 内嵌轨道底</summary>
        public static readonly Color HealthBarInnerBackground = new Color(0.1f, 0.1f, 0.1f, 0.6f);
        /// <summary>一级警告黄（负担 50-70）</summary>
        public static readonly Color WarningYellow = new Color(1f, 0.9f, 0.3f, 1f);
        /// <summary>二级警告橙（负担 70-90/小地图营火）</summary>
        public static readonly Color WarningOrange = new Color(1f, 0.6f, 0.2f, 1f);
        /// <summary>三级警告红（负担 90-100）</summary>
        public static readonly Color WarningRed = new Color(1f, 0.2f, 0.2f, 1f);

        // ═══════════════════════════════════════════
        // 负担系统专属
        // ═══════════════════════════════════════════

        /// <summary>负担晕影紫（#6B2FA0）</summary>
        public static readonly Color BurdenVignette = new Color(0.42f, 0.18f, 0.63f, 1f);
        /// <summary>负担条图标紫</summary>
        public static readonly Color BurdenIcon = new Color(0.6f, 0.4f, 0.9f, 1f);

        // ═══════════════════════════════════════════
        // 组件专属 Token（无语义复用价值，按组件命名）
        // ═══════════════════════════════════════════

        /// <summary>武器图标底座背景</summary>
        public static readonly Color WeaponIconBackground = new Color(0.2f, 0.2f, 0.3f, 0.8f);
        /// <summary>闪避冷却环-就绪态（值同 AccentGreen，组件语义独立保留，便于日后单独调优）</summary>
        public static readonly Color CooldownRingReady = AccentGreen;
        /// <summary>闪避冷却环-冷却中（值同 BorderDim，组件语义独立保留，便于日后单独调优）</summary>
        public static readonly Color CooldownRingBusy = BorderDim;
        /// <summary>武器图标冷却中变暗</summary>
        public static readonly Color WeaponIconCooldownDim = new Color(0.5f, 0.5f, 0.5f, 0.8f);
        /// <summary>武器图标占位淡紫（无 sprite 时）</summary>
        public static readonly Color WeaponIconPlaceholder = new Color(0.7f, 0.65f, 0.85f, 0.5f);
        /// <summary>小地图背景（同 PanelBackground 色相，90% 不透明）</summary>
        public static readonly Color MinimapBackground = new Color(0.10f, 0.09f, 0.16f, 0.90f);
        /// <summary>快捷槽-空槽背景</summary>
        public static readonly Color QuickSlotEmpty = new Color(0.2f, 0.2f, 0.3f, 0.7f);
        /// <summary>快捷槽-有物品背景</summary>
        public static readonly Color QuickSlotFilled = new Color(0.3f, 0.3f, 0.4f, 0.9f);
        /// <summary>冷却遮罩黑（60% 不透明）</summary>
        public static readonly Color CooldownMask = new Color(0f, 0f, 0f, 0.6f);
        /// <summary>召唤轮盘槽位高亮</summary>
        public static readonly Color SummonWheelHighlight = new Color(0.4f, 0.6f, 0.8f, 0.9f);
        /// <summary>召唤轮盘空槽</summary>
        public static readonly Color SummonWheelEmpty = new Color(0.1f, 0.1f, 0.1f, 0.5f);
        /// <summary>背包格子-空（#1E1E2E）</summary>
        public static readonly Color InventorySlotEmpty = new Color(0.118f, 0.118f, 0.180f);
        /// <summary>背包格子-有物品（#2A2A3A）</summary>
        public static readonly Color InventorySlotFilled = new Color(0.165f, 0.165f, 0.227f);
        /// <summary>区域名横幅背景（50% 透明黑）</summary>
        public static readonly Color OverlayDim = new Color(0f, 0f, 0f, 0.5f);
        /// <summary>等级徽章上的 "Lv" 小字（深棕）</summary>
        public static readonly Color LevelBadgeText = new Color(0.15f, 0.12f, 0.05f, 1f);

        // ═══════════════════════════════════════════
        // 稀有度色（背包/物品）
        // ═══════════════════════════════════════════

        /// <summary>稀有度-优秀（绿）</summary>
        public static readonly Color RarityUncommon = new Color(0.3f, 0.8f, 0.3f);
        /// <summary>稀有度-稀有（紫）</summary>
        public static readonly Color RarityRare = new Color(0.6f, 0.4f, 1f);
        /// <summary>稀有度-传说（金）（值同 AccentGold，稀有度语义独立保留，便于日后单独调优）</summary>
        public static readonly Color RarityLegendary = AccentGold;

        // ═══════════════════════════════════════════
        // 标准字号
        // ═══════════════════════════════════════════

        /// <summary>大标题字号</summary>
        public const int FontSizeTitle = 28;
        /// <summary>按钮/详情名字号</summary>
        public const int FontSizeButton = 20;
        /// <summary>常规标签字号</summary>
        public const int FontSizeLabel = 16;
        /// <summary>正文/状态条文字字号</summary>
        public const int FontSizeBody = 14;
        /// <summary>小字（辅助说明/数值）</summary>
        public const int FontSizeSmall = 12;
        /// <summary>键位角标字号</summary>
        public const int FontSizeKey = 11;
        /// <summary>角落键位提示小字（如 Space）</summary>
        public const int FontSizeKeyHint = 10;
        /// <summary>副标题字号（区域名副标题/等级数字）</summary>
        public const int FontSizeSubtitle = 18;
        /// <summary>区域名标题字号</summary>
        public const int FontSizeAreaTitle = 24;

        // ═══════════════════════════════════════════
        // 标准间距 / 尺寸
        // ═══════════════════════════════════════════

        /// <summary>标准元素间距（槽位间隔/布局 padding，8px）</summary>
        public const float SpacingStandard = 8f;
        /// <summary>标准正方形槽位边长（快捷槽/武器图标，56px）</summary>
        public const float SlotSizeStandard = 56f;
    }
}
