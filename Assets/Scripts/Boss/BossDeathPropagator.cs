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
        GlobalEventManager.Instance.TriggerBossDefeated(boss.gameObject);
    }
}
