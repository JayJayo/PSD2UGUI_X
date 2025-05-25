/*
    PSD2UGUI - Photoshop to Unity UGUI Converter
    Copyright (c) 2024
    All rights reserved.
*/

using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEditor;

namespace PSD2UGUI
{
    /// <summary>
    /// PSD导出配置解析器
    /// </summary>
    public class Psd2UIFormParser
    {
        /// <summary>
        /// 从JSON文件加载配置
        /// </summary>
        /// <param name="jsonPath">JSON文件路径</param>
        /// <returns>配置对象</returns>
        public static Psd2UIFormConfig LoadFromJson(string jsonPath)
        {
            if (!File.Exists(jsonPath))
            {
                Debug.LogError($"配置文件不存在: {jsonPath}");
                return null;
            }

            string jsonContent = File.ReadAllText(jsonPath);
            Psd2UIFormConfig config = ScriptableObject.CreateInstance<Psd2UIFormConfig>();
            
            try
            {
                JsonUtility.FromJsonOverwrite(jsonContent, config);
                return config;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"解析配置文件失败: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// 保存配置到JSON文件
        /// </summary>
        /// <param name="config">配置对象</param>
        /// <param name="jsonPath">JSON文件路径</param>
        public static void SaveToJson(Psd2UIFormConfig config, string jsonPath)
        {
            try
            {
                string jsonContent = JsonUtility.ToJson(config, true);
                File.WriteAllText(jsonPath, jsonContent);
                AssetDatabase.Refresh();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"保存配置文件失败: {e.Message}");
            }
        }

        /// <summary>
        /// 验证配置是否有效
        /// </summary>
        /// <param name="config">配置对象</param>
        /// <returns>是否有效</returns>
        public static bool ValidateConfig(Psd2UIFormConfig config)
        {
            if (config == null)
            {
                Debug.LogError("配置对象为空");
                return false;
            }

            if (config.canvasSize.x <= 0 || config.canvasSize.y <= 0)
            {
                Debug.LogError("画布大小无效");
                return false;
            }

            if (config.layers == null || config.layers.Count == 0)
            {
                Debug.LogError("图层列表为空");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 获取图层路径
        /// </summary>
        /// <param name="config">配置对象</param>
        /// <param name="layer">图层配置</param>
        /// <returns>图层路径</returns>
        public static string GetLayerPath(Psd2UIFormConfig config, LayerConfig layer)
        {
            if (layer == null) return string.Empty;

            List<string> pathParts = new List<string>();
            pathParts.Add(layer.name);

            // 使用parentId构建路径
            LayerConfig currentLayer = layer;
            while (currentLayer.parentId != null)
            {
                var parentLayer = FindLayer(config, currentLayer.parentId);
                if (parentLayer != null)
                {
                    pathParts.Add(parentLayer.name);
                    currentLayer = parentLayer;
                }
                else
                {
                    break;
                }
            }

            pathParts.Reverse();
            return string.Join("/", pathParts);
        }

        /// <summary>
        /// 查找图层
        /// </summary>
        /// <param name="config">配置对象</param>
        /// <param name="layerId">图层ID</param>
        /// <returns>图层配置</returns>
        public static LayerConfig FindLayer(Psd2UIFormConfig config, string layerId)
        {
            if (config == null || string.IsNullOrEmpty(layerId)) return null;

            foreach (var layer in config.layers)
            {
                if (layer.id == layerId) return layer;
                
                if (layer.children != null)
                {
                    var childLayer = FindLayerInChildren(layer.children, layerId);
                    if (childLayer != null) return childLayer;
                }
            }

            return null;
        }

        /// <summary>
        /// 在子图层中查找图层
        /// </summary>
        /// <param name="layers">图层列表</param>
        /// <param name="layerId">图层ID</param>
        /// <returns>图层配置</returns>
        private static LayerConfig FindLayerInChildren(List<LayerConfig> layers, string layerId)
        {
            if (layers == null) return null;

            foreach (var layer in layers)
            {
                if (layer.id == layerId) return layer;
                
                if (layer.children != null)
                {
                    var childLayer = FindLayerInChildren(layer.children, layerId);
                    if (childLayer != null) return childLayer;
                }
            }

            return null;
        }
    }
} 