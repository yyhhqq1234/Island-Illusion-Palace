using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Weapon_", menuName = "IIP/Weapon Data")]
public class WeaponData : ScriptableObject
{
    public string weaponName = "New Weapon";
    public WeaponType weaponType;
    public ElementType elementType;
    public Rarity rarity = Rarity.Common;

    public float baseDamageMin = 12f;
    public float baseDamageMax = 15f;
    public float attackInterval = 0.45f;
    public float attackRange = 2f;
    public Sprite icon;
    public Sprite weaponSprite;

    [TextArea] public string specialEffectDesc = "";
    public string obtainMethod = "初始武器";

    [Header("武器特效")]
    public List<WeaponEffectData> secondaryEffects = new List<WeaponEffectData>();
    public WeaponEffectData comboEffect;  // 10连击触发（仅CrystalArm）

    public float GetRandomDamage()
    {
        return Random.Range(baseDamageMin, baseDamageMax);
    }

    public enum Rarity { Common, Uncommon, Rare, Epic, Legendary }
}
