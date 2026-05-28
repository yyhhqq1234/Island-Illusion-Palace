using System.Collections.Generic;
using UnityEngine;

public interface IWeaponProvider
{
    WeaponType currentWeaponType { get; set; }
    float baseDamage { get; set; }
    float attackInterval { get; set; }
    int enhancementLevel { get; set; }
    int maxEnhancementLevel { get; }
    float GetCurrentDamage();
    bool EnhanceWeapon(IInventoryService inventory);
    float CalculateDamageToEnemy(EnemyAI enemy);
}

public interface IBattleEventHandler
{
    void OnEnemyDefeated(EnemyAI enemy);
    void OnDamageDealt(float amount, GameObject target);
    void OnDamageTaken(float amount, GameObject source);
    void OnBossPhaseChanged(int newPhase);
}
