using UnityEngine;
using System;

[CreateAssetMenu(fileName = "AbilityEffect_", menuName = "IIP/Scripted Ability Effect")]
public class ScriptedAbilityEffect : ScriptableObject
{
    public string effectName = "New Ability Effect";
    public EffectType type = EffectType.Heal;
    public float value = 20f;
    [Range(0f, 600f)] public float duration = 0f;

    [Header("负担相关")]
    public float burdenChange = 0f;      // 正=增加负担, 负=减少负担
    public float burdenRateMultiplier = 1f; // 负担增长倍率

    [Header("召唤相关")]
    public float summonBuffPercent = 0f;  // 召唤物属性提升%
    public float summonDurationBonus = 0f; // 召唤物持续时间+

    [Header("特殊效果")]
    public bool revealWeaknesses = false;  // 揭示弱点
    public bool revealHiddenObjects = false; // 揭示隐藏物
    public bool setTeleportPoint = false;  // 设置传送点
    public bool singleUseRevive = false;   // 一次性复活

    public enum EffectType { Heal, ManaRestore, SummonBuff, BurdenReduce, RevealWeakness, RevealHidden, SetTeleport, SingleRevive, EnvironmentStabilize }

    [TextArea] public string onUseLog = "";

    public void Execute(MonoBehaviour context)
    {
        var health = context.GetComponent<HealthSystem>();
        var mana = context.GetComponent<ManaSystem>();
        var burden = context.GetComponent<BurdenSystem>();
        var summon = context.GetComponent<SummonSystem>();

        switch (type)
        {
            case EffectType.Heal:
                if (health != null) health.Heal(health.maxHealth * (value / 100f));
                break;
            case EffectType.ManaRestore:
                if (mana != null) mana.RestoreMana(mana.maxMana * (value / 100f));
                break;
            case EffectType.BurdenReduce:
                if (burden != null) burden.RemoveBurden(Mathf.Abs(burdenChange));
                break;
            case EffectType.SingleRevive:
                if (health != null && health.currentHealth <= 0) health.Heal(health.maxHealth * 0.3f);
                break;
            default:
                // RevealWeakness / RevealHidden / SetTeleport / SummonBuff 由调用方在上下文中处理
                break;
        }

        if (!string.IsNullOrEmpty(onUseLog))
            Debug.Log($"[AbilityEffect] {effectName}: {onUseLog}");
    }
}
