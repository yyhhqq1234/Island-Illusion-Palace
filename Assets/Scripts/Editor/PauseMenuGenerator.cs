using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

/// <summary>
/// 暂停菜单生成器 - 用于创建紧凑、居中的暂停菜单UI
/// </summary>
public static class PauseMenuGenerator
{
    [MenuItem("GameObject/IIP/Create Compact Pause Menu", false, 10)]
    public static void CreateCompactPauseMenu()
    {
        // 1. 创建根对象 - PauseMenu (带有Canvas)
        GameObject pauseMenuRoot = new GameObject("PauseMenu");
        Undo.RegisterCreatedObjectUndo(pauseMenuRoot, "Create PauseMenu");

        // 添加Canvas组件
        Canvas canvas = pauseMenuRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        // 添加CanvasScaler - 实现自适应
        CanvasScaler canvasScaler = pauseMenuRoot.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920, 1080);
        canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        canvasScaler.matchWidthOrHeight = 0.5f;

        // 添加GraphicRaycaster
        pauseMenuRoot.AddComponent<GraphicRaycaster>();

        // 添加PauseMenu脚本
        PauseMenu pauseMenu = pauseMenuRoot.AddComponent<PauseMenu>();

        // 2. 创建背景遮罩
        GameObject dimMaskObj = new GameObject("DimMask");
        Undo.RegisterCreatedObjectUndo(dimMaskObj, "Create DimMask");
        dimMaskObj.transform.SetParent(pauseMenuRoot.transform, false);

        Image dimMaskImage = dimMaskObj.AddComponent<Image>();
        dimMaskImage.color = new Color(0, 0, 0, 0.7f);

        RectTransform dimMaskRect = dimMaskObj.GetComponent<RectTransform>();
        dimMaskRect.anchorMin = Vector2.zero;
        dimMaskRect.anchorMax = Vector2.one;
        dimMaskRect.sizeDelta = Vector2.zero;

        // 3. 创建暂停菜单面板
        GameObject pausePanelObj = new GameObject("PausePanel");
        Undo.RegisterCreatedObjectUndo(pausePanelObj, "Create PausePanel");
        pausePanelObj.transform.SetParent(pauseMenuRoot.transform, false);

        // 面板RectTransform - 居中显示
        RectTransform pausePanelRect = pausePanelObj.AddComponent<RectTransform>();
        pausePanelRect.anchorMin = new Vector2(0.5f, 0.5f);
        pausePanelRect.anchorMax = new Vector2(0.5f, 0.5f);
        pausePanelRect.pivot = new Vector2(0.5f, 0.5f);
        pausePanelRect.sizeDelta = new Vector2(450, 550); // 紧凑的尺寸

        // 面板背景
        Image pausePanelImage = pausePanelObj.AddComponent<Image>();
        pausePanelImage.color = new Color(0.08f, 0.08f, 0.12f, 0.95f); // 深色调

        // 4. 创建垂直布局容器
        GameObject contentObj = new GameObject("Content");
        Undo.RegisterCreatedObjectUndo(contentObj, "Create Content");
        contentObj.transform.SetParent(pausePanelObj.transform, false);

        RectTransform contentRect = contentObj.AddComponent<RectTransform>();
        contentRect.anchorMin = Vector2.zero;
        contentRect.anchorMax = Vector2.one;
        contentRect.sizeDelta = new Vector2(-40, -40);

        VerticalLayoutGroup layoutGroup = contentObj.AddComponent<VerticalLayoutGroup>();
        layoutGroup.childControlWidth = true;
        layoutGroup.childControlHeight = true;
        layoutGroup.childForceExpandWidth = true;
        layoutGroup.childForceExpandHeight = false;
        layoutGroup.spacing = 20;
        layoutGroup.padding = new RectOffset(20, 20, 20, 20);

        ContentSizeFitter contentSizeFitter = contentObj.AddComponent<ContentSizeFitter>();
        contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // 5. 创建标题
        GameObject titleObj = new GameObject("Title");
        Undo.RegisterCreatedObjectUndo(titleObj, "Create Title");
        titleObj.transform.SetParent(contentObj.transform, false);

        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "游戏暂停";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 32;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = new Color(0.9f, 0.85f, 1f);

        // 6. 创建按钮
        Button resumeBtn = CreateButton("ResumeButton", "继续游戏", contentObj.transform);
        Button settingsBtn = CreateButton("SettingsButton", "设置", contentObj.transform);
        Button mainMenuBtn = CreateButton("MainMenuButton", "返回主菜单", contentObj.transform);
        Button quitBtn = CreateButton("QuitButton", "退出游戏", contentObj.transform);

        // 7. 创建确认对话框
        GameObject confirmDialogObj = CreateConfirmDialog(pauseMenuRoot.transform);

