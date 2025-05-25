/*
    PSD2UGUI - Photoshop to Unity UGUI Converter
    Copyright (c) 2024
    All rights reserved.
*/

using UnityEngine;
using System.Collections.Generic;

namespace PSD2UGUI
{
    /// <summary>
    /// PSD导出配置
    /// </summary>
    [System.Serializable]
    public class Psd2UIFormConfig : ScriptableObject
    {
        /// <summary>
        /// 画布大小
        /// </summary>
        [System.Serializable]
        public class CanvasSize
        {
            public float width;
            public float height;

            public Vector2 ToVector2()
            {
                return new Vector2(width, height);
            }
        }

        /// <summary>
        /// 图层位置
        /// </summary>
        [System.Serializable]
        public class LayerPosition
        {
            public float x;
            public float y;
            public float width;
            public float height;
        }

        /// <summary>
        /// 图层颜色
        /// </summary>
        [System.Serializable]
        public class LayerColor
        {
            public float r;
            public float g;
            public float b;
            public float a;
        }

        /// <summary>
        /// 图层效果
        /// </summary>
        [System.Serializable]
        public class LayerEffects
        {
            public bool dropShadow;
            public bool innerShadow;
            public bool outerGlow;
            public bool innerGlow;
            public bool bevelEmboss;
        }

        /// <summary>
        /// 文本信息
        /// </summary>
        [System.Serializable]
        public class TextInfo
        {
            public string text;
            public float size;
            public int alignment;
            public float leading;
            public float tracking;
            public LayerColor color;
        }

        /// <summary>
        /// 图层配置
        /// </summary>
        [System.Serializable]
        public class LayerConfig
        {
            public string id;
            public string name;
            public string type;
            public bool visible;
            public float opacity;
            public LayerPosition position;
            public LayerColor color;
            public LayerEffects effects;
            public TextInfo textInfo;
            public string parentId;
            public List<LayerConfig> children = new List<LayerConfig>();
        }

        public string name;
        public CanvasSize canvasSize;
        public List<LayerConfig> layers = new List<LayerConfig>();
        public string version;
    }
} 