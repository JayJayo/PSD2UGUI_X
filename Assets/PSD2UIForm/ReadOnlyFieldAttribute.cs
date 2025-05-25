/*
    PSD2UGUI - Photoshop to Unity UGUI Converter
    Copyright (c) 2024
    All rights reserved.
 */

#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UGF.EditorTools.Psd2UGUI
{
    /// <summary>
    /// 只读字段特性，用于在Unity编辑器中标记只读字段
    /// </summary>
    public class ReadOnlyFieldAttribute : PropertyAttribute
    {
        /// <summary>
        /// 是否在编辑器中显示
        /// </summary>
        public bool ShowInInspector { get; private set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="showInInspector">是否在编辑器中显示</param>
        public ReadOnlyFieldAttribute(bool showInInspector = true)
        {
            ShowInInspector = showInInspector;
        }
    }
    [CustomPropertyDrawer(typeof(ReadOnlyFieldAttribute))]
    public class ReadOnlyFieldDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }
}
#endif
