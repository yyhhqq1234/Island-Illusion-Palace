#if UNITY_EDITOR
using System;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace ComfyUI
{
    /// <summary>
    /// 本地图片接收管理器，负责保存图片到磁盘并刷新 Unity 资源数据库
    /// </summary>
    public static class ComfyUIImageReceiver
    {
        // ==========================================
        // 路径常量
        // ==========================================

        private const string BaseGeneratedPath = "Assets/ArtMaterials/_Generated";
        private const string ConceptsSubfolder = "Concepts";
        private const string SpritesSubfolder = "Sprites";

        // ==========================================
        // 文件名生成
        // ==========================================

        /// <summary>
        /// 生成唯一文件名，格式: {assetType}_{stage}_{timestamp}.png
        /// </summary>
        public static string GenerateUniqueFilename(string assetType, string stage)
        {
            string safeAssetType = SanitizeFilename(assetType);
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            return $"{safeAssetType}_{stage}_{timestamp}.png";
        }

        /// <summary>
        /// 生成精灵帧文件名
        /// </summary>
        public static string GenerateFrameFilename(string assetType, string animationState, int frameIndex)
        {
            string safeAssetType = SanitizeFilename(assetType);
            string safeAnimState = SanitizeFilename(animationState);
            return $"{safeAssetType}_{safeAnimState}_{frameIndex:D3}.png";
        }

        // ==========================================
        // 路径获取
        // ==========================================

        /// <summary>
        /// 获取概念图输出路径
        /// </summary>
        public static string GetConceptOutputPath(string assetType)
        {
            string safeAssetType = SanitizeFilename(assetType);
            return Path.Combine(BaseGeneratedPath, ConceptsSubfolder, safeAssetType);
        }

        /// <summary>
        /// 获取精灵帧输出路径
        /// </summary>
        public static string GetSpriteOutputPath(string assetType, string animationState)
        {
            string safeAssetType = SanitizeFilename(assetType);
            string safeAnimState = SanitizeFilename(animationState);
            return Path.Combine(BaseGeneratedPath, SpritesSubfolder, safeAssetType, safeAnimState);
        }

        /// <summary>
        /// 获取最新概念图路径（按文件名查找）
        /// </summary>
        public static string GetConceptImagePath(string assetType)
        {
            string dir = GetConceptOutputPath(assetType);
            string fullDir = Path.Combine(Application.dataPath, "..", dir);

            if (!Directory.Exists(fullDir))
                return null;

            var files = Directory.GetFiles(fullDir, "*.png");
            if (files.Length == 0)
                return null;

            // 按修改时间排序，返回最新的
            string latest = null;
            DateTime latestTime = DateTime.MinValue;
            foreach (var file in files)
            {
                var writeTime = File.GetLastWriteTime(file);
                if (writeTime > latestTime)
                {
                    latestTime = writeTime;
                    latest = file;
                }
            }

            if (latest != null)
            {
                // 转换为相对路径
                return "Assets" + latest.Replace(Application.dataPath, "").Replace("\\", "/");
            }
            return null;
        }

        // ==========================================
        // 文件操作
        // ==========================================

        /// <summary>
        /// 保存图片字节数据到磁盘
        /// </summary>
        public static void SaveImage(byte[] imageData, string filePath)
        {
            if (imageData == null || imageData.Length == 0)
            {
                throw new ArgumentException("[ComfyUI] 图片数据为空");
            }

            EnsureDirectoryExists(Path.GetDirectoryName(filePath));
            File.WriteAllBytes(filePath, imageData);
            Debug.Log($"[ComfyUI] 图片已保存: {filePath} ({imageData.Length / 1024} KB)");
        }

        /// <summary>
        /// 保存图片并刷新资源数据库
        /// </summary>
        public static string SaveAndRefreshImage(byte[] imageData, string assetType, string stage)
        {
            string dir = GetConceptOutputPath(assetType);
            if (stage != "concept")
            {
                dir = GetSpriteOutputPath(assetType, stage);
            }

            string filename = GenerateUniqueFilename(assetType, stage);
            string fullPath = Path.Combine(Application.dataPath, "..", dir, filename);

            SaveImage(imageData, fullPath);

            // 转换为相对路径
            string relativePath = Path.Combine(dir, filename).Replace("\\", "/");
            RefreshAssetDatabase();

            return relativePath;
        }

        // ==========================================
        // 目录管理
        // ==========================================

        /// <summary>
        /// 确保目录存在
        /// </summary>
        public static void EnsureDirectoryExists(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                Debug.Log($"[ComfyUI] 创建目录: {path}");
            }
        }

        /// <summary>
        /// 确保所有生成目录存在
        /// </summary>
        public static void EnsureAllDirectories()
        {
            EnsureDirectoryExists(Path.Combine(Application.dataPath, "..", BaseGeneratedPath));
            EnsureDirectoryExists(Path.Combine(Application.dataPath, "..", BaseGeneratedPath, ConceptsSubfolder));
            EnsureDirectoryExists(Path.Combine(Application.dataPath, "..", BaseGeneratedPath, SpritesSubfolder));
        }

        // ==========================================
        // 资源数据库刷新
        // ==========================================

        /// <summary>
        /// 刷新 Unity 资源数据库
        /// </summary>
        public static void RefreshAssetDatabase()
        {
            AssetDatabase.Refresh();
            Debug.Log("[ComfyUI] AssetDatabase 已刷新");
        }

        // ==========================================
        // 私有辅助
        // ==========================================

        /// <summary>
        /// 清理文件名中的非法字符
        /// </summary>
        private static string SanitizeFilename(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "unknown";

            char[] invalidChars = Path.GetInvalidFileNameChars();
            foreach (char c in invalidChars)
            {
                name = name.Replace(c, '_');
            }

            // 替换常见路径分隔符
            name = name.Replace("/", "_").Replace("\\", "_");

            return name;
        }
    }
}
#endif