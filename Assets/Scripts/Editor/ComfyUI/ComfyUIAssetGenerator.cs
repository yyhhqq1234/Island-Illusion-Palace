#if UNITY_EDITOR
using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;

namespace ComfyUI
{
    /// <summary>
    /// 核心生成管线，负责两阶段生成流程：概念图 → 精灵帧
    /// </summary>
    public class ComfyUIAssetGenerator
    {
        // ==========================================
        // 配置
        // ==========================================

        private ComfyUIClient client;
        private string serverUrl;

        // 概念图默认参数
        private const int ConceptWidth = 1024;
        private const int ConceptHeight = 1024;
        private const int ConceptSteps = 20;
        private const float ConceptCfg = 7f;

        // 精灵帧默认参数
        private const int SpriteWidth = 1024;   // 精灵表整体尺寸
        private const int SpriteHeight = 1024;
        private const int SpriteSteps = 15;
        private const float SpriteCfg = 8f;

        // ==========================================
        // 构造函数
        // ==========================================

        public ComfyUIAssetGenerator(string serverUrl)
        {
            this.serverUrl = serverUrl.TrimEnd('/');
            this.client = new ComfyUIClient(serverUrl);
        }

        // ==========================================
        // 阶段一：概念图生成
        // ==========================================

        /// <summary>
        /// 生成概念设定图
        /// </summary>
        /// <param name="assetType">资产类型名称</param>
        /// <param name="outputDir">输出目录（可选，默认使用标准路径）</param>
        /// <returns>保存的图片路径</returns>
        public async Task<string> GenerateConceptArt(string assetType, string outputDir = null)
        {
            Debug.Log($"[ComfyUI] ===== 阶段一：生成概念图 [{assetType}] =====");

            // 获取模板
            string positivePrompt = ComfyUIPromptTemplates.GetFullPositivePrompt(assetType);
            string negativePrompt = ComfyUIPromptTemplates.GetNegativePrompt(assetType);

            var template = ComfyUIPromptTemplates.GetTemplate(assetType);
            int width = template?.width ?? ConceptWidth;
            int height = template?.height ?? ConceptHeight;

            Debug.Log($"[ComfyUI] 正向提示词: {positivePrompt.Substring(0, Math.Min(100, positivePrompt.Length))}...");
            Debug.Log($"[ComfyUI] 反向提示词: {negativePrompt.Substring(0, Math.Min(100, negativePrompt.Length))}...");
            Debug.Log($"[ComfyUI] 尺寸: {width}x{height}, 步数: {ConceptSteps}, CFG: {ConceptCfg}");

            // 调用 ComfyUI 生成
            string tempPath = await client.GenerateImage(
                positivePrompt,
                negativePrompt,
                width,
                height,
                ConceptSteps,
                ConceptCfg,
                seed: -1,
                filenamePrefix: assetType
            );

            // 确定输出路径
            if (string.IsNullOrEmpty(outputDir))
            {
                outputDir = ComfyUIImageReceiver.GetConceptOutputPath(assetType);
            }

            string fullOutputDir = Path.Combine(Application.dataPath, "..", outputDir);
            ComfyUIImageReceiver.EnsureDirectoryExists(fullOutputDir);

            string filename = ComfyUIImageReceiver.GenerateUniqueFilename(assetType, "concept");
            string finalPath = Path.Combine(fullOutputDir, filename);

            // 复制到最终位置
            File.Copy(tempPath, finalPath, overwrite: true);
            File.Delete(tempPath); // 删除临时文件

            string relativePath = Path.Combine(outputDir, filename).Replace("\\", "/");
            ComfyUIImageReceiver.RefreshAssetDatabase();

            Debug.Log($"[ComfyUI] 概念图生成完成: {relativePath}");
            return relativePath;
        }

        // ==========================================
        // 阶段二：精灵帧生成
        // ==========================================

