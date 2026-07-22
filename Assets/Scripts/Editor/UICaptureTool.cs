using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// UI 自渲染截图工具 —— 不依赖 GameView 呈现。
/// 原理：临时正交相机 + 将目标 Canvas 切到 ScreenSpaceCamera 模式 → 渲染到 RT →
/// 读像素 → 垂直翻转（RT 原点在左下）→ 缩放至最长边 maxSide → JPG 落盘。
/// 审查链路：script-execute 调用 Capture*() → Read 工具直接看图。
/// </summary>
public static class UICaptureTool
{
    public const string ShotsDir = @"d:\Program Files\Unity\U3Dproject\Island-Illusion-Palace\tools\local-vision\shots";

    /// <summary>查找 Canvas（含未激活）。canvasName 为 GameObject 名。</summary>
    static Canvas FindCanvas(string canvasName)
    {
        foreach (var c in Object.FindObjectsOfType<Canvas>(true))
            if (c.gameObject.name == canvasName) return c;
        return null;
    }

    /// <summary>确保目标到根的路径全部激活，返回被临时激活的节点列表（用于还原）。</summary>
    static List<GameObject> EnsureActive(Transform t)
    {
        var activated = new List<GameObject>();
        while (t != null)
        {
            if (!t.gameObject.activeSelf)
            {
                activated.Add(t.gameObject);
                t.gameObject.SetActive(true);
            }
            t = t.parent;
        }
        return activated;
    }

