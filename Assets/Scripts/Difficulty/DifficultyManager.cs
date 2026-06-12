using UnityEngine;

namespace IIP.Difficulty
{
    public enum DifficultyMode { Easy, Normal, Hard, Hardcore }

    [CreateAssetMenu(fileName = "DifficultyConfig", menuName = "IIP/Difficulty Config")]
    public class DifficultyConfig : ScriptableObject
    {
        [Header("敌人属性倍率")]
        [Range(0.5f, 2f)] public float enemyDamageMultiplier = 1f;
        [Range(0.5f, 2f)] public float enemyHealthMultiplier = 1f;
        [Range(0.5f, 2f)] public float enemySpeedMultiplier = 1f;

        [Header("资源掉落倍率")]
        [Range(0.1f, 3f)] public float soulDropMultiplier = 1f;
        [Range(0.1f, 3f)] public float essenceDropMultiplier = 1f;
        [Range(0.1f, 3f)] public float materialDropMultiplier = 1f;

        [Header("负担倍率")]
        [Range(0.1f, 3f)] public float burdenGainMultiplier = 1f;
        [Range(0.1f, 3f)] public float burdenRecoveryMultiplier = 1f;

        [Header("特殊机制开关")]
        public bool enableEnemyBerserk = false;
        public bool enableTimeCollapse = false;
        public bool enableSoulCorrosion = false;

        [Header("门槛")]
        public float burdenPenaltyThreshold = 70f;
        public float deathSoulLossRate = 0.2f;
        public int deathSoulLossMinimum = 100;
    }

    public class DifficultyManager : MonoBehaviour, IDifficultyService
    {
        public static DifficultyManager Instance { get; private set; }

        public DifficultyMode currentMode = DifficultyMode.Normal;
        DifficultyMode IDifficultyService.currentMode { get => currentMode; set => currentMode = value; }
        public DifficultyConfig config;

        [Header("灵魂获取保底")]
        [Tooltip("灵魂获取最低保底倍率（默认35%）")]
        public float soulDropFloor = 0.35f;

        [Header("动态难度")]
        [Tooltip("是否启用基于玩家表现的动态难度调整")]
        public bool dynamicDifficultyEnabled = true;

        private bool hasCompletedNormal = false;
        private bool hasCompletedHard = false;
        private int consecutiveWins = 0;
        private int consecutiveLosses = 0;
        private float currentRegionClearTime = 0f;
        private bool isBossNoHit = false;
        private int resurrectionBuffRemaining = 0;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public float GetEnemyHealthMultiplier()
        {
            return currentMode switch
            {
                DifficultyMode.Easy => 0.80f,
                DifficultyMode.Normal => 1.00f,
                DifficultyMode.Hard => 1.30f,
                DifficultyMode.Hardcore => 1.50f,
                _ => 1f
            };
        }

        public float GetEnemyDamageMultiplier()
        {
            return currentMode switch
            {
                DifficultyMode.Easy => 0.80f,
                DifficultyMode.Normal => 1.00f,
                DifficultyMode.Hard => 1.30f,
                DifficultyMode.Hardcore => 1.50f,
                _ => 1f
            };
        }

        public float GetBaseSoulDropMultiplier()
        {
            return currentMode switch
            {
                DifficultyMode.Easy => 1.20f,
                DifficultyMode.Normal => 1.00f,
                DifficultyMode.Hard => 0.80f,
                DifficultyMode.Hardcore => 0.70f,
                _ => 1f
            };
        }

        public float GetSoulDropMultiplier()
        {
            // 保留向后兼容，返回基础倍率
            return GetBaseSoulDropMultiplier();
        }

        public float GetFinalSoulDropMultiplier(float burdenPenaltyMultiplier)
        {
            float baseMultiplier = GetBaseSoulDropMultiplier();
            float rawMultiplier = baseMultiplier * burdenPenaltyMultiplier;
            float finalMultiplier = Mathf.Max(soulDropFloor, rawMultiplier);
            
            Debug.Log($"[DifficultyManager] 灵魂倍率计算: 基础={baseMultiplier:F2} × 负担惩罚={burdenPenaltyMultiplier:F2} = {rawMultiplier:F2}, 保底后={finalMultiplier:F2}");
            
            return finalMultiplier;
        }

