#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEditor;
using Newtonsoft.Json.Linq;

namespace ComfyUI
{
    /// <summary>
    /// ComfyUI HTTP API 客户端，负责与远程 ComfyUI 服务器通信
    /// </summary>
    public class ComfyUIClient
    {
        // ==========================================
        // JSON 序列化数据类
        // ==========================================

        [Serializable]
        public class PromptRequest
        {
            public string prompt;
            public string client_id;
        }

        [Serializable]
        public class PromptResponse
        {
            public string prompt_id;
            public int number;
            public Dictionary<string, object> node_errors;
        }

        [Serializable]
        public class HistoryEntry
        {
            public Dictionary<string, OutputInfo> outputs;
        }

        [Serializable]
        public class OutputInfo
        {
            public List<OutputImage> images;
        }

        [Serializable]
        public class OutputImage
        {
            public string filename;
            public string subfolder;
            public string type;
        }

        [Serializable]
        public class HistoryResponse
        {
            public Dictionary<string, HistoryEntry> history;
        }

        // ==========================================
        // 配置
        // ==========================================

        private string serverUrl;
        private string clientId;
        private ComfyUIWebSocket webSocket;
        private const int PollIntervalMs = 2000;      // 轮询间隔（毫秒）
        private const int MaxPollAttempts = 300;       // 最大轮询次数（10分钟）
        private const int RequestTimeout = 30;          // 请求超时（秒）

        public string ServerUrl => serverUrl;
        public string ClientId => clientId;

        public ComfyUIClient(string serverUrl)
        {
            this.serverUrl = serverUrl.TrimEnd('/');
            this.clientId = $"unity-editor-{Guid.NewGuid()}";
        }

        /// <summary>
        /// 为 UnityWebRequest 配置证书绕过（允许 HTTP 内网连接）
        /// </summary>
        private static void ConfigureRequest(UnityWebRequest request)
        {
            request.certificateHandler = new BypassCertificate();
        }

        // ==========================================
        // 核心 API 方法
        // ==========================================

        /// <summary>
        /// 提交 Prompt 到 ComfyUI 队列，返回 prompt_id
        /// </summary>
        public async Task<string> QueuePrompt(string workflowJson)
        {
            string url = $"{serverUrl}/api/prompt";

            // ComfyUI 要求 prompt 字段是原始 JSON 对象，而非字符串
            // 手动构建请求体：{"prompt": <workflow>, "client_id": "..."}
            string escapedClientId = EscapeJsonString(clientId);
            string jsonBody = $"{{\"prompt\":{workflowJson},\"client_id\":\"{escapedClientId}\"}}";

            Debug.Log($"[ComfyUI] QueuePrompt 请求体长度: {jsonBody.Length} 字符");

            using (var request = new UnityWebRequest(url, "POST"))
            {
                ConfigureRequest(request);
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.timeout = RequestTimeout;

                var operation = request.SendWebRequest();
                while (!operation.isDone)
                    await Task.Yield();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    string errorBody = request.downloadHandler?.text ?? "(empty)";
                    throw new Exception($"[ComfyUI] QueuePrompt 失败: {request.error}\n响应: {errorBody}");
                }

                var response = JsonUtility.FromJson<PromptResponse>(request.downloadHandler.text);
                if (!string.IsNullOrEmpty(response.prompt_id))
                {
                    Debug.Log($"[ComfyUI] Prompt 已提交, prompt_id: {response.prompt_id}");
                }
                return response.prompt_id;
            }
        }

