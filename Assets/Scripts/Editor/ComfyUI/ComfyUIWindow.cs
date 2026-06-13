#if UNITY_EDITOR
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;

namespace ComfyUI
{
    /// <summary>
    /// ComfyUI 美术资产生成器 Editor 窗口
    /// </summary>
    public class ComfyUIWindow : EditorWindow
    {
        // ==========================================
        // EditorPrefs Keys
        // ==========================================
        private const string PrefServerUrl = "ComfyUI_ServerUrl";
        private const string PrefAssetTypeIndex = "ComfyUI_AssetTypeIndex";
        private const string PrefGenerationStage = "ComfyUI_GenerationStage";
        private const string PrefWidth = "ComfyUI_Width";
        private const string PrefHeight = "ComfyUI_Height";
        private const string PrefSteps = "ComfyUI_Steps";
        private const string PrefCfg = "ComfyUI_Cfg";
        private const string PrefSeed = "ComfyUI_Seed";
        private const string PrefOutputDir = "ComfyUI_OutputDir";
        private const string PrefFrameCount = "ComfyUI_FrameCount";
        private const string PrefAnimationState = "ComfyUI_AnimationState";

        // ==========================================
        // 状态
        // ==========================================
        private string serverUrl = "http://10.150.164.64:8188";
        private bool isConnected = false;
        private bool isTestingConnection = false;
        private bool isGenerating = false;

        private int selectedAssetTypeIndex = 0;
        private string[] assetTypeNames;

        // 生成阶段: 0 = 概念图, 1 = 精灵帧, 2 = 两阶段完整
        private int generationStage = 0;
        private readonly string[] stageOptions = { "概念图", "精灵帧", "两阶段完整" };

        private string positivePrompt = "";
        private string negativePrompt = "";
        private int width = 1024;
        private int height = 1024;
        private int steps = 20;
        private float cfg = 7f;
        private int seed = -1;
        private string outputDir = "";
        private int frameCount = 4;
        private string animationState = "idle";

        private string statusMessage = "就绪";
        private float progress = 0f;
        private bool showProgress = false;

        private Vector2 scrollPos;

        // ==========================================
        // 窗口入口
        // ==========================================

        [MenuItem("Tools/ComfyUI/Asset Generator")]
        public static void ShowWindow()
        {
            var window = GetWindow<ComfyUIWindow>("ComfyUI 美术资产生成器");
            window.minSize = new Vector2(480, 600);
            window.Show();
        }

        // ==========================================
        // Unity 生命周期
        // ==========================================

        private void OnEnable()
        {
            LoadSettings();
            RefreshAssetTypes();
        }

        private void OnDisable()
        {
            SaveSettings();
        }

        // ==========================================
        // 设置持久化
        // ==========================================

        private void LoadSettings()
        {
            serverUrl = EditorPrefs.GetString(PrefServerUrl, "http://10.150.164.64:8188");
            selectedAssetTypeIndex = EditorPrefs.GetInt(PrefAssetTypeIndex, 0);
            generationStage = EditorPrefs.GetInt(PrefGenerationStage, 0);
            width = EditorPrefs.GetInt(PrefWidth, 1024);
            height = EditorPrefs.GetInt(PrefHeight, 1024);
            steps = EditorPrefs.GetInt(PrefSteps, 20);
            cfg = EditorPrefs.GetFloat(PrefCfg, 7f);
            seed = EditorPrefs.GetInt(PrefSeed, -1);
            outputDir = EditorPrefs.GetString(PrefOutputDir, "");
            frameCount = EditorPrefs.GetInt(PrefFrameCount, 4);
            animationState = EditorPrefs.GetString(PrefAnimationState, "idle");
        }