        public float GetBurdenGainMultiplier()
        {
            return currentMode switch
            {
                DifficultyMode.Easy => 0.70f,
                DifficultyMode.Normal => 1.00f,
                DifficultyMode.Hard => 1.50f,
                DifficultyMode.Hardcore => 2.00f,
                _ => 1f
            };
        }

        public float GetFinalEssenceDropMultiplier()
        {
            if (!dynamicDifficultyEnabled)
            {
                return GetBaseEssenceDropMultiplier();
            }

            float baseMultiplier = GetBaseEssenceDropMultiplier();
            float dynamicBonus = 0f;

            if (consecutiveWins >= 5) dynamicBonus += 0.10f;
            if (consecutiveWins >= 10) dynamicBonus += 0.10f; // 总共+20%

            // BOSS无伤额外+50%（仅该次掉落）
            if (isBossNoHit)
            {
                dynamicBonus += 0.50f;
            }

            // 复活Buff全属性+10%
            if (resurrectionBuffRemaining > 0)
            {
                dynamicBonus += 0.10f;
            }

            return baseMultiplier * (1f + dynamicBonus);
        }

        public float GetFinalMaterialDropMultiplier()
        {
            if (!dynamicDifficultyEnabled)
            {
                return GetBaseMaterialDropMultiplier();
            }

            float baseMultiplier = GetBaseMaterialDropMultiplier();
            float dynamicBonus = 0f;

            if (consecutiveWins >= 5) dynamicBonus += 0.10f;
            if (consecutiveWins >= 10) dynamicBonus += 0.10f; // 总共+20%

            // BOSS无伤额外+50%（仅该次掉落）
            if (isBossNoHit)
            {
                dynamicBonus += 0.50f;
            }

            // 复活Buff全属性+10%
            if (resurrectionBuffRemaining > 0)
            {
                dynamicBonus += 0.10f;
            }

            return baseMultiplier * (1f + dynamicBonus);
        }

        public float GetDynamicEnemyAttributeMultiplier()
        {
            if (!dynamicDifficultyEnabled)
            {
                return 1f;
            }

            float multiplier = 1f;

            // 连败 ≥ 3 场：敌人属性 -10%
            if (consecutiveLosses >= 3)
            {
                multiplier *= 0.90f;
            }

            // 复活Buff：临时全属性 +10%（对玩家是增益，对敌人无影响）
            // 这里只处理敌人属性，复活Buff不影响敌人

            return multiplier;
        }

        public float GetDynamicEliteDensityMultiplier()
        {
            if (!dynamicDifficultyEnabled)
            {
                return 1f;
            }

            // 快速清图（< 3 分钟）：下一区域精英密度 +40%
            if (currentRegionClearTime > 0f && currentRegionClearTime < 180f)
            {
                return 1.40f;
            }

            return 1f;
        }

        public float GetDynamicEnemyAggressionMultiplier(float burdenValue)
        {
            if (!dynamicDifficultyEnabled)
            {
                return 1f;
            }

            // 70-90：敌人侵略性 +15%
            if (burdenValue >= 70f && burdenValue <= 90f)
            {
                return 1.15f;
            }

            // > 90：已经通过 ShouldEnemiesHuntPlayer 单独处理
            return 1f;
        }

        public float GetDynamicEnemyDetectionRange(float burdenValue)
        {
            if (!dynamicDifficultyEnabled)
            {
                return 1f;
            }

            // < 30：敌人侦查范围 -10%
            if (burdenValue < 30f)
            {
                return 0.90f;
            }

            return 1f;
        }

