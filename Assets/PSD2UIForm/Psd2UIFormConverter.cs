/*
    PSD2UGUI - Photoshop to Unity UGUI Converter
    Copyright (c) 2024
    All rights reserved.
 */
#if UNITY_EDITOR
using Aspose.PSD.FileFormats.Psd;
using Aspose.PSD.FileFormats.Psd.Layers;
using Aspose.PSD.FileFormats.Psd.Layers.SmartObjects;
using Aspose.PSD.ImageLoadOptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

#if UNITY_2018_3_OR_NEWER
using UnityEditor.Experimental.SceneManagement;
#endif

namespace UGF.EditorTools.Psd2UGUI
{
    /// <summary>
    /// Psd2UIForm转换器的Inspector编辑器
    /// </summary>
    [CustomEditor(typeof(Psd2UIFormConverter))]
    public class Psd2UIFormConverterInspector : UnityEditor.Editor
    {
        /// <summary>
        /// 目标转换器实例
        /// </summary>
        Psd2UIFormConverter targetLogic;

        /// <summary>
        /// 解析PSD节点按钮
        /// </summary>
        GUIContent parsePsd2NodesBt;

        /// <summary>
        /// 导出UI精灵按钮
        /// </summary>
        GUIContent exportUISpritesBt;

        /// <summary>
        /// 生成UI表单按钮
        /// </summary>
        GUIContent generateUIFormBt;

        /// <summary>
        /// 按钮高度选项
        /// </summary>
        GUILayoutOption btHeight;

