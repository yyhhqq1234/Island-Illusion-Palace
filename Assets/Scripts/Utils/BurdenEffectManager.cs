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
    private float lastBurdenPercent = 0f;

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
    }

    void Update()
    {
        if (burdenSystem == null) return;

        float burdenPercent = burdenSystem.GetBurdenPercent();

        if (Mathf.Abs(burdenPercent - lastBurdenPercent) > 0.05f)
        {
            UpdateBurdenEffects(burdenPercent);
            lastBurdenPercent = burdenPercent;
        }

        if (burdenPercent > highBurdenThreshold)
        {
            UpdateHighBurdenVisuals(burdenPercent);
        }
        else
        {
            ResetVisuals();
        }
    }

    void UpdateBurdenEffects(float burdenPercent)
    {
        if (audioManager != null)
        {
            Debug.Log("负担值变化: " + (burdenPercent * 100) + "%");
        }
    }

    void UpdateHighBurdenVisuals(float burdenPercent)
    {
        if (!isHighBurden)
        {
            isHighBurden = true;
            OnEnterHighBurden();
        }

        float intensity = (burdenPercent - highBurdenThreshold) / (1f - highBurdenThreshold);
        currentDistortion = Mathf.Lerp(currentDistortion, intensity * 0.1f, Time.deltaTime * 2f);

        if (mainCamera != null)
        {
            mainCamera.backgroundColor = Color.Lerp(normalColor, highBurdenColor, intensity * 0.3f);
        }
    }

    void ResetVisuals()
    {
        if (isHighBurden)
        {
            isHighBurden = false;
            OnExitHighBurden();
        }

        currentDistortion = Mathf.Lerp(currentDistortion, 0f, Time.deltaTime * 2f);

        if (mainCamera != null)
        {
            mainCamera.backgroundColor = Color.Lerp(mainCamera.backgroundColor, normalColor, Time.deltaTime * 2f);
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
