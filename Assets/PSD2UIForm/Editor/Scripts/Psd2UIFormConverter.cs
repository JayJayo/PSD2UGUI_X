/*
    PSD2UGUI - Photoshop to Unity UGUI Converter
    Copyright (c) 2024
    All rights reserved.
*/

using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.UI;
using TMPro;

namespace PSD2UGUI
{
    /// <summary>
    /// PSD导出UI转换器
    /// </summary>
    public class Psd2UIFormConverter : EditorWindow
    {
        private string configPath = "";
        private string outputPath = "Assets/Resources/UI/Prefabs";
        private Psd2UIFormConfig currentConfig;

        [MenuItem("Tools/PSD2UGUI/Converter")]
        public static void ShowWindow()
        {
            GetWindow<Psd2UIFormConverter>("PSD2UGUI Converter");
        }

        private void OnGUI()
        {
            GUILayout.Label("PSD2UGUI Converter", EditorStyles.boldLabel);

            EditorGUILayout.Space();

            // 配置文件路径
            EditorGUILayout.BeginHorizontal();
            configPath = EditorGUILayout.TextField("Config Path", configPath);
            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                string path = EditorUtility.OpenFilePanel("Select Config File", "", "json");
                if (!string.IsNullOrEmpty(path))
                {
                    configPath = path;
                    LoadConfig();
                }
            }
            EditorGUILayout.EndHorizontal();

            // 输出路径
            EditorGUILayout.BeginHorizontal();
            outputPath = EditorGUILayout.TextField("Output Path", outputPath);
            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                string path = EditorUtility.SaveFolderPanel("Select Output Folder", "Assets", "");
                if (!string.IsNullOrEmpty(path))
                {
                    outputPath = path;
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // 转换按钮
            GUI.enabled = currentConfig != null;
            if (GUILayout.Button("Generate UI Form"))
            {
                GenerateUIForm();
            }
            GUI.enabled = true;

            // 显示当前配置信息
            if (currentConfig != null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Current Config", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Canvas Size: {currentConfig.canvasSize.width}x{currentConfig.canvasSize.height}");
                EditorGUILayout.LabelField($"Layer Count: {currentConfig.layers.Count}");
                EditorGUILayout.LabelField($"Version: {currentConfig.version}");
            }
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        private void LoadConfig()
        {
            if (string.IsNullOrEmpty(configPath)) return;

            currentConfig = Psd2UIFormParser.LoadFromJson(configPath);
            if (currentConfig == null)
            {
                EditorUtility.DisplayDialog("Error", "Failed to load config file.", "OK");
            }
        }

        /// <summary>
        /// 生成UI预制体
        /// </summary>
        private void GenerateUIForm()
        {
            if (currentConfig == null)
            {
                EditorUtility.DisplayDialog("Error", "No config loaded.", "OK");
                return;
            }

            // 确保输出目录存在
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            // 生成UI预制体
            GameObject prefab = Psd2UIFormGenerator.GenerateUIForm(currentConfig, outputPath);
            if (prefab != null)
            {
                EditorUtility.DisplayDialog("Success", "UI Form generated successfully!", "OK");
                Selection.activeObject = prefab;
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Failed to generate UI Form.", "OK");
            }
        }

        /// <summary>
        /// 转换PSD配置为UI预制体
        /// </summary>
        /// <param name="config">PSD配置</param>
        /// <returns>生成的UI预制体</returns>
        public static GameObject ConvertToPrefab(Psd2UIFormConfig config)
        {
            if (config == null)
            {
                Debug.LogError("配置为空");
                return null;
            }

            // 创建根Canvas
            GameObject canvasObj = new GameObject(config.name);
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            // 创建根Panel
            GameObject panelObj = new GameObject("Panel");
            panelObj.transform.SetParent(canvasObj.transform, false);
            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.sizeDelta = Vector2.zero;
            panelRect.anchoredPosition = Vector2.zero;

            // 设置Panel大小为画布大小
            panelRect.sizeDelta = config.canvasSize.ToVector2();

            // 生成所有图层
            foreach (var layer in config.layers)
            {
                if (string.IsNullOrEmpty(layer.parentId))
                {
                    GenerateLayer(layer, panelRect);
                }
            }

            return canvasObj;
        }

        /// <summary>
        /// 生成图层
        /// </summary>
        /// <param name="layer">图层配置</param>
        /// <param name="parent">父节点</param>
        /// <returns>生成的游戏对象</returns>
        private static GameObject GenerateLayer(Psd2UIFormConfig.LayerConfig layer, Transform parent)
        {
            if (layer == null) return null;

            GameObject layerObj = new GameObject(layer.name);
            layerObj.transform.SetParent(parent, false);

            // 设置RectTransform
            RectTransform rectTransform = layerObj.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = new Vector2(layer.position.width, layer.position.height);
            rectTransform.anchoredPosition = new Vector2(layer.position.x, layer.position.y);

            // 根据图层类型添加组件
            switch (layer.type)
            {
                case "Group":
                    // 组不需要特殊组件
                    break;

                case "Text":
                    GenerateTextLayer(layerObj, layer);
                    break;

                case "Image":
                    GenerateImageLayer(layerObj, layer);
                    break;

                case "SolidColor":
                    GenerateSolidColorLayer(layerObj, layer);
                    break;
            }

            // 设置可见性
            layerObj.SetActive(layer.visible);

            // 生成子图层
            foreach (var child in layer.children)
            {
                GenerateLayer(child, layerObj.transform);
            }

            return layerObj;
        }

        /// <summary>
        /// 生成文本图层
        /// </summary>
        /// <param name="obj">游戏对象</param>
        /// <param name="layer">图层配置</param>
        private static void GenerateTextLayer(GameObject obj, Psd2UIFormConfig.LayerConfig layer)
        {
            if (layer.textInfo == null) return;

            TextMeshProUGUI text = obj.AddComponent<TextMeshProUGUI>();
            text.text = layer.textInfo.text;
            text.fontSize = layer.textInfo.size;
            text.color = new Color(layer.textInfo.color.r, layer.textInfo.color.g, layer.textInfo.color.b, layer.textInfo.color.a);
            text.alignment = (TextAlignmentOptions)layer.textInfo.alignment;
            text.lineSpacing = layer.textInfo.leading;
            text.characterSpacing = layer.textInfo.tracking;
            text.alpha = layer.opacity;
        }

        /// <summary>
        /// 生成图片图层
        /// </summary>
        /// <param name="obj">游戏对象</param>
        /// <param name="layer">图层配置</param>
        private static void GenerateImageLayer(GameObject obj, Psd2UIFormConfig.LayerConfig layer)
        {
            Image image = obj.AddComponent<Image>();
            image.color = new Color(layer.color.r, layer.color.g, layer.color.b, layer.opacity);

            // 加载图片资源
            string imagePath = $"Assets/Resources/UI/Images/{layer.name}.png";
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(imagePath);
            if (sprite != null)
            {
                image.sprite = sprite;
            }
        }

        /// <summary>
        /// 生成纯色图层
        /// </summary>
        /// <param name="obj">游戏对象</param>
        /// <param name="layer">图层配置</param>
        private static void GenerateSolidColorLayer(GameObject obj, Psd2UIFormConfig.LayerConfig layer)
        {
            Image image = obj.AddComponent<Image>();
            image.color = new Color(layer.color.r, layer.color.g, layer.color.b, layer.opacity);
        }
    }
} 