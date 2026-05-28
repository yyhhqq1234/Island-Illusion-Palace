using UnityEngine;

public class TimeGuardianBoss : BossAI
{
    [Header("时空守护者专属")]
    public float slowFieldRadius = 5f;
    public float slowFieldStrength = 0.5f;
    public float timeWarpInterval = 8f;
    public ElementType guardianElement = ElementType.Soul;

    private float timeWarpTimer;

    void Awake()
    {
        currentPhase = BossPhase.Phase1;
    }

    void Update()
    {
        UpdateTimeWarp();
    }

    void UpdateTimeWarp()
    {
        timeWarpTimer += Time.deltaTime;
        if (timeWarpTimer >= timeWarpInterval && currentPhase >= BossPhase.Phase3 && !isInvulnerable)
        {
            timeWarpTimer = 0f;
            CastTimeWarp();
        }
    }

    void CastTimeWarp()
    {
        SetInvulnerable(1.5f);
        Debug.Log("[时空守护者] 发动时空扭曲！短暂无敌并恢复生命值");

        currentHealth = Mathf.Min(maxHealth, currentHealth + maxHealth * 0.05f);

        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, slowFieldRadius);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Player"))
                {
                    var pc = hit.GetComponent<PlayerController>();
                    if (pc != null)
                        StartCoroutine(ApplySlow(pc));
                }
            }
        }
    }

    System.Collections.IEnumerator ApplySlow(PlayerController pc)
    {
        float originalSpeed = pc.moveSpeed;
        float originalRunSpeed = pc.runSpeed;
        pc.moveSpeed *= slowFieldStrength;
        pc.runSpeed *= slowFieldStrength;
        Debug.Log($"[时空守护者] 玩家被时空力场减速 {slowFieldStrength * 100}%");
        yield return new WaitForSeconds(2f);
        pc.moveSpeed = originalSpeed;
        pc.runSpeed = originalRunSpeed;
        Debug.Log("[时空守护者] 减速效果结束");
    }

    protected override void Die()
    {
        Debug.Log("[时空守护者] 被击败，时空屏障瓦解...");
        if (BattleEventManager.Instance != null)
            BattleEventManager.Instance.TriggerBossDefeated(gameObject);
        base.Die();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.5f, 0.2f, 0.8f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, slowFieldRadius);
    }
}
