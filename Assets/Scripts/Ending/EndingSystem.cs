using UnityEngine;

namespace IIP.Ending
{
    public enum EndingType { PerfectRebuild, IncompleteReturn, PowerBacklash, SacrificeRedemption, TimeTraveler }

    public class EndingSystem : MonoBehaviour, IEndingService
    {
        public static EndingSystem Instance { get; private set; }

        [Header("碎片状态")]
        public int coreFragments = 0;
        public int memoryFragments = 0;

        [Header("负担状态")]
        public float finalBurden = 0;
        public bool purificationDone = false;

        [Header("难度")]
        public bool isHardcoreMode = false;

        [Header("最终战状态")]
        public bool qteSuccess = false;
        public bool choseAcceptReality = false;

        [Header("统计数据")]
        public int forbiddenAlchemyCount = 0;
        public int totalCycles = 1;
        public int cyclesWithFullFragments = 0;
        public bool allRecipesUnlocked = false;
        public bool superBeaconCrafted = false;
        public bool burdenNeverExceeded20 = true;

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

        public EndingType DetermineEnding()
        {
            if (totalCycles >= 3 && cyclesWithFullFragments >= 3
                && allRecipesUnlocked && isHardcoreMode
                && burdenNeverExceeded20 && superBeaconCrafted)
                return EndingType.TimeTraveler;

            if (finalBurden > 70 && !purificationDone)
                return EndingType.PowerBacklash;
            if (forbiddenAlchemyCount >= 5)
                return EndingType.PowerBacklash;
            if (coreFragments < 3 && !choseAcceptReality)
                return EndingType.PowerBacklash;

            if (choseAcceptReality && memoryFragments >= 5 && purificationDone)
                return EndingType.SacrificeRedemption;

            if (coreFragments == 3 && memoryFragments >= 5
                && finalBurden < 30 && qteSuccess && isHardcoreMode)
                return EndingType.PerfectRebuild;

            return EndingType.IncompleteReturn;
        }

        public string GetEndingHint()
        {
            string hint = coreFragments == 3
                ? "莎娜的灵魂核心已完整 ✓"
                : "灵魂核心仍有缺失";

            if (memoryFragments >= 5)
                hint += "\n墨语对莎娜的思念足够深刻 ✓";

            if (finalBurden > 70)
                hint += "\n⚠ 墨语的灵魂承受着巨大压力";
            else if (finalBurden < 30)
                hint += "\n灵魂纯净，状态极佳 ✓";

            if (!isHardcoreMode)
                hint += "\n或许更高的难度能解锁更多可能";

            return hint;
        }
    }
}
