Shader "MyShader/OutlineEffect" {
    Properties {
        _OutlineWidth ("Outline Width", Range(0, 10)) = 3

    }
 
    SubShader {
        Tags {
            // 渲染队列: Background(1000, 后台)、Geometry(2000, 几何体, 默认)、Transparent(3000, 透明)、Overlay(4000, 覆盖)
            "Queue" = "Transparent+110"
            "RenderType" = "Transparent"
            "DisableBatching" = "True"
        }
 
        // 将待描边物体的屏幕区域像素对应的模板值标记为1
        Pass {
Cull Off // 关闭剔除渲染, 取值有: Off、Front、Back, Off表示正面和背面都渲染
ZTest Always // 总是通过深度测试, 使得物体即使被遮挡时, 也能生成模板
ZWrite Off // 关闭深度缓存, 避免该物体遮挡前面的物体
ColorMask 0 // 允许通过的颜色通道, 取值有: 0、R、G、B、A、RGBA的组合(RG、RGB等), 0表示不渲染颜色
 
Stencil { // 模板测试, 只有通过模板测试的像素才会渲染
Ref 1 // 设定参考值为1

Pass Replace // 如果通过模板测试, 将像素的模板值设置为参考值(1), 模板值的初值为0, 没有Comp表示总是通过模板测试
}
}
 
        // 绘制模板标记外的物体像素, 即膨胀的外环上的像素
        Pass {
            Cull Off // 关闭剔除渲染, 取值有: Off、Front、Back, Off表示正面和背面都渲染
            ZTest Always // 总是通过深度测试, 使得物体即使被遮挡时, 也能生成描边
            ZWrite Off // 关闭深度缓存, 避免该物体遮挡前面的物体
            Blend SrcAlpha OneMinusSrcAlpha // 混合测试, 与背后的物体颜色混合
            ColorMask RGB // 允许通过的颜色通道, 取值有: 0、R、G、B、A、RGBA的组合(RG、RGB等), 0表示不渲染颜色
 
            Stencil { // 模板测试, 只有通过模板测试的像素才会渲染
                Ref 1 // 设定参考值为1
                Comp NotEqual // 这里只有模板值为0的像素才会通过测试, 即只有膨胀的外环上的像素能通过模板测试
            }
 
            CGPROGRAM
            #include "UnityCG.cginc"
 
            #pragma vertex vert
            #pragma fragment frag
 
            uniform float _OutlineWidth;

   
            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float3 smoothNormal : TEXCOORD3; // 平滑的法线, 对相同顶点的所有法线取平均值
            };
 
            struct v2f {
                float4 position : SV_POSITION;
            };
 
            v2f vert(appdata input) {
                v2f output;
                float3 normal = any(input.smoothNormal) ? input.smoothNormal : input.normal; // 光滑的法线
                float3 viewPosition = UnityObjectToViewPos(input.vertex); // 相机坐标系下的顶点坐标
                float3 viewNormal = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, normal)); // 相机坐标系下的法线向量
                // 裁剪坐标系下的顶点坐标, 将顶点坐标沿着法线方向向外延伸, 延伸的部分就是描边部分
                // 乘以(-viewPosition.z)是为了抵消透视变换造成的描边宽度近大远小效果, 使得物体无论距离相机多远, 描边宽度都不发生变化
                // 除以1000是为了将描边宽度单位转换到1mm(这里的宽度是世界坐标系中的宽度, 而不是屏幕上的宽度)
                output.position = UnityViewToClipPos(viewPosition + viewNormal * _OutlineWidth * (-viewPosition.z) / 1000);
                return output;
            }
 
            fixed4 frag(v2f input) : SV_Target {
return float4(0.97, 0.91, 0.31, 1);
            }
 
            ENDCG
        }
    }
}