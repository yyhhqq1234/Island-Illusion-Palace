using System.Collections.Generic;
using UnityEngine;

public interface IWeaponProvider
{
    WeaponType CurrentWeaponType { get; }
    float BaseDamage { get; }
    float AttackInterval { get; }
    int EnhancementLevel { get; }
    int MaxEnhancementLevel { get; }
    ElementType WeaponElement { get; }
    float GetCurrentDamage();
    bool EnhanceWeapon(IInventoryService inventory);
    void SwitchWeapon(WeaponType newType);
    float CalculateDamageToEnemy(EnemyAI enemy);
}

public enum WeaponType { Sword, Staff, Scythe, CrystalArm }
public enum ElementType { None, Frost, Water, Fire, Lightning, Soul, Holy }

public interface IBattleEventHandler
{
    void OnEnemyDefeated(EnemyAI enemy);
    void OnDamageDealt(float amount, GameObject target);
    void OnDamageTaken(float amount, GameObject source);
    void OnBossPhaseChanged(int newPhase);
}
