/*
    PSD2UGUI - Photoshop to Unity UGUI Converter
    Copyright (c) 2024
    All rights reserved.
*/

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEditor;

namespace PSD2UGUI
{
    /// <summary>
    /// PSD导出UI生成器
    /// </summary>
    public class Psd2UIFormGenerator
    {
        /// <summary>
        /// 生成UI预制体
        /// </summary>
        /// <param name="config">配置对象</param>
        /// <param name="outputPath">输出路径</param>
        /// <returns>生成的预制体</returns>
        public static GameObject GenerateUIForm(Psd2UIFormConfig config, string outputPath)
        {
            if (!Psd2UIFormParser.ValidateConfig(config))
            {
                return null;
            }

            // 创建根Canvas
            GameObject canvasObj = new GameObject("Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            // 创建根RectTransform
            RectTransform rootRect = canvasObj.GetComponent<RectTransform>();
            rootRect.sizeDelta = config.canvasSize.ToVector2();

            // 创建UI容器
            GameObject containerObj = new GameObject("Container");
            containerObj.transform.SetParent(canvasObj.transform, false);
            RectTransform containerRect = containerObj.AddComponent<RectTransform>();
            containerRect.anchorMin = Vector2.zero;
            containerRect.anchorMax = Vector2.one;
            containerRect.sizeDelta = Vector2.zero;
            containerRect.anchoredPosition = Vector2.zero;

            // 生成所有图层
            foreach (var layer in config.layers)
            {
                GenerateLayer(layer, containerObj.transform);
            }

            // 保存预制体
            string prefabPath = $"{outputPath}/{config.name}.prefab";
            PrefabUtility.SaveAsPrefabAsset(canvasObj, prefabPath);
            Object.DestroyImmediate(canvasObj);

            return AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
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
            if (layer.children != null)
            {
                foreach (var child in layer.children)
                {
                    GenerateLayer(child, layerObj.transform);
                }
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

        /// <summary>
        /// 应用图层效果
        /// </summary>
        /// <param name="obj">游戏对象</param>
        /// <param name="effects">效果配置</param>
        private static void ApplyLayerEffects(GameObject obj, Psd2UIFormConfig.LayerEffects effects)
        {
            if (effects == null) return;

            // 添加阴影效果
            if (effects.dropShadow)
            {
                Shadow shadow = obj.AddComponent<Shadow>();
                shadow.effectColor = new Color(0, 0, 0, 0.5f);
                shadow.effectDistance = new Vector2(2, -2);
            }

            // 添加其他效果...
        }
    }
} 