        /// <summary>
        /// 生成逐帧精灵（基于概念图）
        /// </summary>
        /// <param name="assetType">资产类型名称</param>
        /// <param name="conceptImagePath">概念图路径（可选，用于 img2img 参考）</param>
        /// <param name="outputDir">输出目录</param>
        /// <param name="frameCount">帧数</param>
        /// <param name="animationState">动画状态名（idle, walk, attack, etc.）</param>
        /// <returns>保存的图片路径</returns>
        public async Task<string> GenerateSpriteFrames(
            string assetType,
            string conceptImagePath,
            string outputDir,
            int frameCount = 4,
            string animationState = "idle")
        {
            Debug.Log($"[ComfyUI] ===== 阶段二：生成精灵帧 [{assetType}/{animationState}] =====");

            // 获取基础提示词
            string basePositive = ComfyUIPromptTemplates.GetFullPositivePrompt(assetType);
            string negativePrompt = ComfyUIPromptTemplates.GetNegativePrompt(assetType);

            // 追加精灵帧专用提示词
            string spritePrompt = $"{basePositive}, " +
                $"(sprite sheet:1.3), {frameCount} frames, {animationState} animation, " +
                $"consistent character design, same character as reference, " +
                $"top-down orthographic view, transparent background";

            Debug.Log($"[ComfyUI] 精灵提示词: {spritePrompt.Substring(0, Math.Min(120, spritePrompt.Length))}...");
            Debug.Log($"[ComfyUI] 帧数: {frameCount}, 动画: {animationState}");

            // 如果有概念图路径，使用 img2img 管线
            string tempPath;
            if (!string.IsNullOrEmpty(conceptImagePath))
            {
                tempPath = await GenerateSpriteWithImg2Img(spritePrompt, conceptImagePath, assetType);
            }
            else
            {
                // 纯文本生成
                tempPath = await client.GenerateImage(
                    spritePrompt,
                    negativePrompt,
                    SpriteWidth,
                    SpriteHeight,
                    SpriteSteps,
                    SpriteCfg,
                    seed: -1
                );
            }

            // 确定输出路径
            if (string.IsNullOrEmpty(outputDir))
            {
                outputDir = ComfyUIImageReceiver.GetSpriteOutputPath(assetType, animationState);
            }

            string fullOutputDir = Path.Combine(Application.dataPath, "..", outputDir);
            ComfyUIImageReceiver.EnsureDirectoryExists(fullOutputDir);

            string filename = ComfyUIImageReceiver.GenerateUniqueFilename(assetType, $"sprite_{animationState}");
            string finalPath = Path.Combine(fullOutputDir, filename);

            // 复制到最终位置
            File.Copy(tempPath, finalPath, overwrite: true);
            File.Delete(tempPath);

            string relativePath = Path.Combine(outputDir, filename).Replace("\\", "/");
            ComfyUIImageReceiver.RefreshAssetDatabase();

            Debug.Log($"[ComfyUI] 精灵帧生成完成: {relativePath}");
            return relativePath;
        }

        /// <summary>
        /// 使用 img2img 管线生成精灵帧（基于概念图，使用 qwen-edit 工作流）
        /// </summary>
        private async Task<string> GenerateSpriteWithImg2Img(
            string prompt,
            string conceptImagePath,
            string assetType)
        {
            Debug.Log($"[ComfyUI] 使用 img2img 管线 (qwen-edit), 概念图: {conceptImagePath}");

            // 将相对路径转换为绝对路径
            string absolutePath = conceptImagePath;
            if (!Path.IsPathRooted(absolutePath))
            {
                absolutePath = Path.Combine(Application.dataPath, "..", absolutePath);
            }

            // 上传概念图到 ComfyUI
            string uploadedName = await client.UploadImage(absolutePath, "input");

            // 构建 qwen-edit 图生图工作流
            int seed = UnityEngine.Random.Range(1, int.MaxValue);
            string workflowJson = client.BuildImageEditWorkflow(
                prompt,
                uploadedName,
                SpriteWidth,
                SpriteHeight,
                seed,
                SpriteSteps,
                SpriteCfg,
                assetType
            );

            // 提交任务
            string promptId = await client.QueuePrompt(workflowJson);

            // 等待完成
            var outputs = await client.GetHistory(promptId);

            if (outputs == null || outputs.Count == 0)
            {
                throw new Exception("[ComfyUI] img2img 生成完成但没有输出图片");
            }

            // 下载第一张图片
            string tempPath = Path.Combine(Application.dataPath, "_Generated", "_temp", $"img2img_{promptId}.png");
            ComfyUIImageReceiver.EnsureDirectoryExists(Path.GetDirectoryName(tempPath));

            var firstOutput = outputs[0];
            await client.DownloadImage(firstOutput.filename, firstOutput.subfolder, tempPath);

            return tempPath;
        }

        // ==========================================
        // 完整两阶段生成
        // ==========================================

        /// <summary>
        /// 完整两阶段生成流程：概念图 → 精灵帧
        /// </summary>
        public async Task<(string conceptPath, string spritePath)> GenerateFullPipeline(
            string assetType,
            int frameCount = 4,
            string animationState = "idle")
        {
            // 阶段一：概念图
            string conceptPath = await GenerateConceptArt(assetType);

            // 阶段二：精灵帧
            string spritePath = await GenerateSpriteFrames(assetType, conceptPath, null, frameCount, animationState);

            return (conceptPath, spritePath);
        }
    }
}
#endif