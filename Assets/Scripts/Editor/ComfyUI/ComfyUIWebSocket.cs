#if UNITY_EDITOR
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace ComfyUI
{
    /// <summary>
    /// ComfyUI WebSocket 客户端，用于实时接收生成进度和结果
    /// </summary>
    public class ComfyUIWebSocket
    {
        // ==========================================
        // 事件
        // ==========================================

        /// <summary>进度更新: (节点名, 当前值, 最大值)</summary>
        public event Action<string, int, int> OnProgress;

        /// <summary>图片就绪</summary>
        public event Action<ComfyUIClient.OutputImage> OnImageReady;

        /// <summary>执行完成 (prompt_id)</summary>
        public event Action<string> OnExecutionComplete;

        /// <summary>执行中状态 (node, prompt_id)</summary>
        public event Action<string, string> OnExecuting;

        /// <summary>错误</summary>
        public event Action<string> OnError;

        /// <summary>WebSocket 已连接</summary>
        public event Action OnConnected;

        /// <summary>WebSocket 已断开</summary>
        public event Action OnDisconnected;

        // ==========================================
        // 私有字段
        // ==========================================

        private ClientWebSocket ws;
        private CancellationTokenSource cts;
        private string serverUrl;
        private string clientId;
        private string wsUrl;

        private const int ReceiveBufferSize = 4096;
        private const int MaxReconnectAttempts = 3;
        private const int ReconnectDelayMs = 2000;

        // ==========================================
        // 构造函数
        // ==========================================

        /// <summary>
        /// 创建 WebSocket 客户端
        /// </summary>
        /// <param name="serverUrl">ComfyUI 服务器地址（如 http://10.150.164.64:8188）</param>
        /// <param name="clientId">客户端唯一标识</param>
        public ComfyUIWebSocket(string serverUrl, string clientId)
        {
            this.serverUrl = serverUrl.TrimEnd('/');
            this.clientId = clientId;

            // 将 http:// 转换为 ws://, https:// 转换为 wss://
            if (this.serverUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                wsUrl = this.serverUrl.Replace("https://", "wss://") + "/ws";
            }
            else if (this.serverUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            {
                wsUrl = this.serverUrl.Replace("http://", "ws://") + "/ws";
            }
            else
            {
                wsUrl = $"ws://{this.serverUrl}/ws";
            }
        }

        // ==========================================
        // 公共方法
        // ==========================================

        /// <summary>
        /// 连接到 ComfyUI WebSocket 服务器
        /// </summary>
        public async Task Connect()
        {
            try
            {
                ws = new ClientWebSocket();
                cts = new CancellationTokenSource();

                Debug.Log($"[ComfyUIWebSocket] 正在连接到: {wsUrl}?clientId={clientId}");

                var connectUri = new Uri($"{wsUrl}?clientId={clientId}");
                await ws.ConnectAsync(connectUri, cts.Token);

                if (ws.State == WebSocketState.Open)
                {
                    Debug.Log($"[ComfyUIWebSocket] 连接成功! clientId={clientId}");
                    OnConnected?.Invoke();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ComfyUIWebSocket] 连接失败: {ex.Message}");
                OnError?.Invoke($"WebSocket 连接失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 断开 WebSocket 连接
        /// </summary>
        public async Task Disconnect()
        {
            try
            {
                cts?.Cancel();

                if (ws != null && ws.State == WebSocketState.Open)
                {
                    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client disconnect", CancellationToken.None);
                    Debug.Log("[ComfyUIWebSocket] 已断开连接");
                }

                ws?.Dispose();
                ws = null;
                OnDisconnected?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ComfyUIWebSocket] 断开连接时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 监听指定 prompt_id 的执行过程
        /// 持续接收消息直到执行完成或取消
        /// </summary>
        public async Task ListenForPrompt(string promptId)
        {
            if (ws == null || ws.State != WebSocketState.Open)
            {
                Debug.LogError("[ComfyUIWebSocket] WebSocket 未连接，无法监听");
                OnError?.Invoke("WebSocket 未连接");
                return;
            }

            Debug.Log($"[ComfyUIWebSocket] 开始监听 prompt: {promptId}");

            var buffer = new byte[ReceiveBufferSize];
            var messageBuilder = new StringBuilder();

            try
            {
                int reconnectCount = 0;

                while (ws.State == WebSocketState.Open && !cts.Token.IsCancellationRequested)
                {
                    WebSocketReceiveResult result;
                    messageBuilder.Clear();

                    // 接收消息（可能分片）
                    do
                    {
                        result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token);

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            Debug.Log("[ComfyUIWebSocket] 服务器关闭了连接");
                            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server closed", CancellationToken.None);
                            OnDisconnected?.Invoke();

                            // 尝试重连
                            if (reconnectCount < MaxReconnectAttempts)
                            {
                                reconnectCount++;
                                Debug.Log($"[ComfyUIWebSocket] 尝试重连 ({reconnectCount}/{MaxReconnectAttempts})...");
                                await Task.Delay(ReconnectDelayMs);
                                await Connect();
                            }
                            return;
                        }

                        string chunk = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        messageBuilder.Append(chunk);

                    } while (!result.EndOfMessage);

                    string fullMessage = messageBuilder.ToString();
                    ProcessMessage(fullMessage, promptId);
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log("[ComfyUIWebSocket] 监听已取消");
            }
            catch (WebSocketException ex)
            {
                Debug.LogError($"[ComfyUIWebSocket] WebSocket 错误: {ex.Message}");
                OnError?.Invoke($"WebSocket 错误: {ex.Message}");

                // 尝试重连
                if (reconnectCount < MaxReconnectAttempts)
                {
                    reconnectCount++;
                    Debug.Log($"[ComfyUIWebSocket] 尝试重连 ({reconnectCount}/{MaxReconnectAttempts})...");
                    await Task.Delay(ReconnectDelayMs);
                    await Connect();
                    if (ws.State == WebSocketState.Open)
                    {
                        await ListenForPrompt(promptId);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ComfyUIWebSocket] 监听异常: {ex.Message}");
                OnError?.Invoke($"监听异常: {ex.Message}");
            }
        }

        // ==========================================
        // 消息解析
        // ==========================================

        /// <summary>
        /// 解析 WebSocket 收到的 JSON 消息并触发对应事件
        /// </summary>
        private void ProcessMessage(string json, string expectedPromptId)
        {
            if (string.IsNullOrWhiteSpace(json))
                return;

            try
            {
                // ComfyUI WebSocket 消息格式:
                // {"type": "progress", "data": {"value": 5, "max": 20}}
                // {"type": "executing", "data": {"node": "123", "prompt_id": "xxx"}}
                // {"type": "executing", "data": {"node": null, "prompt_id": "xxx"}}  ← 执行完成
                // {"type": "executed", "data": {"node": "9", "output": {"images": [{"filename":"...","subfolder":"...","type":"output"}]}, "prompt_id": "xxx"}}

                string type = ExtractJsonValue(json, "type");

                switch (type)
                {
                    case "progress":
                        int value = ExtractJsonInt(json, "value");
                        int max = ExtractJsonInt(json, "max");
                        Debug.Log($"[ComfyUIWebSocket] 进度: {value}/{max}");
                        OnProgress?.Invoke("KSampler", value, max);
                        break;

                    case "executing":
                        string node = ExtractJsonValue(json, "node");
                        string execPromptId = ExtractJsonValue(json, "prompt_id");
                        OnExecuting?.Invoke(node, execPromptId);

                        // node 为 null 表示执行完成
                        if (node == null || node == "null" || string.IsNullOrEmpty(node))
                        {
                            Debug.Log($"[ComfyUIWebSocket] 执行完成! prompt_id={execPromptId}");
                            OnExecutionComplete?.Invoke(execPromptId);
                        }
                        else
                        {
                            Debug.Log($"[ComfyUIWebSocket] 正在执行节点: {node}");
                        }
                        break;

                    case "executed":
                        // 解析输出的图片信息
                        ParseExecutedOutput(json);
                        break;

                    case "status":
                        // 状态消息，非关键
                        break;

                    default:
                        // 未知消息类型，记录日志（调试用）
                        if (json.Length < 200)
                        {
                            Debug.Log($"[ComfyUIWebSocket] 收到消息: {json}");
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ComfyUIWebSocket] 解析消息失败: {ex.Message}\n原始消息: {json.Substring(0, Math.Min(200, json.Length))}");
            }
        }

        /// <summary>
        /// 解析 executed 消息中的输出图片信息
        /// </summary>
        private void ParseExecutedOutput(string json)
        {
            // 查找 "images" 数组
            int imagesIdx = json.IndexOf("\"images\"", StringComparison.Ordinal);
            if (imagesIdx < 0) return;

            int arrayStart = json.IndexOf('[', imagesIdx);
            if (arrayStart < 0) return;

            // 逐项解析图片
            int searchPos = arrayStart + 1;
            while (searchPos < json.Length)
            {
                int objStart = json.IndexOf('{', searchPos);
                if (objStart < 0) break;

                int objEnd = json.IndexOf('}', objStart);
                if (objEnd < 0) break;

                string imageJson = json.Substring(objStart, objEnd - objStart + 1);

                string filename = ExtractJsonValue(imageJson, "filename");
                string subfolder = ExtractJsonValue(imageJson, "subfolder");
                string type = ExtractJsonValue(imageJson, "type");

                if (!string.IsNullOrEmpty(filename))
                {
                    var output = new ComfyUIClient.OutputImage
                    {
                        filename = filename,
                        subfolder = subfolder ?? "",
                        type = type ?? "output"
                    };
                    Debug.Log($"[ComfyUIWebSocket] 图片就绪: {filename}");
                    OnImageReady?.Invoke(output);
                }

                searchPos = objEnd + 1;

                // 检查是否还有更多图片
                int nextBrace = json.IndexOf('{', searchPos);
                int arrayEnd = json.IndexOf(']', searchPos);
                if (arrayEnd < 0 || (nextBrace >= 0 && nextBrace > arrayEnd)) break;
            }
        }

        // ==========================================
        // JSON 辅助解析
        // ==========================================

        /// <summary>
        /// 从 JSON 字符串中提取指定 key 的字符串值
        /// </summary>
        private static string ExtractJsonValue(string json, string key)
        {
            string searchKey = $"\"{key}\"";
            int keyIdx = json.IndexOf(searchKey, StringComparison.Ordinal);
            if (keyIdx < 0) return null;

            int colonIdx = json.IndexOf(':', keyIdx);
            if (colonIdx < 0) return null;

            // 跳过冒号后的空白
            int start = colonIdx + 1;
            while (start < json.Length && (json[start] == ' ' || json[start] == '\t'))
                start++;

            if (start >= json.Length) return null;

            // 字符串值
            if (json[start] == '"')
            {
                int endQuote = start + 1;
                while (endQuote < json.Length)
                {
                    if (json[endQuote] == '"' && json[endQuote - 1] != '\\')
                        break;
                    endQuote++;
                }
                return json.Substring(start + 1, endQuote - start - 1);
            }

            // null 值
            if (json[start] == 'n' && json.Substring(start, 4) == "null")
                return null;

            // 数字或布尔值也返回文本
            int end = start;
            while (end < json.Length && (char.IsLetterOrDigit(json[end]) || json[end] == '.' || json[end] == '-'))
                end++;
            return json.Substring(start, end - start);
        }

        /// <summary>
        /// 从 JSON 字符串中提取指定 key 的整数值
        /// </summary>
        private static int ExtractJsonInt(string json, string key)
        {
            string val = ExtractJsonValue(json, key);
            if (int.TryParse(val, out int result))
                return result;
            return 0;
        }
    }
}
#endif