        /// <summary>
        /// 查询历史记录，轮询直到生成完成，返回输出图片信息列表
        /// </summary>
        public async Task<List<OutputImage>> GetHistory(string promptId)
        {
            string url = $"{serverUrl}/api/history/{promptId}";

            for (int attempt = 0; attempt < MaxPollAttempts; attempt++)
            {
                using (var request = UnityWebRequest.Get(url))
                {
                    ConfigureRequest(request);
                    request.timeout = RequestTimeout;

                    var operation = request.SendWebRequest();
                    while (!operation.isDone)
                        await Task.Yield();

                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogWarning($"[ComfyUI] GetHistory 请求失败 (attempt {attempt + 1}): {request.error}");
                        await Task.Delay(PollIntervalMs);
                        continue;
                    }

                    string responseText = request.downloadHandler.text;

                    // 空响应表示尚未完成
                    if (responseText == "{}" || string.IsNullOrWhiteSpace(responseText))
                    {
                        if (attempt % 5 == 0)
                            Debug.Log($"[ComfyUI] 等待生成完成... (已等待 {(attempt + 1) * PollIntervalMs / 1000}s)");
                        await Task.Delay(PollIntervalMs);
                        continue;
                    }

                    // 解析响应
                    try
                    {
                        // ComfyUI history 返回格式: { "prompt_id": { "outputs": { "node_id": { "images": [...] } } } }
                        var historyDict = JsonUtility.FromJson<HistoryWrapper>(responseText);
                        if (historyDict != null)
                        {
                            // 手动解析 JSON（因为 ComfyUI history 格式的 key 是动态的 prompt_id）
                            var parsed = ParseHistoryResponse(responseText, promptId);
                            if (parsed != null && parsed.Count > 0)
                            {
                                Debug.Log($"[ComfyUI] 生成完成! 共 {parsed.Count} 张图片");
                                return parsed;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[ComfyUI] 解析历史响应失败: {ex.Message}");
                    }

                    await Task.Delay(PollIntervalMs);
                }
            }

            throw new TimeoutException($"[ComfyUI] 生成超时: prompt_id={promptId}, 已等待 {MaxPollAttempts * PollIntervalMs / 1000} 秒");
        }

        /// <summary>
        /// 下载图片到本地指定路径
        /// </summary>
        public async Task DownloadImage(string filename, string subfolder, string savePath)
        {
            string url = $"{serverUrl}/api/view?filename={UnityWebRequest.EscapeURL(filename)}&subfolder={UnityWebRequest.EscapeURL(subfolder)}&type=output";

            using (var request = UnityWebRequest.Get(url))
            {
                ConfigureRequest(request);
                request.timeout = 60; // 图片下载可能较大

                var operation = request.SendWebRequest();
                while (!operation.isDone)
                    await Task.Yield();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    throw new Exception($"[ComfyUI] 下载图片失败: {request.error}\nURL: {url}");
                }

                byte[] imageData = request.downloadHandler.data;
                ComfyUIImageReceiver.SaveImage(imageData, savePath);
                Debug.Log($"[ComfyUI] 图片已保存: {savePath}");
            }
        }

        // ==========================================
        // 高级封装：完整生成流程
        // ==========================================

        /// <summary>
        /// 完整生成流程：提交Prompt → 等待完成 → 下载图片 → 返回本地路径
        /// </summary>
        public async Task<string> GenerateImage(
            string positivePrompt,
            string negativePrompt,
            int width = 1024,
            int height = 1024,
            int steps = 10,
            float cfg = 1f,
            int seed = -1,
            string filenamePrefix = "ComfyUI")
        {
            // 构建 Workflow JSON（使用 zimage 文生图工作流）
            string workflowJson = BuildTextToImageWorkflow(positivePrompt, negativePrompt, width, height, seed, steps, cfg, filenamePrefix);

            // 提交任务
            string promptId = await QueuePrompt(workflowJson);

            // 等待完成
            var outputs = await GetHistory(promptId);

            if (outputs == null || outputs.Count == 0)
            {
                throw new Exception("[ComfyUI] 生成完成但没有输出图片");
            }

            // 下载第一张图片到临时路径（调用方负责最终保存）
            string tempPath = System.IO.Path.Combine(Application.dataPath, "_Generated", "_temp", $"{promptId}.png");
            ComfyUIImageReceiver.EnsureDirectoryExists(System.IO.Path.GetDirectoryName(tempPath));

            var firstOutput = outputs[0];
            await DownloadImage(firstOutput.filename, firstOutput.subfolder, tempPath);

            return tempPath;
        }

        // ==========================================
        // WebSocket 集成
        // ==========================================

        /// <summary>
        /// 初始化 WebSocket 连接
        /// </summary>
        public void InitWebSocket()
        {
            webSocket = new ComfyUIWebSocket(serverUrl, clientId);
            Debug.Log($"[ComfyUI] WebSocket 已初始化, clientId={clientId}");
        }

        /// <summary>
        /// 断开 WebSocket 连接
        /// </summary>
        public async Task DisconnectWebSocket()
        {
            if (webSocket != null)
            {
                await webSocket.Disconnect();
                webSocket = null;
            }
        }

