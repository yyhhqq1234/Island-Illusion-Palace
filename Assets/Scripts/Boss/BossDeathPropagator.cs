using UnityEngine;

public class BossSummonEvent : UnityEngine.Events.UnityEvent<GameObject> { }

public class BossDeathPropagator : MonoBehaviour
{
    public static BossDeathPropagator Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    public void OnBossDied(BossAI boss)
    {
        if (BattleEventManager.Instance != null)
            BattleEventManager.Instance.TriggerBossDefeated(boss.gameObject);
    }
}
