using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BattleSystem : MonoBehaviour
{
    [Header("战斗系统设置")]
    public PlayerController playerController;
    public WeaponSystem weaponSystem;
    public HealthSystem playerHealth;
    
    [Header("武器UI")]
    public Image currentWeaponIcon;
    public Text weaponNameText;
    public Text weaponDamageText;
    public Text weaponAttackRateText;
    
    [Header("武器图标")]
    public Sprite swordIcon;
    public Sprite staffIcon;
    public Sprite daggerIcon;
    
    [Header("战斗统计")]
    public int enemiesDefeated = 0;
    public int totalDamageDealt = 0;
    public int totalDamageTaken = 0;
    
    [Header("武器切换")]
    private int currentWeaponIndex = 0;
    private List<WeaponType> availableWeapons = new List<WeaponType>();
    
    void Start()
    {
        InitializeBattleSystem();
    }
    
    void Update()
    {
        HandleWeaponSwitching();
        UpdateWeaponUI();
    }
    
    // 初始化战斗系统
    void InitializeBattleSystem()
    {
        // 获取必要组件
        if (playerController == null)
            playerController = FindObjectOfType<PlayerController>();
        if (weaponSystem == null)
            weaponSystem = FindObjectOfType<WeaponSystem>();
        if (playerHealth == null)
            playerHealth = FindObjectOfType<HealthSystem>();
        
        // 初始化可用武器列表
        availableWeapons.Add(WeaponType.Sword); availableWeapons.Add(WeaponType.Staff); availableWeapons.Add(WeaponType.Scythe);
        
        // 设置初始武器
        if (weaponSystem != null)
        {
            weaponSystem.SwitchWeapon(availableWeapons[currentWeaponIndex]);
        }
    }
    
    // 处理武器切换
    void HandleWeaponSwitching()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && availableWeapons.Count > 0)
        {
            SwitchToWeapon(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && availableWeapons.Count > 1)
        {
            SwitchToWeapon(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) && availableWeapons.Count > 2)
        {
            SwitchToWeapon(2);
        }
        
        // 使用鼠标滚轮切换武器
        float scroll = Input.mouseScrollDelta.y;
        if (scroll != 0)
        {
            int direction = scroll > 0 ? -1 : 1; // 向上滚动切换到前一个，向下滚动切换到后一个
            int nextIndex = (currentWeaponIndex + direction) % availableWeapons.Count;
            if (nextIndex < 0)
                nextIndex = availableWeapons.Count - 1;
            
            SwitchToWeapon(nextIndex);
        }
    }
    
    // 切换到指定武器
    void SwitchToWeapon(int weaponIndex)
    {
        if (weaponIndex >= 0 && weaponIndex < availableWeapons.Count)
        {
            currentWeaponIndex = weaponIndex;
            
            if (weaponSystem != null)
            {
                weaponSystem.SwitchWeapon(availableWeapons[currentWeaponIndex]);
            }
            
            Debug.Log("切换到武器: " + availableWeapons[currentWeaponIndex]);
        }
    }
    
    // 更新武器UI
    void UpdateWeaponUI()
    {
        if (weaponSystem == null || currentWeaponIcon == null) return;
        
        // 更新武器图标
        switch (weaponSystem.currentWeaponType)
        {
            case WeaponType.Sword:
                if (swordIcon != null)
                    currentWeaponIcon.sprite = swordIcon;
                weaponNameText.text = "剑";
                break;
            case WeaponType.Staff:
                if (staffIcon != null)
                    currentWeaponIcon.sprite = staffIcon;
                weaponNameText.text = "法杖";
                break;
            case WeaponType.Scythe:
                if (daggerIcon != null)
                    currentWeaponIcon.sprite = daggerIcon;
                weaponNameText.text = "镰刀";
                break;
        }
        
        // 更新武器属性文本
        weaponDamageText.text = "伤害: " + weaponSystem.baseDamage.ToString("F1");
        weaponAttackRateText.text = "攻速: " + (1f / weaponSystem.attackInterval).ToString("F1");
    }
    
    // 记录造成的伤害
    public void RecordDamageDealt(float damage)
    {
        totalDamageDealt += (int)damage;
    }
    
    // 记录受到的伤害
    public void RecordDamageTaken(float damage)
    {
        totalDamageTaken += (int)damage;
    }
    
    // 记录击败的敌人
    public void RecordEnemyDefeated()
    {
        enemiesDefeated++;
    }
    
    // 获取战斗统计数据
    public string GetBattleStats()
    {
        return string.Format(
            "击败敌人: {0}\n造成伤害: {1}\n受到伤害: {2}\n当前武器: {3}",
            enemiesDefeated,
            totalDamageDealt,
            totalDamageTaken,
            weaponSystem != null ? weaponSystem.currentWeaponType.ToString() : "未知"
        );
    }
    
    // 玩家死亡处理
    public void OnPlayerDeath()
    {
        Debug.Log("玩家死亡！战斗结束。");
        Debug.Log(GetBattleStats());
        
        // 在实际游戏中，这里会触发游戏结束逻辑
        // 例如显示游戏结束界面、保存统计数据等
    }
    
    // 重置战斗统计
    public void ResetBattleStats()
    {
        enemiesDefeated = 0;
        totalDamageDealt = 0;
        totalDamageTaken = 0;
    }
    
    // 检查是否所有敌人都被击败（用于关卡完成检测）
    public bool AreAllEnemiesDefeated()
    {
        // 在实际游戏中，这会检查场景中是否还有存活的敌人
        // 这里简化为返回false，因为实际检测需要引用敌人列表
        return false;
    }
}