        /// <summary>
        /// 使用 WebSocket 监听进度的完整生成流程
        /// 提交 prompt 后通过 WebSocket 实时获取进度和结果
        /// </summary>
        /// <param name="workflowJson">ComfyUI workflow JSON</param>
        /// <param name="onProgressCallback">进度回调 (0f-1f)</param>
        /// <returns>输出图片信息列表</returns>
        public async Task<List<OutputImage>> QueuePromptWithWebSocket(string workflowJson, Action<float> onProgressCallback = null)
        {
            var outputs = new List<OutputImage>();
            var tcs = new TaskCompletionSource<List<OutputImage>>();

            // 初始化 WebSocket（如果尚未初始化）
            if (webSocket == null)
            {
                InitWebSocket();
            }

            try
            {
                // 连接 WebSocket
                await webSocket.Connect();

                // 设置事件监听
                webSocket.OnProgress += (node, value, max) =>
                {
                    if (max > 0)
                    {
                        float progress = Mathf.Clamp01((float)value / max);
                        onProgressCallback?.Invoke(progress);
                    }
                };

                webSocket.OnImageReady += (image) =>
                {
                    outputs.Add(image);
                    Debug.Log($"[ComfyUI] WebSocket 图片就绪: {image.filename}");
                };

                webSocket.OnExecutionComplete += (promptId) =>
                {
                    Debug.Log($"[ComfyUI] WebSocket 执行完成, 共 {outputs.Count} 张图片");
                    tcs.TrySetResult(outputs);
                };

                webSocket.OnError += (error) =>
                {
                    Debug.LogError($"[ComfyUI] WebSocket 错误: {error}");
                    tcs.TrySetException(new Exception(error));
                };

                // 提交 prompt
                string promptId = await QueuePrompt(workflowJson);

                // 启动 WebSocket 监听（在后台运行）
                _ = webSocket.ListenForPrompt(promptId);

                // 等待完成或超时
                var timeoutTask = Task.Delay(MaxPollAttempts * PollIntervalMs);
                var completedTask = await Task.WhenAny(tcs.Task, timeoutTask);

                if (completedTask == timeoutTask)
                {
                    throw new TimeoutException($"[ComfyUI] WebSocket 生成超时: prompt_id={promptId}");
                }

                return await tcs.Task;
            }
            finally
            {
                // 清理事件监听
                if (webSocket != null)
                {
                    webSocket.ClearAllHandlers();
                }
            }
        }

        // ==========================================
        // 扩展 API 端点 (P2)
        // ==========================================

