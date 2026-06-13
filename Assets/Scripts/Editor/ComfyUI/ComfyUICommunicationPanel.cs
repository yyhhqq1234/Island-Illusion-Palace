#if UNITY_EDITOR
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json.Linq;

namespace ComfyUI
{
    /// <summary>
    /// ComfyUI 通信面板 — 使用前必须先打开此面板
    /// 连接到服务器 8189 端口，实时显示服务器状态、GPU 信息、优化建议
    /// </summary>
    public class ComfyUICommunicationPanel : EditorWindow
    {
        // ==========================================
        // 单例
        // ==========================================
        public static ComfyUICommunicationPanel Instance { get; private set; }

        // ==========================================
        // 配置
        // ==========================================
        private const string DefaultCommUrl = "ws://10.150.164.64:8189";
        private const float PollInterval = 5f;

        // ==========================================
        // 状态
        // ==========================================
        private string commWsUrl = DefaultCommUrl;

        private bool isConnected = false;
        private bool isConnecting = false;

        // 服务器状态数据
        private string gpuName = "N/A";
        private long vramUsed = 0;
        private long vramTotal = 0;
        private long vramFree = 0;
        private int queueRunning = 0;
        private int queuePending = 0;
        private int connectedClients = 0;
        private double uptime = 0;
        private string statusLevel = "未知";
        private string[] optimizationTips = Array.Empty<string>();

        private string statusMessage = "未连接";

        // WebSocket
        private ClientWebSocket ws;
        private CancellationTokenSource wsCts;
        private float pollTimer = 0f;

        private Vector2 scrollPos;

        // ==========================================
        // 窗口入口
        // ==========================================

        [MenuItem("Tools/ComfyUI/通信面板")]
        public static void ShowWindow()
        {
            var window = GetWindow<ComfyUICommunicationPanel>("ComfyUI 通信面板");
            window.minSize = new Vector2(350, 400);
            window.Show();
        }

        /// <summary>
        /// 确保通信面板已打开（供其他 ComfyUI 窗口调用）
        /// </summary>
        public static void EnsureOpen()
        {
            if (Instance == null)
            {
                ShowWindow();
            }
            else
            {
                Instance.Focus();
            }
        }

        // ==========================================
        // Unity 生命周期
        // ==========================================

        private void OnEnable()
        {
            Instance = this;
            EditorApplication.update += OnEditorUpdate;
            _ = ConnectAsync();
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
            _ = DisconnectAsync();
            if (Instance == this)
                Instance = null;
        }

        private void OnEditorUpdate()
        {
            pollTimer += Time.deltaTime;
            if (pollTimer >= PollInterval)
            {
                pollTimer = 0f;
                if (!isConnected && !isConnecting)
                {
                    _ = ConnectAsync();
                }
            }
            Repaint();
        }

        // ==========================================
        // WebSocket 通信
        // ==========================================

        private async Task ConnectAsync()
        {
            isConnecting = true;
            statusMessage = "正在连接通信服务器...";

            try
            {
                wsCts = new CancellationTokenSource();
                ws = new ClientWebSocket();

                var uri = new Uri(commWsUrl);
                await ws.ConnectAsync(uri, wsCts.Token);

                if (ws.State == WebSocketState.Open)
                {
                    isConnected = true;
                    statusMessage = "已连接";
                    Debug.Log("[ComfyUI通信面板] 已连接到 8189 端口");

                    // 开始接收消息
                    _ = ReceiveLoopAsync();
                }
            }
            catch (Exception ex)
            {
                isConnected = false;
                statusMessage = $"连接失败: {ex.Message}";
                Debug.LogWarning($"[ComfyUI通信面板] 连接失败 (8189): {ex.Message}");
            }
            finally
            {
                isConnecting = false;
            }
        }

        private async Task DisconnectAsync()
        {
            try
            {
                wsCts?.Cancel();
                if (ws != null && ws.State == WebSocketState.Open)
                {
                    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Panel closed", CancellationToken.None);
                }
                ws?.Dispose();
                ws = null;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ComfyUI通信面板] 断开异常: {ex.Message}");
            }
            finally
            {
                isConnected = false;
                statusMessage = "已断开";
            }
        }

