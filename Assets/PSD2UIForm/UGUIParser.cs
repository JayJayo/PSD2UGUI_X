/*
    联系作者:
    https://blog.csdn.net/final5788
    https://github.com/sunsvip
 */
#if UNITY_EDITOR
using Aspose.PSD.FileFormats.Psd.Layers.FillLayers;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

namespace UGF.EditorTools.Psd2UGUI
{
    /// <summary>
    /// UI类型枚举，定义了所有支持的UI组件类型
    /// </summary>
    public enum GUIType
    {
        /// <summary>
        /// 空类型
        /// </summary>
        Null = 0,
        /// <summary>
        /// 图片组件
        /// </summary>
        Image,
        /// <summary>
        /// 原始图片组件
        /// </summary>
        RawImage,
        /// <summary>
        /// 文本组件
        /// </summary>
        Text,
        /// <summary>
        /// 按钮组件
        /// </summary>
        Button,
        /// <summary>
        /// 下拉框组件
        /// </summary>
        Dropdown,
        /// <summary>
        /// 输入框组件
        /// </summary>
        InputField,
        /// <summary>
        /// 开关组件
        /// </summary>
        Toggle,
        /// <summary>
        /// 滑动条组件
        /// </summary>
        Slider,
        /// <summary>
        /// 滚动视图组件
        /// </summary>
        ScrollView,
        /// <summary>
        /// 遮罩组件
        /// </summary>
        Mask,
        /// <summary>
        /// 纯色填充组件
        /// </summary>
        FillColor,
        /// <summary>
        /// TextMeshPro文本组件
        /// </summary>
        TMPText,
        /// <summary>
        /// TextMeshPro按钮组件
        /// </summary>
        TMPButton,
        /// <summary>
        /// TextMeshPro下拉框组件
        /// </summary>
        TMPDropdown,
        /// <summary>
        /// TextMeshPro输入框组件
        /// </summary>
        TMPInputField,
        /// <summary>
        /// TextMeshPro开关组件
        /// </summary>
        TMPToggle,

        //UI的子类型, 以101开始。 0-100预留给UI类型, 新类型从尾部追加
        /// <summary>
        /// 通用背景
        /// </summary>
        Background = 101,

        //Button的子类型
        /// <summary>
        /// 按钮高亮状态
        /// </summary>
        Button_Highlight,
        /// <summary>
        /// 按钮按下状态
        /// </summary>
        Button_Press,
        /// <summary>
        /// 按钮选中状态
        /// </summary>
        Button_Select,
        /// <summary>
        /// 按钮禁用状态
        /// </summary>
        Button_Disable,
        /// <summary>
        /// 按钮文本
        /// </summary>
        Button_Text,

        //Dropdown/TMPDropdown的子类型
        /// <summary>
        /// 下拉框标签
        /// </summary>
        Dropdown_Label,
        /// <summary>
        /// 下拉框箭头
        /// </summary>
        Dropdown_Arrow,

        //InputField/TMPInputField的子类型
        /// <summary>
        /// 输入框占位符
        /// </summary>
        InputField_Placeholder,
        /// <summary>
        /// 输入框文本
        /// </summary>
        InputField_Text,

        //Toggle的子类型
        /// <summary>
        /// 开关勾选框
        /// </summary>
        Toggle_Checkmark,
        /// <summary>
        /// 开关标签
        /// </summary>
        Toggle_Label,

        //Slider的子类型
        /// <summary>
        /// 滑动条填充
        /// </summary>
        Slider_Fill,
        /// <summary>
        /// 滑动条手柄
        /// </summary>
        Slider_Handle,

