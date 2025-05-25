/*
    PSD2UGUI - Photoshop to Unity UGUI Converter
    Copyright (c) 2024
    All rights reserved.
*/

using UnityEngine;
using System;
using System.Collections.Generic;

namespace PSD2UGUI
{
    /// <summary>
    /// PSD导出配置数据类
    /// </summary>
    [Serializable]
    public class Psd2UIFormConfig : ScriptableObject
    {
        /// <summary>
        /// 画布大小
        /// </summary>
        public Vector2Int canvasSize;

        /// <summary>
        /// 图层列表
        /// </summary>
        public List<LayerConfig> layers = new List<LayerConfig>();

        /// <summary>
        /// 版本号
        /// </summary>
        public string version;
    }

    /// <summary>
    /// 图层配置
    /// </summary>
    [Serializable]
    public class LayerConfig
    {
        /// <summary>
        /// 图层ID
        /// </summary>
        public string id;

        /// <summary>
        /// 图层名称
        /// </summary>
        public string name;

        /// <summary>
        /// 图层类型
        /// </summary>
        public string type;

        /// <summary>
        /// 是否可见
        /// </summary>
        public bool visible;

        /// <summary>
        /// 不透明度
        /// </summary>
        public float opacity;

        /// <summary>
        /// 位置和大小
        /// </summary>
        public Rect position;

        /// <summary>
        /// 图层效果
        /// </summary>
        public LayerEffects effects;

        /// <summary>
        /// 颜色信息
        /// </summary>
        public Color color;

        /// <summary>
        /// 父图层ID
        /// </summary>
        public string parentId;

        /// <summary>
        /// 子图层列表
        /// </summary>
        public List<LayerConfig> children;

        /// <summary>
        /// 文本信息
        /// </summary>
        public TextInfo textInfo;
    }

    /// <summary>
    /// 图层效果
    /// </summary>
    [Serializable]
    public class LayerEffects
    {
        /// <summary>
        /// 投影
        /// </summary>
        public bool dropShadow;

        /// <summary>
        /// 内阴影
        /// </summary>
        public bool innerShadow;

        /// <summary>
        /// 外发光
        /// </summary>
        public bool outerGlow;

        /// <summary>
        /// 内发光
        /// </summary>
        public bool innerGlow;

        /// <summary>
        /// 斜面和浮雕
        /// </summary>
        public bool bevelEmboss;
    }

    /// <summary>
    /// 文本信息
    /// </summary>
    [Serializable]
    public class TextInfo
    {
        /// <summary>
        /// 文本内容
        /// </summary>
        public string text;

        /// <summary>
        /// 字体
        /// </summary>
        public string font;

        /// <summary>
        /// 字体大小
        /// </summary>
        public float size;

        /// <summary>
        /// 文本颜色
        /// </summary>
        public Color color;

        /// <summary>
        /// 对齐方式
        /// </summary>
        public int alignment;

        /// <summary>
        /// 行间距
        /// </summary>
        public float leading;

        /// <summary>
        /// 字间距
        /// </summary>
        public float tracking;
    }
} 