        private async Task ReceiveLoopAsync()
        {
            var buffer = new byte[8192];
            var messageBuilder = new StringBuilder();

            try
            {
                while (ws != null && ws.State == WebSocketState.Open && !wsCts.Token.IsCancellationRequested)
                {
                    messageBuilder.Clear();
                    WebSocketReceiveResult result;

                    do
                    {
                        result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), wsCts.Token);

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            Debug.Log("[ComfyUI通信面板] 服务器关闭了连接");
                            isConnected = false;
                            statusMessage = "服务器关闭连接";
                            return;
                        }

                        messageBuilder.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
                    } while (!result.EndOfMessage);

                    string json = messageBuilder.ToString();
                    ProcessMessage(json);
                }
            }
            catch (OperationCanceledException)
            {
                // 正常取消
            }
            catch (WebSocketException ex)
            {
                Debug.LogWarning($"[ComfyUI通信面板] WebSocket 错误: {ex.Message}");
                isConnected = false;
                statusMessage = $"连接中断: {ex.Message}";
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ComfyUI通信面板] 接收异常: {ex.Message}");
                isConnected = false;
                statusMessage = $"异常: {ex.Message}";
            }
        }

        private void ProcessMessage(string json)
        {
            try
            {
                var jObj = JObject.Parse(json);
                string type = jObj["type"]?.ToString();

                switch (type)
                {
                    case "welcome":
                        var data = jObj["data"];
                        clientId = data?["client_id"]?.ToString() ?? "";
                        Debug.Log($"[ComfyUI通信面板] 欢迎! client_id={clientId}");
                        break;

                    case "server_status":
                        ParseServerStatus(jObj["data"] as JObject);
                        break;

                    case "optimization_tip":
                        var tip = jObj["data"]?["message"]?.ToString();
                        if (!string.IsNullOrEmpty(tip))
                        {
                            Debug.Log($"[ComfyUI通信面板] 优化建议: {tip}");
                        }
                        break;

                    case "pong":
                        // 心跳响应
                        break;

                    case "ack":
                        Debug.Log($"[ComfyUI通信面板] 操作确认: {jObj["data"]?["message"]}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ComfyUI通信面板] 消息解析失败: {ex.Message}");
            }
        }

        private void ParseServerStatus(JObject data)
        {
            if (data == null) return;

            try
            {
                var gpu = data["gpu"] as JObject;
                if (gpu != null)
                {
                    gpuName = gpu["name"]?.ToString() ?? gpuName;
                    vramUsed = gpu["vram_used"]?.Value<long>() ?? vramUsed;
                    vramTotal = gpu["vram_total"]?.Value<long>() ?? vramTotal;
                    vramFree = gpu["vram_free"]?.Value<long>() ?? vramFree;
                }

                var queue = data["queue"] as JObject;
                if (queue != null)
                {
                    queueRunning = queue["running"]?.ToObject<JArray>()?.Count ?? 0;
                    queuePending = queue["pending"]?.ToObject<JArray>()?.Count ?? 0;
                }

                connectedClients = data["connected_clients"]?.Value<int>() ?? connectedClients;
                uptime = data["uptime"]?.Value<double>() ?? uptime;

                // 优化建议
                var tips = data["optimization_tips"] as JArray;
                if (tips != null)
                {
                    optimizationTips = new string[tips.Count];
                    for (int i = 0; i < tips.Count; i++)
                    {
                        var tipObj = tips[i] as JObject;
                        if (tipObj != null)
                        {
                            string level = tipObj["level"]?.ToString() ?? "info";
                            string msg = tipObj["message"]?.ToString() ?? "";
                            optimizationTips[i] = $"[{level}] {msg}";
                        }
                    }
                }

                // 确定状态等级
                if (optimizationTips.Length > 0)
                {
                    foreach (var tip in optimizationTips)
                    {
                        if (tip.Contains("[warning]"))
                        {
                            statusLevel = "警告";
                            break;
                        }
                        statusLevel = "正常";
                    }
                }
                else
                {
                    statusLevel = "正常";
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ComfyUI通信面板] 解析 server_status 失败: {ex.Message}");
            }
        }

        // ==========================================
        // GUI 绘制
        // ==========================================

        private string clientId = "";

        private void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            DrawHeader();
            DrawConnectionStatus();
            DrawGpuInfo();
            DrawQueueInfo();
            DrawClientsInfo();
            DrawOptimizationTips();

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("ComfyUI 通信面板", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        private void DrawConnectionStatus()
        {
            EditorGUILayout.LabelField("连接状态", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("8189 端口:", GUILayout.Width(80));

            Color originalColor = GUI.color;
            GUI.color = isConnected ? Color.green : (isConnecting ? Color.yellow : Color.red);
            string statusText = isConnected ? "● 已连接" : (isConnecting ? "◉ 连接中..." : "○ 未连接");
            EditorGUILayout.LabelField(statusText, GUILayout.Width(100));
            GUI.color = originalColor;

            GUI.enabled = !isConnecting;
            if (GUILayout.Button(isConnected ? "重连" : "连接", GUILayout.Width(60)))
            {
                _ = DisconnectAsync().ContinueWith(_ => ConnectAsync());
            }
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(clientId))
            {
                EditorGUILayout.LabelField($"Client ID: {clientId}", EditorStyles.miniLabel);
            }

            EditorGUILayout.LabelField(statusMessage, EditorStyles.miniLabel);
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        private void DrawGpuInfo()
        {
            EditorGUILayout.LabelField("GPU 状态", EditorStyles.boldLabel);

            EditorGUILayout.LabelField($"GPU: {gpuName}");

            // VRAM 进度条
            if (vramTotal > 0)
            {
                float ratio = (float)vramUsed / vramTotal;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("显存:", GUILayout.Width(50));
                Rect progressRect = EditorGUILayout.GetControlRect(GUILayout.Height(18));
                Color barColor = ratio > 0.85f ? Color.red : (ratio > 0.6f ? Color.yellow : Color.green);
                Color origBg = GUI.backgroundColor;
                GUI.backgroundColor = barColor;
                EditorGUI.ProgressBar(progressRect, ratio, $"{vramUsed}/{vramTotal} MB ({ratio * 100:F0}%)");
                GUI.backgroundColor = origBg;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.LabelField($"  已用: {vramUsed} MB | 空闲: {vramFree} MB", EditorStyles.miniLabel);
            }
            else
            {
                EditorGUILayout.LabelField("显存: 等待数据...", EditorStyles.miniLabel);
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        private void DrawQueueInfo()
        {
            EditorGUILayout.LabelField("任务队列", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"运行中: {queueRunning}", GUILayout.Width(100));
            EditorGUILayout.LabelField($"等待中: {queuePending}", GUILayout.Width(100));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        private void DrawClientsInfo()
        {
            EditorGUILayout.LabelField("服务器信息", EditorStyles.boldLabel);

            EditorGUILayout.LabelField($"已连接客户端: {connectedClients}");
            EditorGUILayout.LabelField($"运行时间: {FormatUptime(uptime)}");
            EditorGUILayout.LabelField($"服务器状态: {statusLevel}");

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        private void DrawOptimizationTips()
        {
            if (optimizationTips.Length == 0) return;

            EditorGUILayout.LabelField("优化建议", EditorStyles.boldLabel);

            foreach (var tip in optimizationTips)
            {
                if (tip.Contains("[warning]"))
                {
                    GUI.color = new Color(1f, 0.8f, 0.3f);
                }
                else if (tip.Contains("[success]"))
                {
                    GUI.color = Color.green;
                }
                else
                {
                    GUI.color = Color.white;
                }

                EditorGUILayout.LabelField(tip, EditorStyles.wordWrappedLabel);
                GUI.color = Color.white;
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        // ==========================================
        // 辅助方法
        // ==========================================

        private static string FormatUptime(double seconds)
        {
            if (seconds < 60) return $"{seconds:F0}秒";
            if (seconds < 3600) return $"{seconds / 60:F0}分{seconds % 60:F0}秒";
            var ts = TimeSpan.FromSeconds(seconds);
            return $"{(int)ts.TotalHours}时{ts.Minutes}分{ts.Seconds}秒";
        }
    }
}
#endif