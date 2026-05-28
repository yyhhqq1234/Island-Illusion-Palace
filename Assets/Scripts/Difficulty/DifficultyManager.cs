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

    public class DifficultyManager : MonoBehaviour
    {
        public static DifficultyManager Instance { get; private set; }

        public DifficultyMode currentMode = DifficultyMode.Normal;
        public DifficultyConfig config;

        private bool hasCompletedNormal = false;
        private bool hasCompletedHard = false;

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

        public float GetSoulDropMultiplier()
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
    }
}
