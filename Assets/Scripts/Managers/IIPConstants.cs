public static class IIPConstants
{
    // ── 场景名称 ──
    public const string SceneMainMenu = "MainMenu";
    public const string SceneGamePlay = "GamePlay";

    // ── Tags ──
    public const string TagPlayer = "Player";
    public const string TagEnemy = "Enemy";
    public const string TagSummonedCreature = "SummonedCreature";
    public const string TagSafeZone = "SafeZone";
    public const string TagCampfire = "Campfire";

    // ── Layers ──
    public const string LayerPlayer = "Player";
    public const string LayerEnemy = "Enemy";
    public const string LayerDefault = "Default";

    // ── 负担阈值 ──
    public const float BurdenNormalMax = 30f;
    public const float BurdenHighThreshold = 70f;
    public const float BurdenCriticalThreshold = 90f;
    public const float BurdenAbsoluteMax = 100f;

    // ── 召唤参数 ──
    public const int MaxActiveSummons = 3;
    public const float SummonGlobalCooldown = 2f;
    public const float QuickSummonCooldown = 1f;
    public const int MaxMemoryFragmentSlots = 3;

    // ── 炼金参数 ──
    public const int NewbieProtectionCount = 3;
    public const float SuccessRateMin = 0.05f;
    public const float SuccessRateMax = 0.95f;

    // ── 武器参数 ──
    public const int MaxEnhancementLevel = 5;
    public static readonly float[] EnhancementBonus = { 0f, 0.06f, 0.12f, 0.18f, 0.24f, 0.30f };

    // ── 玩家 KeyCode 别名（方便查找替换） ──
    public const string InputMoveHorizontal = "Horizontal";
    public const string InputMoveVertical = "Vertical";
}
