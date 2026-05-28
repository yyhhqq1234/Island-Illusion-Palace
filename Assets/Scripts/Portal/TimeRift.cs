using UnityEngine;

public class TimeRift : MonoBehaviour
{
    [Header("裂隙设置")]
    public MapType destinationType = MapType.MemoryFragment;
    public float lifetime = 45f;
    public bool isInstancedRift = false;

    [Header("视觉效果")]
    public SpriteRenderer riftRenderer;
    public ParticleSystem riftParticles;
    public float pulseSpeed = 2f;
    public float pulseAmount = 0.15f;

    private float age;
    private Vector3 baseScale;

    void Start()
    {
        baseScale = transform.localScale;
        if (riftRenderer == null) riftRenderer = GetComponent<SpriteRenderer>();
        if (autoCloseTime <= 0) autoCloseTime = lifetime;

        // 低频闪烁预警最后10秒
        StartCoroutine(LifetimeWarning());
    }

    public float autoCloseTime;

    void Update()
    {
        age += Time.deltaTime;

        float pulse = 1f + Mathf.Sin(age * pulseSpeed) * pulseAmount;
        transform.localScale = baseScale * pulse;

        float alpha = Mathf.Lerp(1f, 0f, Mathf.Clamp01((age - lifetime + 5f) / 5f));
        if (riftRenderer != null)
        {
            var c = riftRenderer.color;
            c.a = alpha;
            riftRenderer.color = c;
        }

        if (age >= lifetime)
            Destroy(gameObject);
    }

    System.Collections.IEnumerator LifetimeWarning()
    {
        yield return new WaitForSeconds(lifetime - 10f);
        if (riftParticles != null)
            riftParticles.Play();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (age < 1f) return;

        Debug.Log($"[时空裂隙] 玩家进入裂隙，传送到 {destinationType}");
        var map = FindObjectOfType<IntegratedMapSystem>();
        if (map != null)
        {
            map.SetMapType(destinationType);
            map.GenerateNewMap();
        }
        Destroy(gameObject);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 1, 1, 0.5f);
        Gizmos.DrawWireSphere(transform.position, 0.8f);
    }
}