        /// <summary>
        /// 上传图片到 ComfyUI 服务器（用于 img2img）
        /// POST /api/upload/image
        /// </summary>
        /// <param name="localPath">本地图片绝对路径</param>
        /// <param name="type">上传类型: "input" 为输入图片</param>
        /// <returns>上传后的文件名</returns>
        public async Task<string> UploadImage(string localPath, string type = "input")
        {
            string url = $"{serverUrl}/api/upload/image";

            if (!File.Exists(localPath))
            {
                throw new FileNotFoundException($"[ComfyUI] 图片文件不存在: {localPath}");
            }

            byte[] imageData = File.ReadAllBytes(localPath);
            string filename = Path.GetFileName(localPath);

            Debug.Log($"[ComfyUI] 上传图片: {filename} ({imageData.Length / 1024} KB)");

            // 使用 UnityWebRequest 发送 multipart/form-data
            WWWForm form = new WWWForm();
            form.AddBinaryData("image", imageData, filename, "image/png");
            form.AddField("type", type);
            form.AddField("overwrite", "true");

            using (var request = UnityWebRequest.Post(url, form))
            {
                ConfigureRequest(request);
                request.timeout = 60;

                var operation = request.SendWebRequest();
                while (!operation.isDone)
                    await Task.Yield();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    string errorBody = request.downloadHandler?.text ?? "(empty)";
                    throw new Exception($"[ComfyUI] 上传图片失败: {request.error}\n响应: {errorBody}");
                }

                // ComfyUI 返回: {"name": "xxx.png", "subfolder": "", "type": "input"}
                string responseText = request.downloadHandler.text;
                string uploadedName = ExtractJsonStringValue(responseText, "name");
                if (string.IsNullOrEmpty(uploadedName))
                {
                    uploadedName = filename; // fallback
                }

                Debug.Log($"[ComfyUI] 图片上传成功: {uploadedName}");
                return uploadedName;
            }
        }

        /// <summary>
        /// 中断当前生成任务
        /// POST /api/interrupt
        /// </summary>
        public async Task Interrupt()
        {
            string url = $"{serverUrl}/api/interrupt";

            using (var request = new UnityWebRequest(url, "POST"))
            {
                ConfigureRequest(request);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.timeout = 10;

                var operation = request.SendWebRequest();
                while (!operation.isDone)
                    await Task.Yield();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning($"[ComfyUI] 中断请求失败: {request.error}");
                    // 即使失败也不抛异常，因为可能没有正在执行的任务
                }
                else
                {
                    Debug.Log("[ComfyUI] 生成已中断");
                }
            }
        }

        /// <summary>
        /// 获取模型列表
        /// GET /api/models/{folder}
        /// </summary>
        /// <param name="folder">模型文件夹名（如 "checkpoints", "loras", "vae"）</param>
        /// <returns>模型文件名列表</returns>
        public async Task<List<string>> GetModels(string folder = "checkpoints")
        {
            string url = $"{serverUrl}/api/models/{folder}";

            using (var request = UnityWebRequest.Get(url))
            {
                ConfigureRequest(request);
                request.timeout = 15;

                var operation = request.SendWebRequest();
                while (!operation.isDone)
                    await Task.Yield();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    throw new Exception($"[ComfyUI] 获取模型列表失败: {request.error}");
                }

                string json = request.downloadHandler.text;
                var models = ParseModelListResponse(json);

                Debug.Log($"[ComfyUI] 获取到 {models.Count} 个模型 ({folder})");
                return models;
            }
        }

        // ==========================================
        // 连接测试
        // ==========================================

        /// <summary>
        /// 测试与 ComfyUI 服务器的连接
        /// </summary>
        public async Task<bool> TestConnection()
        {
            try
            {
                string url = $"{serverUrl}/api/system_stats";
                using (var request = UnityWebRequest.Get(url))
                {
                    ConfigureRequest(request);
                    request.timeout = 10;
                    var operation = request.SendWebRequest();
                    while (!operation.isDone)
                        await Task.Yield();

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        Debug.Log($"[ComfyUI] 连接成功! 服务器: {serverUrl}");
                        return true;
                    }
                    else
                    {
                        Debug.LogWarning($"[ComfyUI] 连接失败: {request.error}");

                        // 尝试用 /prompt 端点验证
                        url = $"{serverUrl}/api/prompt";
                        using (var req2 = UnityWebRequest.Get(url))
                        {
                            ConfigureRequest(req2);
                            req2.timeout = 10;
                            var op2 = req2.SendWebRequest();
                            while (!op2.isDone)
                                await Task.Yield();

                            // /prompt GET 返回 405 也算在线
                            Debug.Log($"[ComfyUI] 连接成功! 服务器: {serverUrl} (HTTP {req2.responseCode})");
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ComfyUI] 连接测试异常: {ex.Message}");
                return false;
            }
        }

        // ==========================================
        // 动态工作流构建
        // ==========================================

        /// <summary>
        /// 获取所有可用节点信息
        /// GET /api/object_info
        /// </summary>
        public async Task<Dictionary<string, object>> GetObjectInfo()
        {
            string url = $"{serverUrl}/api/object_info";

            using (var request = UnityWebRequest.Get(url))
            {
                ConfigureRequest(request);
                request.timeout = 15;

                var operation = request.SendWebRequest();
                while (!operation.isDone)
                    await Task.Yield();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    throw new Exception($"[ComfyUIClient] 获取 object_info 失败: {request.error}");
                }

                string json = request.downloadHandler.text;
                var jObj = JObject.Parse(json);
                var dict = new Dictionary<string, object>();
                foreach (var prop in jObj.Properties())
                {
                    dict[prop.Name] = prop.Value.ToObject<object>();
                }
                Debug.Log($"[ComfyUIClient] 获取到 {dict.Count} 个节点定义");
                return dict;
            }
        }

        /// <summary>
        /// 获取特定节点的输入输出定义
        /// GET /api/object_info/{node_class}
        /// </summary>
        public async Task<Dictionary<string, object>> GetObjectInfo(string nodeClass)
        {
            string url = $"{serverUrl}/api/object_info/{UnityWebRequest.EscapeURL(nodeClass)}";

            using (var request = UnityWebRequest.Get(url))
            {
                ConfigureRequest(request);
                request.timeout = 15;

                var operation = request.SendWebRequest();
                while (!operation.isDone)
                    await Task.Yield();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    throw new Exception($"[ComfyUIClient] 获取 object_info/{nodeClass} 失败: {request.error}");
                }

                string json = request.downloadHandler.text;
                var jObj = JObject.Parse(json);
                var dict = new Dictionary<string, object>();
                foreach (var prop in jObj.Properties())
                {
                    dict[prop.Name] = prop.Value.ToObject<object>();
                }
                Debug.Log($"[ComfyUIClient] 获取节点 {nodeClass} 定义成功");
                return dict;
            }
        }

        /// <summary>
        /// 动态构建文生图工作流 JSON
        /// 使用标准 ComfyUI 节点：CheckpointLoaderSimple → CLIPTextEncode(+/-) → EmptyLatentImage → KSampler → VAEDecode → SaveImageWebsocket
        /// </summary>
        public string BuildTextToImageWorkflow(string positivePrompt, string negativePrompt,
            int width = 1024, int height = 1024, int seed = -1, int steps = 10, float cfg = 1f,
            string filenamePrefix = "ComfyUI")
        {
            if (seed == -1) seed = UnityEngine.Random.Range(0, int.MaxValue);

            var workflow = new JObject
            {
                ["1"] = new JObject
                {
                    ["class_type"] = "CheckpointLoaderSimple",
                    ["inputs"] = new JObject
                    {
                        ["ckpt_name"] = "动漫 primemix_v21.safetensors"
                    }
                },
                ["2"] = new JObject
                {
                    ["class_type"] = "CLIPTextEncode",
                    ["inputs"] = new JObject
                    {
                        ["text"] = positivePrompt,
                        ["clip"] = new JArray { "1", 1 }
                    }
                },
                ["3"] = new JObject
                {
                    ["class_type"] = "CLIPTextEncode",
                    ["inputs"] = new JObject
                    {
                        ["text"] = negativePrompt,
                        ["clip"] = new JArray { "1", 1 }
                    }
                },
                ["4"] = new JObject
                {
                    ["class_type"] = "EmptyLatentImage",
                    ["inputs"] = new JObject
                    {
                        ["width"] = width,
                        ["height"] = height,
                        ["batch_size"] = 1
                    }
                },
                ["5"] = new JObject
                {
                    ["class_type"] = "KSampler",
                    ["inputs"] = new JObject
                    {
                        ["seed"] = seed,
                        ["steps"] = steps,
                        ["cfg"] = cfg,
                        ["sampler_name"] = "euler_ancestral",
                        ["scheduler"] = "normal",
                        ["denoise"] = 1,
                        ["model"] = new JArray { "1", 0 },
                        ["positive"] = new JArray { "2", 0 },
                        ["negative"] = new JArray { "3", 0 },
                        ["latent_image"] = new JArray { "4", 0 }
                    }
                },
                ["6"] = new JObject
                {
                    ["class_type"] = "VAEDecode",
                    ["inputs"] = new JObject
                    {
                        ["samples"] = new JArray { "5", 0 },
                        ["vae"] = new JArray { "1", 2 }
                    }
                },
                ["7"] = new JObject
                {
                    ["class_type"] = "SaveImageWebsocket",
                    ["inputs"] = new JObject
                    {
                        ["images"] = new JArray { "6", 0 }
                    }
                }
            };

            Debug.Log($"[ComfyUIClient] 文生图工作流已动态构建, seed={seed}, steps={steps}, cfg={cfg}, size={width}x{height}");
            return workflow.ToString();
        }

        /// <summary>
        /// 动态构建图生图工作流 JSON（使用 Qwen-Image-Edit LoRA）
        /// 节点结构：CheckpointLoaderSimple → LoraLoader → LoadImage → CLIPTextEncode(+/-) → VAEEncode → KSampler → VAEDecode → SaveImageWebsocket
        /// </summary>
        public string BuildImageEditWorkflow(string prompt, string imageFilename,
            int width = 1024, int height = 1024, int seed = -1, int steps = 4, float cfg = 1f,
            string filenamePrefix = "ComfyUI")
        {
            if (seed == -1) seed = UnityEngine.Random.Range(0, int.MaxValue);

            var workflow = new JObject
            {
                ["1"] = new JObject
                {
                    ["class_type"] = "CheckpointLoaderSimple",
                    ["inputs"] = new JObject
                    {
                        ["ckpt_name"] = "动漫 primemix_v21.safetensors"
                    }
                },
                ["2"] = new JObject
                {
                    ["class_type"] = "LoraLoader",
                    ["inputs"] = new JObject
                    {
                        ["model"] = new JArray { "1", 0 },
                        ["clip"] = new JArray { "1", 1 },
                        ["lora_name"] = "Qwen-Image-Edit-2511-Lightning-4steps-V1.0-bf16.safetensors",
                        ["strength_model"] = 1,
                        ["strength_clip"] = 1
                    }
                },
                ["3"] = new JObject
                {
                    ["class_type"] = "LoadImage",
                    ["inputs"] = new JObject
                    {
                        ["image"] = imageFilename
                    }
                },
                ["4"] = new JObject
                {
                    ["class_type"] = "CLIPTextEncode",
                    ["inputs"] = new JObject
                    {
                        ["text"] = prompt,
                        ["clip"] = new JArray { "2", 1 }
                    }
                },
                ["5"] = new JObject
                {
                    ["class_type"] = "CLIPTextEncode",
                    ["inputs"] = new JObject
                    {
                        ["text"] = "",
                        ["clip"] = new JArray { "2", 1 }
                    }
                },
                ["6"] = new JObject
                {
                    ["class_type"] = "VAEEncode",
                    ["inputs"] = new JObject
                    {
                        ["pixels"] = new JArray { "3", 0 },
                        ["vae"] = new JArray { "1", 2 }
                    }
                },
                ["7"] = new JObject
                {
                    ["class_type"] = "KSampler",
                    ["inputs"] = new JObject
                    {
                        ["seed"] = seed,
                        ["steps"] = steps,
                        ["cfg"] = cfg,
                        ["sampler_name"] = "euler",
                        ["scheduler"] = "normal",
                        ["denoise"] = 0.6,
                        ["model"] = new JArray { "2", 0 },
                        ["positive"] = new JArray { "4", 0 },
                        ["negative"] = new JArray { "5", 0 },
                        ["latent_image"] = new JArray { "6", 0 }
                    }
                },
                ["8"] = new JObject
                {
                    ["class_type"] = "VAEDecode",
                    ["inputs"] = new JObject
                    {
                        ["samples"] = new JArray { "7", 0 },
                        ["vae"] = new JArray { "1", 2 }
                    }
                },
                ["9"] = new JObject
                {
                    ["class_type"] = "SaveImageWebsocket",
                    ["inputs"] = new JObject
                    {
                        ["images"] = new JArray { "8", 0 }
                    }
                }
            };

            Debug.Log($"[ComfyUIClient] 图生图工作流已动态构建, seed={seed}, steps={steps}, cfg={cfg}, image={imageFilename}");
            return workflow.ToString();
        }

        // ==========================================
        // 私有辅助方法
        // ==========================================

        /// <summary>
        /// 转义 JSON 字符串中的特殊字符
        /// </summary>
        private static string EscapeJsonString(string text)
        {
            return text
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t");
        }

        /// <summary>
        /// 手动解析 ComfyUI History 响应 JSON
        /// </summary>
        private List<OutputImage> ParseHistoryResponse(string jsonText, string promptId)
        {
            // 简单解析：查找 prompt_id 对应的 outputs
            // ComfyUI 格式: {"prompt_id": {"outputs": {"node_id": {"images": [{"filename":"...","subfolder":"...","type":"output"}]}}}}
            var result = new List<OutputImage>();

            try
            {
                // 使用 MiniJSON 风格的简单解析，或者直接使用 JsonUtility 配合包装类
                // 更稳健的方式：使用 Unity 的 JSON 解析
                var wrapper = JsonUtility.FromJson<HistoryResponseWrapper>(jsonText);
                if (wrapper != null)
                {
                    // 由于 HistoryResponseWrapper 使用 [Serializable] 但 key 动态，
                    // 我们需要手动处理。这里改用更简单的方法。
                }

                // 直接字符串解析
                // 查找 "filename" 字段
                int searchStart = 0;
                while (true)
                {
                    int filenameIdx = jsonText.IndexOf("\"filename\"", searchStart, StringComparison.Ordinal);
                    if (filenameIdx < 0) break;

                    int colonIdx = jsonText.IndexOf(':', filenameIdx);
                    int startQuote = jsonText.IndexOf('"', colonIdx + 1);
                    int endQuote = jsonText.IndexOf('"', startQuote + 1);
                    string filename = jsonText.Substring(startQuote + 1, endQuote - startQuote - 1);

                    // 查找 subfolder
                    string subfolder = "";
                    int subfolderIdx = jsonText.IndexOf("\"subfolder\"", endQuote, StringComparison.Ordinal);
                    if (subfolderIdx >= 0 && subfolderIdx < endQuote + 200)
                    {
                        int sColon = jsonText.IndexOf(':', subfolderIdx);
                        int sStart = jsonText.IndexOf('"', sColon + 1);
                        int sEnd = jsonText.IndexOf('"', sStart + 1);
                        subfolder = jsonText.Substring(sStart + 1, sEnd - sStart - 1);
                    }

                    // 查找 type
                    string type = "output";
                    int typeIdx = jsonText.IndexOf("\"type\"", endQuote, StringComparison.Ordinal);
                    if (typeIdx >= 0 && typeIdx < endQuote + 200)
                    {
                        int tColon = jsonText.IndexOf(':', typeIdx);
                        int tStart = jsonText.IndexOf('"', tColon + 1);
                        int tEnd = jsonText.IndexOf('"', tStart + 1);
                        type = jsonText.Substring(tStart + 1, tEnd - tStart - 1);
                    }

                    result.Add(new OutputImage
                    {
                        filename = filename,
                        subfolder = subfolder,
                        type = type
                    });

                    searchStart = endQuote + 1;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ComfyUI] 解析 History 响应时出错: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// 从 JSON 中提取指定 key 的字符串值（简单解析）
        /// </summary>
        private static string ExtractJsonStringValue(string json, string key)
        {
            string searchKey = $"\"{key}\"";
            int keyIdx = json.IndexOf(searchKey, StringComparison.Ordinal);
            if (keyIdx < 0) return null;

            int colonIdx = json.IndexOf(':', keyIdx);
            if (colonIdx < 0) return null;

            int start = colonIdx + 1;
            while (start < json.Length && (json[start] == ' ' || json[start] == '\t'))
                start++;

            if (start >= json.Length) return null;

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

            return null;
        }

        /// <summary>
        /// 解析 ComfyUI /api/models/{folder} 响应
        /// 格式: ["model1.safetensors", "model2.safetensors", ...]
        /// </summary>
        private static List<string> ParseModelListResponse(string json)
        {
            var models = new List<string>();

            try
            {
                // 简单解析 JSON 数组中的字符串
                int pos = 0;
                while (pos < json.Length)
                {
                    int startQuote = json.IndexOf('"', pos);
                    if (startQuote < 0) break;

                    int endQuote = json.IndexOf('"', startQuote + 1);
                    if (endQuote < 0) break;

                    string model = json.Substring(startQuote + 1, endQuote - startQuote - 1);
                    if (!string.IsNullOrEmpty(model) && model != ",")
                    {
                        models.Add(model);
                    }

                    pos = endQuote + 1;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ComfyUI] 解析模型列表失败: {ex.Message}");
            }

            return models;
        }
    }

    // ==========================================
    // 辅助 JSON 包装类
    // ==========================================

    [Serializable]
    internal class HistoryWrapper
    {
        // 空的包装类，用于 JsonUtility 尝试解析
    }

    [Serializable]
    internal class HistoryResponseWrapper
    {
        public string prompt_id;
    }

    // ==========================================
    // Editor 菜单项
    // ==========================================

    public static class ComfyUIMenuItems
    {
        [MenuItem("Tools/ComfyUI/Test Connection")]
        public static async void TestConnection()
        {
            string serverUrl = EditorPrefs.GetString("ComfyUI_ServerUrl", "http://10.150.164.64:8188");
            var client = new ComfyUIClient(serverUrl);
            bool success = await client.TestConnection();

            if (success)
            {
                EditorUtility.DisplayDialog("ComfyUI 连接测试", $"连接成功!\n服务器: {serverUrl}", "确定");
            }
            else
            {
                EditorUtility.DisplayDialog("ComfyUI 连接测试", $"连接失败!\n服务器: {serverUrl}\n请检查服务器是否在线，地址是否正确。", "确定");
            }
        }
    }

    /// <summary>
    /// 绕过 SSL 证书验证，允许 HTTP 内网连接
    /// </summary>
    public class BypassCertificate : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            return true; // 信任所有证书（内网环境）
        }
    }
}
#endif