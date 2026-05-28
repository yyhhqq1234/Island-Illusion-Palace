using UnityEngine;

public enum PortalState { Locked, Unlocking, Open }

public class TimePortal : MonoBehaviour
{
    [Header("传送门设置")]
    public PortalState currentState = PortalState.Locked;
    public MapType destinationMap = MapType.Forest;
    public float unlockDelay = 1.5f;
    public float autoCloseTime = 0f;

    [Header("视觉组件")]
    public SpriteRenderer portalRenderer;
    public ParticleSystem portalParticles;
    public Sprite lockedSprite;
    public Sprite openSprite;
    public Color lockedColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
    public Color openColor = new Color(0.5f, 0.3f, 1f, 1f);

    private float closeTimer;

    void Start()
    {
        if (portalRenderer == null)
            portalRenderer = GetComponent<SpriteRenderer>();

        RefreshVisuals();
    }

    void Update()
    {
        if (autoCloseTime > 0 && currentState == PortalState.Open)
        {
            closeTimer += Time.deltaTime;
            if (closeTimer >= autoCloseTime)
                Destroy(gameObject);
        }
    }

    public void Unlock()
    {
        if (currentState != PortalState.Locked) return;
        StartCoroutine(UnlockSequence());
    }

    System.Collections.IEnumerator UnlockSequence()
    {
        currentState = PortalState.Unlocking;
        RefreshVisuals();
        Debug.Log("[时空传送门] 正在激活...");

        yield return new WaitForSeconds(unlockDelay);

        currentState = PortalState.Open;
        RefreshVisuals();
        Debug.Log("[时空传送门] 已激活！可以传送到下一个区域");
    }

    void RefreshVisuals()
    {
        if (portalRenderer == null) return;

        switch (currentState)
        {
            case PortalState.Locked:
                if (lockedSprite != null) portalRenderer.sprite = lockedSprite;
                portalRenderer.color = lockedColor;
                if (portalParticles != null) portalParticles.Stop();
                break;

            case PortalState.Unlocking:
                if (portalParticles != null) portalParticles.Play();
                break;

            case PortalState.Open:
                if (openSprite != null) portalRenderer.sprite = openSprite;
                portalRenderer.color = openColor;
                if (portalParticles != null) portalParticles.Play();
                break;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (currentState != PortalState.Open) return;

        Debug.Log($"[时空传送门] 玩家进入传送门，传送到 {destinationMap}");
        var map = FindObjectOfType<IntegratedMapSystem>();
        if (map != null)
        {
            map.SetMapType(destinationMap);
            map.GenerateNewMap();
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = currentState == PortalState.Open ? Color.magenta : Color.gray;
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
}
