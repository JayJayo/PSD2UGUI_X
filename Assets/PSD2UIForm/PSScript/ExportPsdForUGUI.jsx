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
//app.activeDocument.suspendHistory("Convert2SmartObject", "convertAndExport();");
//~ convertLayersToSmartObjects (app.activeDocument.layers);


// JSON polyfill for ExtendScript
if (typeof JSON !== 'object') {
    JSON = {};
}

// JSON.stringify polyfill
// 仅在原生 JSON.stringify 不存在时实现（如 ExtendScript 环境）
if (typeof JSON.stringify !== 'function') {
    JSON.stringify = function(obj, replacer, space) {
        // 缓存已处理对象（用于检测循环引用）
        var cache = [];
        
        // 核心序列化函数
        function serialize(key, value) {
            // 处理循环引用
            if (typeof value === 'object' && value !== null) {
                if (cache.indexOf(value) !== -1) return '[Circular]';
                cache.push(value);
            }

            // 优先调用对象的 toJSON 方法
            if (value && typeof value.toJSON === 'function') {
                value = value.toJSON(key);
            }

            // 应用自定义 replacer
            if (replacer) {
                value = typeof replacer === 'function' 
                    ? replacer(key, value)
                    : (replacer.indexOf(key) !== -1 ? value : undefined);
            }

            // 按类型序列化
            if (value === null) return 'null';
            switch (typeof value) {
                case 'string': return '"' + value.replace(/"/g, '\\"') + '"';
                case 'number': return isFinite(value) ? String(value) : 'null';
                case 'boolean': return String(value);
                case 'object':
                    if (Array.isArray(value)) {
                        return '[' + value.map(function(v, i) {
                            return serialize(i, v) || 'null';
                        }).join(',') + ']';
                    } else {
                        var props = [];
                        for (var k in value) {
                            if (value.hasOwnProperty(k)) {
                                var v = serialize(k, value[k]);
                                if (v !== undefined) {
                                    props.push('"' + k + '":' + v);
                                }
                            }
                        }
                        return '{' + props.join(',') + '}';
                    }
                default: return undefined; // 忽略 function/undefined/symbol
            }
        }

        // 执行序列化
        try {
            var result = serialize('', obj);
            cache = null;
            
            // 美化输出（简化版）
            if (space) {
                if (typeof space === 'number') {
                    space = ' '.repeat(Math.min(10, space));
                }
                result = result.replace(/[{,]/g, '$&\n' + space);
            }
            return result;
        } catch (e) {
            console.error('JSON.stringify error:', e);
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
    var effects = {};
    try {
        var ref = new ActionReference();
        var keyLayerEffects = app.charIDToTypeID('Lefx');
        ref.putProperty(app.charIDToTypeID('Prpr'), keyLayerEffects);
        ref.putEnumerated(app.charIDToTypeID('Lyr '), app.charIDToTypeID('Ordn'), app.charIDToTypeID('Trgt'));
        var desc = executeActionGet(ref);
        if (desc.hasKey(keyLayerEffects)) {
            // 处理各种效果
            if (desc.hasKey(stringIDToTypeID('dropShadow'))) {
                effects.dropShadow = true;
            }
            if (desc.hasKey(stringIDToTypeID('innerShadow'))) {
                effects.innerShadow = true;
            }
            if (desc.hasKey(stringIDToTypeID('outerGlow'))) {
                effects.outerGlow = true;
            }
            if (desc.hasKey(stringIDToTypeID('innerGlow'))) {
                effects.innerGlow = true;
            }
            if (desc.hasKey(stringIDToTypeID('bevelEmboss'))) {
                effects.bevelEmboss = true;
            }
        }
    } catch (e) {
        // 忽略错误
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
    
    if (layer.kind === LayerKind.SOLIDFILL) {
        try {
            var ref = new ActionReference();
            ref.putEnumerated(charIDToTypeID("Lyr "), charIDToTypeID("Ordn"), charIDToTypeID("Trgt"));
            var desc = executeActionGet(ref);
            
            if (desc.hasKey(charIDToTypeID("Clr "))) {
                var colorDesc = desc.getObjectValue(charIDToTypeID("Clr "));
                if (colorDesc.hasKey(charIDToTypeID("Rd  "))) {
                    color.r = colorDesc.getInteger(charIDToTypeID("Rd  "));
                }
                if (colorDesc.hasKey(charIDToTypeID("Grn "))) {
                    color.g = colorDesc.getInteger(charIDToTypeID("Grn "));
                }
                if (colorDesc.hasKey(charIDToTypeID("Bl  "))) {
                    color.b = colorDesc.getInteger(charIDToTypeID("Bl  "));
                }
            }
            color.a = layer.opacity / 100;
        } catch (e) {
            // 如果获取颜色失败，使用默认白色
            $.writeln("获取颜色失败: " + e);
        }
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
    var layerInfo = {
        id: layer.id.toString(),
        name: layer.name,
        type: getLayerType(layer),
        visible: layer.visible,
        opacity: layer.opacity / 100,
        position: {
            x: layer.bounds[0],
            y: layer.bounds[1],
            width: layer.bounds[2] - layer.bounds[0],
            height: layer.bounds[3] - layer.bounds[1]
        },
        effects: getLayerEffects(layer),
        color: getLayerColor(layer),
        parentId: parentPath.length > 0 ? parentPath[parentPath.length - 1] : null,
        children: []
    };

    // 处理文本图层
    if (layer.kind === LayerKind.TEXT) {
        layerInfo.textInfo = getTextLayerInfo(layer);
    }

    // 处理图层组
    if (layer.typename === "LayerSet") {
        var newPath = parentPath.slice();
        newPath.push(layer.id.toString());
        
        for (var i = 0; i < layer.layers.length; i++) {
            var childLayer = processLayer(layer.layers[i], i, newPath);
            layerInfo.children.push(childLayer);
        }
    }

    return layerInfo;
}

/**
 * 导出PSD配置
 */
function exportPsdConfig() {
    var doc = app.activeDocument;
    
    // 设置画布大小
    config.canvasSize.width = doc.width;
    config.canvasSize.height = doc.height;
    
    // 处理所有图层
    for (var i = 0; i < doc.layers.length; i++) {
        var layer = processLayer(doc.layers[i], i, []);
        config.layers.push(layer);
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