        //ScrollView的子类型
        /// <summary>
        /// 滚动视图可视区域遮罩
        /// </summary>
        ScrollView_Viewport,
        /// <summary>
        /// 水平滚动条背景
        /// </summary>
        ScrollView_HorizontalBarBG,
        /// <summary>
        /// 水平滚动条
        /// </summary>
        ScrollView_HorizontalBar,
        /// <summary>
        /// 垂直滚动条背景
        /// </summary>
        ScrollView_VerticalBarBG,
        /// <summary>
        /// 垂直滚动条
        /// </summary>
        ScrollView_VerticalBar,
    }
    /// <summary>
    /// UI解析规则类，定义了UI组件的解析规则
    /// </summary>
    [Serializable]
    public class UGUIParseRule
    {
        /// <summary>
        /// UI类型
        /// </summary>
        public GUIType UIType;
        /// <summary>
        /// 类型匹配标识数组
        /// </summary>
        public string[] TypeMatches;
        /// <summary>
        /// UI预制体模板
        /// </summary>
        public GameObject UIPrefab;
        /// <summary>
        /// UI辅助类全名
        /// </summary>
        public string UIHelper;
        /// <summary>
        /// 注释说明
        /// </summary>
        public string Comment;
    }
    /// <summary>
    /// UGUIParser编辑器类，用于在Unity编辑器中显示和编辑UGUIParser组件
    /// </summary>
    [CustomEditor(typeof(UGUIParser))]
    public class UGUIParserEditor : Editor
    {
        private SerializedProperty readmeProperty;
        SerializedProperty defaultTextType;
        SerializedProperty defaultImageType;
        private string[] textTypesDisplay;
        private int[] textTypes;
        private string[] imageTypesDisplay;
        private int[] imageTypes;
        private void OnEnable()
        {
            readmeProperty = serializedObject.FindProperty("readmeDoc");
            defaultTextType = serializedObject.FindProperty("defaultTextType");
            defaultImageType = serializedObject.FindProperty("defaultImageType");

            var textEnums = new GUIType[] { GUIType.Text, GUIType.TMPText };
            textTypes = new int[textEnums.Length];
            textTypesDisplay = new string[textEnums.Length];
            for (int i = 0; i < textEnums.Length; i++)
            {
                var textEnum = textEnums[i];
                textTypes[i] = (int)textEnum;
                textTypesDisplay[i] = textEnum.ToString();
            }

            var imageEnums = new GUIType[] { GUIType.Image, GUIType.RawImage };
            imageTypes = new int[imageEnums.Length];
            imageTypesDisplay = new string[imageEnums.Length];
            for (int i = 0; i < imageEnums.Length; i++)
            {
                var imageEnum = imageEnums[i];
                imageTypes[i] = (int)imageEnum;
                imageTypesDisplay[i] = imageEnum.ToString();
            }
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            if (GUILayout.Button("使用教程"))
            {
                Application.OpenURL("https://blog.csdn.net/final5788");
            }
            if (GUILayout.Button("导出使用文档"))
            {
                (target as UGUIParser).ExportReadmeDoc();
            }
            EditorGUILayout.LabelField("使用说明:");
            readmeProperty.stringValue = EditorGUILayout.TextArea(readmeProperty.stringValue, GUILayout.Height(100));

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("默认文本类型:", GUILayout.Width(150));
                defaultTextType.enumValueIndex = EditorGUILayout.IntPopup(defaultTextType.enumValueIndex, textTypesDisplay, textTypes);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("默认图片类型:", GUILayout.Width(150));
                defaultImageType.enumValueIndex = EditorGUILayout.IntPopup(defaultImageType.enumValueIndex, imageTypesDisplay, imageTypes);
                EditorGUILayout.EndHorizontal();
            }
            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }
    }
    /// <summary>
    /// PSD2UIForm配置类，用于配置PSD到UI的转换规则
    /// </summary>
    [CreateAssetMenu(fileName = "Psd2UIFormConfig", menuName = "ScriptableObject/Psd2UIForm Config【Psd2UIForm工具配置】")]
    public class UGUIParser : ScriptableObject
    {
        /// <summary>
        /// UI类型分隔符
        /// </summary>
        public const char UITYPE_SPLIT_CHAR = '.';
        /// <summary>
        /// UI类型最大值
        /// </summary>
        public const int UITYPE_MAX = 100;
        [HideInInspector][SerializeField] GUIType defaultTextType = GUIType.Text;
        [HideInInspector][SerializeField] GUIType defaultImageType = GUIType.Image;
        [SerializeField] GameObject uiFormTemplate;
        [SerializeField] UGUIParseRule[] rules;
        [HideInInspector][SerializeField] string readmeDoc = "使用说明";

        /// <summary>
        /// 获取默认文本类型
        /// </summary>
        public GUIType DefaultText => defaultTextType;
        /// <summary>
        /// 获取默认图片类型
        /// </summary>
        public GUIType DefaultImage => defaultImageType;
        /// <summary>
        /// 获取UI表单模板
        /// </summary>
        public GameObject UIFormTemplate => uiFormTemplate;
        private static UGUIParser mInstance = null;
        /// <summary>
        /// 获取UGUIParser单例实例
        /// </summary>
        public static UGUIParser Instance
        {
            get
            {
                if (mInstance == null)
                {
                    var guid = AssetDatabase.FindAssets("t:UGUIParser").FirstOrDefault();
                    mInstance = AssetDatabase.LoadAssetAtPath<UGUIParser>(AssetDatabase.GUIDToAssetPath(guid));
                }
                return mInstance;
            }
        }
        /// <summary>
        /// 判断是否为主要的UI类型
        /// </summary>
        /// <param name="tp">要检查的UI类型</param>
        /// <returns>如果是主要UI类型返回true，否则返回false</returns>
        public static bool IsMainUIType(GUIType tp)
        {
            return (int)tp <= UITYPE_MAX;
        }
        /// <summary>
        /// 获取UI类型的辅助类类型
        /// </summary>
        /// <param name="uiType">UI类型</param>
        /// <returns>对应的辅助类Type</returns>
        public Type GetHelperType(GUIType uiType)
        {
            if (uiType == GUIType.Null) return null;
            var rule = GetRule(uiType);
            if (rule == null || string.IsNullOrWhiteSpace(rule.UIHelper)) return null;

            return Type.GetType(rule.UIHelper);
        }
        /// <summary>
        /// 获取UI类型的解析规则
        /// </summary>
        /// <param name="uiType">UI类型</param>
        /// <returns>对应的解析规则</returns>
        public UGUIParseRule GetRule(GUIType uiType)
        {
            foreach (var rule in rules)
            {
                if (rule.UIType == uiType) return rule;
            }
            return null;
        }
        /// <summary>
        /// 尝试解析图层节点
        /// </summary>
        /// <param name="layer">要解析的图层节点</param>
        /// <param name="result">解析结果</param>
        /// <returns>解析是否成功</returns>
        public bool TryParse(PsdLayerNode layer, out UGUIParseRule result)
        {
            result = null;
            var layerName = layer.BindPsdLayer.Name;
            if (HasUITypeFlag(layerName, out var tpFlag))
            {
                var tpTag = tpFlag.Substring(1);
                foreach (var rule in rules)
                {
                    foreach (var item in rule.TypeMatches)
                    {
                        if (tpTag.CompareTo(item.ToLower()) == 0)
                        {
                            result = rule;
                            return true;
                        }
                    }
                }
            }

            switch (layer.LayerType)
            {
                case PsdLayerType.TextLayer:
                    result = rules.First(itm => itm.UIType == defaultTextType);
                    break;
                case PsdLayerType.LayerGroup:
                    result = rules.First(itm => itm.UIType == GUIType.Null);
                    break;
                default:
                    result = rules.First(itm => itm.UIType == defaultImageType);
                    break;
            }
            return result != null;
        }
        /// <summary>
        /// 检查图层名称是否包含UI类型标记
        /// </summary>
        /// <param name="layerName">图层名称</param>
        /// <param name="tpFlag">输出的类型标记</param>
        /// <returns>是否包含UI类型标记</returns>
        public static bool HasUITypeFlag(string layerName, out string tpFlag)
        {
            tpFlag = null;
            if (string.IsNullOrWhiteSpace(layerName) || layerName.EndsWith(UGUIParser.UITYPE_SPLIT_CHAR.ToString())) return false;
            int startIdx = -1;
            for (int i = layerName.Length - 1; i >= 0; i--)
            {
                if (layerName[i] == UGUIParser.UITYPE_SPLIT_CHAR)
                {
                    startIdx = i;
                    break;
                }
            }
            if (startIdx <= 0) return false;

            tpFlag = layerName.Substring(startIdx);
            return true;
        }
        /// <summary>
        /// 设置RectTransform组件的大小和位置
        /// </summary>
        /// <param name="layerNode">图层节点</param>
        /// <param name="uiNode">UI节点组件</param>
        /// <param name="pos">是否设置位置</param>
        /// <param name="width">是否设置宽度</param>
        /// <param name="height">是否设置高度</param>
        /// <param name="extSize">额外大小</param>
        public static void SetRectTransform(PsdLayerNode layerNode, UnityEngine.Component uiNode, bool pos = true, bool width = true, bool height = true, int extSize = 0)
        {
            if (uiNode != null && layerNode != null)
            {
                var rect = layerNode.LayerRect;
                var rectTransform = uiNode.GetComponent<RectTransform>();
                if (width) rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rect.size.x + extSize);
                if (height) rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rect.size.y + extSize);
                if (pos)
                {
                    //rectTransform.position = rect.position + rectTransform.rect.size * (rectTransform.pivot - Vector2.one * 0.5f) * 0.01f;
                    rectTransform.SetPositionAndRotation(rect.position + rectTransform.rect.size * (rectTransform.pivot - Vector2.one * 0.5f) * 0.01f, Quaternion.identity);
                }
            }
        }

        /// <summary>
        /// 将图层节点转换为Texture2D
        /// </summary>
        /// <param name="layerNode">图层节点</param>
        /// <returns>转换后的Texture2D</returns>
        public static Texture2D LayerNode2Texture(PsdLayerNode layerNode)
        {
            if (layerNode != null)
            {
                var spAssetName = layerNode.ExportImageAsset(false);
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(spAssetName);
                return texture;
            }
            return null;
        }
        /// <summary>
        /// 将图层节点转换为Sprite
        /// </summary>
        /// <param name="layerNode"></param>
        /// <param name="auto9Slice">若没有设置Sprite的九宫,是否自动计算并设置九宫</param>
        /// <returns></returns>
        public static Sprite LayerNode2Sprite(PsdLayerNode layerNode, bool auto9Slice = false)
        {
            if (layerNode != null)
            {
                var spAssetName = layerNode.ExportImageAsset(true);
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spAssetName);
                if (sprite != null)
                {
                    if (auto9Slice)
                    {
                        var spImpt = AssetImporter.GetAtPath(spAssetName) as TextureImporter;
                        var rawReadable = spImpt.isReadable;
                        if (!rawReadable)
                        {
                            spImpt.isReadable = true;
                            spImpt.SaveAndReimport();
                        }
                        if (spImpt.spriteBorder == Vector4.zero)
                        {
                            spImpt.spriteBorder = CalculateTexture9SliceBorder(sprite.texture, layerNode.BindPsdLayer.Opacity);
                            spImpt.isReadable = rawReadable;
                            spImpt.SaveAndReimport();
                        }
                    }
                    return sprite;
                }
            }
            return null;
        }
        /// <summary>
        /// 计算纹理的九宫格边界
        /// </summary>
        /// <param name="texture">要计算的纹理</param>
        /// <param name="alphaThreshold">透明度阈值(0-255)</param>
        /// <returns>九宫格边界值</returns>
        public static Vector4 CalculateTexture9SliceBorder(Texture2D texture, byte alphaThreshold = 3)
        {
            int width = texture.width;
            int height = texture.height;

            Color32[] pixels = texture.GetPixels32();
            int minX = width;
            int minY = height;
            int maxX = 0;
            int maxY = 0;

            // 寻找不透明像素的最小和最大边界
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int pixelIndex = y * width + x;
                    Color32 pixel = pixels[pixelIndex];

                    if (pixel.a >= alphaThreshold)
                    {
                        minX = Mathf.Min(minX, x);
                        minY = Mathf.Min(minY, y);
                        maxX = Mathf.Max(maxX, x);
                        maxY = Mathf.Max(maxY, y);
                    }
                }
            }

            // 计算最优的borderSize
            int borderSizeX = (maxX - minX) / 3;
            int borderSizeY = (maxY - minY) / 3;
            int borderSize = Mathf.Min(borderSizeX, borderSizeY);

            // 根据边界和Border Size计算Nine Slice Border
            int left = minX + borderSize;
            int right = maxX - borderSize;
            int top = minY + borderSize;
            int bottom = maxY - borderSize;

            // 确保边界在纹理范围内
            left = Mathf.Clamp(left, 0, width - 1);
            right = Mathf.Clamp(right, 0, width - 1);
            top = Mathf.Clamp(top, 0, height - 1);
            bottom = Mathf.Clamp(bottom, 0, height - 1);

            return new Vector4(left, top, width - right, height - bottom);
        }

        /// <summary>
        /// 设置Text组件的样式
        /// </summary>
        /// <param name="txtLayer">文本图层节点</param>
        /// <param name="text">要设置的Text组件</param>
        public static void SetTextStyle(PsdLayerNode txtLayer, UnityEngine.UI.Text text)
        {
            if (text == null) return;
            text.gameObject.SetActive(txtLayer != null);
            if (txtLayer != null && txtLayer.ParseTextLayerInfo(out var str, out var size, out var charSpace, out float lineSpace, out var col, out var style, out var tmpStyle, out var fName))
            {
                var tFont = FindFontAsset(fName);
                if (tFont != null) text.font = tFont;
                text.text = str;
                text.fontSize = size;
                text.fontStyle = style;
                text.color = col;
                text.lineSpacing = lineSpace;
            }
        }
        /// <summary>
        /// 设置TextMeshProUGUI组件的样式
        /// </summary>
        /// <param name="txtLayer">文本图层节点</param>
        /// <param name="text">要设置的TextMeshProUGUI组件</param>
        public static void SetTextStyle(PsdLayerNode txtLayer, TextMeshProUGUI text)
        {
            if (txtLayer != null && txtLayer.ParseTextLayerInfo(out var str, out var size, out var charSpace, out float lineSpace, out var col, out var style, out var tmpStyle, out var fName))
            {
                var tFont = FindTMPFontAsset(fName);
                if (tFont != null) text.font = tFont;
                text.text = str;
                text.fontSize = size;
                text.fontStyle = tmpStyle;
                text.color = col;
                text.characterSpacing = charSpace;
                text.lineSpacing = lineSpace;
            }
        }
        /// <summary>
        /// 获取修正后的字体名称
        /// </summary>
        /// <param name="fontName">原始字体名称</param>
        /// <returns>修正后的字体名称</returns>
        public static string GetFixedFontName(string fontName)
        {
            string fixedFontName = Regex.Replace(fontName, "[^A-Za-z0-9]+", " ");
            return fixedFontName;
        }
        /// <summary>
        /// 查找TextMeshPro字体资源
        /// </summary>
        /// <param name="fontName">字体名称</param>
        /// <returns>找到的TextMeshPro字体资源</returns>
        public static TMP_FontAsset FindTMPFontAsset(string fontName)
        {
            string fixedFontName = GetFixedFontName(fontName);
            var fontGuids = AssetDatabase.FindAssets("t:TMP_FontAsset");
            foreach (var guid in fontGuids)
            {
                var fontPath = AssetDatabase.GUIDToAssetPath(guid);
                var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath);
                if (font != null && (font.faceInfo.familyName == fontName || font.faceInfo.familyName == fixedFontName))
                {
                    return font;
                }
            }

            return null;
        }
        /// <summary>
        /// 查找Font Asset
        /// </summary>
        /// <param name="fontName"></param>
        /// <returns></returns>
        public static UnityEngine.Font FindFontAsset(string fontName)
        {
            var fontNameLow = fontName.ToLower();
            string fixedFontName = GetFixedFontName(fontNameLow);
            var fontGuids = AssetDatabase.FindAssets("t:font");
            foreach (var guid in fontGuids)
            {
                var fontPath = AssetDatabase.GUIDToAssetPath(guid);
                var font = AssetImporter.GetAtPath(fontPath) as TrueTypeFontImporter;
                var assetFontNameLow = font.fontTTFName.ToLower();
                if (font != null && (assetFontNameLow.CompareTo(fontNameLow) == 0 || assetFontNameLow.CompareTo(fixedFontName) == 0))
                {
                    return AssetDatabase.LoadAssetAtPath<UnityEngine.Font>(fontPath);
                }
            }
            return null;
        }

        internal static UnityEngine.Color LayerNode2Color(PsdLayerNode fillColor, Color defaultColor)
        {
            if (fillColor != null && fillColor.BindPsdLayer is FillLayer fillLayer)
            {
                var layerColor = fillLayer.GetPixel(fillLayer.Width / 2, fillLayer.Height / 2);
                return new UnityEngine.Color(layerColor.R, layerColor.G, layerColor.B, fillLayer.Opacity) / (float)255;
            }
            return defaultColor;
        }
        /// <summary>
        /// 导出UI设计师使用规则文档
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        internal void ExportReadmeDoc()
        {
            var exportDir = EditorUtility.SaveFolderPanel("选择文档导出路径", Application.dataPath, null);
            if (string.IsNullOrWhiteSpace(exportDir) || !Directory.Exists(exportDir))
            {
                return;
            }

            var docFile = Path.Combine(exportDir, "Psd2UGUI设计师使用文档.doc");
            var strBuilder = new StringBuilder();
            strBuilder.AppendLine("使用说明:");
            strBuilder.AppendLine(this.readmeDoc);
            strBuilder.AppendLine(Environment.NewLine + Environment.NewLine);
            strBuilder.AppendLine("UI类型标识: 图层/组命名以'.类型'结尾");
            strBuilder.AppendLine("UI类型标识列表:");

            foreach (var rule in rules)
            {
                if (rule.UIType == GUIType.Null) continue;

                strBuilder.AppendLine($"{rule.UIType}: {rule.Comment}");
                strBuilder.Append("类型标识: ");
                foreach (var tag in rule.TypeMatches)
                {
                    strBuilder.Append($".{tag}, ");
                }
                strBuilder.AppendLine();
                strBuilder.AppendLine();
            }

            try
            {
                File.WriteAllText(docFile, strBuilder.ToString(), System.Text.Encoding.UTF8);
                EditorUtility.RevealInFinder(docFile);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

        }
        public static string GetRelativePath(string basePath, string path)
        {
            Uri baseUri = new Uri(basePath.EndsWith("\\") ? basePath : basePath + "\\");
            Uri pathUri = new Uri(path);
            string relativePath = Uri.UnescapeDataString(baseUri.MakeRelativeUri(pathUri).ToString());
            return relativePath.Replace("/", Path.DirectorySeparatorChar.ToString());
        }
    }
}
#endif