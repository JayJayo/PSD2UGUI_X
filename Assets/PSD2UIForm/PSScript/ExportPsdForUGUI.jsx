/*
    PSD2UGUI - Photoshop to Unity UGUI Converter
    Copyright (c) 2024
    All rights reserved.
 */

/**
 * 联系作者:
 * https://blog.csdn.net/final5788
 * https://github.com/sunsvip
 */

/**
 * 检查图层是否有效果
 * @param {Layer} layer - 要检查的图层
 * @returns {boolean} 图层是否有效果
 */
function hasLayerEffect(layer) {
    app.activeDocument.activeLayer = layer;
	var hasEffect = false;
	try {
		var ref = new ActionReference();
		var keyLayerEffects = app.charIDToTypeID( 'Lefx' );
		ref.putProperty( app.charIDToTypeID( 'Prpr' ), keyLayerEffects );
		ref.putEnumerated( app.charIDToTypeID( 'Lyr ' ), app.charIDToTypeID( 'Ordn' ), app.charIDToTypeID( 'Trgt' ) );
		var desc = executeActionGet( ref );
		if ( desc.hasKey( keyLayerEffects ) ) {
			hasEffect = true;
		}
	}catch(e) {
		hasEffect = false;
	}
	return hasEffect;
}

/**
 * 将图层转换为智能对象
 * @param {Array} layers - 要转换的图层数组
 */
function convertLayersToSmartObjects(layers) 
{
    for (var i = layers.length - 1; i >= 0; i--) 
    {
        var layer = layers[i];
        if (layer.typename === "LayerSet")
        {
               convertLayersToSmartObjects(layer.layers); // 递归转换图层组中的图层
        } 
        else
        {
            if(layer.kind === LayerKind.SOLIDFILL){
                rasterizeLayer(layer);
            }else if (hasLayerEffect(layer) || layer.kind === LayerKind.TEXT){
                convertToSmartObject(layer); // Convert layers with layer effects to smart objects
            }
        }
    }
}
// 栅格化图层
function rasterizeLayer(layer) {
    var desc = new ActionDescriptor();
    var ref = new ActionReference();
    ref.putIdentifier(charIDToTypeID('Lyr '), layer.id);
    desc.putReference(charIDToTypeID('null'), ref);
    executeAction(stringIDToTypeID('rasterizeLayer'), desc, DialogModes.NO);
  }
// 递归遍历图层函数
function convertToSmartObject(layer) {
    var desc = new ActionDescriptor();
    var ref = new ActionReference();
    ref.putIdentifier(charIDToTypeID('Lyr '), layer.id);
    desc.putReference(charIDToTypeID('null'), ref);
    // 创建一个新的智能对象
    var idnewPlacedLayer = stringIDToTypeID("newPlacedLayer");
    executeAction(idnewPlacedLayer, desc, DialogModes.NO);
}
// 导出处理后的PSD文件
function exportPSD() {
  var doc = app.activeDocument;
  var savePath = Folder.selectDialog("选择psd导出路径");
  if (savePath != null) {
    var saveOptions = new PhotoshopSaveOptions();
    saveOptions.embedColorProfile = true;
    saveOptions.alphaChannels = true;

    var saveFile = new File(savePath + "/" + doc.name);
    doc.saveAs(saveFile, saveOptions, true, Extension.LOWERCASE);
    alert("PSD已成功导出!");
  }
}
function convertAndExport(){
    convertLayersToSmartObjects (app.activeDocument.layers);
    //exportPSD();
}
app.activeDocument.suspendHistory("Convert2SmartObject", "convertAndExport();");
//~ convertLayersToSmartObjects (app.activeDocument.layers);