        // 8. 配置PauseMenu脚本引用
        pauseMenu.pausePanel = pausePanelObj;
        pauseMenu.dimMask = dimMaskImage;
        pauseMenu.resumeButton = resumeBtn;
        pauseMenu.settingsButton = settingsBtn;
        pauseMenu.mainMenuButton = mainMenuBtn;
        pauseMenu.quitButton = quitBtn;
        pauseMenu.confirmDialogPanel = confirmDialogObj;
        pauseMenu.dialogText = confirmDialogObj.transform.Find("DialogText").GetComponent<Text>();
        pauseMenu.confirmButton = confirmDialogObj.transform.Find("ButtonGroup/ConfirmButton").GetComponent<Button>();
        pauseMenu.cancelButton = confirmDialogObj.transform.Find("ButtonGroup/CancelButton").GetComponent<Button>();

        // 9. 初始隐藏
        pausePanelObj.SetActive(false);
        dimMaskObj.SetActive(false);
        confirmDialogObj.SetActive(false);

        // 选中创建的对象
        Selection.activeGameObject = pauseMenuRoot;
        Debug.Log("[PauseMenuGenerator] 紧凑的暂停菜单已创建！");
    }

    private static Button CreateButton(string name, string text, Transform parent)
    {
        GameObject btnObj = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(btnObj, "Create Button " + name);
        btnObj.transform.SetParent(parent, false);

        // 按钮图片
        Image btnImage = btnObj.AddComponent<Image>();
        btnImage.color = new Color(0.15f, 0.15f, 0.2f, 0.9f);

        // 按钮组件
        Button btn = btnObj.AddComponent<Button>();

        // 按钮文本
        GameObject textObj = new GameObject("Text");
        Undo.RegisterCreatedObjectUndo(textObj, "Create Button Text");
        textObj.transform.SetParent(btnObj.transform, false);

        Text btnText = textObj.AddComponent<Text>();
        btnText.text = text;
        btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        btnText.fontSize = 20;
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.color = Color.white;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        return btn;
    }

    private static GameObject CreateConfirmDialog(Transform parent)
    {
        GameObject dialogObj = new GameObject("ConfirmDialog");
        Undo.RegisterCreatedObjectUndo(dialogObj, "Create ConfirmDialog");
        dialogObj.transform.SetParent(parent, false);

        // 对话框面板
        RectTransform dialogRect = dialogObj.AddComponent<RectTransform>();
        dialogRect.anchorMin = new Vector2(0.5f, 0.5f);
        dialogRect.anchorMax = new Vector2(0.5f, 0.5f);
        dialogRect.pivot = new Vector2(0.5f, 0.5f);
        dialogRect.sizeDelta = new Vector2(400, 200);

        Image dialogImage = dialogObj.AddComponent<Image>();
        dialogImage.color = new Color(0.1f, 0.1f, 0.15f, 0.98f);

        // 对话框文本
        GameObject textObj = new GameObject("DialogText");
        Undo.RegisterCreatedObjectUndo(textObj, "Create DialogText");
        textObj.transform.SetParent(dialogObj.transform, false);

        Text dialogText = textObj.AddComponent<Text>();
        dialogText.text = "确定要执行此操作吗？";
        dialogText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        dialogText.fontSize = 18;
        dialogText.alignment = TextAnchor.MiddleCenter;
        dialogText.color = Color.white;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 0.4f);
        textRect.anchorMax = new Vector2(1, 1);
        textRect.offsetMin = new Vector2(20, 0);
        textRect.offsetMax = new Vector2(-20, -20);

        // 按钮组
        GameObject buttonGroupObj = new GameObject("ButtonGroup");
        Undo.RegisterCreatedObjectUndo(buttonGroupObj, "Create ButtonGroup");
        buttonGroupObj.transform.SetParent(dialogObj.transform, false);

        RectTransform buttonGroupRect = buttonGroupObj.AddComponent<RectTransform>();
        buttonGroupRect.anchorMin = new Vector2(0, 0);
        buttonGroupRect.anchorMax = new Vector2(1, 0.4f);
        buttonGroupRect.offsetMin = new Vector2(20, 20);
        buttonGroupRect.offsetMax = new Vector2(-20, 0);

        HorizontalLayoutGroup horizontalLayout = buttonGroupObj.AddComponent<HorizontalLayoutGroup>();
        horizontalLayout.childControlWidth = true;
        horizontalLayout.childControlHeight = true;
        horizontalLayout.childForceExpandWidth = true;
        horizontalLayout.childForceExpandHeight = true;
        horizontalLayout.spacing = 20;

        // 创建确认和取消按钮
        CreateDialogButton("ConfirmButton", "确认", buttonGroupObj.transform);
        CreateDialogButton("CancelButton", "取消", buttonGroupObj.transform);

        return dialogObj;
    }

    private static void CreateDialogButton(string name, string text, Transform parent)
    {
        GameObject btnObj = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(btnObj, "Create Dialog Button " + name);
        btnObj.transform.SetParent(parent, false);

        Image btnImage = btnObj.AddComponent<Image>();
        btnImage.color = new Color(0.2f, 0.2f, 0.25f, 0.9f);

        Button btn = btnObj.AddComponent<Button>();

        GameObject textObj = new GameObject("Text");
        Undo.RegisterCreatedObjectUndo(textObj, "Create Dialog Button Text");
        textObj.transform.SetParent(btnObj.transform, false);

        Text btnText = textObj.AddComponent<Text>();
        btnText.text = text;
        btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        btnText.fontSize = 18;
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.color = Color.white;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
    }
}
