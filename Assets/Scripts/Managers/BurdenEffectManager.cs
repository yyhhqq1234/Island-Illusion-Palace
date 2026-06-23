using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BurdenEffectManager : MonoBehaviour
{
    [Header("视觉效果组件")]
    public Camera mainCamera;
    private UniversalAdditionalCameraData cameraData;

    [Header("高负担效果设置")]
    [Range(0f, 1f)]
    public float highBurdenThreshold = 0.7f;
    [Range(0f, 1f)]
    public float midBurdenThreshold = 0.3f;

    [Header("视觉扭曲效果")]
    public Material distortionMaterial;
    private bool isHighBurden = false;
    private float currentDistortion = 0f;

    [Header("颜色调整")]
    public Color normalColor = Color.white;
    public Color highBurdenColor = new Color(0.8f, 0.8f, 1f, 1f);

    [Header("音效引用")]
    private GameplayAudioManager audioManager;
    private BurdenSystem burdenSystem;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera != null)
        {
            cameraData = mainCamera.GetUniversalAdditionalCameraData();
        }

        audioManager = GameplayAudioManager.Instance;
        burdenSystem = GetComponent<BurdenSystem>();
        if (burdenSystem == null)
            burdenSystem = FindObjectOfType<BurdenSystem>();

        // 订阅负担等级变化事件，由事件驱动而非主动轮询
        GlobalEventManager.Instance.OnGlobalBurdenLevelChanged += OnGlobalBurdenLevelChanged;
    }

    void OnDestroy()
    {
        var gem = GlobalEventManager.Instance;
        if (gem != null) gem.OnGlobalBurdenLevelChanged -= OnGlobalBurdenLevelChanged;
    }

    void OnGlobalBurdenLevelChanged(GlobalBurdenLevel level)
    {
        if (burdenSystem == null) return;

        switch (level)
        {
            case GlobalBurdenLevel.Critical:
            case GlobalBurdenLevel.High:
                if (!isHighBurden)
                {
                    isHighBurden = true;
                    OnEnterHighBurden();
                }
                break;
            case GlobalBurdenLevel.Normal:
                if (isHighBurden)
                {
                    isHighBurden = false;
                    OnExitHighBurden();
                }
                break;
        }
    }

    void Update()
    {
        // 负担效果现在由 OnGlobalBurdenLevelChanged 事件驱动切换状态
        // Update 仅负责持续性视觉效果渐变（distortion、背景色）
        if (isHighBurden)
        {
            float burdenPercent = burdenSystem != null ? burdenSystem.GetBurdenPercent() : 0f;
            float intensity = (burdenPercent - highBurdenThreshold) / (1f - highBurdenThreshold);
            currentDistortion = Mathf.Lerp(currentDistortion, intensity * 0.1f, Time.deltaTime * 2f);

            if (mainCamera != null)
            {
                mainCamera.backgroundColor = Color.Lerp(normalColor, highBurdenColor, intensity * 0.3f);
            }
        }
        else
        {
            currentDistortion = Mathf.Lerp(currentDistortion, 0f, Time.deltaTime * 2f);

            if (mainCamera != null)
            {
                mainCamera.backgroundColor = Color.Lerp(mainCamera.backgroundColor, normalColor, Time.deltaTime * 2f);
            }
        }
    }

    void UpdateBurdenEffects(float burdenPercent)
    {
        if (audioManager != null)
        {
            Debug.Log("负担值变化: " + (burdenPercent * 100) + "%");
        }
    }

    void OnEnterHighBurden()
    {
        Debug.Log("进入高负担状态！");
    }

    void OnExitHighBurden()
    {
        Debug.Log("退出高负担状态");
    }

    public float GetDistortionIntensity()
    {
        return currentDistortion;
    }
}