        /// <summary>
        /// 当Inspector启用时调用
        /// </summary>
        private void OnEnable()
        {
            btHeight = GUILayout.Height(30);
            targetLogic = target as Psd2UIFormConverter;
            parsePsd2NodesBt = new GUIContent("解析psd图层", "把psd图层解析为可编辑节点树");
            exportUISpritesBt = new GUIContent("导出Images", "导出勾选的psd图层为碎图");
            generateUIFormBt = new GUIContent("生成UIForm", "根据解析后的节点树生成UIForm Prefab");
            if (string.IsNullOrWhiteSpace(Psd2UIFormSettings.Instance.UIFormOutputDir))
            {
                Debug.LogWarning($"UIForm输出路径为空!");
            }
        }
        /// <summary>
        /// 当Inspector禁用时调用
        /// </summary>
        private void OnDisable()
        {
            Psd2UIFormSettings.Save();
        }
        /// <summary>
        /// 绘制Inspector界面
        /// </summary>
        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginVertical("box");
            {
                //EditorGUILayout.BeginHorizontal();
                //{
                //    EditorGUILayout.LabelField("自动压缩图片:", GUILayout.Width(150));
                //    Psd2UIFormSettings.Instance.CompressImage = EditorGUILayout.Toggle(Psd2UIFormSettings.Instance.CompressImage);
                //    EditorGUILayout.EndHorizontal();
                //}
                if (GUILayout.Button("查看使用文档"))
                {
                    Application.OpenURL("https://blog.csdn.net/final5788");
                    GUIUtility.ExitGUI();
                }
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("UI图片导出路径:", GUILayout.Width(150));
                    Psd2UIFormSettings.Instance.UIImagesOutputDir = EditorGUILayout.TextField(Psd2UIFormSettings.Instance.UIImagesOutputDir);
                    if (GUILayout.Button("选择路径", GUILayout.Width(80)))
                    {
                        var retPath = EditorUtility.OpenFolderPanel("选择导出路径", Psd2UIFormSettings.Instance.UIImagesOutputDir, null);
                        if (!string.IsNullOrWhiteSpace(retPath))
                        {
                            if (!retPath.StartsWith("Assets/"))
                            {
                                retPath = UGUIParser.GetRelativePath(Application.dataPath, retPath);
                            }
                            Psd2UIFormSettings.Instance.UIImagesOutputDir = retPath;
                            Psd2UIFormSettings.Save();
                        }
                        GUIUtility.ExitGUI();
                    }
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.BeginHorizontal();
                {
                    Psd2UIFormSettings.Instance.UseUIFormOutputDir = EditorGUILayout.ToggleLeft("使用UIForm导出路径:", Psd2UIFormSettings.Instance.UseUIFormOutputDir, GUILayout.Width(150));
                    EditorGUI.BeginDisabledGroup(!Psd2UIFormSettings.Instance.UseUIFormOutputDir);
                    {
                        Psd2UIFormSettings.Instance.UIFormOutputDir = EditorGUILayout.TextField(Psd2UIFormSettings.Instance.UIFormOutputDir);
                        if (GUILayout.Button("选择路径", GUILayout.Width(80)))
                        {
                            var retPath = EditorUtility.OpenFolderPanel("选择导出路径", Psd2UIFormSettings.Instance.UIFormOutputDir, null);
                            if (!string.IsNullOrWhiteSpace(retPath))
                            {
                                if (!retPath.StartsWith("Assets/"))
                                {
                                    retPath = UGUIParser.GetRelativePath(Application.dataPath, retPath);
                                }
                                Psd2UIFormSettings.Instance.UIFormOutputDir = retPath;
                                Psd2UIFormSettings.Save();
                            }
                            GUIUtility.ExitGUI();
                        }
                        EditorGUI.EndDisabledGroup();
                    }
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndVertical();
            }


            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button(parsePsd2NodesBt, btHeight))
                {
                    Psd2UIFormConverter.ParsePsd2LayerPrefab(targetLogic.PsdAssetName, targetLogic);
                }
                if (GUILayout.Button(exportUISpritesBt, btHeight))
                {
                    targetLogic.ExportSprites();
                }
                EditorGUILayout.EndHorizontal();
            }
            if (GUILayout.Button(generateUIFormBt, btHeight))
            {
                targetLogic.GenerateUIForm();
            }
            base.OnInspectorGUI();
        }
        /// <summary>
        /// 是否显示预览界面
        /// </summary>
        public override bool HasPreviewGUI()
        {
            return targetLogic.BindPsdAsset != null;
        }
        /// <summary>
        /// 绘制预览界面
        /// </summary>
        /// <param name="r">预览区域</param>
        /// <param name="background">背景样式</param>
        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            GUI.DrawTexture(r, targetLogic.BindPsdAsset.texture, ScaleMode.ScaleToFit);
            //base.OnPreviewGUI(r, background);
        }
    }
    /// <summary>
    /// PSD文件转成UIForm预制体的转换器
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(SpriteRenderer))]
    public class Psd2UIFormConverter : MonoBehaviour
    {
        /// <summary>
        /// 图层操作记录名称
        /// </summary>
        const string RecordLayerOperation = "Change Export Image";

        /// <summary>
        /// 单例实例
        /// </summary>
        public static Psd2UIFormConverter Instance { get; private set; }

        /// <summary>
        /// PSD文件修改时间标识
        /// </summary>
        [ReadOnlyField][SerializeField] public string psdAssetChangeTime;//文件修改时间标识
        [Tooltip("UIForm名字")][SerializeField] private string uiFormName;
        [Tooltip("关联的psd文件")][SerializeField] private UnityEngine.Sprite psdAsset;
        [Header("Debug:")][SerializeField] bool drawLayerRectGizmos = true;
        [SerializeField] UnityEngine.Color drawLayerRectGizmosColor = UnityEngine.Color.green;

        /// <summary>
        /// PSD文件解析实例
        /// </summary>
        private PsdImage psdInstance;//psd文件解析实例
        private GUIStyle uiTypeLabelStyle;
        /// <summary>
        /// PSD资源名称
        /// </summary>
        public string PsdAssetName => psdAsset != null ? AssetDatabase.GetAssetPath(psdAsset) : null;
        /// <summary>
        /// 绑定的PSD资源
        /// </summary>
        public UnityEngine.Sprite BindPsdAsset => psdAsset;
        /// <summary>
        /// UI表单画布大小
        /// </summary>
        public Vector2Int UIFormCanvasSize =>new Vector2Int(psdInstance.Width, psdInstance.Height);
        /// <summary>
        /// 当组件启用时调用
        /// </summary>
        private void OnEnable()
        {
            Instance = this;
            uiTypeLabelStyle = new GUIStyle();
            uiTypeLabelStyle.fontSize = 13;
            uiTypeLabelStyle.fontStyle = UnityEngine.FontStyle.BoldAndItalic;
            UnityEngine.ColorUtility.TryParseHtmlString("#7ED994", out var color);
            uiTypeLabelStyle.normal.textColor = color;

            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;

            if (psdInstance == null && !string.IsNullOrWhiteSpace(PsdAssetName))
            {
                RefreshNodesBindLayer();
            }
        }

        /// <summary>
        /// 当组件启动时调用
        /// </summary>
        private void Start()
        {
            RefreshNodesBindLayer();
        }
        /// <summary>
        /// 初始化Aspose许可证
        /// </summary>
        [InitializeOnLoadMethod]
        static void InitAsposeLicense()
        {
            Debug.LogWarning("请设置你的Aspose证书, 否则导出图片带有水印");
            //new Aspose.PSD.License().SetLicense(new MemoryStream(Convert.FromBase64String("Your License Key")));
        }
        /// <summary>
        /// 绘制Gizmos
        /// </summary>
        private void OnDrawGizmos()
        {
            if (drawLayerRectGizmos)
            {
                var nodes = this.GetComponentsInChildren<PsdLayerNode>();
                Gizmos.color = drawLayerRectGizmosColor;
                foreach (var item in nodes)
                {
                    if (item.NeedExportImage())
                    {
                        Gizmos.DrawWireCube(item.LayerRect.position * 0.01f, item.LayerRect.size * 0.01f);
                    }

                }
            }
        }

        /// <summary>
        /// 在层级窗口中绘制GUI
        /// </summary>
        /// <param name="instanceID">实例ID</param>
        /// <param name="selectionRect">选择区域</param>
        private void OnHierarchyGUI(int instanceID, Rect selectionRect)
        {
            if (Event.current == null) return;
            var node = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (node == null || node == this.gameObject) return;
            if (!node.TryGetComponent<PsdLayerNode>(out var layer)) return;

            Rect tmpRect = selectionRect;
            tmpRect.x = 35;
            tmpRect.width = 10;
            Undo.RecordObject(layer, RecordLayerOperation);
            EditorGUI.BeginChangeCheck();
            {
                layer.markToExport = EditorGUI.Toggle(tmpRect, layer.markToExport);
                if (EditorGUI.EndChangeCheck())
                {
                    if (Selection.gameObjects.Length > 1) SetExportImageTg(Selection.gameObjects, layer.markToExport);
                    EditorUtility.SetDirty(layer);
                }
            }
            tmpRect.width = Mathf.Clamp(selectionRect.xMax * 0.2f, 100, 200);
            tmpRect.x = selectionRect.xMax - tmpRect.width;
            if (EditorGUI.DropdownButton(tmpRect, new GUIContent(layer.UIType.ToString()), FocusType.Passive))
            {
                var dropdownMenu = PopUITypesMenu(layer, selectUIType =>
                {
                    layer.SetUIType(selectUIType);
                    EditorUtility.SetDirty(layer);
                });

                dropdownMenu.ShowAsContext();
            }
        }
        /// <summary>
        /// 弹出UI类型菜单
        /// </summary>
        /// <param name="layer">图层节点</param>
        /// <param name="onSelectEnum">选择回调</param>
        /// <returns>通用菜单</returns>
        private GenericMenu PopUITypesMenu(PsdLayerNode layer, Action<GUIType> onSelectEnum)
        {
            var names = Enum.GetValues(typeof(GUIType));
            var dropdownMenu = new GenericMenu();
            foreach (GUIType item in names)
            {
                string itemName = UGUIParser.IsMainUIType(item) ? item.ToString() : item.ToString().Replace('_', '/');
                dropdownMenu.AddItem(new GUIContent(itemName), item.Equals(layer.UIType), () => { onSelectEnum(item); });
            }
            return dropdownMenu;
        }

        /// <summary>
        /// 批量设置导出图片标记
        /// </summary>
        /// <param name="selects">选中的游戏对象</param>
        /// <param name="exportImg">是否导出图片</param>
        private void SetExportImageTg(GameObject[] selects, bool exportImg)
        {
            var selectLayerNodes = selects.Where(item => item?.GetComponent<PsdLayerNode>() != null).ToArray();
            foreach (var layer in selectLayerNodes)
            {
                layer.GetComponent<PsdLayerNode>().markToExport = exportImg;
            }
        }
        /// <summary>
        /// 当组件禁用时调用
        /// </summary>
        private void OnDisable()
        {
            
            EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyGUI;
        }
        /// <summary>
        /// 当组件销毁时调用
        /// </summary>
        private void OnDestroy()
        {
            if (this.psdInstance != null && !psdInstance.Disposed)
            {
                psdInstance.Dispose();
            }
        }

        /// <summary>
        /// 刷新节点绑定的图层
        /// </summary>
        private void RefreshNodesBindLayer()
        {
            if (psdInstance == null || psdInstance.Disposed)
            {
                if (!File.Exists(PsdAssetName))
                {
                    Debug.LogError($"刷新节点绑定图层失败! psd文件不存在");
                    return;
                }
                var psdOpts = new PsdLoadOptions()
                {
                    LoadEffectsResource = true,
                    ReadOnlyMode = false,
                };
                try
                {
                    psdInstance = Aspose.PSD.Image.Load(PsdAssetName, psdOpts) as PsdImage;
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    return;
                }
                
            }
            var layers = GetComponentsInChildren<PsdLayerNode>(true);
            foreach (var layer in layers)
            {
                layer.InitPsdLayers(psdInstance);
            }

            var spRender = gameObject.GetComponent<SpriteRenderer>() ?? gameObject.AddComponent<SpriteRenderer>();
            spRender.sprite = this.psdAsset;
        }
        
        /// <summary>
        /// PSD2UIForm编辑器菜单项
        /// </summary>
        [MenuItem("Assets/Psd2UIForm Editor", priority = 0)]
        static void Psd2UIFormPrefabMenu()
        {
            if (Selection.activeObject == null) return;
            var assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (Path.GetExtension(assetPath).ToLower().CompareTo(".psd") != 0)
            {
                Debug.LogWarning($"选择的文件({assetPath})不是psd格式, 工具只支持psd转换为UIForm");
                return;
            }
            string psdLayerPrefab = GetPsdLayerPrefabPath(assetPath);
            if (!File.Exists(psdLayerPrefab))
            {
                if (ParsePsd2LayerPrefab(assetPath))
                {
                    OpenPsdLayerEditor(psdLayerPrefab);
                }
            }
            else
            {
                OpenPsdLayerEditor(psdLayerPrefab);
            }
        }

        /// <summary>
        /// 检查PSD资源是否已更改
        /// </summary>
        /// <returns>是否已更改</returns>
        public bool CheckPsdAssetHasChanged()
        {
            if (psdAsset == null) return false;
            var fileTag = GetAssetChangeTag(PsdAssetName);
            return psdAssetChangeTime.CompareTo(fileTag) != 0;
        }
        /// <summary>
        /// 获取资源更改标记
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <returns>更改标记</returns>
        public static string GetAssetChangeTag(string fileName)
        {
            return new FileInfo(fileName).LastWriteTimeUtc.ToString("yyyyMMddHHmmss");
        }
        /// <summary>
        /// 打开PSD图层信息预制体
        /// </summary>
        /// <param name="psdLayerPrefab">PSD图层预制体路径</param>
        public static void OpenPsdLayerEditor(string psdLayerPrefab)
        {
#if UNITY_2018_3_OR_NEWER
            var prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(psdLayerPrefab);
            if (prefabAsset != null)
            {
                AssetDatabase.OpenAsset(prefabAsset);
            }
#else
            AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<GameObject>(psdLayerPrefab));
#endif
        }
        /// <summary>
        /// 将PSD图层解析为节点预制体
        /// </summary>
        /// <param name="psdFile">PSD文件路径</param>
        /// <param name="instanceRoot">实例根节点</param>
        /// <returns>是否成功</returns>
        public static bool ParsePsd2LayerPrefab(string psdFile, Psd2UIFormConverter instanceRoot = null)
        {
            if (!File.Exists(psdFile))
            {
                Debug.LogError($"Error: Psd文件不存在:{psdFile}");
                return false;
            }
            var texImporter = AssetImporter.GetAtPath(psdFile) as TextureImporter;
            if (texImporter.textureType != TextureImporterType.Sprite)
            {
                texImporter.textureType = TextureImporterType.Sprite;
                texImporter.mipmapEnabled = false;
                texImporter.alphaIsTransparency = true;
                texImporter.SaveAndReimport();
            }

            var prefabFile = GetPsdLayerPrefabPath(psdFile);
            var rootName = Path.GetFileNameWithoutExtension(prefabFile);

            bool needDestroyInstance = instanceRoot == null;
            if (instanceRoot != null)
            {
                ParsePsdLayer2Root(psdFile, instanceRoot);
                instanceRoot.RefreshNodesBindLayer();
                return true;
            }
            else
            {
                Psd2UIFormConverter rootLayer = CreatePsdLayerRoot(rootName);
                rootLayer.psdAssetChangeTime = GetAssetChangeTag(psdFile);
                rootLayer.SetPsdAsset(psdFile);
                ParsePsdLayer2Root(psdFile, rootLayer);

                PrefabUtility.SaveAsPrefabAsset(rootLayer.gameObject, prefabFile, out bool savePrefabSuccess);
                if (needDestroyInstance) GameObject.DestroyImmediate(rootLayer.gameObject);
                AssetDatabase.Refresh();
                if (savePrefabSuccess && AssetDatabase.GUIDFromAssetPath(StageUtility.GetCurrentStage().assetPath) != AssetDatabase.GUIDFromAssetPath(prefabFile))
                {
                    var prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabFile);
                    if (prefabAsset != null)
                    {
                        AssetDatabase.OpenAsset(prefabAsset);
                    }
                }

                return savePrefabSuccess;
            }
        }
        /// <summary>
        /// 解析PSD图层到根节点
        /// </summary>
        /// <param name="psdFile">PSD文件路径</param>
        /// <param name="converter">转换器实例</param>
        private static void ParsePsdLayer2Root(string psdFile, Psd2UIFormConverter converter)
        {
            var prefabFile = GetPsdLayerPrefabPath(psdFile);
            //清空已有节点重新解析
            for (int i = converter.transform.childCount - 1; i >= 0; i--)
            {
                GameObject.DestroyImmediate(converter.transform.GetChild(i).gameObject);
            }

            var psdOpts = new PsdLoadOptions()
            {
                LoadEffectsResource = true,
                ReadOnlyMode = false
            };
            try
            {
                using (var psd = Aspose.PSD.Image.Load(psdFile, psdOpts) as PsdImage)
                {
                    List<GameObject> layerNodes = new List<GameObject> { converter.gameObject };

                    for (int i = 0; i < psd.Layers.Length; i++)
                    {
                        var layer = psd.Layers[i];
                        var curLayerType = layer.GetLayerType();
                        if (curLayerType == PsdLayerType.SectionDividerLayer)
                        {
                            var layerGroup = (layer as SectionDividerLayer).GetRelatedLayerGroup();
                            var layerGroupIdx = ArrayUtility.IndexOf(psd.Layers, layerGroup);
                            var layerGropNode = CreatePsdLayerNode(layerGroup, layerGroupIdx);
                            layerNodes.Add(layerGropNode.gameObject);
                        }
                        else if (curLayerType == PsdLayerType.LayerGroup)
                        {
                            var lastLayerNode = layerNodes.Last();
                            layerNodes.Remove(lastLayerNode);

                            if (layerNodes.Count > 0)
                            {
                                var parentLayerNode = layerNodes.Last();
                                lastLayerNode.transform.SetParent(parentLayerNode.transform);
                            }
                        }
                        else
                        {
                            var newLayerNode = CreatePsdLayerNode(layer, i);
                            newLayerNode.transform.SetParent(layerNodes.Last().transform);
                            newLayerNode.transform.localPosition = Vector3.zero;
                        }
                    }
                }
                converter.psdAssetChangeTime = GetAssetChangeTag(psdFile);
                var childrenNodes = converter.GetComponentsInChildren<PsdLayerNode>(true);
                foreach (var item in childrenNodes)
                {
                    item.RefreshUIHelper(false);
                }
                EditorUtility.SetDirty(converter.gameObject);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            
            var prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabFile);
            if (prefabAsset != null)
            {
                AssetDatabase.OpenAsset(prefabAsset);
            }
        }
        /// <summary>
        /// 设置PSD资源
        /// </summary>
        /// <param name="psdFile">PSD文件路径</param>
        private void SetPsdAsset(string psdFile)
        {
            this.psdAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Sprite>(psdFile);
            if (string.IsNullOrWhiteSpace(Psd2UIFormSettings.Instance.UIImagesOutputDir))
            {
                Psd2UIFormSettings.Instance.UIImagesOutputDir = Path.GetDirectoryName(psdFile);
            }
            if (string.IsNullOrWhiteSpace(this.uiFormName))
            {
                this.uiFormName = this.psdAsset.name;
            }
        }

        /// <summary>
        /// 获取解析好的PSD图层文件路径
        /// </summary>
        /// <param name="psd">PSD文件路径</param>
        /// <returns>图层文件路径</returns>
        public static string GetPsdLayerPrefabPath(string psd)
        {
            return Path.Combine(Path.GetDirectoryName(psd), Path.GetFileNameWithoutExtension(psd) + "_psd_layers_parsed.prefab");
        }
        /// <summary>
        /// 创建PSD图层根节点
        /// </summary>
        /// <param name="rootName">根节点名称</param>
        /// <returns>转换器实例</returns>
        private static Psd2UIFormConverter CreatePsdLayerRoot(string rootName)
        {
            var node = new GameObject(rootName);
            node.gameObject.tag = "EditorOnly";
            var layerRoot = node.AddComponent<Psd2UIFormConverter>();
            return layerRoot;
        }
        /// <summary>
        /// 创建PSD图层节点
        /// </summary>
        /// <param name="layer">PSD图层</param>
        /// <param name="bindLayerIdx">绑定图层索引</param>
        /// <returns>图层节点</returns>
        private static PsdLayerNode CreatePsdLayerNode(Layer layer, int bindLayerIdx)
        {
            string nodeName = layer.Name;
            if (string.IsNullOrWhiteSpace(nodeName))
            {
                nodeName = $"PsdLayer-{bindLayerIdx}";
            }
            else
            {
                if (UGUIParser.HasUITypeFlag(nodeName, out var tpFlag))
                {
                    nodeName = nodeName.Substring(0, nodeName.Length - tpFlag.Length);
                }
            }
            var node = new GameObject(nodeName);
            node.gameObject.tag = "EditorOnly";
            var layerNode = node.AddComponent<PsdLayerNode>();
            layerNode.BindPsdLayerIndex = bindLayerIdx;
            InitLayerNodeData(layerNode, layer);
            return layerNode;
        }

        /// <summary>
        /// 初始化图层节点数据
        /// </summary>
        /// <param name="layerNode">图层节点</param>
        /// <param name="layer">PSD图层</param>
        private static void InitLayerNodeData(PsdLayerNode layerNode, Layer layer)
        {
            if (layer == null || layer.Disposed) return;
            var layerTp = layer.GetLayerType();
            layerNode.BindPsdLayer = layer;
            if (UGUIParser.Instance.TryParse(layerNode, out var initRule))
            {
                layerNode.SetUIType(initRule.UIType, false);
            }
            layerNode.markToExport = layerTp != PsdLayerType.LayerGroup && !(layerTp == PsdLayerType.TextLayer && layerNode.UIType.ToString().EndsWith("Text") && layerNode.UIType != GUIType.FillColor);
            layerNode.gameObject.SetActive(layer.IsVisible);
        }

        /// <summary>
        /// 导出PSD图层为精灵图片
        /// </summary>
        internal void ExportSprites()
        {
            //var pngOpts = new PngOptions()
            //{
            //    ColorType = Aspose.PSD.FileFormats.Png.PngColorType.Truecolor
            //};
            //this.psdInstance.Save("Assets/AAAGame/Sprites/UI/Preview.png", pngOpts);

            //return;
            var exportLayers = this.GetComponentsInChildren<PsdLayerNode>().Where(node => node.NeedExportImage());
            var exportDir = GetUIFormImagesOutputDir();
            if (!Directory.Exists(exportDir))
            {
                Directory.CreateDirectory(exportDir);
            }
            int exportIdx = 0;
            int totalCount = exportLayers.Count();
            foreach (var layer in exportLayers)
            {
                var assetName = layer.ExportImageAsset();
                if (assetName == null)
                {
                    Debug.LogWarning($"导出图层[name:{layer.name}, layerIdx:{layer.BindPsdLayerIndex}]图片失败!");
                }
                ++exportIdx;
                EditorUtility.DisplayProgressBar($"导出进度({exportIdx}/{totalCount})", $"导出UI图片:{assetName}", exportIdx / (float)totalCount);
            }
            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
        }
        /// <summary>
        /// 根据解析后的节点树生成UI表单预制体
        /// </summary>
        internal void GenerateUIForm()
        {
            if (Psd2UIFormSettings.Instance.UseUIFormOutputDir && string.IsNullOrWhiteSpace(Psd2UIFormSettings.Instance.UIFormOutputDir))
            {
                Debug.LogError($"生成UIForm失败! UIForm导出路径为空:{Psd2UIFormSettings.Instance.UIFormOutputDir}");
                return;
            }
            if (Psd2UIFormSettings.Instance.UseUIFormOutputDir)
            {
                ExportUIPrefab(Psd2UIFormSettings.Instance.UIFormOutputDir);
            }
            else
            {
                string lastSaveDir = string.IsNullOrWhiteSpace(Psd2UIFormSettings.Instance.LastUIFormOutputDir) ? "Assets" : Psd2UIFormSettings.Instance.LastUIFormOutputDir;
                string selectDir = EditorUtility.SaveFolderPanel("保存目录", lastSaveDir, null);
                if (!string.IsNullOrWhiteSpace(selectDir))
                {
                    if (!selectDir.StartsWith("Assets/"))
                        selectDir = UGUIParser.GetRelativePath(Application.dataPath, selectDir);
                    Psd2UIFormSettings.Instance.LastUIFormOutputDir = selectDir;
                    ExportUIPrefab(selectDir);
                }
            }
        }
        /// <summary>
        /// 导出UI预制体
        /// </summary>
        /// <param name="outputDir">输出目录</param>
        /// <returns>是否成功</returns>
        private bool ExportUIPrefab(string outputDir)
        {
            if (!string.IsNullOrWhiteSpace(outputDir))
            {
                if (!Directory.Exists(outputDir))
                {
                    try
                    {
                        Directory.CreateDirectory(outputDir);
                        AssetDatabase.Refresh();
                    }
                    catch (Exception err)
                    {
                        Debug.LogError($"导出UI prefab失败:{err.Message}");
                        return false;
                    }
                }
            }
            if (string.IsNullOrWhiteSpace(uiFormName))
            {
                Debug.LogError("导出UI Prefab失败: UI Form Name为空, 请填写UI Form Name.");
                return false;
            }
            var prefabName = Path.Combine(outputDir, $"{uiFormName}.prefab");
            if (File.Exists(prefabName))
            {
                if (!EditorUtility.DisplayDialog("警告", $"prefab文件已存在, 是否覆盖:{prefabName}", "覆盖生成", "取消生成"))
                {
                    return false;
                }
            }
            var uiHelpers = GetAvailableUIHelpers();
            if (uiHelpers == null || uiHelpers.Length < 1)
            {
                return false;
            }
            var uiFormRoot = GameObject.Instantiate(UGUIParser.Instance.UIFormTemplate, Vector3.zero, Quaternion.identity);
            uiFormRoot.name = uiFormName;
            Vector3 canvasPosition = uiFormRoot.GetComponent<RectTransform>().anchoredPosition;
            int curIdx = 0;
            int totalCount = uiHelpers.Length;
            foreach (var uiHelper in uiHelpers)
            {
                EditorUtility.DisplayProgressBar($"生成UIFrom:({curIdx++}/{totalCount})", $"正在生成UI元素:{uiHelper.name}", curIdx /
                (float)totalCount);
                var uiElement = uiHelper.CreateUI();
                if (uiElement == null) continue;

                var goPath = GetGameObjectInstanceIdPath(uiHelper.gameObject, out var goNames);
                var parentNode = GetOrCreateNodeByInstanceIdPath(uiFormRoot, goPath, goNames);
                uiElement.transform.SetParent(parentNode.transform, true);
                uiElement.transform.position += canvasPosition;
            }
            var uiStrKeys = uiFormRoot.GetComponentsInChildren<UIStringKey>(true);
            for (int i = uiStrKeys.Length - 1; i >= 0; i--)
            {
                DestroyImmediate(uiStrKeys[i]);
            }

            var uiPrefab = PrefabUtility.SaveAsPrefabAsset(uiFormRoot, prefabName, out bool saveSuccess);
            if (saveSuccess)
            {
                DestroyImmediate(uiFormRoot);
                Selection.activeGameObject = uiPrefab;
            }
            EditorUtility.ClearProgressBar();
            return true;
        }

        /// <summary>
        /// 根据实例ID路径获取或创建节点
        /// </summary>
        /// <param name="uiFormRoot">UI表单根节点</param>
        /// <param name="goPath">游戏对象路径</param>
        /// <param name="goNames">游戏对象名称</param>
        /// <returns>目标节点</returns>
        private GameObject GetOrCreateNodeByInstanceIdPath(GameObject uiFormRoot, string[] goPath, string[] goNames)
        {
            GameObject result = uiFormRoot;
            if (goPath != null && goNames != null)
            {
                for (int i = 0; i < goPath.Length; i++)
                {
                    var nodeId = goPath[i];
                    var nodeName = goNames[i];
                    GameObject targetNode = null;
                    foreach (Transform child in result.transform)
                    {
                        if (child.gameObject == result) continue;

                        var idKey = child.GetComponent<UIStringKey>();
                        if (idKey != null && nodeId == idKey.Key)
                        {
                            targetNode = child.gameObject;
                            break;
                        }
                    }
                    if (targetNode == null)
                    {
                        targetNode = new GameObject(nodeName);
                        targetNode.transform.SetParent(result.transform, false);
                        targetNode.transform.localPosition = Vector3.zero;
                        targetNode.transform.localRotation = Quaternion.identity;
                        var targetNodeKey = targetNode.GetComponent<UIStringKey>() ?? targetNode.AddComponent<UIStringKey>();
                        targetNodeKey.Key = nodeId;
                    }
                    result = targetNode;
                }
            }
            return result;
        }

        /// <summary>
        /// 获取游戏对象实例ID路径
        /// </summary>
        /// <param name="go">游戏对象</param>
        /// <param name="names">名称数组</param>
        /// <returns>实例ID路径</returns>
        private string[] GetGameObjectInstanceIdPath(GameObject go, out string[] names)
        {
            names = null;
            if (go == null || go.transform.parent == null || go.transform.parent == this.transform) return null;

            var parentGo = go.transform.parent;
            string[] result = new string[1] { parentGo.gameObject.GetInstanceID().ToString() };
            names = new string[1] { parentGo.gameObject.name };
            while (parentGo.parent != null && parentGo.parent != this.transform)
            {
                ArrayUtility.Insert(ref result, 0, parentGo.parent.gameObject.GetInstanceID().ToString());
                ArrayUtility.Insert(ref names, 0, parentGo.parent.gameObject.name);
                parentGo = parentGo.parent;
            }
            return result;
        }
        /// <summary>
        /// 获取可用的UI助手
        /// </summary>
        /// <returns>UI助手数组</returns>
        private UIHelperBase[] GetAvailableUIHelpers()
        {
            var uiHelpers = this.GetComponentsInChildren<UIHelperBase>();
            uiHelpers = uiHelpers.Where(ui => ui.LayerNode.IsMainUIType).ToArray();

            List<int> dependInstIds = new List<int>();
            foreach (var item in uiHelpers)
            {
                foreach (var depend in item.GetDependencies())
                {
                    int dependId = depend.gameObject.GetInstanceID();
                    if (!dependInstIds.Contains(dependId))
                    {
                        dependInstIds.Add(dependId);
                    }
                }
            }
            for (int i = uiHelpers.Length - 1; i >= 0; i--)
            {
                var uiHelper = uiHelpers[i];
                if (dependInstIds.Contains(uiHelper.gameObject.GetInstanceID()))
                {
                    ArrayUtility.RemoveAt(ref uiHelpers, i);
                }
            }
            return uiHelpers;
        }
        /// <summary>
        /// 将图片设置为Sprite或Texture类型
        /// </summary>
        /// <param name="texAssets">纹理资源路径</param>
        /// <param name="isImage">是否为图片</param>
        public static void ConvertTexturesType(string[] texAssets, bool isImage = true)
        {
            foreach (var item in texAssets)
            {
                var texImporter = AssetImporter.GetAtPath(item) as TextureImporter;
                if (texImporter == null)
                {
                    Debug.LogError($"TextureImporter为空:{item}");
                    continue;
                }
                if (isImage)
                {
                    texImporter.textureType = TextureImporterType.Sprite;
                    texImporter.spriteImportMode = SpriteImportMode.Single;
                    texImporter.alphaSource = TextureImporterAlphaSource.FromInput;
                    texImporter.alphaIsTransparency = true;
                    texImporter.mipmapEnabled = false;
                }
                else
                {
                    texImporter.textureType = TextureImporterType.Default;
                    texImporter.textureShape = TextureImporterShape.Texture2D;
                    texImporter.alphaSource = TextureImporterAlphaSource.FromInput;
                    texImporter.alphaIsTransparency = true;
                    texImporter.mipmapEnabled = false;
                }

                texImporter.SaveAndReimport();
            }
        }

        /// <summary>
        /// 压缩图片文件
        /// </summary>
        /// <param name="asset">资源路径</param>
        /// <returns>是否成功</returns>
        public static bool CompressImageFile(string asset)
        {
            var assetPath = asset.StartsWith("Assets/") ? Path.Combine(Directory.GetParent(Application.dataPath).FullName, asset) : asset;
            var compressTool = Utility.Assembly.GetType("UGF.EditorTools.CompressTool");
            if (compressTool == null) return false;

            var compressMethod = compressTool.GetMethod("CompressImageOffline", new Type[] { typeof(string), typeof(string) });
            if (compressMethod == null) return false;

            return (bool)compressMethod.Invoke(null, new object[] { assetPath, assetPath });
        }
        /// <summary>
        /// 获取UI表单对应的图片导出目录
        /// </summary>
        /// <returns>导出目录路径</returns>
        public string GetUIFormImagesOutputDir()
        {
            return Path.Combine(Psd2UIFormSettings.Instance.UIImagesOutputDir, uiFormName);
        }

        /// <summary>
        /// 将图层转换为智能对象图层
        /// </summary>
        /// <param name="layer">要转换的图层</param>
        /// <returns>智能对象图层</returns>
        public SmartObjectLayer ConvertToSmartObjectLayer(Layer layer)
        {
            var smartObj = psdInstance.SmartObjectProvider.ConvertToSmartObject(new Layer[] { layer });
            return smartObj;
        }
    }
}
#endif