        private void SaveSettings()
        {
            EditorPrefs.SetString(PrefServerUrl, serverUrl);
            EditorPrefs.SetInt(PrefAssetTypeIndex, selectedAssetTypeIndex);
            EditorPrefs.SetInt(PrefGenerationStage, generationStage);
            EditorPrefs.SetInt(PrefWidth, width);
            EditorPrefs.SetInt(PrefHeight, height);
            EditorPrefs.SetInt(PrefSteps, steps);
            EditorPrefs.SetFloat(PrefCfg, cfg);
            EditorPrefs.SetInt(PrefSeed, seed);
            EditorPrefs.SetString(PrefOutputDir, outputDir);
            EditorPrefs.SetInt(PrefFrameCount, frameCount);
            EditorPrefs.SetString(PrefAnimationState, animationState);
        }

        private void RefreshAssetTypes()
        {
            assetTypeNames = ComfyUIPromptTemplates.GetAllAssetTypes();
            if (selectedAssetTypeIndex >= assetTypeNames.Length)
                selectedAssetTypeIndex = 0;
        }

        // ==========================================
        // GUI 绘制
        // ==========================================

        private void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            DrawHeader();
            DrawServerSection();
            DrawAssetSection();
            DrawPromptSection();
            DrawGenerationParams();
            DrawActionButtons();
            DrawProgressSection();

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("ComfyUI 美术资产生成器", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        private void DrawServerSection()
        {
            EditorGUILayout.LabelField("服务器", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("地址:", GUILayout.Width(50));
            string newUrl = EditorGUILayout.TextField(serverUrl);
            if (newUrl != serverUrl)
            {
                serverUrl = newUrl;
                isConnected = false;
                SaveSettings();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("状态:", GUILayout.Width(50));

            // 连接状态指示器
            Color originalColor = GUI.color;
            GUI.color = isConnected ? Color.green : Color.red;
            EditorGUILayout.LabelField(isConnected ? "● 已连接" : "○ 未连接", GUILayout.Width(80));
            GUI.color = originalColor;

            GUI.enabled = !isTestingConnection && !isGenerating;
            if (GUILayout.Button("测试连接", GUILayout.Width(80)))
            {
                TestConnectionAsync();
            }
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        private void DrawAssetSection()
        {
            EditorGUILayout.LabelField("资产设置", EditorStyles.boldLabel);

            // 资产类型下拉
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("资产类型:", GUILayout.Width(70));
            int newIndex = EditorGUILayout.Popup(selectedAssetTypeIndex, assetTypeNames);
            if (newIndex != selectedAssetTypeIndex)
            {
                selectedAssetTypeIndex = newIndex;
                UpdatePromptsFromTemplate();
                SaveSettings();
            }
            EditorGUILayout.EndHorizontal();

            // 生成阶段
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("生成阶段:", GUILayout.Width(70));
            int newStage = EditorGUILayout.Popup(generationStage, stageOptions);
            if (newStage != generationStage)
            {
                generationStage = newStage;
                SaveSettings();
            }
            EditorGUILayout.EndHorizontal();

            // 精灵帧专属参数
            if (generationStage >= 1)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("帧数:", GUILayout.Width(70));
                int newFrames = EditorGUILayout.IntField(frameCount);
                if (newFrames != frameCount && newFrames > 0)
                {
                    frameCount = newFrames;
                    SaveSettings();
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("动画状态:", GUILayout.Width(70));
                string newAnim = EditorGUILayout.TextField(animationState);
                if (newAnim != animationState)
                {
                    animationState = newAnim;
                    SaveSettings();
                }
                EditorGUILayout.EndHorizontal();
            }

            // 输出目录
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("输出目录:", GUILayout.Width(70));
            string newDir = EditorGUILayout.TextField(outputDir);
            if (newDir != outputDir)
            {
                outputDir = newDir;
                SaveSettings();
            }
            if (GUILayout.Button("...", GUILayout.Width(30)))
            {
                string selected = EditorUtility.OpenFolderPanel("选择输出目录", "Assets/ArtMaterials/_Generated", "");
                if (!string.IsNullOrEmpty(selected))
                {
                    // 转换为相对路径
                    if (selected.Contains("Assets"))
                    {
                        int idx = selected.IndexOf("Assets", StringComparison.Ordinal);
                        outputDir = selected.Substring(idx);
                    }
                    else
                    {
                        outputDir = selected;
                    }
                    SaveSettings();
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        private void DrawPromptSection()
        {
            EditorGUILayout.LabelField("提示词", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("正向提示词:", EditorStyles.miniBoldLabel);
            Vector2 posSize = EditorGUILayout.GetControlRect(GUILayout.Height(80)).size;
            string newPositive = EditorGUILayout.TextArea(positivePrompt, GUILayout.Height(80));
            if (newPositive != positivePrompt)
            {
                positivePrompt = newPositive;
            }

            EditorGUILayout.LabelField("反向提示词:", EditorStyles.miniBoldLabel);
            string newNegative = EditorGUILayout.TextArea(negativePrompt, GUILayout.Height(60));
            if (newNegative != negativePrompt)
            {
                negativePrompt = newNegative;
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        private void DrawGenerationParams()
        {
            EditorGUILayout.LabelField("生成参数", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("宽度:", GUILayout.Width(50));
            int newWidth = EditorGUILayout.IntField(width);
            if (newWidth != width && newWidth > 0)
            {
                width = newWidth;
                SaveSettings();
            }
            EditorGUILayout.LabelField("高度:", GUILayout.Width(50));
            int newHeight = EditorGUILayout.IntField(height);
            if (newHeight != height && newHeight > 0)
            {
                height = newHeight;
                SaveSettings();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("步数:", GUILayout.Width(50));
            int newSteps = EditorGUILayout.IntField(steps);
            if (newSteps != steps && newSteps > 0)
            {
                steps = newSteps;
                SaveSettings();
            }
            EditorGUILayout.LabelField("CFG:", GUILayout.Width(50));
            float newCfg = EditorGUILayout.FloatField(cfg);
            if (Math.Abs(newCfg - cfg) > 0.01f && newCfg > 0)
            {
                cfg = newCfg;
                SaveSettings();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("种子:", GUILayout.Width(50));
            int newSeed = EditorGUILayout.IntField(seed);
            if (newSeed != seed)
            {
                seed = newSeed;
                SaveSettings();
            }
            if (seed < 0)
            {
                EditorGUILayout.LabelField("(随机)", EditorStyles.miniLabel);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        private void DrawActionButtons()
        {
            EditorGUILayout.BeginHorizontal();

            GUI.enabled = !isGenerating && isConnected;
            if (GUILayout.Button("生成", GUILayout.Height(30)))
            {
                StartGeneration();
            }

            GUI.enabled = true;

            if (GUILayout.Button("预览", GUILayout.Height(30)))
            {
                PreviewLatestImage();
            }

            GUI.enabled = isGenerating;
            if (GUILayout.Button("取消", GUILayout.Height(30)))
            {
                _ = CancelGeneration();
            }
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();
        }

        private void DrawProgressSection()
        {
            if (showProgress)
            {
                EditorGUILayout.Space(10);
                Rect progressRect = EditorGUILayout.GetControlRect(GUILayout.Height(20));
                EditorGUI.ProgressBar(progressRect, progress, $"{progress * 100:F0}%");
                EditorGUILayout.LabelField($"状态: {statusMessage}", EditorStyles.miniLabel);
            }
        }

        // ==========================================
        // 业务逻辑
        // ==========================================

        private void UpdatePromptsFromTemplate()
        {
            if (assetTypeNames == null || selectedAssetTypeIndex >= assetTypeNames.Length)
                return;

            string assetType = assetTypeNames[selectedAssetTypeIndex];
            positivePrompt = ComfyUIPromptTemplates.GetFullPositivePrompt(assetType);
            negativePrompt = ComfyUIPromptTemplates.GetNegativePrompt(assetType);
        }

        private async void TestConnectionAsync()
        {
            isTestingConnection = true;
            statusMessage = "正在测试连接...";
            Repaint();

            try
            {
                var client = new ComfyUIClient(serverUrl);
                isConnected = await client.TestConnection();
                statusMessage = isConnected ? "连接成功!" : "连接失败";
            }
            catch (Exception ex)
            {
                isConnected = false;
                statusMessage = $"连接错误: {ex.Message}";
                Debug.LogError($"[ComfyUI] 连接测试异常: {ex}");
            }
            finally
            {
                isTestingConnection = false;
                Repaint();
            }
        }

        private async void StartGeneration()
        {
            if (string.IsNullOrWhiteSpace(positivePrompt))
            {
                EditorUtility.DisplayDialog("错误", "请输入正向提示词", "确定");
                return;
            }

            isGenerating = true;
            showProgress = true;
            progress = 0f;
            statusMessage = "正在初始化...";

            try
            {
                var generator = new ComfyUIAssetGenerator(serverUrl);
                string assetType = assetTypeNames[selectedAssetTypeIndex];

                switch (generationStage)
                {
                    case 0: // 概念图
                        statusMessage = "正在生成概念图...";
                        progress = 0.1f;
                        Repaint();

                        string conceptPath = await generator.GenerateConceptArt(assetType, outputDir);
                        progress = 1f;
                        statusMessage = $"概念图生成完成!\n{conceptPath}";
                        ShowGenerationComplete(conceptPath);
                        break;

                    case 1: // 精灵帧
                        statusMessage = "正在生成精灵帧...";
                        progress = 0.1f;
                        Repaint();

                        string spritePath = await generator.GenerateSpriteFrames(
                            assetType, null, outputDir, frameCount, animationState);
                        progress = 1f;
                        statusMessage = $"精灵帧生成完成!\n{spritePath}";
                        ShowGenerationComplete(spritePath);
                        break;

                    case 2: // 两阶段完整
                        statusMessage = "阶段 1/2: 正在生成概念图...";
                        progress = 0.1f;
                        Repaint();

                        var (cp, sp) = await generator.GenerateFullPipeline(
                            assetType, frameCount, animationState);

                        progress = 1f;
                        statusMessage = $"两阶段生成完成!\n概念图: {cp}\n精灵帧: {sp}";
                        EditorUtility.DisplayDialog("生成完成",
                            $"概念图: {cp}\n精灵帧: {sp}", "确定");
                        Selection.activeObject = AssetDatabase.LoadAssetAtPath<Texture2D>(cp);
                        break;
                }
            }
            catch (Exception ex)
            {
                statusMessage = $"生成失败: {ex.Message}";
                Debug.LogError($"[ComfyUI] 生成异常: {ex}");
                EditorUtility.DisplayDialog("生成失败", ex.Message, "确定");
            }
            finally
            {
                isGenerating = false;
                showProgress = false;
                Repaint();
            }
        }

        private async Task CancelGeneration()
        {
            try
            {
                var client = new ComfyUIClient(serverUrl);
                await client.Interrupt();
                Debug.Log("[ComfyUI] 生成已取消");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ComfyUI] 取消失败: {ex.Message}");
            }
            isGenerating = false;
            showProgress = false;
            statusMessage = "已取消";
            Repaint();
        }

        private void ShowGenerationComplete(string assetPath)
        {
            EditorUtility.DisplayDialog("生成完成", $"图片已保存到:\n{assetPath}", "确定");

            // 尝试选中生成的资源
            if (!string.IsNullOrEmpty(assetPath))
            {
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
                if (tex != null)
                {
                    Selection.activeObject = tex;
                    EditorGUIUtility.PingObject(tex);
                }
            }
        }

        private void PreviewLatestImage()
        {
            string assetType = assetTypeNames[selectedAssetTypeIndex];
            string path = ComfyUIImageReceiver.GetConceptImagePath(assetType);

            if (string.IsNullOrEmpty(path))
            {
                EditorUtility.DisplayDialog("预览", "未找到概念图，请先生成。", "确定");
                return;
            }

            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex != null)
            {
                Selection.activeObject = tex;
                EditorGUIUtility.PingObject(tex);
            }
            else
            {
                EditorUtility.DisplayDialog("预览", $"无法加载图片: {path}", "确定");
            }
        }
    }
}
#endif