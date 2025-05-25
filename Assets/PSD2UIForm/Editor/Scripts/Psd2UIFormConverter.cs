/*
    PSD2UGUI - Photoshop to Unity UGUI Converter
    Copyright (c) 2024
    All rights reserved.
*/

using UnityEngine;
using UnityEditor;
using System.IO;

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
                EditorGUILayout.LabelField($"Canvas Size: {currentConfig.canvasSize.x}x{currentConfig.canvasSize.y}");
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
    }
} 