    /// <summary>把 RT 读成 JPG 写盘（垂直翻转 + 等比缩放），返回文件路径。</summary>
    static string RtToJpg(RenderTexture rt, string fileName, int maxSide)
    {
        int w = rt.width, h = rt.height;
        var prev = RenderTexture.active;
        RenderTexture.active = rt;
        var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, w, h), 0, 0, false);
        tex.Apply();
        RenderTexture.active = prev;

        float scale = Mathf.Min(1f, (float)maxSide / Mathf.Max(w, h));
        int tw = Mathf.RoundToInt(w * scale), th = Mathf.RoundToInt(h * scale);
        var scaled = new Texture2D(tw, th, TextureFormat.RGB24, false);
        var src = tex.GetPixels();
        var dst = new Color[tw * th];
        for (int y = 0; y < th; y++)
        {
            int sy = Mathf.Clamp(Mathf.RoundToInt(y / scale), 0, h - 1);
            for (int x = 0; x < tw; x++)
            {
                int sx = Mathf.Clamp(Mathf.RoundToInt(x / scale), 0, w - 1);
                dst[(th - 1 - y) * tw + x] = src[sy * w + sx]; // 垂直翻转
            }
        }
        scaled.SetPixels(dst);
        scaled.Apply();

        if (!fileName.EndsWith(".jpg")) fileName += ".jpg";
        string path = Path.Combine(ShotsDir, fileName);
        File.WriteAllBytes(path, scaled.EncodeToJPG(85));
        Object.DestroyImmediate(tex);
        Object.DestroyImmediate(scaled);
        return path;
    }

    /// <summary>
    /// 只渲染指定 Canvas 的 UI（中性暗底），用于面板审查。
    /// 画布未激活时自动临时激活并在结束后还原。
    /// </summary>
    public static string CaptureUI(string canvasName, string fileName, int maxSide = 1024)
    {
        var target = FindCanvas(canvasName);
        if (target == null) return "ERR canvas not found: " + canvasName;

        var activated = EnsureActive(target.transform);

        // 临时相机：只渲 UI 层
        var camGo = new GameObject("__UICapCam");
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.11f, 0.11f, 0.13f);
        cam.cullingMask = 1 << 5; // UI layer
        cam.nearClipPlane = -100f;
        cam.farClipPlane = 100f;

        var rt = new RenderTexture(1920, 1080, 24);
        cam.targetTexture = rt;

        // 记录并切换画布状态
        var oldMode = target.renderMode;
        var oldCam = target.worldCamera;
        var oldSort = target.sortingOrder;
        var oldLayers = new List<KeyValuePair<GameObject, int>>();
        CollectLayers(target.transform, oldLayers);
        SetLayerRecursively(target.gameObject, 5);
        target.renderMode = RenderMode.ScreenSpaceCamera;
        target.worldCamera = cam;
        target.planeDistance = 100f;
        Canvas.ForceUpdateCanvases();
        cam.Render();

        string path = RtToJpg(rt, fileName, maxSide);

        // 还原
        target.renderMode = oldMode;
        target.worldCamera = oldCam;
        target.sortingOrder = oldSort;
        RestoreLayers(oldLayers);
        cam.targetTexture = null;
        rt.Release();
        Object.DestroyImmediate(camGo);
        foreach (var go in activated) go.SetActive(false);

        return "OK " + path;
    }

    /// <summary>
    /// 世界画面 + 所有顶层 Overlay Canvas 合成截图，用于 HUD 审查。
    /// 世界部分用 Camera.main 渲染；UI 部分切到 ScreenSpaceCamera 叠加。
    /// </summary>
    public static string CaptureComposite(string fileName, int maxSide = 1024)
    {
        var mainCam = Camera.main;
        if (mainCam == null) return "ERR Camera.main not found";

        var rt = new RenderTexture(1920, 1080, 24);

        // 1) 世界
        var oldMainRT = mainCam.targetTexture;
        mainCam.targetTexture = rt;
        mainCam.Render();

        // 2) UI 叠加（所有 root overlay canvas）
        var camGo = new GameObject("__UICapCam");
        var uiCam = camGo.AddComponent<Camera>();
        uiCam.orthographic = true;
        uiCam.clearFlags = CameraClearFlags.Nothing;
        uiCam.cullingMask = 1 << 5;
        uiCam.nearClipPlane = -100f;
        uiCam.farClipPlane = 100f;
        uiCam.targetTexture = rt;

        var states = new List<CanvasState>();
        foreach (var c in Object.FindObjectsOfType<Canvas>(false))
        {
            if (c.transform.parent != null) continue; // 只处理 root canvas
            states.Add(new CanvasState(c));
            CollectLayers(c.transform, states[states.Count - 1].layers);
            SetLayerRecursively(c.gameObject, 5);
            c.renderMode = RenderMode.ScreenSpaceCamera;
            c.worldCamera = uiCam;
            c.planeDistance = 100f;
        }
        Canvas.ForceUpdateCanvases();
        uiCam.Render();

        string path = RtToJpg(rt, fileName, maxSide);

        foreach (var s in states) s.Restore();
        mainCam.targetTexture = oldMainRT;
        uiCam.targetTexture = null;
        rt.Release();
        Object.DestroyImmediate(camGo);
        return "OK " + path;
    }

    class CanvasState
    {
        public Canvas canvas;
        public RenderMode mode;
        public Camera cam;
        public List<KeyValuePair<GameObject, int>> layers = new List<KeyValuePair<GameObject, int>>();
        public CanvasState(Canvas c) { canvas = c; mode = c.renderMode; cam = c.worldCamera; }
        public void Restore()
        {
            canvas.renderMode = mode;
            canvas.worldCamera = cam;
            RestoreLayers(layers);
        }
    }

    static void CollectLayers(Transform t, List<KeyValuePair<GameObject, int>> list)
    {
        list.Add(new KeyValuePair<GameObject, int>(t.gameObject, t.gameObject.layer));
        foreach (Transform c in t) CollectLayers(c, list);
    }

    static void RestoreLayers(List<KeyValuePair<GameObject, int>> list)
    {
        foreach (var kv in list) if (kv.Key != null) kv.Key.layer = kv.Value;
    }

    static void SetLayerRecursively(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform c in go.transform) SetLayerRecursively(c.gameObject, layer);
    }
}
