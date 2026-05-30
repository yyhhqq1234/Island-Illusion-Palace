using UnityEngine;
using System.Collections.Generic;

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

    [Header("传送设置")]
    [Tooltip("是否传送到随机地图")]
    public bool randomDestination = true;
    [Tooltip("是否排除最终区域")]
    public bool excludeFinalArea = true;
    [Tooltip("是否排除特殊区域")]
    public bool excludeSpecialArea = true;

    private float age;
    private Vector3 baseScale;

    private static readonly List<MapType> FinalAreas = new List<MapType>
    {
        MapType.TruthCorridor
    };

    private static readonly List<MapType> SpecialAreas = new List<MapType>
    {
        MapType.LabFragment,
        MapType.MemoryFragment
    };

    private static readonly List<MapType> AllMapTypes = new List<MapType>
    {
        MapType.Forest,
        MapType.Wasteland,
        MapType.Desert,
        MapType.RockLand,
        MapType.Wetland,
        MapType.IceField,
        MapType.Volcano,
        MapType.RuinCity,
        MapType.ForgottenManor,
        MapType.AncientTemple,
        MapType.LabFragment,
        MapType.MemoryFragment,
        MapType.TruthCorridor
    };

    void Start()
    {
        baseScale = transform.localScale;
        if (riftRenderer == null) riftRenderer = GetComponent<SpriteRenderer>();
        if (autoCloseTime <= 0) autoCloseTime = lifetime;

        if (randomDestination)
        {
            SelectRandomDestination();
        }

        StartCoroutine(LifetimeWarning());
    }

    void OnEnable()
    {
        if (randomDestination && age == 0)
        {
            SelectRandomDestination();
        }
    }

    void SelectRandomDestination()
    {
        MapType currentMap = GetCurrentMapType();
        List<MapType> availableDestinations = GetAvailableDestinations(currentMap);

        if (availableDestinations.Count == 0)
        {
            destinationType = MapType.Forest;
            Debug.LogWarning("[时空裂隙]无可用目的地，默认传送至 Forest");
            return;
        }

        destinationType = availableDestinations[Random.Range(0, availableDestinations.Count)];
        Debug.Log($"[时空裂隙] 随机选择目的地: {destinationType}");
    }

    List<MapType> GetAvailableDestinations(MapType currentMap)
    {
        List<MapType> available = new List<MapType>();

        foreach (MapType mapType in AllMapTypes)
        {
            if (mapType == currentMap)
                continue;

            if (excludeFinalArea && FinalAreas.Contains(mapType))
                continue;

            if (excludeSpecialArea && SpecialAreas.Contains(mapType))
                continue;

            available.Add(mapType);
        }

        if (available.Count == 0)
        {
            foreach (MapType mapType in AllMapTypes)
            {
                if (mapType != currentMap)
                    available.Add(mapType);
            }
        }

        return available;
    }

    MapType GetCurrentMapType()
    {
        var mapSystem = FindObjectOfType<IntegratedMapSystem>();
        if (mapSystem != null)
        {
            return mapSystem.currentMapType;
        }
        return MapType.Forest;
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
