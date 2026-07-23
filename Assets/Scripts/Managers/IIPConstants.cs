using UnityEngine;

public static class IIPConstants
{
    // ── 场景名称 ──
    public const string SceneMainMenu = "MainMenu";
    public const string SceneGamePlay = "Forest";

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

    // ═══════════════════════════════════════════
    // 玩家输入键位（对应 PC按键映射方案.md）
    // ═══════════════════════════════════════════
    public const KeyCode KeyMoveUp       = KeyCode.W;
    public const KeyCode KeyMoveDown     = KeyCode.S;
    public const KeyCode KeyMoveLeft     = KeyCode.A;
    public const KeyCode KeyMoveRight    = KeyCode.D;
    public const KeyCode KeyMoveUpAlt    = KeyCode.UpArrow;
    public const KeyCode KeyMoveDownAlt  = KeyCode.DownArrow;
    public const KeyCode KeyMoveLeftAlt  = KeyCode.LeftArrow;
    public const KeyCode KeyMoveRightAlt = KeyCode.RightArrow;
    public const KeyCode KeyRun          = KeyCode.LeftShift;
    public const KeyCode KeyRunAlt       = KeyCode.RightShift;
    public const KeyCode KeyDash         = KeyCode.Space;
    public const KeyCode KeyInteract     = KeyCode.E;
    public const KeyCode KeyInventory    = KeyCode.I;
    public const KeyCode KeyQuickSummon  = KeyCode.F;
    public const KeyCode KeySummonWheel  = KeyCode.R;
    public const KeyCode KeyRecallAll    = KeyCode.LeftAlt;
    public const KeyCode KeyRecallAllAlt = KeyCode.RightAlt;
    public const KeyCode KeyQuickItem1   = KeyCode.Alpha1;
    public const KeyCode KeyQuickItem2   = KeyCode.Alpha2;
    public const KeyCode KeyQuickItem3   = KeyCode.Alpha3;

    // ── 设置项 PlayerPrefs 键（设置面板 ↔ 消费系统共用，避免魔法字符串散落）──
    public const string PrefKeyShowDamageNumbers = "IIP_ShowDamageNumbers";
    public const string PrefKeyAutoPickup = "IIP_AutoPickup";
}
