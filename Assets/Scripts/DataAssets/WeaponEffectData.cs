using UnityEngine;

[CreateAssetMenu(fileName = "WeaponEffect_", menuName = "IIP/Weapon Effect")]
public class WeaponEffectData : ScriptableObject
{
    public string effectName = "New Effect";
    public EffectTrigger trigger = EffectTrigger.OnSecondaryAttack;
    public EffectTarget target = EffectTarget.SingleEnemy;
    public EffectValueType valueType = EffectValueType.DamageMultiplier;
    public float value = 1.3f;
    [TextArea] public string description = "";

    public enum EffectTrigger { OnSecondaryAttack, OnHit, OnKill, OnComboCount, Always }
    public enum EffectTarget { SingleEnemy, AllEnemiesInRange, Self, SummonedCreatures }
    public enum EffectValueType { DamageMultiplier, FlatDamage, HealPercent, ManaRestore, BuffSummon, DebuffEnemy }

    public void Execute(WeaponSystem weapon, GameObject target)
    {
        float dmg = weapon.GetCurrentDamage();
        switch (valueType)
        {
            case EffectValueType.DamageMultiplier:
                weapon.ExecuteAttack(dmg * value);
                break;
            case EffectValueType.FlatDamage:
                weapon.ExecuteAttack(dmg + value);
                break;
            case EffectValueType.HealPercent:
                weapon.HealPercent(value);
                break;
            case EffectValueType.ManaRestore:
                weapon.RestoreMana(value);
                break;
            case EffectValueType.BuffSummon:
                weapon.BuffSummons(value);
                break;
            case EffectValueType.DebuffEnemy:
                if (target != null) weapon.DebuffEnemy(target, value);
                break;
        }
        Debug.Log($"[WeaponEffect] {effectName}: {valueType} × {value}");
    }
}