// 检查并实现 JSON.stringify（完全兼容 ExtendScript）
if (typeof JSON === 'undefined' || typeof JSON.stringify !== 'function') {
    if (typeof JSON === 'undefined') JSON = {};
    
    JSON.stringify = function(obj, replacer, space) {
        var cache = [];
        function serialize(key, value) {
            // 处理循环引用
            if (typeof value === 'object' && value !== null) {
                // 检查是否已经处理过这个对象
                for (var i = 0; i < cache.length; i++) {
                    if (cache[i] === value) return '[Circular]';
                }
                cache.push(value);
            }
            
            // 处理特殊对象（Date/RegExp）
            if (value instanceof Date) return '"' + value.toISOString() + '"';
            if (value instanceof RegExp) return '"' + value.toString() + '"';
            
            // 调用自定义 toJSON
            if (value && typeof value.toJSON === 'function') {
                value = value.toJSON(key);
            }
            
            // 应用 replacer
            if (replacer) {
                value = typeof replacer === 'function' 
                    ? replacer(key, value)
                    : (replacer && replacer.length && replacer.indexOf(key) !== -1 ? value : undefined);
            }
            
            // 基础类型处理
            if (value === null || typeof value !== 'object') {
                return typeof value === 'string' 
                    ? '"' + value.replace(/"/g, '\\"') + '"'
                    : String(value);
            }
            
            // 数组/对象处理
            if (value.constructor === Array) {
                var arrResult = [];
                for (var i = 0; i < value.length; i++) {
                    var v = serialize(i, value[i]);
                    if (v !== undefined) arrResult.push(v);
                }
                return '[' + arrResult.join(',') + ']';
            }
            
            var props = [];
            for (var k in value) {
                if (value.hasOwnProperty(k)) {
                    var v = serialize(k, value[k]);
                    if (v !== undefined) props.push('"' + k + '":' + v);
                }
            }
            return '{' + props.join(',') + '}';
        }
        
        try {
            var result = serialize('', obj);
            cache = null;
            return result;
        } catch (e) {
            // ExtendScript 专用错误输出
            if (typeof $.writeln === 'function') {
                $.writeln('JSON.stringify error: ' + e.message);
            }
            return 'null';
        }
    };
}

// 配置对象
var config = {
    canvasSize: {
        width: 0,
        height: 0
    },
    layers: [],
    version: "1.0.0"
};

/**
 * 获取图层类型
 * @param {Layer} layer - Photoshop图层
 * @returns {string} 图层类型
 */
function getLayerType(layer) {
    if (layer.typename === "LayerSet") {
        return "Group";
    } else if (layer.kind === LayerKind.TEXT) {
        return "Text";
    } else if (layer.kind === LayerKind.SOLIDFILL) {
        return "SolidColor";
    } else {
        return "Image";
    }
}

/**
 * 获取图层效果
 * @param {Layer} layer - Photoshop图层
 * @returns {Object} 效果配置
 */
function getLayerEffects(layer) {
    // 创建默认效果对象，所有效果默认为false
    var effects = {
        dropShadow: false,
        innerShadow: false,
        outerGlow: false,
        innerGlow: false,
        bevelEmboss: false
    };

    try {
        var ref = new ActionReference();
        var keyLayerEffects = app.charIDToTypeID('Lefx');
        ref.putProperty(app.charIDToTypeID('Prpr'), keyLayerEffects);
        ref.putEnumerated(app.charIDToTypeID('Lyr '), app.charIDToTypeID('Ordn'), app.charIDToTypeID('Trgt'));
        var desc = executeActionGet(ref);
        
        if (desc.hasKey(keyLayerEffects)) {
            var effectsDesc = desc.getObjectValue(keyLayerEffects);
            
            // 检查各种效果
            if (effectsDesc.hasKey(stringIDToTypeID('dropShadow'))) {
                effects.dropShadow = true;
            }
            if (effectsDesc.hasKey(stringIDToTypeID('innerShadow'))) {
                effects.innerShadow = true;
            }
            if (effectsDesc.hasKey(stringIDToTypeID('outerGlow'))) {
                effects.outerGlow = true;
            }
            if (effectsDesc.hasKey(stringIDToTypeID('innerGlow'))) {
                effects.innerGlow = true;
            }
            if (effectsDesc.hasKey(stringIDToTypeID('bevelEmboss'))) {
                effects.bevelEmboss = true;
            }
        }
    } catch (e) {
        // 如果获取效果失败，返回默认效果对象
        $.writeln("获取图层效果失败: " + e);
    }

    return effects;
}

/**
 * 获取图层颜色信息
 * @param {Layer} layer - Photoshop图层
 * @returns {Object} 颜色信息
 */
function getLayerColor(layer) {
    var color = {
        r: 255,
        g: 255,
        b: 255,
        a: 1
    };
    
    try {
        if (layer.kind === LayerKind.SOLIDFILL) {
            // 对于纯色图层，使用fillColor
            var fillColor = layer.fillColor;
            if (fillColor) {
                color.r = fillColor.rgb.red;
                color.g = fillColor.rgb.green;
                color.b = fillColor.rgb.blue;
            }
        } else if (layer.kind === LayerKind.TEXT) {
            // 对于文本图层，使用文本颜色
            var textColor = layer.textItem.color;
            if (textColor) {
                color.r = textColor.rgb.red;
                color.g = textColor.rgb.green;
                color.b = textColor.rgb.blue;
            }
        } else {
            // 对于其他图层，尝试获取前景色
            var foregroundColor = app.foregroundColor;
            if (foregroundColor) {
                color.r = foregroundColor.rgb.red;
                color.g = foregroundColor.rgb.green;
                color.b = foregroundColor.rgb.blue;
            }
        }
        
        // 设置不透明度
        color.a = layer.opacity / 100;
    } catch (e) {
        // 如果获取颜色失败，使用默认白色
        $.writeln("获取颜色失败: " + e);
    }
    
    return color;
}

/**
 * 获取文本图层信息
 * @param {Layer} layer - Photoshop文本图层
 * @returns {Object} 文本信息
 */
function getTextLayerInfo(layer) {
    if (layer.kind !== LayerKind.TEXT) return null;
    
    return {
        text: layer.textItem.contents,
        font: layer.textItem.font,
        size: layer.textItem.size,
        color: {
            r: layer.textItem.color.rgb.red,
            g: layer.textItem.color.rgb.green,
            b: layer.textItem.color.rgb.blue,
            a: layer.opacity / 100
        },
        alignment: layer.textItem.justification,
        leading: layer.textItem.leading,
        tracking: layer.textItem.tracking
    };
}

/**
 * 处理图层
 * @param {Layer} layer - Photoshop图层
 * @param {number} index - 图层索引
 * @param {Array} parentPath - 父图层路径
 */
function processLayer(layer, index, parentPath) {
    // 计算位置和尺寸
    var bounds = layer.bounds;
    var position = {
        x: Number(bounds[0].value),
        y: Number(bounds[1].value),
        width: Number(bounds[2].value - bounds[0].value),
        height: Number(bounds[3].value - bounds[1].value)
    };

    var layerInfo = {
        id: layer.id.toString(),
        name: layer.name,
        type: getLayerType(layer),
        visible: layer.visible,
        opacity: layer.opacity / 100,
        position: position,
        effects: getLayerEffects(layer),
        color: getLayerColor(layer),
        parentId: parentPath.length > 0 ? parentPath[parentPath.length - 1] : null
    };

    // 处理文本图层
    if (layer.kind === LayerKind.TEXT) {
        layerInfo.textInfo = getTextLayerInfo(layer);
    }

    // 将当前图层添加到配置中
    config.layers.push(layerInfo);

    // 处理图层组
    if (layer.typename === "LayerSet") {
        var newPath = parentPath.slice();
        newPath.push(layer.id.toString());
        
        for (var i = 0; i < layer.layers.length; i++) {
            processLayer(layer.layers[i], i, newPath);
        }
    }
}

/**
 * 导出PSD配置
 */
function exportPsdConfig() {
    var doc = app.activeDocument;
    
    // 重置配置
    config = {
        canvasSize: {
            width: doc.width,
            height: doc.height
        },
        layers: [],
        version: "1.0.0"
    };
    
    // 处理所有图层
    for (var i = 0; i < doc.layers.length; i++) {
        processLayer(doc.layers[i], i, []);
    }
    
    // 保存配置
    var savePath = Folder.selectDialog("选择配置保存路径");
    if (savePath != null) {
        var configFile = new File(savePath + "/" + doc.name.replace(".psd", "") + "_config.json");
        configFile.open("w");
        configFile.write(JSON.stringify(config, null, 2));
        configFile.close();
        
        alert("配置已成功导出!");
    }
}

// 执行导出
app.activeDocument.suspendHistory("ExportPsdConfig", "exportPsdConfig();");