        public bool ShouldEnemiesHuntPlayer(float burdenValue)
        {
            if (!dynamicDifficultyEnabled)
            {
                return false;
            }

            // > 90：全图敌人向玩家靠拢
            return burdenValue > 90f;
        }

        public float GetLegendaryMaterialChanceBonus()
        {
            if (!dynamicDifficultyEnabled)
            {
                return 0f;
            }

            // 连胜 ≥ 10 场：传奇材料概率 +5%
            if (consecutiveWins >= 10)
            {
                return 0.05f;
            }

            return 0f;
        }

        public float GetTimeRiftSpawnChanceBonus(float burdenValue)
        {
            if (!dynamicDifficultyEnabled)
            {
                return 0f;
            }

            // 70-90：时空裂隙出现概率 +10%
            if (burdenValue >= 70f && burdenValue <= 90f)
            {
                return 0.10f;
            }

            return 0f;
        }

        public void RecordBattleWin()
        {
            consecutiveWins++;
            consecutiveLosses = 0;

            if (resurrectionBuffRemaining > 0)
            {
                ConsumeResurrectionBuff();
            }

            // 清掉BOSS无伤标记（仅该次有效）
            isBossNoHit = false;

            Debug.Log($"[DifficultyManager] 记录战斗胜利，连胜={consecutiveWins}, 连败={consecutiveLosses}");
        }

        public void RecordBattleLoss()
        {
            consecutiveLosses++;
            consecutiveWins = 0;

            // 清掉BOSS无伤标记
            isBossNoHit = false;

            Debug.Log($"[DifficultyManager] 记录战斗失败，连胜={consecutiveWins}, 连败={consecutiveLosses}");
        }

        public void RecordRegionCleared(float clearTime)
        {
            currentRegionClearTime = clearTime;
            Debug.Log($"[DifficultyManager] 记录区域清图，耗时={clearTime:F1}秒");
        }

        public void RecordBossNoHit()
        {
            isBossNoHit = true;
            Debug.Log($"[DifficultyManager] 记录BOSS无伤击败，本次掉落+50%");
        }

        public void RecordPlayerResurrection()
        {
            resurrectionBuffRemaining = 3;
            Debug.Log($"[DifficultyManager] 记录玩家复活，获得3场战斗全属性+10% Buff");
        }

        public void ConsumeResurrectionBuff()
        {
            if (resurrectionBuffRemaining > 0)
            {
                resurrectionBuffRemaining--;
                Debug.Log($"[DifficultyManager] 消耗复活Buff，剩余={resurrectionBuffRemaining}场");
            }
        }

        public void ResetDynamicDifficulty()
        {
            consecutiveWins = 0;
            consecutiveLosses = 0;
            currentRegionClearTime = 0f;
            isBossNoHit = false;
            resurrectionBuffRemaining = 0;

            Debug.Log($"[DifficultyManager] 重置所有动态难度状态");
        }

        public bool CanSelectMode(DifficultyMode mode)
        {
            return mode switch
            {
                DifficultyMode.Easy => true,
                DifficultyMode.Normal => true,
                DifficultyMode.Hard => hasCompletedNormal,
                DifficultyMode.Hardcore => hasCompletedHard,
                _ => false
            };
        }

        public void MarkNormalCompleted() => hasCompletedNormal = true;
        public void MarkHardCompleted() => hasCompletedHard = true;

        private float GetBaseEssenceDropMultiplier()
        {
            return currentMode switch
            {
                DifficultyMode.Easy => 0.80f,
                DifficultyMode.Normal => 1.00f,
                DifficultyMode.Hard => 1.20f,
                DifficultyMode.Hardcore => 1.50f,
                _ => 1f
            };
        }

        private float GetBaseMaterialDropMultiplier()
        {
            return currentMode switch
            {
                DifficultyMode.Easy => 1.10f,
                DifficultyMode.Normal => 1.00f,
                DifficultyMode.Hard => 1.00f,
                DifficultyMode.Hardcore => 0.90f,
                _ => 1f
            };
        }
    }
}
